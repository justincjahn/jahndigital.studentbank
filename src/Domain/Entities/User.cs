using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a backend user of the application (administrators)
/// </summary>
public class User : SoftDeletableEntity
{
    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Foreign key for user's role.
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    ///     Backing field for Email.
    /// </summary>
    private string _email = default!;

    /// <summary>
    ///     The email address of the user.
    /// </summary>
    /// <value></value>
    public string Email
    {
        get => _email;
        set => _email = value.ToLower();
    }

    /// <summary>
    ///     The user's role.
    /// </summary>
    public Role Role { get; set; } = default!;

    /// <summary>
    ///     The encrypted credentials of the user.
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    ///     A list of JWT refresh tokens for the user.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();

    /// <summary>
    ///     Get or set the date the student last logged in.
    /// </summary>
    public DateTime? DateLastLogin { get; set; }

    /// <summary>
    ///     Get or set the date the user was registered.
    /// </summary>
    public DateTime? DateRegistered { get; set; }
}
