using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakeForYou.BusinessLogic.Entites
{
    public class Quotation
    {
        [Key]
        public long QuotationId { get; set; }

        public long OrderId { get; set; }

        public int? ProposedPrice { get; set; }

        public string? Message { get; set; }

        public long? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Status { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;
    }
}