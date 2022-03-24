using Microsoft.AspNetCore.Http;
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

namespace Project_X.Controllers
{
    [ApiController]
    [Route("/api")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly ICandidateService _candidateService;
        private readonly IOfferService _offerService;

        public ApplicationsController(IApplicationService applicationService, ICandidateService candidateService, IOfferService offerService)
        {
            _applicationService = applicationService;
            _candidateService = candidateService;
            _offerService = offerService;
        }

        [HttpGet]
        [Route("applications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetAll()
        {
            var applications = await _applicationService.GetApplications();
            var applicationsResponse = new List<ApplicationResponse>();

            foreach (var application in applications)
            {
                var applicationResponse = new ApplicationResponse
                {
                    Date = application.Date,
                    Status = application.Status,
                    CandidateId = application.CandidateId,
                    OfferId = application.OfferId
                };

                applicationsResponse.Add(applicationResponse);
            }
            return Ok(applicationsResponse);
        }

        [HttpGet]
        [Route("applications;candidate={candidateId:long};offer={offerId:long}")]
        [Route("applications;offer={offerId:long};candidate={candidateId:long}")]
        [Route("applications;offer={offerId:long}")]
        [Route("applications;candidate={candidateId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long? candidateId, [FromRoute] long? offerId)
        {
            Candidate associatedCandidate;
            Offer associatedOffer;

            if (candidateId.HasValue && offerId.HasValue)
            {
                associatedCandidate = await _candidateService.GetCandidateById(candidateId.Value);
                associatedOffer = await _offerService.GetOfferById(offerId.Value);

                if(associatedCandidate == null)
                {
                    return NotFound($"Candidate With Id {candidateId.Value} Does Not Exist!");
                }

                if (associatedOffer == null)
                {
                    return NotFound($"Offer With Id {offerId.Value} Does Not Exist!");
                }

                var application = await _applicationService.GetApplicationByCandidateIdAndOfferId(candidateId.Value, offerId.Value);

                if(application == null)
                {
                    return NotFound($"Application With Candidate Id {candidateId.Value} And Offer Id {offerId.Value} Does Not Exist!");
                }

                var applicationResponse = new ApplicationResponse
                {
                    Date = application.Date,
                    Status = application.Status,
                    CandidateId = application.CandidateId,
                    OfferId = application.OfferId
                };

                return Ok(applicationResponse);
            }

            IEnumerable<Application> applications = new List<Application>();
            var applicationsResponse = new List<ApplicationResponse>();

            if (candidateId.HasValue)
            {
                associatedCandidate = await _candidateService.GetCandidateById(candidateId.Value);

                if (associatedCandidate == null)
                {
                    return NotFound($"Candidate With Id {candidateId.Value} Does Not Exist!");
                }

                applications = await _applicationService.GetApplicationsByCandidateId(candidateId.Value);
            }
            else if (offerId.HasValue)
            {
                associatedOffer = await _offerService.GetOfferById(offerId.Value);

                if (associatedOffer == null)
                {
                    return NotFound($"Offer With Id {offerId.Value} Does Not Exist!");
                }

                applications = await _applicationService.GetApplicationsByOfferId(offerId.Value);
            }

            foreach (var application in applications)
            {
                var applicationResponse = new ApplicationResponse
                {
                    Date = application.Date,
                    Status = application.Status,
                    CandidateId = application.CandidateId,
                    OfferId = application.OfferId
                };

                applicationsResponse.Add(applicationResponse);
            }
            return Ok(applicationsResponse);
        }

        [HttpPost]
        [Route("applications")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Add([FromForm] AddApplicationRequest applicationRequest)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }

            var associatedCandidate = await _candidateService.GetCandidateById(applicationRequest.CandidateId);
            var associatedOffer = await _offerService.GetOfferById(applicationRequest.OfferId);

            if (associatedCandidate == null)
            {
                return NotFound($"Candidate With Id {applicationRequest.CandidateId} Does Not Exist!");
            }

            if (associatedOffer == null)
            {
                return NotFound($"Offer With Id {applicationRequest.OfferId} Does Not Exist!");
            }

            var application = new Application
            {
                Date = applicationRequest.Date,
                Status = applicationRequest.Status.ToString(),
                Candidate = associatedCandidate,
                Offer = associatedOffer
            };

            if (!await _applicationService.AddApplication(application))
            {
                return StatusCode(500, "Internal Error Occured While Adding Application!");
            }

            var locationUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/applications;candidate={application.CandidateId};offer={application.OfferId}";
            var applicationResponse = new ApplicationResponse
            {
                Date = application.Date,
                Status = application.Status,
                CandidateId = application.CandidateId,
                OfferId = application.OfferId
            };

            return Created(locationUrl, applicationResponse);
        }

        [HttpPut]
        [Route("applications;candidate={candidateId:long};offer={offerId:long}")]
        [Route("applications;offer={offerId:long};candidate={candidateId:long}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update([FromForm] UpdateApplicationRequest applicationRequest, [FromRoute] long candidateId, [FromRoute] long offerId)
        {
            if (ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }

            var applicationToUpdate = await _applicationService.GetApplicationByCandidateIdAndOfferId(candidateId, offerId);

            if (applicationToUpdate == null)
            {
                return NotFound($"Application With Candidate Id {candidateId} And Offer Id {offerId} Does Not Exist!");

            }

            if (applicationRequest.CandidateId.HasValue)
            {
                var associatedCandidate = await _candidateService.GetCandidateById(applicationRequest.CandidateId.Value);

                if (associatedCandidate == null)
                {
                    return NotFound($"Candidate With Id {candidateId} Does Not Exist!");
                }

                applicationToUpdate.Candidate = associatedCandidate;
            }

            if (applicationRequest.OfferId.HasValue)
            {
                var associatedOffer = await _offerService.GetOfferById(applicationRequest.OfferId.Value);

                if (associatedOffer == null)
                {
                    return NotFound($"Offer With Id {offerId} Does Not Exist!");
                }

                applicationToUpdate.Offer = associatedOffer;
            }

            applicationToUpdate.Date = applicationRequest.Date ?? applicationToUpdate.Date;
            applicationToUpdate.Status = applicationRequest.Status.ToString() ?? applicationToUpdate.Status;

            if (!await _applicationService.UpdateApplication(applicationToUpdate))
            {
                return StatusCode(500, "Internal Error Occured While Updating Application!");
            }

            var applicationResponse = new ApplicationResponse
            {
                Date = applicationToUpdate.Date,
                Status = applicationToUpdate.Status,
                CandidateId = applicationToUpdate.CandidateId,
                OfferId = applicationToUpdate.OfferId
            };

            return Ok(applicationResponse);
        }

        [HttpDelete]
        [Route("applications;candidate={candidateId:long};offer={offerId:long}")]
        [Route("applications;offer={offerId:long};candidate={candidateId:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long candidateId, [FromRoute] long offerId)
        {
            var applicationToDelete = await _applicationService.GetApplicationByCandidateIdAndOfferId(candidateId, offerId);
            
            if (applicationToDelete == null)
            {
                return NotFound($"Application With Candidate Id {candidateId} And Offer Id {offerId} Does Not Exist!");
            }

            if (!await _applicationService.DeleteApplication(candidateId, offerId))
            {
                return StatusCode(500, "Internal Error Occured While Deleting Application!");
            }

            return NoContent();
        }
    }
}
