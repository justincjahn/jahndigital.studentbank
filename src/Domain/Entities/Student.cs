using System.Text.RegularExpressions;
using JahnDigital.StudentBank.Domain.Common;

namespace JahnDigital.StudentBank.Domain.Entities;

/// <summary>
///     Represents a student
/// </summary>
public class Student : SoftDeletableEntity
{
    /// <summary>
    ///     Backing field for <see cref="AccountNumber" />.
    /// </summary>
    private string _accountNumber = null!;

    /// <summary>
    ///     The unique ID number of the entity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Student's account number. Left-zero-fill to 10 characters.
    /// </summary>
    public string AccountNumber
    {
        get => _accountNumber;

        set
        {
            if (!Regex.IsMatch(value, "^[0-9]{1,10}$"))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Can be a maximum of 10 digits."
                );
            }

            _accountNumber = value.PadLeft(10, '0');
        }
    }

    /// <summary>
    ///     Backing field for Email.
    /// </summary>
    private string? _email;

    /// <summary>
    ///     The email address of the user.
    /// </summary>
    /// <value></value>
    public string? Email
    {
        get => _email;
        set => _email = value?.ToLower() ?? null;
    }

    /// <summary>
    ///     The student's given name.
    /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    ///     The student's surname.
    /// </summary>
    public string LastName { get; set; } = default!;

    /// <summary>
    ///     The encrypted credentials of the user.
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// </summary>
    public long GroupId { get; set; }

    /// <summary>
    ///     Student's group (class/period/etc.).
    /// </summary>
    public Group Group { get; set; } = null!;

    /// <summary>
    ///     Student's shares.
    /// </summary>
    public ICollection<Share> Shares { get; set; } = new HashSet<Share>();

    /// <summary>
    ///     A list of JWT refresh tokens for the user.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();

    /// <summary>
    ///     Get a list of purchases this student has made.
    /// </summary>
    public ICollection<StudentPurchase> Purchases { get; set; } = new HashSet<StudentPurchase>();

    /// <summary>
    ///     Get a list of stocks this student has purchased.
    /// </summary>
    public ICollection<StudentStock> Stocks { get; set; } = new HashSet<StudentStock>();

    /// <summary>
    ///     Get or set the date the student last logged in.
    /// </summary>
    public DateTime? DateLastLogin { get; set; }

    /// <summary>
    ///     Get or set the date the student was registered.
    /// </summary>
    public DateTime? DateRegistered { get; set; }
}
