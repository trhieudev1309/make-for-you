using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MakeForYou.Presentation.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>Register as a Buyer (Role=0) or Seller (Role=1).</summary>
        /// <response code="201">Account created</response>
        /// <response code="400">Validation error or duplicate email</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterRespond), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RegisterRespond), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (result.Success)
            {
                _logger.LogInformation("User registered successfully: userId={UserId}", result.UserId);
                return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
            }
            _logger.LogWarning("Registration failed: {Reason}", result.Message);
            return BadRequest(result);
        }
    }
}
