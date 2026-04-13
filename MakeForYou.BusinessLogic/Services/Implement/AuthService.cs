using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");

        public AuthService(IUserRepository userRepository, ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<RegisterRespond> RegisterAsync(RegisterRequest req)
        {
            if (!PasswordRegex.IsMatch(req.Password))
            {
                return new RegisterRespond
                {
                    Success = false,
                    Message = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character."
                };
            }

            if (await _userRepository.EmailExistsAsync(req.Email))
            {
                return Fail("Email already exists.");
            }

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = req.Password,
                Phone = req.Phone,
                Role = req.Role,
                CreatedAt = DateTime.UtcNow,
                Status = 0
            };

            var createdUser = await _userRepository.CreateUserAsync(user);

            if (createdUser != null)
            {
                await CreateRoleProfileAsync(createdUser);
                return new RegisterRespond
                {
                    Success = true,
                    Message = "Registration successful.",
                    UserId = createdUser.UserId
                };
            }
            else
            {
                return Fail("Failed to create user.");
            }
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
    }
}
