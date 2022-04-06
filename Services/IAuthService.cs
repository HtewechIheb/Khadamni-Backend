using Project_X.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Services
{
    public interface IAuthService
    {
        public Task<AuthResult> LoginAsync(string email, string password);
        public Task<AuthResult> RegisterCompanyAsync(string email, string password, Company company);
        public Task<AuthResult> RegisterCandidateAsync(string email, string password, Candidate candidate);
        public Task<AuthResult> RefreshTokenAsync(string token, string refreshToken);
    }
}
