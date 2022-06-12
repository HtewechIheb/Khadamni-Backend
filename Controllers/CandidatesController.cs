using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Project_X.DTO.Responses;
using Project_X.Repositories.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Project_X.Controllers
{
    [ApiController]
    [Route("/api/candidates/")]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateRepository _candidateRepository;

        public CandidatesController(ICandidateRepository candidateService)
        {
            _candidateRepository = candidateService;
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetAll()
        {
            var candidates = await _candidateRepository.GetCandidates();
            var candidateDTOs = new List<CandidateDTO>();
            foreach (var candidate in candidates)
            {
                var candidateDTO = new CandidateDTO
                {
                    Id = candidate.Id,
                    FirstName = candidate.FirstName,
                    LastName = candidate.LastName,
                    Address = candidate.Address,
                    Gender = candidate.Gender,
                    Birthdate = candidate.Birthdate
                };

                candidateDTOs.Add(candidateDTO);
            }
            return Ok(candidateDTOs);
        }

        [HttpGet]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var candidate = await _candidateRepository.GetCandidateById(id);

            if (candidate == null)
            {
                return NotFound($"Candidate With ID {id} Does Not Exist!");
            }

            var candidateDTO = new CandidateDTO
            {
                Id = candidate.Id,
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Address = candidate.Address,
                Gender = candidate.Gender,
                Birthdate = candidate.Birthdate
            };

            return Ok(candidateDTO);
        }

        [HttpGet]
        [Route("{id:long}/{resource}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> DownloadResource(long id, string resource)
        {
            var candidate = await _candidateRepository.GetCandidateById(id);

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

        [HttpDelete]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var candidateToDelete = await _candidateRepository.GetCandidateById(id);

            if (candidateToDelete == null)
            {
                return NotFound($"Candidate With ID {id} Does Not Exist!");
            }

            if (!await _candidateRepository.DeleteCandidate(id))
            {
                return StatusCode(500, "Internal Error Occured While Deleting Candidate!");
            }

            return NoContent();
        }
    }
}
