using System;
using System.Linq;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace jahndigital.studentbank.server.Controllers
{
    /// <summary>
    /// Handle user authentication and displaying user information.
    /// </summary>
    [ApiController, Route("[controller]"), Authorize(Roles =
        Constants.Role.ROLE_SUPERUSER + "," + Constants.Role.ROLE_STUDENT)]
    public class StudentsController : ControllerBase
    {
        private IStudentService _studentService;

        public StudentsController(IStudentService userService) => _studentService = userService;

        /// <summary>
        /// Authenticate a user via email and password and return a JWT token.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost("authenticate"), AllowAnonymous]
        public IActionResult Authenticate([FromBody] AuthenticateRequest model)
        {
            var response = _studentService.Authenticate(model, getIp());

            if (response == null) {
                return BadRequest(new { message = "Username or password incorrect." });
            }

            setTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        /// <summary>
        /// Refresh a user's token.
        /// </summary>
        /// <returns></returns>
        [HttpPost("refresh-token"), AllowAnonymous]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _studentService.RefreshToken(refreshToken, getIp());

            if (response == null) {
                return Unauthorized(new { message = "Invalid Token"});
            }

            setTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        /// <summary>
        /// Revoke a JWT token.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(token)) {
                return BadRequest(new { message = "Token is required."});
            }

            var response = _studentService.RevokeToken(token, getIp());

            if (!response) {
                return NotFound(new { message = "Token not found." });
            }

            return Ok(new { message = "Token Revoked" });
        }

        /// <summary>
        /// Get a student by ID.
        /// </summary>
        /// <param name="studentId">The ID number of the user.</param>
        /// <param name="context">Database context.</param>
        [HttpGet("{studentId}"), Authorize(Policy =
            Constants.AuthPolicy.DataOwner + "<" + Constants.Privilege.PRIVILEGE_MANAGE_STUDENTS + ">")]
        public ActionResult<Student> GetById(int studentId, [FromServices] AppDbContext context)
        {
            var student = context.Students.SingleOrDefault(x => x.Id == studentId);

            if (student == null) {
                return NotFound(new { message = "User not found." });
            }

            return student;
        }

        /// <summary>
        /// Get the refresh tokens of a specific student.
        /// </summary>
        /// <param name="studentId">The ID number of the user.</param>
        /// <param name="context"></param>
        [HttpGet("{studentId}/refresh-tokens"), Authorize(Policy = Constants.AuthPolicy.DataOwner)]
        public ActionResult<RefreshToken> GetRefreshTokens(int studentId, [FromServices] AppDbContext context)
        {
            var student = context.Users.SingleOrDefault(x => x.Id == studentId);
            if (student == null) return NotFound(new { message = "User not found."});
            return Ok(student.RefreshTokens);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string getIp()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For")) {
                return Request.Headers["X-Forwarded-For"];
            } else {
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
    }
}
