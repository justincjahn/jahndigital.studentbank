using System.Text.Json.Serialization;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a backend user of the application (administrators)
/// </summary>
public class User
{
    /// <summary>
    ///     The unique ID of the user.
    /// </summary>
    /// <value></value>
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
    [JsonIgnore]
    public string Password { get; set; } = default!;

    /// <summary>
    ///     A list of JWT refresh tokens for the user.
    /// </summary>
    [JsonIgnore]
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();

    /// <summary>
    ///     Get or set the date the student last logged in.
    /// </summary>
    public DateTime? DateLastLogin { get; set; } = null;

    /// <summary>
    ///     Get or set the date the user was created.
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Get or set the date the user was registered.
    /// </summary>
    public DateTime? DateRegistered { get; set; } = null;

    /// <summary>
    ///     Get or set the date the user was deleted.
    /// </summary>
    public DateTime? DateDeleted { get; set; } = null;
}