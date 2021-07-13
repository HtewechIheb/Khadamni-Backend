using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Models
{
    public class Offer
    {
        public long Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Spots { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string ExperienceLowerBound { get; set; }
        public string ExperienceUpperBound { get; set; }
        public virtual Company Company { get; set; }
        public virtual ICollection<Application> Applications { get; set; }
    }
}
