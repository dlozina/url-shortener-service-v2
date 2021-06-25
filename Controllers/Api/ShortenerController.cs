using LiteDB;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    [Route("shorten")]
    [ApiController]
    public class ShortenerController : ControllerBase
    {
        private readonly IUrls _urls;
        private readonly IUrlHelper _urlHelper;
        private readonly ILiteDatabase _context;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ShortenerController(IUrls urls, IUrlHelper urlHelper, ILiteDatabase context)
        {
            _urls = urls;
            _urlHelper = urlHelper;
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllContacts(string shortUrl = "")
        {
            if (String.IsNullOrEmpty(shortUrl))
                return BadRequest();

            try
            {
                var db = _context.GetCollection<UrlData>();
                var id = _urlHelper.GetId(shortUrl);
                var entry = db.Find(p => p.Id == id).FirstOrDefault();

                return Ok();
            }
            catch (Exception ex)
            {
                log.Error("ShortenerApiError: ", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public IActionResult CreateContact([FromBody] UrlData url)
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
                var newEntry = new UrlData { Url = url.Url };
                var id = db.Insert(newEntry);

                var shortenUrl = _urlHelper.GetShortUrl(id.AsInt32);
                
                // To do - Konstruirati pravi link

                return Created("shortUrl", shortenUrl);
            }
            catch (Exception ex)
            {
                log.Error("ShortenerApiError: ", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
