using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shortener.Service.Services.Interface
{
    public interface IUrlHelper
    {
        string GetShortUrl(int id);

        int GetId(string shortUrl);
    }
}
