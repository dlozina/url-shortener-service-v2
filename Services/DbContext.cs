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
            var entry = db.Find(p => p.Id == id).FirstOrDefault();

            return entry;
        }
    }
}