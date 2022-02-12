using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities;

public class RefreshToken : EntityBase
{
    /// <summary>
    ///     Get or set the unique ID of this record.
    /// </summary>
    public long Id { get; set; } = default!;

    /// <summary>
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    ///     The date the token was created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    ///     The date the token expires.
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    ///     The date the token was revoked (if any).
    /// </summary>
    public DateTime? Revoked { get; set; }

    /// <summary>
    ///     If the token is expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= Expires;

    /// <summary>
    ///     The IP address that created the token.
    /// </summary>
    public string CreatedByIpAddress { get; set; } = default!;

    /// <summary>
    ///     The IP address that revoked the certificate.
    /// </summary>
    public string? RevokedByIpAddress { get; set; } = default!;

    /// <summary>
    ///     The token that replaced this one.
    /// </summary>
    public string? ReplacedByToken { get; set; } = default!;

    /// <summary>
    /// </summary>
    public bool IsActive => Revoked == null && !IsExpired;
}
