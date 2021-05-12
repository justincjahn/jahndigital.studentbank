using System.Threading.Tasks;
using jahndigital.studentbank.server.Models;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Contract to describe methods that authenticate users.
    /// </summary>
    public interface IUserService
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
        /// Revoke a valid JWT token.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
         Task<bool> RevokeTokenAsync(string token, string ipAddress);
    }
}
