using JahnDigital.StudentBank.Domain.Exceptions;

namespace JahnDigital.StudentBank.Application.Common.Exceptions
{
    public class ShareTypeNotFoundException : NotFoundException
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
