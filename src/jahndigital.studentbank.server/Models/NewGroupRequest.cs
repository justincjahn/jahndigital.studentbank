namespace jahndigital.studentbank.server.Models
{
#nullable disable

    /// <summary>
    ///     Request data to create a group.
    /// </summary>
    public class NewGroupRequest
    {
        /// <summary>
        ///     Get or set the name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Get or set the Instance ID of the group.
        /// </summary>
        public long InstanceId { get; set; }
    }
}
