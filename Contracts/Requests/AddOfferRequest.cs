using Project_X.Contracts.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Contracts.Requests
{
    public class AddOfferRequest
    {
        [Required]
        public OfferCategory Category { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Spots { get; set; }
        [Required]
        public OfferType Type { get; set; }
        [Required]
        public string ExperienceLowerBound { get; set; }
        public string ExperienceUpperBound { get; set; }
        [Required]
        public long CompanyId { get; set; }
    }
}
