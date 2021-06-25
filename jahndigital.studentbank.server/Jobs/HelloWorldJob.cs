using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace jahndigital.studentbank.server.Jobs
{
    public class HelloWorldJob : IJob
    {
        private readonly ILogger<HelloWorldJob> _logger;

        public HelloWorldJob(ILogger<HelloWorldJob> logger) => _logger = logger;

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Hello, World!");
            return Task.CompletedTask;
        }
    }
}
