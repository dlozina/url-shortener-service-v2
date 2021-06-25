using Microsoft.AspNetCore.WebUtilities;
using Shortener.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shortener.Service.Services
{
    public class UrlHelper : IUrlHelper
    {
        // Transform the "Id" property on this object into a short string
        public string GetShortUrl(int id)
        {
            return WebEncoders.Base64UrlEncode(BitConverter.GetBytes(id));
        }

        // Reverse short url text back into an interger Id
        public int GetId(string shortUrl)
        {
            return BitConverter.ToInt32(WebEncoders.Base64UrlDecode(shortUrl));
        }
    }
}
