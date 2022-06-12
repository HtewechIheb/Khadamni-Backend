using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Project_X.Configuration;
using Project_X.DTO.Requests;
using Project_X.DTO.Responses;
using Project_X.Models;
using Project_X.Services.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Project_X.Utilities.GlobalConstants;
using static Project_X.Utilities.GlobalMethods;

namespace Project_X.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JWTConfig _jwtConfig;

        public AuthController(IAuthService authService, IOptionsMonitor<JWTConfig> optionsMonitor)
        {
            _jwtConfig = optionsMonitor.CurrentValue;
            _authService = authService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorDTO);
            }

            var authResult = await _authService.LoginAsync(loginDTO.Email, loginDTO.Password);

            if (!authResult.Succeeded)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = authResult.Errors
                };

                return BadRequest(errorDTO);
            }

            SetRefreshTokenCookie(authResult.RefreshToken);

            var authDTO = new AuthDTO
            {
                Token = authResult.Token,
            };

            return Ok(authDTO);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("registercompany")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RegisterCompany([FromForm] RegisterCompanyDTO registerCompanyDTO)
        {
            if (!ModelState.IsValid)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorDTO);
            }

            using MemoryStream photoFileStream = new MemoryStream();
            await registerCompanyDTO.LogoFile.CopyToAsync(photoFileStream);

            var company = new Company
            {
                Name = registerCompanyDTO.Name,
                Address = registerCompanyDTO.Address,
                Description = registerCompanyDTO.Description,
                ContactNumber = registerCompanyDTO.ContactNumber,
                Category = registerCompanyDTO.Category,
                LogoFile = photoFileStream.ToArray(),
                LogoFileName = GenerateFileName(ResumePrefix, registerCompanyDTO.LogoFile.FileName)
            };

            var authResult = await _authService.RegisterCompanyAsync(registerCompanyDTO.Email, registerCompanyDTO.Password, company);

            if (!authResult.Succeeded)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = authResult.Errors
                };

                return BadRequest(errorDTO);
            }

            SetRefreshTokenCookie(authResult.RefreshToken);

            var authDTO = new AuthDTO
            {
                Token = authResult.Token,
            };

            return Ok(authDTO);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("registercandidate")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RegisterCandidate([FromForm] RegisterCandidateDTO registerCandidateDTO)
        {
            if (!ModelState.IsValid)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorDTO);
            }

            using MemoryStream resumeFileStream = new MemoryStream(), photoFileStream = new MemoryStream();
            await registerCandidateDTO.ResumeFile.CopyToAsync(resumeFileStream);
            await registerCandidateDTO.PhotoFile.CopyToAsync(photoFileStream);

            var candidate = new Candidate
            {
                FirstName = registerCandidateDTO.FirstName,
                LastName = registerCandidateDTO.LastName,
                Address = registerCandidateDTO.Address,
                Gender = registerCandidateDTO.Gender,
                ResumeFile = resumeFileStream.ToArray(),
                ResumeFileName = GenerateFileName(ResumePrefix, registerCandidateDTO.ResumeFile.FileName),
                PhotoFile = photoFileStream.ToArray(),
                PhotoFileName = GenerateFileName(PhotoPrefix, registerCandidateDTO.PhotoFile.FileName),
                Birthdate = registerCandidateDTO.Birthdate
            };

            var authResult = await _authService.RegisterCandidateAsync(registerCandidateDTO.Email, registerCandidateDTO.Password, candidate);

            if (!authResult.Succeeded)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = authResult.Errors
                };

                return BadRequest(errorDTO);
            }

            SetRefreshTokenCookie(authResult.RefreshToken);

            var authDTO = new AuthDTO
            {
                Token = authResult.Token,
            };

            return Ok(authDTO);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("refreshtoken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDTO)
        {
            if (!ModelState.IsValid)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorDTO);
            }

            var refreshToken = Request.Cookies.FirstOrDefault(cookie => cookie.Key == "refresh_token").Value;

            if(string.IsNullOrEmpty(refreshToken))
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = new[] { "Refresh Token Cookie Is Not Set!" }
                };

                return BadRequest(errorDTO);
            }

            var authResult = await _authService.RefreshTokenAsync(refreshTokenDTO.Token, refreshToken);

            if (!authResult.Succeeded)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = authResult.Errors
                };

                return BadRequest(errorDTO);
            }

            SetRefreshTokenCookie(authResult.RefreshToken);

            var authDTO = new AuthDTO
            {
                Token = authResult.Token,
            };

            return Ok(authDTO);
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenLifeTime),
            };

            Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
        }
    }
}


/*
[HttpPut]
[Route("{id:long}")]
[Consumes("multipart/form-data")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorDTO), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
[ProducesDefaultResponseType]
public async Task<IActionResult> Update([FromForm] UpdateCandidateRequest candidateRequest, [FromRoute] long id)
{
    if (!ModelState.IsValid)
    {
        var errorResponse = new ErrorDTO
        {
            Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
        };

        return BadRequest(errorResponse);
    }

    var candidateToUpdate = await _candidateRepository.GetCandidateById(id);

    if (candidateToUpdate == null)
    {
        return NotFound($"Candidate With ID {id} Does Not Exist!");
    }

    using MemoryStream resumeFileStream = new MemoryStream(), photoFileStream = new MemoryStream();

    candidateToUpdate.FirstName = candidateRequest.FirstName ?? candidateToUpdate.FirstName;
    candidateToUpdate.LastName = candidateRequest.LastName ?? candidateToUpdate.LastName;
    candidateToUpdate.Address = candidateRequest.Address ?? candidateToUpdate.Address;
    candidateToUpdate.Gender = candidateRequest.Gender ?? candidateToUpdate.Gender;
    if (candidateRequest.ResumeFile != null)
    {
        await candidateRequest.ResumeFile.CopyToAsync(resumeFileStream);
        candidateToUpdate.ResumeFile = resumeFileStream.ToArray();
        candidateToUpdate.ResumeFileName = GenerateFileName(ResumePrefix, candidateRequest.ResumeFile.FileName);
    }
    if (candidateRequest.PhotoFile != null)
    {
        await candidateRequest.PhotoFile.CopyToAsync(photoFileStream);
        candidateToUpdate.PhotoFile = photoFileStream.ToArray();
        candidateToUpdate.PhotoFileName = GenerateFileName(PhotoPrefix, candidateRequest.PhotoFile.FileName);
    }
    candidateToUpdate.Birthdate = candidateRequest.Birthdate ?? candidateToUpdate.Birthdate;

    if (!await _candidateRepository.UpdateCandidate(candidateToUpdate))
    {
        return StatusCode(500, "Internal Error Occured While Updating Candidate!");
    }

    var candidateDTO = new CandidateDTO
    {
        Id = candidateToUpdate.Id,
        FirstName = candidateToUpdate.FirstName,
        LastName = candidateToUpdate.LastName,
        Address = candidateToUpdate.Address,
        Gender = candidateToUpdate.Gender,
        Birthdate = candidateToUpdate.Birthdate
    };

    return Ok(candidateDTO);
}

[HttpPut]
[Route("{id:long}")]
[Consumes("multipart/form-data")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorDTO), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
[ProducesDefaultResponseType]
public async Task<IActionResult> Update([FromForm] UpdateCompanyRequest companyRequest, [FromRoute] long id)
{
    if (!ModelState.IsValid)
    {
        var errorResponse = new ErrorDTO
        {
            Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
        };

        return BadRequest(errorResponse);
    }
    var companyToUpdate = await _companyRepository.GetCompanyById(id);

    if (companyToUpdate == null)
    {
        return NotFound($"Company With ID {id} Does Not Exist!");
    }

    using MemoryStream photoFileStream = new MemoryStream();

    companyToUpdate.Name = companyRequest.Name ?? companyToUpdate.Name;
    companyToUpdate.Address = companyRequest.Address ?? companyToUpdate.Address;
    companyToUpdate.Description = companyRequest.Description ?? companyToUpdate.Description;
    companyToUpdate.ContactNumber = companyRequest.ContactNumber ?? companyToUpdate.ContactNumber;
    companyToUpdate.Category = companyRequest.Category ?? companyToUpdate.Category;
    if (companyRequest.LogoFile != null)
    {
        await companyRequest.LogoFile.CopyToAsync(photoFileStream);
        companyToUpdate.LogoFile = photoFileStream.ToArray();
        companyToUpdate.LogoFileName = GenerateFileName(ResumePrefix, companyRequest.LogoFile.FileName);
    }

    if (!await _companyRepository.UpdateCompany(companyToUpdate))
    {
        return StatusCode(500, "Internal Error Occured While Updating Company!");
    }

    var companyDTO = new CompanyDTO
    {
        Id = companyToUpdate.Id,
        Name = companyToUpdate.Name,
        Address = companyToUpdate.Address,
        Description = companyToUpdate.Description,
        ContactNumber = companyToUpdate.ContactNumber,
        Category = companyToUpdate.Category
    };

    return Ok(companyDTO);
}
*/