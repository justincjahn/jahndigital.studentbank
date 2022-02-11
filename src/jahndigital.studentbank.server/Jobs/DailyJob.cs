using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JahnDigital.StudentBank.Application.Common.Utils;
using JahnDigital.StudentBank.Application.ShareTypes.Commands.ResetWithdrawalLimit;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace jahndigital.studentbank.server.Jobs
{
    /// <summary>
    /// Runs daily tasks, such as withdrawal limit resets.
    /// </summary>
    public class DailyJob : IJob, IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly ISender _mediatr;
        private readonly ILogger<DailyJob> _logger;

        public DailyJob(IDbContextFactory<AppDbContext> factory, ISender mediatr,
            ILogger<DailyJob> logger)
        {
            _dbContext = factory.CreateDbContext();
            _mediatr = mediatr;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // Get a list of Share Types and their last reset times
            List<ShareType> shareTypes = await _dbContext.ShareTypes.ToListAsync();

            await RunDaily(shareTypes);
            await RunWeekly(shareTypes);
            await RunMonthly(shareTypes);
            await RunQuarterly(shareTypes);
            await RunYearly(shareTypes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shareTypes"></param>
        private async Task RunDaily(List<ShareType> shareTypes)
        {
            IEnumerable<ShareType>? filtered =
                shareTypes.Where(x => x.WithdrawalLimitPeriod == Period.Daily && x.WithdrawalLimitCount > 0);

            foreach (ShareType? shareType in filtered)
            {
                _logger.LogInformation(
                    $"Running daily withdrawal reset for Share Type: {shareType.Id}:{shareType.Name}");

                var command = new ResetWithdrawalLimitCommand(shareType.Id);
                await _mediatr.Send(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shareTypes"></param>
        private async Task RunWeekly(IEnumerable<ShareType> shareTypes)
        {
            DateTime nextRun = PeriodCalculationService.NextWeekly();
            DateTime previousRun = PeriodCalculationService.PreviousWeekly();

            IEnumerable<ShareType> filtered = nextRun == DateTime.Today
                ? shareTypes.Where(x => x.WithdrawalLimitPeriod == Period.Weekly && x.WithdrawalLimitCount > 0)
                : shareTypes.Where(x => x.WithdrawalLimitLastReset <= previousRun && x.WithdrawalLimitCount > 0);

            foreach (ShareType? shareType in filtered)
            {
                _logger.LogInformation(
                    $"Running weekly withdrawal reset for Share Type: {shareType.Id}:{shareType.Name}");

                var command = new ResetWithdrawalLimitCommand(shareType.Id);
                await _mediatr.Send(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shareTypes"></param>
        private async Task RunMonthly(IEnumerable<ShareType> shareTypes)
        {
            DateTime nextRun = PeriodCalculationService.NextMonthly();
            DateTime previousRun = PeriodCalculationService.PreviousMonthly();

            IEnumerable<ShareType> filtered = nextRun == DateTime.Today
                ? shareTypes.Where(x => x.WithdrawalLimitPeriod == Period.Monthly && x.WithdrawalLimitCount > 0)
                : shareTypes.Where(x => x.WithdrawalLimitLastReset <= previousRun && x.WithdrawalLimitCount > 0);

            foreach (ShareType? shareType in filtered)
            {
                _logger.LogInformation(
                    $"Running monthly withdrawal reset for Share Type: {shareType.Id}:{shareType.Name}");

                var command = new ResetWithdrawalLimitCommand(shareType.Id);
                await _mediatr.Send(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shareTypes"></param>
        private async Task RunQuarterly(IEnumerable<ShareType> shareTypes)
        {
            DateTime nextRun = PeriodCalculationService.NextQuarterly();
            DateTime previousRun = PeriodCalculationService.PreviousQuarterly();

            IEnumerable<ShareType> filtered = nextRun == DateTime.Today
                ? shareTypes.Where(x => x.WithdrawalLimitPeriod == Period.Quarterly && x.WithdrawalLimitCount > 0)
                : shareTypes.Where(x => x.WithdrawalLimitLastReset <= previousRun && x.WithdrawalLimitCount > 0);

            foreach (ShareType? shareType in filtered)
            {
                _logger.LogInformation(
                    $"Running quarterly withdrawal reset for Share Type: {shareType.Id}:{shareType.Name}");

                var command = new ResetWithdrawalLimitCommand(shareType.Id);
                await _mediatr.Send(command);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shareTypes"></param>
        private async Task RunYearly(IEnumerable<ShareType> shareTypes)
        {
            DateTime nextRun = PeriodCalculationService.NextYearly();
            DateTime previousRun = PeriodCalculationService.PreviousYearly();

            IEnumerable<ShareType> filtered = nextRun == DateTime.Today
                ? shareTypes.Where(x => x.WithdrawalLimitPeriod == Period.Annually && x.WithdrawalLimitCount > 0)
                : shareTypes.Where(x => x.WithdrawalLimitLastReset <= previousRun && x.WithdrawalLimitCount > 0);

            foreach (ShareType? shareType in filtered)
            {
                _logger.LogInformation(
                    $"Running annual withdrawal reset for Share Type: {shareType.Id}:{shareType.Name}");

                var command = new ResetWithdrawalLimitCommand(shareType.Id);
                await _mediatr.Send(command);
            }
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
