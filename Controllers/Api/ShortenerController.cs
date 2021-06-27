using AutoMapper;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shortener.Service.DTO;
using Shortener.Service.Model;
using Shortener.Service.Services.Interface;
using System;
using System.Reflection;
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

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string targetUrl = "infobip.com";

        public ShortenerController(IDbContext urls, IUrlHelper urlHelper, ISendSms sendSms, IMapper mapper)
        {
            _dbContext = urls;
            _urlHelper = urlHelper;
            _mapper = mapper;
            _sendSms = sendSms;
        }

        [HttpGet("{shortUrl}")]
        public IActionResult GetUrl(string shortUrl)
        {
            if (String.IsNullOrEmpty(shortUrl))
                return BadRequest();

            try
            {
                var id = _urlHelper.GetId(shortUrl);
                var urlData = _dbContext.GetUrl(id);

                if (urlData == null)
                    return NotFound();

                var urlDataDto = _mapper.Map<UrlDataDto>(urlData);

                //return RedirectPermanent(urlDataDto.Url);
                return Ok(urlDataDto);
            }
            catch (Exception ex)
            {
                log.Error("ShortenerApiError: ", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("shorten")]
        public IActionResult ShortenUrl([FromBody] UrlDataDto longUrl)
        {
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

                return Created("shortUrl", urlDataDto);
            }
            catch (Exception ex)
            {
                log.Error("ShortenerApiError: ", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}