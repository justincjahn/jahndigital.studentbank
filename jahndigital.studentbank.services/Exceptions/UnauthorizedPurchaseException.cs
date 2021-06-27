namespace jahndigital.studentbank.services.Exceptions
{
    public class UnauthorizedPurchaseException : BaseException
    {
        public UnauthorizedPurchaseException() : base("Unauthorized purchase.") { }
    }
}