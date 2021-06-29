using Microsoft.Extensions.Logging;
using Quartz;
using Shortener.Service.Services.Interface;
using System;
using System.Threading.Tasks;

namespace Shortener.Service.Jobs
{
    [DisallowConcurrentExecution]
    public class SendWeeklyNotification : IJob
    {
        private readonly ISendSms _sendSms;
        private readonly ILogger<SendWeeklyNotification> _logger;

        public SendWeeklyNotification(ILogger<SendWeeklyNotification> logger, ISendSms sendSms)
        {
            _logger = logger;
            _sendSms = sendSms;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Job start => Sending Weekly notification.");
            try
            {
                _sendSms.SendWeeklyNotification();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error sending Weekly notification. {ex}");
            }
            _logger.LogInformation("Job end => Sending Weekly notification.");

            return Task.CompletedTask;
        }
    }
}
