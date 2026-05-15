using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class UpdateProgressRequest
{
    [Required, MaxLength(500)]
    public string Note { get; set; } = string.Empty;

    // New status seller wants to set
    [Required, Range(0, 5)]
    public int NewStatus { get; set; }

    // Optional image upload
    public IFormFile? Image { get; set; }
}
