using Project_X.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_X.Services
{
    public interface ICandidateService
    {
        public Task<IEnumerable<Candidate>> GetCandidates();
        public Task<Candidate> GetCandidateById(long id);
        public Task<bool> AddCandidate(Candidate candidate);
        public Task<bool> UpdateCandidate(Candidate candidate);
        public Task<bool> DeleteCandidate(long id);
    }
}
