using JahnDigital.StudentBank.Domain.Enums;

namespace JahnDigital.StudentBank.Application.Common.DTOs;

public record JwtTokenRequest
{
    /// <summary>
    /// The type of user the token is issued to.
    /// </summary>
    public UserType Type { get; init; } = default!;

    /// <summary>
    /// User or Student ID
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Student account number or user email address.
    /// </summary>
    public string Username { get; init; } = default!;

    /// <summary>
    /// The user's role
    /// </summary>
    public string Role { get; init; } = default!;
    
    /// <summary>
    /// Primary email address
    /// </summary>
    public string? Email { get; init; }
    
    /// <summary>
    /// User's given name
    /// </summary>
    public string? FirstName { get; init; }
    
    /// <summary>
    /// User's surname
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Number of minutes before the token expires.
    /// </summary>
    public int? Expires { get; init; }

    /// <summary>
    /// If the preauthorization claim should be set on the token.
    /// </summary>
    public bool Preauthorization { get; init; }
}
