using Microsoft.AspNetCore.WebUtilities;
using Shortener.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shortener.Service.Services
{
    public class UrlHelper : IUrlHelper
    {
        // Using Bijective conversion (one-to-one correspondence)
        // https://github.com/delight-im/ShortURL/blob/master/C%23/ShortURL.cs
        private const string Alphabet = "23456789bcdfghjkmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZ-_";
        private static readonly int Base = Alphabet.Length;

        // Transform the "Id" property on this object into a short string
        public string GetShortUrl(int id)
        {
            //Possible solution
            //return WebEncoders.Base64UrlEncode(BitConverter.GetBytes(id));
            return Encode(id);
        }

        // Reverse short url text back into an interger Id
        public int GetId(string shortUrl)
        {
            //Possible solution
            //return BitConverter.ToInt32(WebEncoders.Base64UrlDecode(shortUrl));
            return Decode(shortUrl);
        }

        private string Encode(int num)
        {
            var sb = new StringBuilder();
            while (num > 0)
            {
                sb.Insert(0, Alphabet.ElementAt(num % Base));
                num = num / Base;
            }
            return sb.ToString();
        }

        private int Decode(string str)
        {
            var num = 0;
            for (var i = 0; i < str.Length; i++)
            {
                num = num * Base + Alphabet.IndexOf(str.ElementAt(i));
            }
            return num;
        }
    }
}
