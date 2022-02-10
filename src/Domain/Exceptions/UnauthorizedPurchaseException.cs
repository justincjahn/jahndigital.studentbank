namespace JahnDigital.StudentBank.Domain.Exceptions
{
    public class UnauthorizedPurchaseException : BaseException
    {
        public UnauthorizedPurchaseException() : base("Unauthorized purchase.") { }
    }
}
