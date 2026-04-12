using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entites
{
    public class Buyer
    {
        [Key, ForeignKey(nameof(User))]
        public long BuyerId { get; set; }

        public User User { get; set; } = null!;
    }
}