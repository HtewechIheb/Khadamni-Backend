using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_X.Contracts.Requests;
using Project_X.Contracts.Responses;
using Project_X.Enumerations;
using Project_X.Models;
using Project_X.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Project_X.Shared.GlobalConstants;
using static Project_X.Shared.GlobalMethods;

namespace Project_X.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICompanyService _companyService;
        private readonly ICandidateService _candidateService;

        public AuthController(IAuthService authService, ICompanyService companyService, ICandidateService candidateService)
        {
            _authService = authService;
            _companyService = companyService;
            _candidateService = candidateService;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }

            var authResult = await _authService.LoginAsync(loginRequest.Email, loginRequest.Password);

            if (!authResult.Succeeded)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = authResult.Errors
                };

                return BadRequest(errorResponse);
            }

            var authResponse = new AuthResponse
            {
                Token = authResult.Token,
                RefreshToken = authResult.RefreshToken
            };

            return Ok(authResponse);
        }

        [HttpPost]
        [Route("registercompany")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RegisterCompany([FromForm] RegisterCompanyRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }

            using MemoryStream photoFileStream = new MemoryStream();
            await registerRequest.LogoFile.CopyToAsync(photoFileStream);

            var company = new Company
            {
                Name = registerRequest.Name,
                Address = registerRequest.Address,
                Description = registerRequest.Description,
                ContactNumber = registerRequest.ContactNumber,
                Category = registerRequest.Category,
                LogoFile = photoFileStream.ToArray(),
                LogoFileName = GenerateFileName(ResumePrefix, registerRequest.LogoFile.FileName)
            };

            var authResult = await _authService.RegisterCompanyAsync(registerRequest.Email, registerRequest.Password, company);

            if (!authResult.Succeeded)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = authResult.Errors
                };

                return BadRequest(errorResponse);
            }

            var authResponse = new AuthResponse
            {
                Token = authResult.Token,
                RefreshToken = authResult.RefreshToken
            };

            return Ok(authResponse);
        }

        [HttpPost]
        [Route("registercandidate")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RegisterCandidate([FromForm] RegisterCandidateRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }

            using MemoryStream resumeFileStream = new MemoryStream(), photoFileStream = new MemoryStream();
            await registerRequest.ResumeFile.CopyToAsync(resumeFileStream);
            await registerRequest.PhotoFile.CopyToAsync(photoFileStream);

            var candidate = new Candidate
            {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Address = registerRequest.Address,
                Gender = registerRequest.Gender,
                ResumeFile = resumeFileStream.ToArray(),
                ResumeFileName = GenerateFileName(ResumePrefix, registerRequest.ResumeFile.FileName),
                PhotoFile = photoFileStream.ToArray(),
                PhotoFileName = GenerateFileName(PhotoPrefix, registerRequest.PhotoFile.FileName),
                Birthdate = registerRequest.Birthdate
            };

            var authResult = await _authService.RegisterCandidateAsync(registerRequest.Email, registerRequest.Password, candidate);

            if (!authResult.Succeeded)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = authResult.Errors
                };

                return BadRequest(errorResponse);
            }

            var authResponse = new AuthResponse
            {
                Token = authResult.Token,
                RefreshToken = authResult.RefreshToken
            };

            return Ok(authResponse);
        }

        [HttpPost]
        [Route("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }

            var authResult = await _authService.RefreshTokenAsync(refreshTokenRequest.Token, refreshTokenRequest.RefreshToken);

            if (!authResult.Succeeded)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = authResult.Errors
                };

                return BadRequest(errorResponse);
            }

            var authResponse = new AuthResponse
            {
                Token = authResult.Token,
                RefreshToken = authResult.RefreshToken
            };

            return Ok(authResponse);
        }
    }
}
