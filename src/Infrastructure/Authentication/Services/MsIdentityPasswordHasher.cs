using Microsoft.AspNetCore.Identity;
using IPasswordHasher = JahnDigital.StudentBank.Application.Common.Interfaces.IPasswordHasher;

namespace JahnDigital.StudentBank.Infrastructure.Authentication.Services;

public class MsIdentityPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        var passwordHasher = new PasswordHasher<object>();
        return passwordHasher.HashPassword(this, password);
    }

    public Task<bool> ValidateAsync(string hashedPassword, string password)
    {
        var passwordHasher = new PasswordHasher<object>();
        var result = passwordHasher.VerifyHashedPassword(this, hashedPassword, password);
        return Task.FromResult(result != PasswordVerificationResult.Failed);
    }
}
