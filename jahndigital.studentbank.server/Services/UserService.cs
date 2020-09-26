using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using jahndigital.studentbank.server.Entities;
using jahndigital.studentbank.server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// 
        /// </summary>
        private AppDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        private readonly AppConfig _config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        public UserService(AppDbContext context, IOptions<AppConfig> config)
        {
            _context = context;
            _config = config.Value;
        }

        /// <inheritdoc />
        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var user = _context.Users.SingleOrDefault(x => x.Email == model.Username);
            if (user == null) return null;

            var valid = user.ValidatePassword(model.Password);
            if (valid == PasswordVerificationResult.Failed) return null;

            if (valid == PasswordVerificationResult.SuccessRehashNeeded) {
                user.Password = model.Password;
            }

            var jwtToken = generateJwtToken(user);
            var refreshToken = generateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refreshToken);
            _context.Update(user);
            _context.SaveChanges();

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        /// <inheritdoc />
        public IEnumerable<Student> GetAllStudents()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<User> GetAllUsers()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user == null) return null;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
            if (refreshToken == null) return null;

            var newToken = generateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            refreshToken.ReplacedByToken = newToken.Token;

            user.RefreshTokens.Add(newToken);
            _context.Update(user);
            _context.SaveChanges();

            var jwtToken = generateJwtToken(user);
            return new AuthenticateResponse(user, jwtToken, newToken.Token);
        }

        /// <inheritdoc />
        public bool RevokeToken(string token, string ipAddress)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == token);
            if (refreshToken == null) return false;
            if (!refreshToken.IsActive) return false;

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            _context.Update(user);
            _context.SaveChanges();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),

                Expires = DateTime.UtcNow.AddMinutes(15),

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate a refresh token from random bytes.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private RefreshToken generateRefreshToken(string ipAddress)
        {
            using (var rng = new RNGCryptoServiceProvider()) {
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
}
