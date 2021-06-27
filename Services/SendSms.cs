using Infobip.Api.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Infobip.Api.Client.Model;
using Infobip.Api.Client;
using log4net;
using System.Reflection;
using Shortener.Service.Services.Interface;

namespace Shortener.Service.Services
{
    public class SendSms : ISendSms
    {
        string basePath = "https://19pww9.api.infobip.com";
        string apiKeyPrefix = "App";
        string apiKey = "19b077f413e61e62a6478b1b2813790f-11e8558b-a296-46b1-9f47-cd163f7a4108";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void NotifyTargetUrlIsShortened()
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
                From = "InfoSMS",
                Destinations = new List<SmsDestination>()
                {
                    new SmsDestination(to: "385958165678")
                },
                Text = "Targeted url has been shortened. (infobip.com)"
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
    }
}
