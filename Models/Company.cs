using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Models
{
    public class Company
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string ContactNumber { get; set; }
        public string Category { get; set; }
        public byte[] LogoFile { get; set; }
        public string LogoFileName { get; set; }
        public string AccountId { get; set; }
        public IdentityUser Account { get; set; }
        public ICollection<Offer> Offers { get; set; }
    }
}
