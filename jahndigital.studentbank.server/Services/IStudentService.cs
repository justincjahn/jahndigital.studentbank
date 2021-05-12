using System.Threading.Tasks;
using jahndigital.studentbank.server.Models;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Contract to describe methods that authenticate students.
    /// </summary>
    public interface IStudentService
    {
        /// <summary>
        /// Authenticate a user or student's request.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
         Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest model, string ipAddress);

        /// <summary>
        /// Refresh a JWT token.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
         Task<AuthenticateResponse?> RefreshTokenAsync(string token, string ipAddress);

        /// <summary>
        /// Pre-authenticate a student using an invite token and account number.  The token that's generated
        /// is used to finalize registration.
        /// </summary>
        /// <param name="inviteCode"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        Task<string> AuthenticateInviteAsync(string inviteCode, string accountNumber);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        Task<bool> RevokeTokenAsync(string token, string ipAddress);
    }
}
