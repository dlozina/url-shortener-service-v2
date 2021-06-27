using Microsoft.Extensions.Logging;
using Quartz;
using Shortener.Service.Services.Interface;
using System;
using System.Threading.Tasks;

namespace Shortener.Service.Jobs
{
    [DisallowConcurrentExecution]
    public class SendMonthlyNotification : IJob
    {
        private readonly ISendSms _sendSms;
        private readonly ILogger<HelloWorldJob> _logger;

        public SendMonthlyNotification(ILogger<HelloWorldJob> logger, ISendSms sendSms)
        {
            _logger = logger;
            _sendSms = sendSms;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Job start => Sending Monthly notification.");
            try
            {
                _sendSms.SendMonthlyNotification();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error sending Monthly notification. {ex}");
            }
            _logger.LogInformation("Job end => Sending Monthly notification.");

            return Task.CompletedTask;
        }
    }
}
