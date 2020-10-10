using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.Models
{
    /// <summary>
    /// Request data to update a Share Type.
    /// </summary>
    public class UpdateShareTypeRequest
    {
        /// <summary>
        /// Get or set the ID number of the share type.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Get or set the name of the share type.
        /// </summary>
        public string? Name {get; set;}

        /// <summary>
        /// Get or set the dividend rate.
        /// </summary>
        public Rate? DividendRate {get; set;}
    }
}
