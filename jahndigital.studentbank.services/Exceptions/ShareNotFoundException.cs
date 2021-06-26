namespace jahndigital.studentbank.services.Exceptions
{
    public class ShareNotFoundException : BaseException
    {
        public long ShareId { get; private set; }

        public ShareNotFoundException(long shareId) : base (
            $"Share #{shareId} not found."
        ) {
            ShareId = shareId;
        }
    }
}