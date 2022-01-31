namespace jahndigital.studentbank.services.Exceptions
{
    public class ShareTypeNotFoundException : BaseException
    {
        public ShareTypeNotFoundException(long shareTypeId) : base(
            $"Share #{shareTypeId} not found."
        )
        {
            ShareTypeId = shareTypeId;
        }

        public long ShareTypeId { get; }
    }
}
