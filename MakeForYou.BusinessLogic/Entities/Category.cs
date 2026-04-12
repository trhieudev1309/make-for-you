using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUNews.BusinessLogic.Entities;

[Table("Category")]
public partial class Category
{
    [Key]
    [Column("CategoryID")]
    public short CategoryId { get; set; }

    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [StringLength(250)]
    public string CategoryDesciption { get; set; } = null!;

    [Column("ParentCategoryID")]
    public short? ParentCategoryId { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("ParentCategory")]
    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();

    [InverseProperty("Category")]
    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();

    [ForeignKey("ParentCategoryId")]
    [InverseProperty("InverseParentCategory")]
    public virtual Category? ParentCategory { get; set; }
}
