namespace jahndigital.studentbank.server.Exceptions
{
    public class UnauthorizedPurchaseException : BaseException
    {
        public UnauthorizedPurchaseException() : base("Unauthorized purchase.") {}
    }
}
