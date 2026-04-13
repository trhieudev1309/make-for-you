using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class Seller
    {
        [Key, ForeignKey(nameof(User))]
        public long SellerId { get; set; }

        public User User { get; set; } = null!;

        public string? Bio { get; set; }
        public string? SkillDescription { get; set; }
        public int? PriceRange { get; set; }
        public int? AvailabilityStatus { get; set; }
        public int? AverageRating { get; set; }
        public int? TotalReviews { get; set; }

        public ICollection<PortfolioItem>? PortfolioItems { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}