using System;
using Microsoft.AspNetCore.Http;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    /// Contains utility methods to help manage JWT tokens.
    /// </summary>
    public class TokenManagerAbstract
    {
        /// <summary>
        /// Set an HTTP cookie containing the current refresh token.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        protected void setTokenCookie(IHttpContextAccessor context, string token)
        {
            var cookieOptions = new CookieOptions{
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            context.HttpContext.Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        /// <summary>
        /// Attempt to get the IP address of the device that placed the HTTP request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected string getIp(IHttpContextAccessor context)
        {
            if (context.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For")) {
                return context.HttpContext.Request.Headers["X-Forwarded-For"];
            } else {
                return context.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
    }
}
