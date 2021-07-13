using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Contracts.Requests
{
    public class UpdateCandidateRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public IFormFile ResumeFile { get; set; }
        public IFormFile PhotoFile { get; set; }
        public DateTime Birthdate { get; set; }
    }
}
