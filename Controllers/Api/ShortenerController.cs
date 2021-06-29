using AutoMapper;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shortener.Service.DTO;
using Shortener.Service.Model;
using Shortener.Service.Services.Interface;
using System;
using System.Reflection;
using System.Text;
using IUrlHelper = Shortener.Service.Services.Interface.IUrlHelper;

namespace Shortener.Service.Controllers.Api
{
    [ApiController]
    public class ShortenerController : ControllerBase
    {
        private readonly IDbContext _dbContext;
        private readonly IUrlHelper _urlHelper;
        private readonly ISendSms _sendSms;
        private readonly IMapper _mapper;
        // FileStream Log
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        // Console log - Just to be user more friendly // Debug
        private readonly ILogger<ShortenerController> _logger;

        private string targetUrl = "infobip.com";

        public ShortenerController(IDbContext urls, IUrlHelper urlHelper, ISendSms sendSms, IMapper mapper, ILogger<ShortenerController> logger)
        {
            _dbContext = urls;
            _urlHelper = urlHelper;
            _mapper = mapper;
            _sendSms = sendSms;
            _logger = logger;
        }

        [HttpGet("{shortUrl}")]
        public IActionResult GetUrl(string shortUrl)
        {
            _logger.LogInformation("Get request => Start");

            if (String.IsNullOrEmpty(shortUrl))
                return BadRequest();

            try
            {
                var id = _urlHelper.GetId(shortUrl);
                var urlData = _dbContext.GetUrl(id);

                if (urlData == null)
                    return NotFound();

                var urlDataDto = _mapper.Map<UrlDataDto>(urlData);

                // Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36
                // Just to avoid version issues
                string userAgent = this.Request.Headers["User-Agent"].ToString();
                if (userAgent.Contains("Mozilla") || 
                    userAgent.Contains("AppleWebKit") || 
                    userAgent.Contains("Chrome") || 
                    userAgent.Contains("Safari") ||
                    userAgent.Contains("Edg"))
                    return RedirectPermanent(urlDataDto.Url);

                _logger.LogInformation("Get request => Finished");
                return Ok(urlDataDto);
            }
            catch (Exception ex)
            {
                log.Error("ShortenerApiError: ", ex);
                _logger.LogInformation("Get request => Error");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("shorten")]
        public IActionResult ShortenUrl([FromBody] UrlDataDto longUrl)
        {
            _logger.LogInformation("Post request => Start");

            if (longUrl == null)
                return BadRequest();

            if (!Uri.TryCreate(longUrl.Url, UriKind.Absolute, out Uri result))
                ModelState.AddModelError("URL", "URL shouldn't be empty");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if(_dbContext.CheckIfUrlExists(longUrl.Url))
                    ModelState.AddModelError("Url", "Url is allready shortened");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newEntry = new UrlData
                {
                    Url = longUrl.Url,
                    ShorteningDateTime = DateTime.Now.Date
                };

                var id = _dbContext.AddUrl(newEntry);

                UrlDataDto urlDataDto = new UrlDataDto() { Url = $"{this.Request.Scheme}://{this.Request.Host}/{_urlHelper.GetShortUrl(id)}" };

                if (longUrl.Url.Contains(targetUrl))
                {
                    log.Info("Target URL is: " + targetUrl);
                    _sendSms.NotifyTargetUrlIsShortened();
                }

                _logger.LogInformation("Post request => Finished");
                return Created("shortUrl", urlDataDto);
            }
            catch (Exception ex)
            {
                log.Error("ShortenerApiError: ", ex);
                _logger.LogInformation("Get request => Error");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}