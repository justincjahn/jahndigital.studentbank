using System.Collections.Generic;
using jahndigital.studentbank.server.Entities;
using jahndigital.studentbank.server.Models;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticate a user or student's request.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
         AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);

        /// <summary>
        /// Refresh a JWT token.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
         AuthenticateResponse RefreshToken(string token, string ipAddress);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
         bool RevokeToken(string token, string ipAddress);
    }
}
