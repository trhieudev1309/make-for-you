using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class CreateSellerPostRequest
    {
        [Required, MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>Optional cover image uploaded from a form.</summary>
        public IFormFile? Image { get; set; }
    }

    public class UpdateSellerPostRequest
    {
        [Required, MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// New image to replace the existing one.
        /// When null the current ImageUrl is kept unchanged.
        /// </summary>
        public IFormFile? Image { get; set; }
    }
}
