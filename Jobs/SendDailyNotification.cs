using Microsoft.Extensions.Logging;
using Quartz;
using Shortener.Service.Services.Interface;
using System;
using System.Threading.Tasks;

namespace Shortener.Service.Jobs
{
    [DisallowConcurrentExecution]
    public class SendDailyNotification : IJob
    {
        private readonly ISendSms _sendSms;
        private readonly ILogger<HelloWorldJob> _logger;

        public SendDailyNotification(ILogger<HelloWorldJob> logger, ISendSms sendSms)
        {
            _logger = logger;
            _sendSms = sendSms;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Job start => Sending Daily notification.");
            try
            {
                _sendSms.SendDailyNotification();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error sending Daily notification. {ex}");
            }
            _logger.LogInformation("Job end => Sending Daily notification.");

            return Task.CompletedTask;
        }
    }
}