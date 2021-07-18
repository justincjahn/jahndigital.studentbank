using System;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.services.DTOs;
using jahndigital.studentbank.services.Interfaces;
using jahndigital.studentbank.utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.services
{
    /// <summary>
    ///     Facilitates authentication and authorization.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        private readonly string _secret;

        private readonly int _tokenLifetime;

        public UserService(IDbContextFactory<AppDbContext> factory, string secret, int tokenLifetime)
        {
            _factory = factory;
            _secret = secret;
            _tokenLifetime = tokenLifetime;
        }

        /// <inheritdoc />
        public async Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest model, string ipAddress)
        {
            await using var context = _factory.CreateDbContext();

            var user = await context.Users
                .Include(x => x.Role)
                .SingleOrDefaultAsync(
                    x => x.Email == model.Username.ToLower()
                        && x.DateDeleted == null
                        && x.DateRegistered != null
                );

            if (user == null) {
                return null;
            }

            var valid = user.ValidatePassword(model.Password);

            if (valid == PasswordVerificationResult.Failed) {
                return null;
            }

            if (valid == PasswordVerificationResult.SuccessRehashNeeded) {
                user.Password = model.Password;
            }

            user.DateLastLogin = DateTime.UtcNow;

            var token = JwtTokenService.GenerateToken(
                _secret,
                Constants.UserType.User,
                user.Id,
                user.Email,
                user.Role.Name,
                user.Email,
                expires: _tokenLifetime
            );

            var refresh = JwtTokenService.GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(refresh);

            await context.SaveChangesAsync();

            return new AuthenticateResponse(user, token, refresh.Token);
        }

        /// <inheritdoc />
        public async Task<AuthenticateResponse?> RefreshTokenAsync(string token, string ipAddress)
        {
            await using var context = _factory.CreateDbContext();

            var user = await context.Users
                .Include(x => x.Role)
                .SingleOrDefaultAsync(u =>
                    u.RefreshTokens.Any(t => t.Token == token)
                    && u.DateDeleted == null);

            if (user == null) {
                return null;
            }

            user.DateLastLogin = DateTime.UtcNow;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (refreshToken == null) {
                return null;
            }

            var newToken = JwtTokenService.GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            refreshToken.ReplacedByToken = newToken.Token;
            user.RefreshTokens.Add(newToken);

            var jwtToken = JwtTokenService.GenerateToken(
                _secret,
                Constants.UserType.User,
                user.Id,
                user.Email,
                user.Role.Name,
                user.Email,
                expires: _tokenLifetime
            );

            await context.SaveChangesAsync();

            return new AuthenticateResponse(user, jwtToken, newToken.Token);
        }

        /// <inheritdoc />
        public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            await using var context = _factory.CreateDbContext();

            var user = await context.Users.SingleOrDefaultAsync(
                u => u.RefreshTokens.Any(t => t.Token == token)
            );

            if (user == null) {
                return false;
            }

            var refreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == token);

            if (refreshToken == null) {
                return false;
            }

            if (!refreshToken.IsActive) {
                return false;
            }

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            await context.SaveChangesAsync();

            return true;
        }
    }
}
