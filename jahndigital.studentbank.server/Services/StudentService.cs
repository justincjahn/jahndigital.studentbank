using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Facilitates authentication and authorization.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;

        private readonly AppConfig _config;

        public StudentService(AppDbContext context, IOptions<AppConfig> config)
        {
            _context = context;
            _config = config.Value;
        }

        /// <inheritdoc />
        public AuthenticateResponse? Authenticate(AuthenticateRequest model, string ipAddress)
        {
            // Only select students in the active instance
            var activeInstances = _context.Instances.Where(x => x.IsActive).Select(x => x.Id);

            // Only select students in groups that aren't deleted
            var groups = _context.Groups
                .Where(x => activeInstances.Contains(x.InstanceId) && x.DateDeleted == null)
                .Select(x => x.Id);

            var student = _context.Students.SingleOrDefault(x =>
                (x.AccountNumber == model.Username || x.Email == model.Username)
                && x.DateDeleted == null
                && groups.Contains(x.GroupId)
            );
        
            if (student == null) return null;

            var valid = student.ValidatePassword(model.Password);
            if (valid == PasswordVerificationResult.Failed) return null;

            if (valid == PasswordVerificationResult.SuccessRehashNeeded) {
                student.Password = model.Password;
            }

            var token = JwtTokenService.GenerateToken(
                _config.Secret,
                Constants.UserType.Student,
                student.Id,
                student.AccountNumber,
                student.Email,
                Constants.Role.Student.Name,
                firstName: student.FirstName,
                lastName: student.LastName
            );

            var refresh = JwtTokenService.GenerateRefreshToken(ipAddress);

            student.RefreshTokens.Add(refresh);
            _context.Update(student);
            _context.SaveChanges();

            return new AuthenticateResponse(student, token, refresh.Token);
        }

        /// <inheritdoc />
        public AuthenticateResponse? RefreshToken(string token, string ipAddress)
        {
            var student = _context.Students.SingleOrDefault(u =>
                u.RefreshTokens.Any(t => t.Token == token)
                && u.DateDeleted == null
            );

            if (student == null) return null;

            var refreshToken = student.RefreshTokens.Single(x => x.Token == token);
            if (refreshToken == null) return null;

            var newToken = JwtTokenService.GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            refreshToken.ReplacedByToken = newToken.Token;

            student.RefreshTokens.Add(newToken);
            _context.Update(student);
            _context.SaveChanges();

            var jwtToken = JwtTokenService.GenerateToken(
                _config.Secret,
                Constants.UserType.User,
                student.Id,
                student.AccountNumber,
                student.Email,
                Constants.Role.Student.Name,
                firstName: student.FirstName,
                lastName: student.LastName
            );

            return new AuthenticateResponse(student, jwtToken, newToken.Token);
        }

        /// <inheritdoc />
        public bool RevokeToken(string token, string ipAddress)
        {
            var student = _context.Students.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
            if (student == null) return false;

            var refreshToken = student.RefreshTokens.SingleOrDefault(x => x.Token == token);
            if (refreshToken == null) return false;
            if (!refreshToken.IsActive) return false;

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            _context.Update(student);
            _context.SaveChanges();

            return true;
        }
    }
}
