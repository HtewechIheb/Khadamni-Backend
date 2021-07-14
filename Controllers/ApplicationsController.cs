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
    [Route("/api/applications/")]
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
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetAll()
        {
            var applications = await _applicationService.GetApplications();
            var applicationResponses = new List<ApplicationResponse>();

            foreach (var application in applications)
            {
                var applicationResponse = new ApplicationResponse
                {
                    Date = application.Date,
                    Status = application.Status,
                    CandidateId = application.CandidateId,
                    OfferId = application.OfferId
                };

                applicationResponses.Add(applicationResponse);
            }
            return Ok(applicationResponses);
        }

        [HttpGet]
        [Route(";candidate={candidateId:long};offer={offerId:long}")]
        [Route(";offer={offerId:long};candidate={candidateId:long}")]
        [Route(";offer={offerId:long}")]
        [Route(";candidate={candidateId:long}")]
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
                    return NotFound($"Candidate With Id {candidateId.Value} And Offer With Id {offerId.Value} Do Not Exist!");
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
            var applicationResponses = new List<ApplicationResponse>();

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

                applicationResponses.Add(applicationResponse);
            }
            return Ok(applicationResponses);
        }

        [HttpPost]
        [Route("")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Add([FromForm] AddApplicationRequest applicationRequest)
        {
            if (ModelState.IsValid)
            {
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
                    Status = applicationRequest.Status,
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
            else
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }
        }

        [HttpPut]
        [Route(";candidate={candidateId:long};offer={offerId:long}")]
        [Route(";offer={offerId:long};candidate={candidateId:long}")]
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
                var applicationToUpdate = await _applicationService.GetApplicationByCandidateIdAndOfferId(candidateId, offerId);
                var associatedCandidate = await _candidateService.GetCandidateById(applicationRequest.CandidateId);
                var associatedOffer = await _offerService.GetOfferById(applicationRequest.OfferId);

                if(associatedCandidate == null)
                {
                    return NotFound($"Candidate With Id {candidateId} Does Not Exist!");
                }

                if(associatedOffer == null)
                {
                    return NotFound($"Offer With Id {offerId} Does Not Exist!");
                }

                if (applicationToUpdate == null)
                {
                    return NotFound($"Application With Candidate Id {candidateId} And Offer Id {offerId} Does Not Exist!");

                }

                applicationToUpdate.Date = applicationRequest.Date ?? applicationToUpdate.Date;
                applicationToUpdate.Status = applicationRequest.Status ?? applicationToUpdate.Status;

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
            else
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }
        }

        [HttpDelete]
        [Route(";candidate={candidateId:long};offer={offerId:long}")]
        [Route(";offer={offerId:long};candidate={candidateId:long}")]
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
