using System.Runtime.InteropServices;

namespace JahnDigital.StudentBank.Application.Common.Interfaces;

public interface IPasswordHasher
{
    /**
     * Generate a hashed password from input.
     */
    public string HashPassword(String password);
    
    /**
     * Validate that the provided <paramref name="hashedPassword"/> matches the provided <paramref name="password"/>.
     */
    public bool Validate(String hashedPassword, String password);
}
