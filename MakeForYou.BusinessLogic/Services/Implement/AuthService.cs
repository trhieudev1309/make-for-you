using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly IEmailService _email;
        private readonly ILogger<AuthService> _logger;

        // Sử dụng \W để chấp nhận mọi ký tự không phải chữ và số (bao gồm cả #, ., _, ...)
        private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_])[A-Za-z\d\W_]{8,}$");

        public AuthService(IUserRepository userRepository, ApplicationDbContext context, IHttpContextAccessor http, IEmailService email, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _context = context;
            _http = http;
            _email = email;
            _logger = logger;
        }

        public async Task<RegisterRespond> RegisterAsync(RegisterRequest req)
        {
            // 1. Password strength
            if (!PasswordRegex.IsMatch(req.Password))
            {
                _logger.LogWarning("Registration failed for email {Email}: password does not meet requirements", req.Email);
                return Fail("Password must be 8–15 characters and include uppercase, lowercase, digit, and special character.");
            }

            // 2. Unique email
            if (await _userRepository.EmailExistsAsync(req.Email))
            {
                _logger.LogWarning("Registration failed: email {Email} already exists", req.Email);
                return Fail("An account with this email already exists.");
            }

            // 3. Build User entity
            var user = new User
            {
                FullName = req.FullName.Trim(),
                Email = req.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12),
                Phone = req.Phone?.Trim(),
                Role = req.Role,
                Status = (int)UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            // 4. Persist User first (generates UserId via DB)
            var created = await _userRepository.CreateUserAsync(user);

            // 5. Create role-specific profile in same transaction
            await CreateRoleProfileAsync(created);

            _logger.LogInformation("User registered: userId={UserId}, role={Role}", created.UserId, (UserRole)req.Role);
            return new RegisterRespond
            {
                Success = true,
                Message = "Account created successfully.",
                UserId = created.UserId
            };
        }


        private async Task CreateRoleProfileAsync(MakeForYou.BusinessLogic.Entities.User user)
        {
            if (user.Role == 0) // Buyer
            {
                var buyer = new Buyer { BuyerId = user.UserId };
                _context.Buyers.Add(buyer);
            }
            else if (user.Role == 1) // Seller
            {
                var seller = new Seller { SellerId = user.UserId };
                _context.Sellers.Add(seller);
            }
            await _context.SaveChangesAsync();
        }

        private static RegisterRespond Fail(string message)
        {
            return new RegisterRespond
            {
                Success = false,
                Message = message
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest req)
        {
            // 1. Find user by email
            var user = await _userRepository.FindByEmailAsync(req.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: email {Email} not found", req.Email);
                return LoginFail("Null email or password.");
            }

            // 2. Check account status
            if (user.Status == (int)UserStatus.Banned)
            {
                _logger.LogWarning("Login denied: user {UserId} is banned", user.UserId);
                return LoginFail("Your account has been suspended.");
            }

            if (user.Status == (int)UserStatus.Inactive)
            {
                _logger.LogWarning("Login denied: user {UserId} is inactive", user.UserId);
                return LoginFail("Your account is inactive. Please contact support.");
            }

            // 3. Verify password (BCrypt comparison — timing-safe)
            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: invalid password for user {UserId}", user.UserId);
                return LoginFail("Invalid email or password.");
            }

            // 4. Build claims and sign in with cookie auth
            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new(ClaimTypes.Name,           user.FullName),
        new(ClaimTypes.Email,          user.Email),
        new(ClaimTypes.Role,           ((UserRole)user.Role).ToString()),
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await _http.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
            new AuthenticationProperties { IsPersistent = false }
            );

            _logger.LogInformation("User {UserId} logged in, role={Role}", user.UserId, (UserRole)user.Role);
            return new LoginResponse
            {
                Success = true,
                Message = "Login successful.",
                UserId = user.UserId,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        private static LoginResponse LoginFail(string msg) =>
    new() { Success = false, Message = msg };



        public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest req)
        {
            var user = await _userRepository.FindByEmailAsync(req.Email);

            // Always return the same message — never reveal if the email exists
            const string msg = "If that email is registered, a reset link has been sent.";
            if (user == null)
            {
                _logger.LogDebug("Password reset requested for unknown email");
                return AuthResult.Ok(msg);
            }

            // Generate a cryptographically random token
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenHash = HashToken(rawToken);

            await _userRepository.InvalidateUserTokensAsync(user.UserId);   // expire old tokens
            await _userRepository.SavePasswordResetTokenAsync(new PasswordResetToken
            {
                UserId = user.UserId,
                Token = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false
            });

            var request = _http.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var resetLink = $"{baseUrl}/Auth/ResetPassword?token={Uri.EscapeDataString(rawToken)}&email={Uri.EscapeDataString(user.Email)}";
            await _email.SendAsync(user.Email, "Reset your MakeForYou password",
                $"<p>Click the link below to reset your password. It expires in 30 minutes.</p>" +
                $"<p><a href='{resetLink}'>Reset Password</a></p>");

            _logger.LogInformation("Password reset email sent for user {UserId}", user.UserId);
            return AuthResult.Ok(msg);
        }

        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest req)
        {
            if (!PasswordRegex.IsMatch(req.NewPassword))
            {
                _logger.LogWarning("Password reset failed for email {Email}: new password does not meet requirements", req.Email);
                return AuthResult.Fail("Password must be 8–15 characters and include uppercase, lowercase, digit, and special character.");
            }

            var tokenHash = HashToken(req.Token);
            var record = await _userRepository.FindValidResetTokenAsync(req.Email, tokenHash);

            if (record == null)
            {
                _logger.LogWarning("Password reset failed for email {Email}: invalid or expired token", req.Email);
                return AuthResult.Fail("This reset link is invalid or has expired.");
            }

            // Update password
            record.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword, workFactor: 12);
            record.IsUsed = true;
            await _userRepository.SavePasswordResetTokenAsync(record);

            _logger.LogInformation("Password reset successfully for user {UserId}", record.UserId);
            return AuthResult.Ok("Your password has been reset. You can now sign in.");
        }

        // Helper — store only the hash of the token, never the raw value
        private static string HashToken(string raw)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
