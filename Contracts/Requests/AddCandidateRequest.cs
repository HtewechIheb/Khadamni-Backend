using Microsoft.AspNetCore.Http;
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
        public IFormFile ResumeFile { get; set; }
        [Required]
        public IFormFile PhotoFile { get; set; }
        public DateTime? Birthdate { get; set; }
    }
}
