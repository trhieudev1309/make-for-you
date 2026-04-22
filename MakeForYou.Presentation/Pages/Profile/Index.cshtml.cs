using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Profile
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUserRepository _userRepo;

        public IndexModel(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public User? UserProfile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            UserProfile = await _userRepo.GetByIdAsync(userId);
            if (UserProfile == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
