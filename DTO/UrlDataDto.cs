using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shortener.Service.DTO
{
    public class UrlDataDto
    {
        [Required]
        public string Url { get; set; }
    }
}
