using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Application.Groups.DTOs;

public class GroupStatisticsResponse
{
    public long GroupId { get; init; }
    public Money AverageShares { get; init; } = Money.Zero;
    public Money TotalShares { get; set; } = Domain.ValueObjects.Money.Zero;
    public double AverageSharesOwned { get; set; } = 0.0f;
    public long TotalSharesOwned { get; set; } = 0;
    public Money AverageStocks { get; set; } = Money.Zero;
    public Money TotalStocks { get; set; } = Domain.ValueObjects.Money.Zero;
}
