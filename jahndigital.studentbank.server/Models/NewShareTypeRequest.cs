using jahndigital.studentbank.utils;

#nullable disable

namespace jahndigital.studentbank.server.Models
{
    /// <summary>
    /// Request data to create a Share Type.
    /// </summary>
    public class NewShareTypeRequest
    {
        /// <summary>
        /// Get or set the name of the share type.
        /// </summary>
        public string Name {get; set;}

        /// <summary>
        /// Get or set the dividend rate.
        /// </summary>
        public Rate DividendRate {get; set;} = Rate.FromRate(0.0m);
    }
}
