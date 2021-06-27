using Shortener.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shortener.Service.Services.Interface
{
    public interface IDbContext
    {
        int AddUrl(UrlData urlData);

        UrlData GetUrl(int id);
    }
}
