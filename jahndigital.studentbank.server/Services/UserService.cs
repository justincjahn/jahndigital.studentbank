using System;
using System.Linq;
using jahndigital.studentbank.server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Facilitates authentication and authorization.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        private readonly AppConfig _config;

        public UserService(AppDbContext context, IOptions<AppConfig> config)
        {
            _context = context;
            _config = config.Value;
        }

        /// <inheritdoc />
        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var user = _context.Users.Include(x => x.Role).SingleOrDefault(x => x.Email == model.Username);
            if (user == null) return null;

            var valid = user.ValidatePassword(model.Password);
            if (valid == PasswordVerificationResult.Failed) return null;

            if (valid == PasswordVerificationResult.SuccessRehashNeeded) {
                user.Password = model.Password;
            }

            var token = JwtTokenService.GenerateToken(
                _config.Secret,
                Constants.UserType.User,
                user.Id,
                user.Email,
                user.Email,
                user.Role.Name
            );

            var refresh = JwtTokenService.GenerateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refresh);
            _context.Update(user);
            _context.SaveChanges();

            return new AuthenticateResponse(user, token, refresh.Token);
        }

        /// <inheritdoc />
        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var user = _context.Users.Include(x => x.Role).SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user == null) return null;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
            if (refreshToken == null) return null;

            var newToken = JwtTokenService.GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            refreshToken.ReplacedByToken = newToken.Token;

            user.RefreshTokens.Add(newToken);
            _context.Update(user);
            _context.SaveChanges();

            var jwtToken = JwtTokenService.GenerateToken(
                _config.Secret,
                Constants.UserType.User,
                user.Id,
                user.Email,
                user.Email,
                user.Role.Name
            );

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
    }
}
