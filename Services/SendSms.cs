using Infobip.Api.Client;
using Infobip.Api.Client.Api;
using Infobip.Api.Client.Model;
using LiteDB;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shortener.Service.Model;
using Shortener.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shortener.Service.Services
{
    public class SendSms : ISendSms
    {
        private readonly ILiteDatabase _context;
        private readonly IConfiguration _config;
        // FileStream Log
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        // Console log
        private readonly ILogger<SendSms> _logger;

        public SendSms(ILiteDatabase context, IConfiguration config, ILogger<SendSms> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public void NotifyTargetUrlIsShortened()
        {
            var smsNotificationMessage = "Targeted url has been shortened. (infobip.com)";
            _logger.LogInformation("Target Url is shortened => Sms send start");
            SendNotificationSms(smsNotificationMessage);
        }

        public void SendDailyNotification()
        {
            DateTime localDate = DateTime.Now.Date;
            var db = _context.GetCollection<UrlData>();
            var results = db.Query()
                .Where(x => x.ShorteningDateTime.Equals(localDate)).ToList();
            var dailyShorteningResults = CountShortenings(results);

            var smsNotificationMessage = $"Report for {localDate}. Number of URL shortenings: {dailyShorteningResults}.";
            _logger.LogInformation($"Report for {localDate}. => Sms send start");
            SendNotificationSms(smsNotificationMessage);
        }

        public void SendWeeklyNotification()
        {
            DateTime localDate = DateTime.Now.Date;
            DateTime weekbefore = localDate.AddDays(-7);

            var db = _context.GetCollection<UrlData>();
            var results = db.Query()
                .Where(x => x.ShorteningDateTime >= weekbefore && x.ShorteningDateTime <= localDate).ToList();
            var weeklyShorteningResults = CountShortenings(results);

            var smsNotificationMessage = $"Report for previous week. Number of URL shortenings: {weeklyShorteningResults}.";
            _logger.LogInformation($"Report for previous week. => Sms send start");
            SendNotificationSms(smsNotificationMessage);
        }

        public void SendMonthlyNotification()
        {
            DateTime localDate = DateTime.Now.Date;
            DateTime monthBefore = localDate.AddMonths(-1);
            var startDate = new DateTime(monthBefore.Year, monthBefore.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var db = _context.GetCollection<UrlData>();
            var results = db.Query()
                .Where(x => x.ShorteningDateTime >= startDate && x.ShorteningDateTime <= endDate).ToList();
            var monthlyShorteningResults = CountShortenings(results);

            var smsNotificationMessage = $"Report for previous month. Number of URL shortenings: {monthlyShorteningResults}.";
            _logger.LogInformation($"Report for previous month. => Sms send start");
            SendNotificationSms(smsNotificationMessage);
        }

        private void SendNotificationSms(string smsNotificationMessage)
        {
            log.Info("SMS send - Start");
            var configuration = new Configuration()
            {
                BasePath = _config["InfobipApi:BasePath"],
                ApiKeyPrefix = _config["InfobipApi:ApiKeyPrefix"],
                ApiKey = _config["InfobipApi:ApiKey"]
            };
            var sendSmsApi = new SendSmsApi(configuration);

            var smsMessage = new SmsTextualMessage()
            {
                From = _config["SmsSettings:From"],
                Destinations = new List<SmsDestination>()
                {
                    new SmsDestination(to: _config["SmsSettings:NotificationSmsNumber"])
                },
                Text = smsNotificationMessage
            };

            var smsRequest = new SmsAdvancedTextualRequest()
            {
                Messages = new List<SmsTextualMessage>() { smsMessage }
            };

            try
            {
                var smsResponse = sendSmsApi.SendSmsMessage(smsRequest);

                System.Diagnostics.Debug.WriteLine($"Status: {smsResponse.Messages.First().Status}");
            }
            catch (ApiException apiException)
            {
                log.Error("ShortenerApiError: ", apiException);
                log.Warn(apiException.ErrorCode);
                log.Warn(apiException.Headers);
                log.Warn(apiException.ErrorContent);
            }
            _logger.LogInformation("Sms send => Finished");
            log.Info("SMS send - Finished");
        }

        private int CountShortenings(List<UrlData> results)
        {
            int shorteningResults = 0;
            if (results.Any())
                shorteningResults = results.Count();

            return shorteningResults;
        }
    }
}