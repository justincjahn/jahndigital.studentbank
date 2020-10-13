using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.Models
{
    /// <summary>
    /// Request to transfer funds from one share to another.
    /// </summary>
    public class NewTransferRequest
    {
        /// <summary>
        /// The source <see cref="dal.Entities.Share"/> ID.
        /// </summary>
        public long SourceShareId {get; set;} = default!;

        /// <summary>
        /// The destination <see cref="dal.Entities.Share"/> ID.
        /// </summary>
        public long DestinationShareId {get; set;} = default!;

        /// <summary>
        /// The amount to transfer.
        /// </summary>
        public Money Amount {get; set;} = default!;

        /// <summary>
        /// Get or set an optional comment.
        /// </summary>
        public string? Comment {get; set;} = default!;
    }
}
