using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace JahnDigital.StudentBank.Infrastructure.Authentication.Services;

/// <summary>
///     Methods to generate a JWT token or refresh token.
/// </summary>
public class JwtTokenService : IJwtTokenGenerator
{
    
    public string Generate(JwtTokenRequest request)
    {
        JwtSecurityTokenHandler handler = new();
        byte[] key = Encoding.ASCII.GetBytes(request.JwtSecret);

        List<Claim>? claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, request.Id.ToString()),
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, request.Role),
            new Claim(ClaimTypes.Email, request.Email ?? ""),
            new Claim(ClaimTypes.GivenName, request.FirstName ?? ""),
            new Claim(ClaimTypes.Surname, request.LastName ?? ""),
            new Claim(Constants.Auth.CLAIM_USER_TYPE, request.Type.Name)
        };

        if (request.Preauthorization)
        {
            claims.Add(
                new Claim(Constants.Auth.CLAIM_PREAUTH_TYPE, Constants.Auth.CLAIM_PREAUTH_YES)
            );
        }

        SecurityTokenDescriptor? descriptor = new SecurityTokenDescriptor
        {
            Issuer = Constants.Auth.Issuer,
            Expires = DateTime.UtcNow.AddMinutes(request.Expires ?? Constants.Auth.DefaultExpirationMinutes),
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        return handler.CreateEncodedJwt(descriptor);
    }

    RefreshToken IJwtTokenGenerator.GenerateRefreshToken(string ipAddress)
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIpAddress = ipAddress
        };
    }
}
