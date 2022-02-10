namespace JahnDigital.StudentBank.Application.Transactions.DTOs;

/// <summary>
///     Represents a request to post dividends for a specific share type.
/// </summary>
public class PostDividendsRequest
{
    /// <summary>
    ///     The <see cref="dal.Entities.ShareType" /> ID to post dividends to.
    /// </summary>
    public long ShareTypeId { get; set; } = default!;

    /// <summary>
    ///     A list of <see cref="dal.Entities.Instance" /> IDs to post dividends to.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<long> Instances { get; set; } = new HashSet<long>();
}

