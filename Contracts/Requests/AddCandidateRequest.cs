using Microsoft.AspNetCore.Http;
using Project_X.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Contracts.Requests
{
    public class AddCandidateRequest
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        [Required]
        [MaxFileSize(5 * 1024 * 1024)]
        [AllowedFileExtensions("pdf")]
        public IFormFile ResumeFile { get; set; }
        [Required]
        [MaxFileSize(2 * 1024 * 1024)]
        [AllowedFileExtensions("jpg", "jpeg", "png")]
        public IFormFile PhotoFile { get; set; }
        public DateTime? Birthdate { get; set; }
    }
}
