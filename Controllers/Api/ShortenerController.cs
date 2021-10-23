using Microsoft.AspNetCore.Mvc;
using Shortener.Service.DTO;
using Shortener.Service.Services.Interface;
using System;

namespace Shortener.Service.Controllers.Api
{
    [ApiController]
    public class ShortenerController : ControllerBase
    {
        private readonly IControllerService _controllerService;

        public ShortenerController(IControllerService controllerService)
        {
            _controllerService = controllerService;
        }

        [HttpGet("{shortUrl}")]
        public IActionResult GetUrl(string shortUrl)
        {
            if (String.IsNullOrEmpty(shortUrl))
                return BadRequest();

            var urlDataDto = _controllerService.GetUrlData(shortUrl);

            if (urlDataDto == null)
                return NotFound();

            var userAgentBrowser = _controllerService.CheckUserAgent(this.Request.Headers["User-Agent"].ToString());

            if (userAgentBrowser)
                return RedirectPermanent(urlDataDto.Url);
            
            return Ok(urlDataDto);
        }

        [HttpPost("shorten")]
        public IActionResult ShortenUrl([FromBody] UrlDataDto requestUrlDataDto)
        {
            if (requestUrlDataDto == null)
                return BadRequest();

            if (!Uri.TryCreate(requestUrlDataDto.Url, UriKind.Absolute, out Uri result))
                ModelState.AddModelError("URL", "URL shouldn't be empty");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_controllerService.CheckIfUrlExists(requestUrlDataDto))
                ModelState.AddModelError("Url", "Url is allready shortened");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var responseUrlDataDto = _controllerService.AddUrlData(requestUrlDataDto, this.Request.Scheme, this.Request.Host.ToString());

            return Created("shortUrl", responseUrlDataDto);
        }
    }
}