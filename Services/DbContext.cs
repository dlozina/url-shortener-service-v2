using LiteDB;
using Shortener.Service.Model;
using Shortener.Service.Services.Interface;
using System.Linq;

namespace Shortener.Service.Services
{
    public class DbContext : IDbContext
    {
        private readonly ILiteDatabase _context;

        public DbContext(ILiteDatabase context)
        {
            _context = context;
        }

        public int AddUrl(UrlData urlData)
        {
            var db = _context.GetCollection<UrlData>(BsonAutoId.Int32);
            var id = db.Insert(urlData);

            return id.AsInt32;
        }

        public UrlData GetUrl(int id)
        {
            var db = _context.GetCollection<UrlData>();
            var entry = db.Query()
                .Where(x => x.Id.Equals(id))
                .ToList().FirstOrDefault();

            return entry;
        }

        public bool CheckIfUrlExists(string longUrl)
        {
            bool exists = false;
            var db = _context.GetCollection<UrlData>();
            exists = db.Query()
                .Where(x => x.Url.Equals(longUrl))
                .Exists();

            return exists;
        }
    }
}