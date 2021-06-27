using Infobip.Api.Client;
using Infobip.Api.Client.Api;
using Infobip.Api.Client.Model;
using LiteDB;
using log4net;
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
        // Infobip api authentication
        private string basePath = "https://19pww9.api.infobip.com";

        private string apiKeyPrefix = "App";
        private string apiKey = "19b077f413e61e62a6478b1b2813790f-11e8558b-a296-46b1-9f47-cd163f7a4108";

        // Sms settings
        private string notificationSmsNumber = "385958165678";
        private string from = "InfoSMS";

        private readonly ILiteDatabase _context;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SendSms(ILiteDatabase context)
        {
            _context = context;
        }

        public void NotifyTargetUrlIsShortened()
        {
            var smsNotificationMessage = "Targeted url has been shortened. (infobip.com)";
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
            SendNotificationSms(smsNotificationMessage);
        }

        public void SendWeeklyNotification()
        {
            DateTime localDate = DateTime.Now.Date;
            DateTime weekbefore = localDate.AddDays(-7);

            var db = _context.GetCollection<UrlData>();
            var results = db.Query()
                .Where(x => x.ShorteningDateTime >= weekbefore && x.ShorteningDateTime <= localDate).ToList();
            var dailyShorteningResults = CountShortenings(results);

            var smsNotificationMessage = $"Report for previous week. Number of URL shortenings: {dailyShorteningResults}.";
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
            var dailyShorteningResults = CountShortenings(results);

            var smsNotificationMessage = $"Report for previous month. Number of URL shortenings: {dailyShorteningResults}.";
            SendNotificationSms(smsNotificationMessage);
        }

        private void SendNotificationSms(string smsNotificationMessage)
        {
            log.Info("SMS send - Start");
            var configuration = new Configuration()
            {
                BasePath = basePath,
                ApiKeyPrefix = apiKeyPrefix,
                ApiKey = apiKey
            };
            var sendSmsApi = new SendSmsApi(configuration);

            var smsMessage = new SmsTextualMessage()
            {
                From = from,
                Destinations = new List<SmsDestination>()
                {
                    new SmsDestination(to: notificationSmsNumber)
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
            log.Info("SMS send - Finished");
        }

        private int CountShortenings(List<UrlData> results)
        {
            int dailyShorteningResults = 0;
            if (results.Any())
                dailyShorteningResults = results.Count();

            return dailyShorteningResults;
        }
    }
}