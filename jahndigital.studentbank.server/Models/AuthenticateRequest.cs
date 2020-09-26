using System.ComponentModel.DataAnnotations;

namespace jahndigital.studentbank.server.Models
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
        public string Username {get;set;}

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Password {get;set;}
    }
}