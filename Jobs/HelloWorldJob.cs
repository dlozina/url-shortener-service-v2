using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace Shortener.Service.Jobs
{
    [DisallowConcurrentExecution]
    public class HelloWorldJob : IJob
    {
        private readonly ILogger<HelloWorldJob> _logger;

        public HelloWorldJob(ILogger<HelloWorldJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Hello Infobip engineer! I am waiting to short some URLs :)");
            return Task.CompletedTask;
        }
    }
}