using JahnDigital.StudentBank.Domain.Entities;

namespace JahnDigital.StudentBank.WebApi.Models
{
    /// <summary>
    ///     Request to link or unlink a stock to an <see cref="Instance" />.
    /// </summary>
    public class LinkStockRequest
    {
        /// <summary>
        ///     The stock to link.
        /// </summary>
        public long StockId { get; set; } = default!;

        /// <summary>
        ///     The instance to link to.
        /// </summary>
        public long InstanceId { get; set; } = default!;
    }
}
