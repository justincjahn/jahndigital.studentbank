using System.Text.Json.Serialization;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.services.DTOs
{
    /// <summary>
    /// </summary>
    public class AuthenticateResponse
    {
        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="jwtToken"></param>
        /// <param name="refreshToken"></param>
        public AuthenticateResponse(User user, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            Username = user.Email;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }

        /// <summary>
        /// </summary>
        /// <param name="student"></param>
        /// <param name="jwtToken"></param>
        /// <param name="refreshToken"></param>
        public AuthenticateResponse(Student student, string jwtToken, string refreshToken)
        {
            Id = student.Id;
            Username = student.AccountNumber;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }

        /// <summary>
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// </summary>
        public string JwtToken { get; }

        /// <summary>
        /// </summary>
        [JsonIgnore]
        public string RefreshToken { get; }
    }
}
