namespace MakeForYou.BusinessLogic.Entities.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Quoted = 2,
        InProgress = 3,
        Completed = 4,
        Delivering = 5,
        Delivered = 6,
        Done = 7,
        Cancelled = 8,
        PendingQuotationSubmit  = 9,   // cart order paid; items need seller quotation
        PendingQuotationAccept  = 10,  // seller quoted customisation items; buyer must accept
        PendingQuotationPayment = 11   // buyer accepted quotation; must pay before proceeding
    }
}
