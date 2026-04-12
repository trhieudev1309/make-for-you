using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUNews.BusinessLogic.Entities;

[Table("SystemAccount")]
public partial class SystemAccount
{
    [Key]
    [Column("AccountID")]
    public short AccountId { get; set; }

    [StringLength(100)]
    public string? AccountName { get; set; }

    [StringLength(70)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? AccountEmail { get; set; }

    public int? AccountRole { get; set; }

    public string? AccountPassword { get; set; }

    [InverseProperty("CreatedBy")]
    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
