using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Profile
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IUserRepository _userRepo;

        public EditModel(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public User? UserProfile { get; set; }

        [BindProperty]
        public EditProfileInput Input { get; set; } = new();

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

            // Populate form with current data
            Input = new EditProfileInput
            {
                FullName = UserProfile.FullName,
                Email = UserProfile.Email,
                Phone = UserProfile.Phone
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            UserProfile = await _userRepo.GetByIdAsync(userId);
            if (UserProfile == null)
            {
                return NotFound();
            }

            // Update fields
            UserProfile.FullName = Input.FullName;
            UserProfile.Phone = Input.Phone;
            // Note: Email updates might require verification; keeping simple for now

            await _userRepo.UpdateAsync(UserProfile);

            return RedirectToPage("Index", new { message = "Profile updated successfully." });
        }
    }

    public class EditProfileInput
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(200)]
        public string FullName { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = null!;

        [MaxLength(50)]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? Phone { get; set; }
    }
}
