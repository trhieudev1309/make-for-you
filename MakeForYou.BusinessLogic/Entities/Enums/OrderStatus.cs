using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Quoted = 1,
        Confirmed = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5
    }
}
