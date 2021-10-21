using Shortener.Service.Services.Interface;
using HashidsNet;

namespace Shortener.Service.Services
{
    public class UrlHelper : IUrlHelper
    {
        private Hashids _hashIds;
        public UrlHelper()
        {
            _hashIds = new Hashids("This is my shortener", 6);
        }

        public string GetShortUrl(int id)
        {
            return _hashIds.Encode(id);
        }

        public int GetId(string shortUrl)
        {
            var decodedId = _hashIds.Decode(shortUrl);
            return decodedId[0];
        }
    }
}
