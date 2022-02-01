using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JahnDigital.StudentBank.Domain.Entities
{
    /// <summary>
    ///     Links a <see cref="Stock" /> to an <see cref="Instance" />.
    /// </summary>
    public class StockInstance
    {
        /// <summary>
        ///     The ID number of the group.
        /// </summary>
        [ForeignKey("Instance"), Required]
        public long InstanceId { get; set; }

        /// <summary>
        ///     The ID number of the stock.
        /// </summary>
        [ForeignKey("Stock"), Required]
        public long StockId { get; set; }

        /// <summary>
        ///     Gets or sets the <see cname="Instance" /> a product has been released to.
        /// </summary>
        [Required]
        public Instance Instance { get; set; } = default!;

        /// <summary>
        ///     The Stock associated with this link.
        /// </summary>
        [Required]
        public Stock Stock { get; set; } = default!;
    }
}
