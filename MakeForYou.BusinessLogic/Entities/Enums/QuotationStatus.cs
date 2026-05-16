using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Entities.Enums
{
    public static class QuotationStatus
    {
        public const int Pending = 0;
        public const int Accepted = 1;
        public const int Rejected = 2;
        public const int Confirmed = 3;
        public const int Cancelled = 4;
    }
}
