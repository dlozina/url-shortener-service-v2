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
        private readonly ILiteDatabase _context;
        private readonly IMapper _mapper;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ShortenerController(IUrls urls, IUrlHelper urlHelper, ILiteDatabase context, IMapper mapper)
        {
            _urls = urls;
            _urlHelper = urlHelper;
            _context = context;
            _mapper = mapper;
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

                var urlDataDto = _mapper.Map<UrlDataDto>(entry);

                return Ok(urlDataDto);
            }
            catch (Exception ex)
            {
                log.Error("ShortenerApiError: ", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("shorten")]
        public IActionResult ShortenUrl([FromBody] UrlDataDto url)
        {
            if (url == null)
                return BadRequest();

            if (!Uri.TryCreate(url.Url, UriKind.Absolute, out Uri result))
                ModelState.AddModelError("URL", "URL shouldn't be empty");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var db = _context.GetCollection<UrlData>(BsonAutoId.Int32);
                var newEntry = new UrlData 
                { 
                    Url = url.Url,
                    ShorteningDateTime = DateTime.Now
                };
                var id = db.Insert(newEntry);
                
                UrlDataDto urlDataDto = new UrlDataDto() { Url = $"{this.Request.Scheme}://{this.Request.Host}/{_urlHelper.GetShortUrl(id.AsInt32)}" };

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
