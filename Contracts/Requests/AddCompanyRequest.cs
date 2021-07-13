using Microsoft.AspNetCore.Http;
using Project_X.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Contracts.Requests
{
    public class AddCompanyRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        public string Description { get; set; }
        [Required]
        [MaxFileSize(2 * 1024 * 1024)]
        [AllowedFileExtensions("jpg", "jpeg", "png")]
        public IFormFile PhotoFile { get; set; }
    }
}
