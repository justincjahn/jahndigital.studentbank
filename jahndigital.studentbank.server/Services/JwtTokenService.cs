using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using jahndigital.studentbank.dal.Entities;
using Microsoft.IdentityModel.Tokens;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Methods to generate a JWT token or refresh token.
    /// </summary>
    public static class JwtTokenService
    {
        /// <summary>
        /// Generates a JWT token using the provided information.
        /// </summary>
        /// <param name="jwtSecret">The secret used to sign the JWT token.</param>
        /// <param name="type">The type of user the token is issued to.</param>
        /// <param name="id">User or Student ID</param>
        /// <param name="username">Student account number or user email address.</param>
        /// <param name="role">The user's role</param>
        /// <param name="email">Primary email address</param>
        /// <param name="firstName">User's given name</param>
        /// <param name="lastName">User's surname</param>
        /// <param name="expires">Number of minutes before the token expires.</param>
        /// <returns>A valid JWT token signed with the provided key.</returns>
        public static string GenerateToken(
            string jwtSecret,
            Constants.UserType type,
            long id,
            string username,
            string role,
            string email="",
            string firstName="",
            string lastName="",
            int? expires=null
        ) {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName),
                new Claim(Constants.Auth.CLAIM_USER_TYPE, type.Name)
            };

            var descriptor = new SecurityTokenDescriptor {
                Issuer = Constants.Auth.Issuer,
                Expires = DateTime.UtcNow.AddMinutes(expires ?? Constants.Auth.DefaultExpirationMinutes),
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            return handler.CreateEncodedJwt(descriptor);
        }

        /// <summary>
        /// Generate a refresh token from random bytes.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>A <see cname="RefreshToken" /> instance with refresh token info.</returns>
        public static RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using var rng = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);
            return new RefreshToken {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIpAddress = ipAddress
            };
        }
    }
}
