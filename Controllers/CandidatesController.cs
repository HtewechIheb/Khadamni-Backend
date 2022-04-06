using Microsoft.AspNetCore.Mvc;
using Project_X.Contracts.Requests;
using Project_X.Contracts.Responses;
using Project_X.Models;
using Project_X.Services;
using static Project_X.Shared.GlobalConstants;
using static Project_X.Shared.GlobalMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Project_X.Controllers
{
    [ApiController]
    [Route("/api/candidates/")]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateService _candidateService;

        public CandidatesController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetAll()
        {
            var candidates = await _candidateService.GetCandidates();
            var candidatesResponse = new List<CandidateResponse>();
            foreach (var candidate in candidates)
            {
                var candidateResponse = new CandidateResponse
                {
                    Id = candidate.Id,
                    FirstName = candidate.FirstName,
                    LastName = candidate.LastName,
                    Address = candidate.Address,
                    Gender = candidate.Gender,
                    Birthdate = candidate.Birthdate
                };

                candidatesResponse.Add(candidateResponse);
            }
            return Ok(candidatesResponse);
        }

        [HttpGet]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var candidate = await _candidateService.GetCandidateById(id);
            
            if (candidate == null)
            {
                return NotFound($"Candidate With ID {id} Does Not Exist!");
            }
            
            var candidateResponse = new CandidateResponse
            {
                Id = candidate.Id,
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Address = candidate.Address,
                Gender = candidate.Gender,
                Birthdate = candidate.Birthdate
            };

            return Ok(candidateResponse);
        }

        [HttpPost]
        [Route("")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Add([FromForm] AddCandidateRequest candidateRequest)
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
            await candidateRequest.ResumeFile.CopyToAsync(resumeFileStream);
            await candidateRequest.PhotoFile.CopyToAsync(photoFileStream);

            var candidate = new Candidate
            {
                FirstName = candidateRequest.FirstName,
                LastName = candidateRequest.LastName,
                Address = candidateRequest.Address,
                Gender = candidateRequest.Gender,
                ResumeFile = resumeFileStream.ToArray(),
                ResumeFileName = GenerateFileName(ResumePrefix, candidateRequest.ResumeFile.FileName),
                PhotoFile = photoFileStream.ToArray(),
                PhotoFileName = GenerateFileName(PhotoPrefix, candidateRequest.PhotoFile.FileName),
                Birthdate = candidateRequest.Birthdate
            };

            if (!await _candidateService.AddCandidate(candidate))
            {
                return StatusCode(500, "Internal Error Occured While Adding Candidate!");
            }

            var locationUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/candidates/{candidate.Id}";
            var candidateResponse = new CandidateResponse
            {
                Id = candidate.Id,
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Address = candidate.Address,
                Gender = candidate.Gender,
                Birthdate = candidate.Birthdate
            };

            return Created(locationUrl, candidateResponse);
        }

        [HttpGet]
        [Route("{id:long}/{resource}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> DownloadResource(long id, string resource)
        {
            var candidate = await _candidateService.GetCandidateById(id);
           
            if (candidate == null)
            {
                return NotFound($"Candidate With ID {id} Does Not Exist!");
            }
            
            MemoryStream byteStream;
            switch (resource)
            {
                case "resume":
                    byteStream = new MemoryStream(candidate.ResumeFile);
                    return new FileStreamResult(byteStream, new MediaTypeHeaderValue("application/octet-stream"))
                    {
                        FileDownloadName = candidate.ResumeFileName
                    };
                case "photo":
                    byteStream = new MemoryStream(candidate.PhotoFile);
                    return new FileStreamResult(byteStream, new MediaTypeHeaderValue("application/octet-stream"))
                    {
                        FileDownloadName = candidate.PhotoFileName
                    };
                default:
                    return NotFound($"Candidate Resource {resource} Does Not Exist!");
            }
        }

        [HttpPut]
        [Route("{id:long}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update([FromForm] UpdateCandidateRequest candidateRequest, [FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }

            var candidateToUpdate = await _candidateService.GetCandidateById(id);

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

            if (!await _candidateService.UpdateCandidate(candidateToUpdate))
            {
                return StatusCode(500, "Internal Error Occured While Updating Candidate!");
            }
                    
            var candidateResponse = new CandidateResponse
            {
                Id = candidateToUpdate.Id,
                FirstName = candidateToUpdate.FirstName,
                LastName = candidateToUpdate.LastName,
                Address = candidateToUpdate.Address,
                Gender = candidateToUpdate.Gender,
                Birthdate = candidateToUpdate.Birthdate
            };

            return Ok(candidateResponse);
        }

        [HttpDelete]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var candidateToDelete = await _candidateService.GetCandidateById(id);
            
            if (candidateToDelete == null)
            {
                return NotFound($"Candidate With ID {id} Does Not Exist!");
            }

            if (!await _candidateService.DeleteCandidate(id))
            {
                return StatusCode(500, "Internal Error Occured While Deleting Candidate!");
            }

            return NoContent();
        }
    }
}
