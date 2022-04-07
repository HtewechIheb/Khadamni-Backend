using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Project_X.Configuration;
using Project_X.Database;
using Project_X.Enumerations;
using Project_X.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Project_X.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenValidationParameters _tokenValidationParams;
        private readonly AppDbContext _appDbContext;
        private readonly ICompanyService _companyService;
        private readonly ICandidateService _candidateService;
        private readonly JWTConfig _jwtConfig;

        public AuthService(UserManager<AppUser> userManager, IOptionsMonitor<JWTConfig> optionsMonitor, TokenValidationParameters tokenValidationParams, AppDbContext appDbContext, ICompanyService companyService, ICandidateService candidateService)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParams = tokenValidationParams;
            _appDbContext = appDbContext;
            _companyService = companyService;
            _candidateService = candidateService;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { $"User With Email {email} Does Not Exist!" }
                };
            }

            var isValid = await _userManager.CheckPasswordAsync(existingUser, password);

            if (!isValid)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { $"Login Failed, Please Verify Your Credentials!" }
                };
            }

            return await GenerateAuthResult(existingUser);
        }

        public async Task<AuthResult> RegisterCompanyAsync(string email, string password, Company company)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { $"User With Email {email} Already Exists!" }
                };
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                Type = UserType.Company
            };

            var userCreated = await _userManager.CreateAsync(user, password);

            if (!userCreated.Succeeded)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = userCreated.Errors.Select(error => error.Description)
                };
            }

            company.Account = user;

            if (!await _companyService.AddCompany(company))
            {
                await _userManager.DeleteAsync(user);

                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Internal Error Occured While Adding Company!" }
                };
            }

            return await GenerateAuthResult(user);
        }

        public async Task<AuthResult> RegisterCandidateAsync(string email, string password, Candidate candidate)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { $"User With Email {email} Already Exists!" }
                };
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                Type = UserType.Candidate
            };

            var userCreated = await _userManager.CreateAsync(user, password);

            if (!userCreated.Succeeded)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = userCreated.Errors.Select(error => error.Description)
                };
            }

            candidate.Account = user;

            if (!await _candidateService.AddCandidate(candidate))
            {
                await _userManager.DeleteAsync(user);

                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Internal Error Occured While Adding Candidate!" }
                };
            }

            return await GenerateAuthResult(user);
        }

        public async Task<AuthResult> RefreshTokenAsync(string token, string refreshToken)
        {
            var validatedToken = ValidateToken(token);

            if (validatedToken == null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Token Is Invalid!" }
                };
            }

            var expiryDate = long.Parse(validatedToken.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Exp).Value);
            var expiryDateUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDate);
            if (expiryDateUtc > DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Token Hasn't Expired Yet!" }
                };
            }

            var storedRefreshToken = await _appDbContext.RefreshTokens.SingleOrDefaultAsync(token => token.Token == refreshToken);
            var jwtTokenJti = validatedToken.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value;

            if (storedRefreshToken == null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Refresh Token Does Not Exist!" }
                };
            }

            if (storedRefreshToken.IsUsed)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Refresh Token Already Used!" }
                };
            }

            if (storedRefreshToken.IsRevoked)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Refresh Token Has Been Revoked!" }
                };
            }

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Refresh Token Has Expired!" }
                };
            }

            if (storedRefreshToken.JwtId != jwtTokenJti)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = new[] { "Refresh Token Does Not Match With This Token!" }
                };
            }

            storedRefreshToken.IsUsed = true;
            _appDbContext.RefreshTokens.Update(storedRefreshToken);
            await _appDbContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(validatedToken.Claims.Single(claim => claim.Type == "id").Value);

            return await GenerateAuthResult(user);
        }

        private async Task<AuthResult> GenerateAuthResult(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.TokenLifeTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var serializedToken = tokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(_jwtConfig.RefreshTokenLength),
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(_jwtConfig.RefreshTokenLifeTime),
                User = user
            };

            await _appDbContext.RefreshTokens.AddAsync(refreshToken);
            await _appDbContext.SaveChangesAsync();

            return new AuthResult
            {
                Succeeded = true,
                Token = serializedToken,
                RefreshToken = refreshToken.Token
            };
        }

        private string GenerateRefreshToken(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(charArray => charArray[random.Next(charArray.Length)]).ToArray())
                + Guid.NewGuid().ToString();
        }

        private ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParams, out var validatedToken);
                if (!IsJwtTokenWithValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private bool IsJwtTokenWithValidSecurityAlgorithm(SecurityToken token)
        {
            return (token is JwtSecurityToken jwtToken) && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
