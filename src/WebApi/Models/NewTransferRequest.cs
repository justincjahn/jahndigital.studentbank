using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.WebApi.Models
{
    /// <summary>
    ///     Request to transfer funds from one share to another.
    /// </summary>
    public class NewTransferRequest
    {
        /// <summary>
        ///     The source <see cref="Share" /> ID.
        /// </summary>
        public long SourceShareId { get; set; } = default!;

        /// <summary>
        ///     The destination <see cref="Share" /> ID.
        /// </summary>
        public long DestinationShareId { get; set; } = default!;

        /// <summary>
        ///     The amount to transfer.
        /// </summary>
        public Money Amount { get; set; } = default!;

        /// <summary>
        ///     Get or set an optional comment.
        /// </summary>
        public string? Comment { get; set; } = default!;
    }
}
