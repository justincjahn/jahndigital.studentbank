namespace jahndigital.studentbank.server.Models
{
    public class UpdateInstanceRequest
    {
        /// <summary>
        ///     Get or set the ID number of the instance.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Get or set the description of the instance.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        ///     Get or set if the instance is active.
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
