using Project_X.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_X.Repositories.Abstractions
{
    public interface ICompanyRepository
    {
        public Task<IEnumerable<Company>> GetCompanies();
        public Task<bool> AddCompany(Company company);
        public Task<Company> GetCompanyById(long id);
        public Task<bool> UpdateCompany(Company company);
        public Task<bool> DeleteCompany(long id);
    }
}
