using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entities
{
    public class Seller
    {
        [Key, ForeignKey(nameof(User))]
        public long SellerId { get; set; }

        public User User { get; set; } = null!;

        [MaxLength(100)]
        public string? ShopName { get; set; }

        [MaxLength(300)]
        public string? ShopDescription { get; set; }

        [MaxLength(100)]
        public string? PickupFullName { get; set; }

        [MaxLength(20)]
        public string? PickupPhone { get; set; }

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(100)]
        public string? Ward { get; set; }

        [MaxLength(300)]
        public string? AddressDetail { get; set; }

        public string? Bio { get; set; }
        public string? SkillDescription { get; set; }
        public int? PriceRange { get; set; }
        public int? AvailabilityStatus { get; set; }
        public int? AverageRating { get; set; }
        public int? TotalReviews { get; set; }

        // Products authored by this seller
        [MaxLength(20)]  public string? BankBin { get; set; }
        [MaxLength(50)]  public string? BankAccountNumber { get; set; }
        [MaxLength(200)] public string? BankAccountName { get; set; }

        public ICollection<Product>? Products { get; set; }

        public ICollection<PortfolioItem>? PortfolioItems { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<SellerPost>? Posts { get; set; }
    }
}