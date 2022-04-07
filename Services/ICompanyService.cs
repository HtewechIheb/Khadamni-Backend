using Project_X.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_X.Services
{
    public interface ICompanyService
    {
        public Task<IEnumerable<Company>> GetCompanies();
        public Task<bool> AddCompany(Company company);
        public Task<Company> GetCompanyById(long id);
        public Task<bool> UpdateCompany(Company company);
        public Task<bool> DeleteCompany(long id);
    }
}
