using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Application.Groups.DTOs;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Groups.Queries.GetGroupStatistics;

public record GetGroupStatisticsQuery(long[] GroupId) : IRequest<IEnumerable<GroupStatisticsResponse>>;

public class GetGroupStatisticsQueryHandler
    : IRequestHandler<GetGroupStatisticsQuery, IEnumerable<GroupStatisticsResponse>>
{
    private readonly IAppDbContext _context;

    public GetGroupStatisticsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    async public Task<IEnumerable<GroupStatisticsResponse>> Handle(
        GetGroupStatisticsQuery request,
        CancellationToken cancellationToken
    )
    {
        var shares = await _context.Shares
            .Include(x => x.Student)
            .Where(x => request.GroupId.Contains(x.Student.GroupId))
            .GroupBy(x => x.Student.GroupId)
            .Select(share => new
            {
                Id = share.Key,
                AverageBalance = share.Average(x => x.RawBalance),
                TotalBalance = share.Sum(x => x.RawBalance)
            })
            .ToListAsync(cancellationToken);

        var stocks = await _context.StudentStocks
            .Include(x => x.Student)
            .Include(x => x.Stock)
            .Where(x => request.GroupId.Contains(x.Student.GroupId))
            .Select(stock => new
            {
                GroupId = stock.Student.GroupId,
                SharesOwned = stock.SharesOwned,
                TotalValue = stock.Stock.RawCurrentValue * stock.SharesOwned
            })
            .GroupBy(x => x.GroupId)
            .Select(stock => new
            {
                Id = stock.Key,
                TotalSharesOwned = stock.Sum(x => x.SharesOwned),
                AverageSharesOwned = stock.Average(x => x.SharesOwned),
                TotalValue = stock.Sum(x => x.TotalValue),
                AverageValue = stock.Average(x => x.TotalValue)
            })
            .ToListAsync(cancellationToken);

        var ret = new Dictionary<long, GroupStatisticsResponse>();

        foreach (var share in shares)
        {
            ret[share.Id] = new GroupStatisticsResponse()
            {
                GroupId = share.Id,
                AverageShares = Money.FromDatabase((long)share.AverageBalance),
                TotalShares = Money.FromDatabase(share.TotalBalance)
            };
        }

        foreach (var stock in stocks)
        {
            ret[stock.Id].AverageSharesOwned = stock.AverageSharesOwned;
            ret[stock.Id].TotalSharesOwned = stock.TotalSharesOwned;
            ret[stock.Id].AverageStocks = Money.FromDatabase((long)stock.AverageValue);
            ret[stock.Id].TotalStocks = Money.FromDatabase(stock.TotalValue);
        }

        return ret.Values;
    }
}
