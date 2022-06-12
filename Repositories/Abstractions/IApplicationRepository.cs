using Project_X.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_X.Repositories.Abstractions
{
    public interface IApplicationRepository
    {
        public Task<IEnumerable<Application>> GetApplications();
        public Task<IEnumerable<Application>> GetApplicationsByCandidateId(long id);
        public Task<IEnumerable<Application>> GetApplicationsByOfferId(long id);
        public Task<Application> GetApplicationByCandidateIdAndOfferId(long candidateId, long offerId);
        public Task<bool> AddApplication(Application application);
        public Task<bool> UpdateApplication(Application application);
        public Task<bool> DeleteApplication(long candidateId, long offerId);
    }
}
