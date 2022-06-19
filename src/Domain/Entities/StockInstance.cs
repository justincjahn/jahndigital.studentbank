using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities
{
    /// <summary>
    ///     Links a <see cref="Stock" /> to an <see cref="Instance" />.
    /// </summary>
    public class StockInstance : EntityBase
    {
        /// <summary>
        ///     The ID number of the group.
        /// </summary>
        public long InstanceId { get; set; }

        /// <summary>
        ///     The ID number of the stock.
        /// </summary>
        public long StockId { get; set; }

        /// <summary>
        ///     Gets or sets the <see cname="Instance" /> a product has been released to.
        /// </summary>
        public Instance Instance { get; set; } = default!;

        /// <summary>
        ///     The Stock associated with this link.
        /// </summary>
        public Stock Stock { get; set; } = default!;
    }
}
