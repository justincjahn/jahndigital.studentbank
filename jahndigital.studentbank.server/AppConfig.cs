namespace jahndigital.studentbank.server
{
    /// <summary>
    /// Configuration settings for the application.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// AppConfig__Secret is used as a unique secret per-environment for JWT tokens
        /// and the like.
        /// </summary>
        public string Secret {get; set;} = default!;
    }
}
