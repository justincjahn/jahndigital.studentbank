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
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;
        private readonly string _secret;
        private readonly int _tokenLifetime;

        public StudentService(AppDbContext context, string secret, int tokenLifetime)
        {
            _context = context;
            _secret = secret;
            _tokenLifetime = tokenLifetime;
        }

        /// <inheritdoc />
        public async Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest model, string ipAddress)
        {
            // Only select students in the active instance
            var activeInstances = await _context.Instances.Where(x => x.IsActive).ToListAsync();

            var student = await _context.Students
                .Include(x => x.Group)
                .Where(x => activeInstances.Select(x => x.Id).Contains(x.Group.InstanceId))
                .Where(x =>
                    (x.AccountNumber == model.Username.PadLeft(10, '0') || x.Email == model.Username.ToLower())
                    && x.DateDeleted == null
                    && x.DateRegistered != null
                )
                .SingleOrDefaultAsync();

            if (student == null) {
                return null;
            }

            var valid = student.ValidatePassword(model.Password);

            if (valid == PasswordVerificationResult.Failed) {
                return null;
            }

            student.DateLastLogin = DateTime.UtcNow;

            if (valid == PasswordVerificationResult.SuccessRehashNeeded) {
                student.Password = model.Password;
            }

            var token = JwtTokenService.GenerateToken(
                _secret,
                Constants.UserType.Student,
                student.Id,
                student.AccountNumber,
                Constants.Role.Student.Name,
                student.Email ?? "",
                student.FirstName,
                student.LastName,
                _tokenLifetime
            );

            var refresh = JwtTokenService.GenerateRefreshToken(ipAddress);
            student.RefreshTokens.Add(refresh);

            await _context.SaveChangesAsync();

            return new AuthenticateResponse(student, token, refresh.Token);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">If the Invite Code or account number is invalid.</exception>
        public async Task<string> AuthenticateInviteAsync(string inviteCode, string accountNumber)
        {
            /*
                SELECT * FROM INSTANCES
                WHERE
                    INSTANCES.INVITECODE = INVITECODE
                    AND INSTANCES.DATEDELETED = NULL
            */
            var instance = await _context.Instances
                    .Where(x => x.IsActive && x.InviteCode.ToUpper() == inviteCode.ToUpper())
                    .SingleOrDefaultAsync()
                ?? throw new ArgumentException(
                    "No instances available with the provided invite code.",
                    nameof(inviteCode)
                );

            /*
                SELECT * FROM STUDENTS
                    LEFT JOIN GROUP ON STUDENT.GROUPID = GROUP.ID
                WHERE
                    GROUP.INSTANCEID == 0
                    AND STUDENTS.ACCOUNTNUMBER = 0000000000;
                    AND STUDENTS.DATEREGISTERED = NULL
                    AND STUDENTS.DATEDELETED = NULL

            */
            var student = await _context.Students
                    .Include(x => x.Group)
                    .Where(x => x.Group.InstanceId == instance.Id)
                    .Where(x =>
                        x.DateDeleted == null
                        && x.DateRegistered == null
                        && x.AccountNumber == accountNumber.PadLeft(10, '0')
                    )
                    .FirstOrDefaultAsync()
                ?? throw new ArgumentException(
                    "No students found with the provided invite code and account number.",
                    nameof(accountNumber)
                );

            return JwtTokenService.GenerateToken(
                _secret,
                Constants.UserType.Student,
                student.Id,
                student.AccountNumber,
                Constants.Role.Student.Name,
                student.Email ?? "",
                student.FirstName,
                student.LastName,
                5,
                true
            );
        }

        /// <inheritdoc />
        public async Task<AuthenticateResponse?> RefreshTokenAsync(string token, string ipAddress)
        {
            var student = await _context.Students.SingleOrDefaultAsync(x =>
                x.DateDeleted == null
                && x.DateRegistered != null
                && x.RefreshTokens.Any(t => t.Token == token)
            );

            if (student == null) {
                return null;
            }

            var refreshToken = student.RefreshTokens.SingleOrDefault(x => x.Token == token);

            if (refreshToken == null) {
                return null;
            }

            var newToken = JwtTokenService.GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            refreshToken.ReplacedByToken = newToken.Token;

            student.RefreshTokens.Add(newToken);
            await _context.SaveChangesAsync();

            var jwtToken = JwtTokenService.GenerateToken(
                _secret,
                Constants.UserType.Student,
                student.Id,
                student.AccountNumber,
                Constants.Role.Student.Name,
                student.Email ?? "",
                student.FirstName,
                student.LastName,
                _tokenLifetime
            );

            return new AuthenticateResponse(student, jwtToken, newToken.Token);
        }

        /// <inheritdoc />
        public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            var student = await _context.Students.SingleOrDefaultAsync(
                u => u.RefreshTokens.Any(t => t.Token == token)
            );

            if (student == null) {
                return false;
            }

            var refreshToken = student.RefreshTokens.SingleOrDefault(x => x.Token == token);

            if (refreshToken == null) {
                return false;
            }

            if (!refreshToken.IsActive) {
                return false;
            }

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIpAddress = ipAddress;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}