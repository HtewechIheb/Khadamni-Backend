using Microsoft.AspNetCore.Mvc;
using Project_X.Contracts.Requests;
using Project_X.Contracts.Responses;
using Project_X.Models;
using Project_X.Services;
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
    [Route("/api/candidates")]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateService _candidateService;

        public CandidatesController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll()
        {
            var candidates = await _candidateService.GetCandidates();
            var candidateResponses = new List<CandidateResponse>();
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

                candidateResponses.Add(candidateResponse);
            }
            return Ok(candidateResponses);
        }

        [HttpGet]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var candidate = await _candidateService.GetCandidateById(id);
            if (candidate != null)
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

                return Ok(candidateResponse);
            }
            else
            {
                return BadRequest($"Candidate With ID {id} Does Not Exist!");
            }
        }

        [HttpPost]
        [Route("")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Add([FromForm] AddCandidateRequest candidateRequest)
        {
            if (ModelState.IsValid)
            {
                using (MemoryStream resumeFileStream = new MemoryStream(), photoFileStream = new MemoryStream())
                {
                    await candidateRequest.ResumeFile.CopyToAsync(resumeFileStream);

                    var candidate = new Candidate
                    {
                        FirstName = candidateRequest.FirstName,
                        LastName = candidateRequest.LastName,
                        Address = candidateRequest.Address,
                        Gender = candidateRequest.Gender,
                        ResumeFile = resumeFileStream.ToArray(),
                        ResumeFileName = candidateRequest.ResumeFile.FileName,
                        PhotoFile = photoFileStream.ToArray(),
                        PhotoFileName = candidateRequest.PhotoFile.FileName,
                        Birthdate = candidateRequest.Birthdate
                    };

                    if (await _candidateService.AddCandidate(candidate))
                    {
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
                    else
                    {
                        return BadRequest("Internal Error Occured While Adding Candidate!");
                    }
                }
            }
            else
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }
        }

        [HttpGet]
        [Route("{id:long}/resume")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> DownloadResume(long id)
        {
            var candidate = await _candidateService.GetCandidateById(id);
            if(candidate != null)
            {
                var candidateResume = new MemoryStream(candidate.ResumeFile);
                return new FileStreamResult(candidateResume, new MediaTypeHeaderValue("application/octet-stream"))
                {  
                    FileDownloadName = candidate.ResumeFileName
                };
            }
            else
            {
                return NotFound("Candidate With ID {id} Does Not Exist!");
            }
        }

        [HttpPut]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update([FromBody] UpdateCandidateRequest candidateRequest, [FromRoute] long id)
        {
            var candidateToUpdate = await _candidateService.GetCandidateById(id);
            if (candidateToUpdate != null)
            {
                using (MemoryStream resumeFileStream = new MemoryStream(), photoFileStream = new MemoryStream())
                {
                    await candidateRequest.ResumeFile.CopyToAsync(resumeFileStream);

                    candidateToUpdate.FirstName = candidateRequest.FirstName;
                    candidateToUpdate.LastName = candidateRequest.LastName;
                    candidateToUpdate.Address = candidateRequest.Address;
                    candidateToUpdate.Gender = candidateRequest.Gender;
                    candidateToUpdate.ResumeFile = resumeFileStream.ToArray();
                    candidateToUpdate.ResumeFileName = candidateRequest.ResumeFile.FileName;
                    candidateToUpdate.PhotoFile = photoFileStream.ToArray();
                    candidateToUpdate.PhotoFileName = candidateRequest.PhotoFile.FileName;
                    candidateToUpdate.Birthdate = candidateRequest.Birthdate;
                }

                if (await _candidateService.UpdateCandidate(candidateToUpdate))
                {
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
                else
                {
                    return BadRequest("Internal Error Occured While Updating Candidate!");
                }
            }
            else
            {
                return NotFound($"Candidate With ID {id} Does Not Exist!");
            }
        }

        [HttpDelete]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var candidateToDelete = await _candidateService.GetCandidateById(id);
            if (candidateToDelete != null)
            {
                if (await _candidateService.DeleteCandidate(id))
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Internal Error Occured While Deleting Candidate!");
                }
            }
            else
            {
                return BadRequest($"Candidate With ID {id} Does Not Exist!");
            }
        }
    }
}
