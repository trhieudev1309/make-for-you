namespace MakeForYou.BusinessLogic.DTOs
{
    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? SkillDescription { get; set; }
        public string? Bio { get; set; }
        public int? PriceRange { get; set; }
        public int? AvailabilityStatus { get; set; }
    }
}