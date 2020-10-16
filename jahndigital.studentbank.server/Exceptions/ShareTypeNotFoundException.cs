namespace jahndigital.studentbank.server.Exceptions
{
    public class ShareTypeNotFoundException : BaseException
    {
        public long ShareTypeId { get; private set; }

        public ShareTypeNotFoundException(long shareTypeId) : base (
            $"Share #{shareTypeId} not found."
        ) {
            ShareTypeId = shareTypeId;
        }
    }
}
