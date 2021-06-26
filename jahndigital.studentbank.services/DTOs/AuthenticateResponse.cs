using System.Text.Json.Serialization;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.services.DTOs
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticateResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id {get; private set;}

        /// <summary>
        /// 
        /// </summary>
        public string Username {get; private set;}

        /// <summary>
        /// 
        /// </summary>
        public string JwtToken {get; private set;}

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public string RefreshToken {get; private set;}

        /// <summary>
        /// 
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
        /// 
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
    }
}
