namespace JahnDigital.StudentBank.Application.Common.Exceptions;

public class ShareNotFoundException : NotFoundException
{
    public ShareNotFoundException(long shareId) : base(
        $"Share #{shareId} not found."
    )
    {
        ShareId = shareId;
    }

    public long ShareId { get; }
}
