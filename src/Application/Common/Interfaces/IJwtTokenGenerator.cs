using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Domain.Entities;

namespace JahnDigital.StudentBank.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generate a JWT token string.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>A valid JWT token signed with the provided key.</returns>
    string Generate(JwtTokenRequest request);

    /// <summary>
    /// Generates a random refresh token.
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    RefreshToken GenerateRefreshToken(string ipAddress);
}
