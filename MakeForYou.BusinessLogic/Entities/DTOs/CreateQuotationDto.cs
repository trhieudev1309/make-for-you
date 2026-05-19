using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Entities.DTOs
{
    public class CreateQuotationDto
    {
        public long OrderId { get; set; }
        public long CreatedBy { get; set; }   // SellerId
        public int ProposedPrice { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
