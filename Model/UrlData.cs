using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shortener.Service.Model
{
    public class UrlData
    {
        public int Id { get; set; }
        [Required]
        public string Url { get; set; }
    }
}
