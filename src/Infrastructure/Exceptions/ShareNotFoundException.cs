namespace JahnDigital.StudentBank.Infrastructure.Exceptions
{
    public class ShareNotFoundException : DatabaseException
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
