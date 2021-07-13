using Project_X.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Services
{
    public interface IApplicationService
    {
        public Task<IEnumerable<Application>> GetApplications();
        public Task<IEnumerable<Application>> GetApplicationsByCandidateId(long id);
        public Task<IEnumerable<Application>> GetApplicationsByOfferId(long id);
        public Task<Application> GetApplicationByCandidateIdAndOfferId(long candidateId, long offerId);
        public Task<bool> AddApplication(Application application);
        public Task<bool> UpdateApplication(Application application);
        public Task<bool> DeleteApplication(long id);
    }
}
