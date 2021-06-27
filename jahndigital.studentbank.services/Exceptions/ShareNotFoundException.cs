namespace jahndigital.studentbank.services.Exceptions
{
    public class ShareNotFoundException : BaseException
    {
        public ShareNotFoundException(long shareId) : base(
            $"Share #{shareId} not found."
        )
        {
            ShareId = shareId;
        }

        public long ShareId { get; }
    }
}