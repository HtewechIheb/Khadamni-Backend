using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Contracts.Requests
{
    public class UpdateCompanyRequest
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
    }
}
