using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Entities.DTOs
{
    public class QuotationDto
    {
        public long QuotationId { get; set; }
        public long OrderId { get; set; }
        public long SellerId { get; set; }
        public long BuyerId { get; set; }
        public int ProposedPrice { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
