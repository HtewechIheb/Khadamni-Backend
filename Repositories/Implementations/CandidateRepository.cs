using Microsoft.EntityFrameworkCore;
using Project_X.Database;
using Project_X.Models;
using Project_X.Repositories.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_X.Repositories.Implementations
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly AppDbContext _appDbContext;

        public CandidateRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Candidate>> GetCandidates()
        {
            return await _appDbContext.Candidates.ToListAsync();
        }

        public async Task<Candidate> GetCandidateById(long id)
        {
            return await _appDbContext.Candidates.FindAsync(id);
        }

        public async Task<bool> AddCandidate(Candidate company)
        {
            await _appDbContext.Candidates.AddAsync(company);
            int added = await _appDbContext.SaveChangesAsync();
            return added > 0;
        }

        public async Task<bool> UpdateCandidate(Candidate company)
        {
            _appDbContext.Candidates.Update(company);
            int updated = await _appDbContext.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeleteCandidate(long id)
        {
            Candidate candidate = await _appDbContext.Candidates.FindAsync(id);
            _appDbContext.Candidates.Remove(candidate);
            int deleted = await _appDbContext.SaveChangesAsync();
            return deleted > 0;
        }
    }
}
