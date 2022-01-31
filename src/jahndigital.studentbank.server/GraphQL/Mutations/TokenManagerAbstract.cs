using System;
using Microsoft.AspNetCore.Http;

namespace jahndigital.studentbank.server.GraphQL.Mutations
{
    /// <summary>
    ///     Contains utility methods to help manage JWT tokens.
    /// </summary>
    public class TokenManagerAbstract
    {
        /// <summary>
        ///     The name of the cookie that stores the refresh token.
        /// </summary>
        public const string TOKEN_COOKIE = "refreshToken";

        /// <summary>
        ///     Get the refresh token from cookies, if set.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected string? GetToken(IHttpContextAccessor context)
        {
            return context.HttpContext?.Request.Cookies?[TOKEN_COOKIE];
        }

        /// <summary>
        ///     Set an HTTP cookie containing the current refresh token.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        protected void SetTokenCookie(IHttpContextAccessor context, string token)
        {
            HttpContext? httpc = context.HttpContext
                ?? throw new ArgumentNullException("Unable to fetch HTTP Context object to set token cookie.");

            CookieOptions? cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            };

            httpc.Response.Cookies.Append(TOKEN_COOKIE, token, cookieOptions);
        }

        /// <summary>
        ///     Delete the refresh token from the response cookies.
        /// </summary>
        /// <param name="context"></param>
        protected void ClearTokenCookie(IHttpContextAccessor context)
        {
            HttpContext? httpc = context.HttpContext
                ?? throw new ArgumentNullException(
                    "Unable to fetch HTTP Context object to clear token cookie.");

            httpc.Response.Cookies.Delete(TOKEN_COOKIE);
        }

        /// <summary>
        ///     Attempt to get the IP address of the device that placed the HTTP request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected string GetIp(IHttpContextAccessor context)
        {
            if (context.HttpContext?.Request.Headers.ContainsKey("X-Forwarded-For") ??
                throw new Exception("Unable to fetch HttpContext."))
            {
                return context.HttpContext.Request.Headers["X-Forwarded-For"];
            }

            return context.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "";
        }
    }
}
