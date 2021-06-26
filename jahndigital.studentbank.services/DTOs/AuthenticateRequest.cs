using System.ComponentModel.DataAnnotations;

namespace jahndigital.studentbank.services.DTOs
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticateRequest
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Username {get; set;} = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Password {get; set;} = string.Empty;
    }
}
