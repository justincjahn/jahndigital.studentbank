using System;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
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
        /// <summary>
        /// The database context to use when querying and updating the data store.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Application configuration containing the token secret.
        /// </summary>
        private readonly AppConfig _config;

        public UserService(AppDbContext context, IOptions<AppConfig> config)
        {
            _context = context;
            _config = config.Value;
        }

        /// <inheritdoc />
        public async Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest model, string ipAddress)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .SingleOrDefaultAsync(
                    x => x.Email == model.Username
                    && x.DateDeleted == null
                    && x.DateRegistered != null
                );

            if (user == null) return null;

            var valid = user.ValidatePassword(model.Password);
            if (valid == PasswordVerificationResult.Failed) return null;

            if (valid == PasswordVerificationResult.SuccessRehashNeeded) {
                user.Password = model.Password;
            }

            user.DateLastLogin = DateTime.UtcNow;

            var token = JwtTokenService.GenerateToken(
                _config.Secret,
                Constants.UserType.User,
                user.Id,
                user.Email,
                user.Role.Name,
                email: user.Email,
                expires: _config.TokenLifetime
            );

            var refresh = JwtTokenService.GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(refresh);

            await _context.SaveChangesAsync();

            return new AuthenticateResponse(user, token, refresh.Token);
        }

        /// <inheritdoc />
        public async Task<AuthenticateResponse?> RefreshTokenAsync(string token, string ipAddress)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .SingleOrDefaultAsync(u =>
                    u.RefreshTokens.Any(t => t.Token == token)
                    && u.DateDeleted == null);

            if (user == null) return null;
            user.DateLastLogin = DateTime.UtcNow;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
            if (refreshToken == null) return null;

            var newToken = JwtTokenService.GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            refreshToken.ReplacedByToken = newToken.Token;
            user.RefreshTokens.Add(newToken);

            var jwtToken = JwtTokenService.GenerateToken(
                _config.Secret,
                Constants.UserType.User,
                user.Id,
                user.Email,
                user.Role.Name,
                email: user.Email,
                expires: _config.TokenLifetime
            );

            await _context.SaveChangesAsync();

            return new AuthenticateResponse(user, jwtToken, newToken.Token);
        }

        /// <inheritdoc />
        public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            var user = await _context.Users.SingleOrDefaultAsync(
                u => u.RefreshTokens.Any(t => t.Token == token)
            );

            if (user == null) return false;

            var refreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == token);
            if (refreshToken == null) return false;
            if (!refreshToken.IsActive) return false;

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
