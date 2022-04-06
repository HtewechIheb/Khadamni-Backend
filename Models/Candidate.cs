using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Models
{
    public class Candidate
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public byte[] ResumeFile { get; set; }
        public string ResumeFileName { get; set; }
        public byte[] PhotoFile { get; set; }
        public string PhotoFileName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string AccountId { get; set; }
        public IdentityUser Account { get; set; }
        public ICollection<Application> Applications { get; set; }
    }
}
