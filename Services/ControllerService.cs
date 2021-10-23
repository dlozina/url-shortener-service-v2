using AutoMapper;
using Microsoft.Extensions.Logging;
using Shortener.Service.DTO;
using Shortener.Service.Model;
using Shortener.Service.Services.Interface;
using System;

namespace Shortener.Service.Services
{
    public class ControllerService : IControllerService
    {
        private IDbContext _dbContext;
        private IUrlHelper _urlHelper;
        private IMapper _mapper;
        private readonly ISendSms _sendSms;
        private readonly ILogger<ControllerService> _logger;
        
        // To expand project target url should be moved to DB or at least settings
        private string targetUrl = "infobip.com";

        public ControllerService(IDbContext dbContext, IUrlHelper urlHelper, IMapper mapper, ISendSms sendSms, ILogger<ControllerService> logger)
        {
            _dbContext = dbContext;
            _urlHelper = urlHelper;
            _mapper = mapper;
            _sendSms = sendSms;
            _logger = logger;
        }

        public UrlDataDto GetUrlData(string shortUrl)
        {
            var id = _urlHelper.GetId(shortUrl);
            var urlData = _dbContext.GetUrl(id);
            var urlDataDto = _mapper.Map<UrlDataDto>(urlData);

            return urlDataDto;
        }

        public UrlDataDto AddUrlData(UrlDataDto urlDataDto, string requestScheme, string requestHost)
        {
            var newEntry = new UrlData
            {
                Url = urlDataDto.Url,
                ShorteningDateTime = DateTime.Now.Date
            };

            var id = _dbContext.AddUrl(newEntry);
            UrlDataDto responseUrlDataDto = new UrlDataDto() { Url = $"{requestScheme}://{requestHost}/{_urlHelper.GetShortUrl(id)}" };
            
            NotifyTargetUrlIsShortened(responseUrlDataDto);

            return responseUrlDataDto;
        }

        private void NotifyTargetUrlIsShortened(UrlDataDto responseUrlDataDto)
        {
            if (responseUrlDataDto.Url.Contains(targetUrl))
            {
                _logger.LogInformation("Target URL is: " + targetUrl);
                _sendSms.NotifyTargetUrlIsShortened();
            }
        }

        public bool CheckUserAgent(string headersUserAgent)
        {
            bool userAgentBrowser = false;
            // Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36
            // Just to avoid version issues
            if (headersUserAgent.Contains("Mozilla") ||
                   headersUserAgent.Contains("AppleWebKit") ||
                   headersUserAgent.Contains("Chrome") ||
                   headersUserAgent.Contains("Safari") ||
                   headersUserAgent.Contains("Edg"))
                userAgentBrowser = true;

            return userAgentBrowser;
        }

        public bool CheckIfUrlExists(UrlDataDto urlDataDto)
        {
            bool urlExists = false;
            urlExists = _dbContext.CheckIfUrlExists(urlDataDto.Url);

            return urlExists;
        }
    }
}