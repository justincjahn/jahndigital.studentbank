namespace jahndigital.studentbank.server.Models
{
    /// <summary>
    /// Request data to update a group.
    /// </summary>
    public class UpdateGroupRequest
    {
        /// <summary>
        /// Get or set the ID number of the group.
        /// </summary>
        public long Id {get; set;}

        /// <summary>
        /// Get or set the name of the group.
        /// </summary>
        public string? Name {get; set;}

        /// <summary>
        /// Get or set the Instance ID of the group.
        /// </summary>
        public long? InstanceId {get; set;}
    }
}
