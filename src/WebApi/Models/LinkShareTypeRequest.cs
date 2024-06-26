using JahnDigital.StudentBank.Domain.Entities;

namespace JahnDigital.StudentBank.WebApi.Models
{
    /// <summary>
    ///     Request to link or unlink a share type to an <see cref="Instance" />.
    /// </summary>
    public class LinkShareTypeRequest
    {
        /// <summary>
        ///     The share type to link.
        /// </summary>
        public long ShareTypeId { get; set; } = default!;

        /// <summary>
        ///     The instance to link to.
        /// </summary>
        public long InstanceId { get; set; } = default!;
    }
}
