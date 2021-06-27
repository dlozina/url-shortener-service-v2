using AutoMapper;
using LiteDB;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shortener.Service.DTO;
using Shortener.Service.Model;
using Shortener.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using IUrlHelper = Shortener.Service.Services.Interface.IUrlHelper;

namespace Shortener.Service.Controllers.Api
{
    [ApiController]
    public class ShortenerController : ControllerBase
    {
        private readonly IUrls _urls;
        private readonly IUrlHelper _urlHelper;
        private readonly ISendSms _sendSms;
        private readonly ILiteDatabase _context;
        private readonly IMapper _mapper;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        string targetUrl = "infobip.com";

        public ShortenerController(IUrls urls, IUrlHelper urlHelper, ISendSms sendSms, ILiteDatabase context, IMapper mapper)
        {
            _urls = urls;
            _urlHelper = urlHelper;
            _context = context;
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
                var db = _context.GetCollection<UrlData>();
                var id = _urlHelper.GetId(shortUrl);
                var entry = db.Find(p => p.Id == id).FirstOrDefault();

                if (entry == null)
                    return NotFound();

                var urlDataDto = _mapper.Map<UrlDataDto>(entry);

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
                var db = _context.GetCollection<UrlData>(BsonAutoId.Int32);
                var newEntry = new UrlData 
                { 
                    Url = longUrl.Url,
                    ShorteningDateTime = DateTime.Now
                };
                var id = db.Insert(newEntry);
                
                UrlDataDto urlDataDto = new UrlDataDto() { Url = $"{this.Request.Scheme}://{this.Request.Host}/{_urlHelper.GetShortUrl(id.AsInt32)}" };

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
