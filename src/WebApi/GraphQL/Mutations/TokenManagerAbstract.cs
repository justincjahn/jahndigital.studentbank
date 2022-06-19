using System;
using Microsoft.AspNetCore.Http;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
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
            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            };

            GetHttpContext(context).Response.Cookies.Append(TOKEN_COOKIE, token, cookieOptions);
        }

        /// <summary>
        ///     Delete the refresh token from the response cookies.
        /// </summary>
        /// <param name="context"></param>
        protected void ClearTokenCookie(IHttpContextAccessor context)
        {
            GetHttpContext(context).Response.Cookies.Delete(TOKEN_COOKIE);
        }

        /// <summary>
        ///     Attempt to get the IP address of the device that placed the HTTP request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected string GetIp(IHttpContextAccessor context)
        {
            HttpContext ctx = GetHttpContext(context);

            if (ctx.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return ctx.Request.Headers["X-Forwarded-For"];
            }

            return ctx.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
        }

        /// <summary>
        /// Fetches the HttpContext object from the context accessor, or throws.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private HttpContext GetHttpContext(IHttpContextAccessor context)
        {
            return context.HttpContext
                ?? throw new ArgumentNullException(
                    nameof(context.HttpContext), "Unable to fetch HTTP Context object to manage token refresh cookies.");
        }
    }
}
