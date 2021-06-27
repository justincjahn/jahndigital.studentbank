using System.ComponentModel.DataAnnotations;

namespace jahndigital.studentbank.server.Models
{
    public class StudentPreauthenticationRequest
    {
        [Required] public string AccountNumber { get; set; } = string.Empty;

        [Required] public string InviteCode { get; set; } = string.Empty;
    }
}