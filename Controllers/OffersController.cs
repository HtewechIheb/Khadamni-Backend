using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_X.Contracts.Requests;
using Project_X.Contracts.Responses;
using Project_X.Models;
using Project_X.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Controllers
{
    [ApiController]
    [Route("/api/offers/")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly ICompanyService _companyService;

        public OffersController(IOfferService offerService, ICompanyService companyService)
        {
            _offerService = offerService;
            _companyService = companyService;
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetAll()
        {
            var offers = await _offerService.GetOffers();
            var offersResponse = new List<OfferResponse>();
            foreach (var offer in offers)
            {
                var offerResponse = new OfferResponse
                {
                    Id = offer.Id,
                    Category = offer.Category,
                    Title = offer.Title,
                    Description = offer.Description,
                    Spots = offer.Spots,
                    Type = offer.Type,
                    ExperienceLowerBound = offer.ExperienceLowerBound,
                    ExperienceUpperBound = offer.ExperienceUpperBound
                };

                offersResponse.Add(offerResponse);
            }
            return Ok(offersResponse);
        }

        [HttpGet]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var offer = await _offerService.GetOfferById(id);

            if (offer == null)
            {
                return NotFound($"Offer With ID {id} Does Not Exist!");
            }

            var offerResponse = new OfferResponse
            {
                Id = offer.Id,
                Category = offer.Category,
                Title = offer.Title,
                Description = offer.Description,
                Spots = offer.Spots,
                Type = offer.Type,
                ExperienceLowerBound = offer.ExperienceLowerBound,
                ExperienceUpperBound = offer.ExperienceUpperBound
            };

            return Ok(offerResponse);
        }

        [HttpPost]
        [Route("")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Add([FromForm] AddOfferRequest offerRequest)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }

            var associatedCompany = await _companyService.GetCompanyById(offerRequest.CompanyId);

            if (associatedCompany == null)
            {
                return NotFound($"Company With Id {offerRequest.CompanyId} Does Not Exist!");
            }

            var offer = new Offer
            {
                Category = offerRequest.Category.ToString(),
                Title = offerRequest.Title,
                Description = offerRequest.Description,
                Spots = offerRequest.Spots,
                Type = offerRequest.Type.ToString(),
                ExperienceLowerBound = offerRequest.ExperienceLowerBound,
                ExperienceUpperBound = offerRequest.ExperienceUpperBound,
                Company = associatedCompany
            };

            if (!await _offerService.AddOffer(offer))
            {
                return StatusCode(500, "Internal Error Occured While Adding Offer!");
            }

            var locationUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/offers/{offer.Id}";
            var offerResponse = new OfferResponse
            {
                Id = offer.Id,
                Category = offer.Category,
                Title = offer.Title,
                Description = offer.Description,
                Spots = offer.Spots,
                Type = offer.Type,
                ExperienceLowerBound = offer.ExperienceLowerBound,
                ExperienceUpperBound = offer.ExperienceUpperBound
            };

            return Created(locationUrl, offerResponse);
        }

        [HttpPut]
        [Route("{id:long}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update([FromForm] UpdateOfferRequest offerRequest, [FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorResponse);
            }

            var offerToUpdate = await _offerService.GetOfferById(id);

            if (offerToUpdate == null)
            {
                return NotFound($"Offer With ID {id} Does Not Exist!");
            }

            if (offerRequest.CompanyId.HasValue)
            {
                var associatedCompany = await _companyService.GetCompanyById(offerRequest.CompanyId.Value);

                if (associatedCompany == null)
                {
                    return NotFound($"Company With Id {offerRequest.CompanyId} Does Not Exist!");
                }

                offerToUpdate.Company = associatedCompany;
            }

            offerToUpdate.Category = offerRequest.Category.ToString() ?? offerToUpdate.Category;
            offerToUpdate.Title = offerRequest.Title ?? offerToUpdate.Title;
            offerToUpdate.Description = offerRequest.Description ?? offerToUpdate.Description;
            offerToUpdate.Spots = offerRequest.Spots ?? offerToUpdate.Spots;
            offerToUpdate.Type = offerRequest.Type.ToString() ?? offerToUpdate.Type;
            offerToUpdate.ExperienceLowerBound = offerRequest.ExperienceLowerBound ?? offerToUpdate.ExperienceLowerBound;
            offerToUpdate.ExperienceUpperBound = offerRequest.ExperienceUpperBound ?? offerToUpdate.ExperienceUpperBound;

            if (!await _offerService.UpdateOffer(offerToUpdate))
            {
                return StatusCode(500, "Internal Error Occured While Updating Offer!");
            }

            var offerResponse = new OfferResponse
            {
                Id = offerToUpdate.Id,
                Category = offerToUpdate.Category,
                Title = offerToUpdate.Title,
                Description = offerToUpdate.Description,
                Spots = offerToUpdate.Spots,
                Type = offerToUpdate.Type,
                ExperienceLowerBound = offerToUpdate.ExperienceLowerBound,
                ExperienceUpperBound = offerToUpdate.ExperienceUpperBound
            };

            return Ok(offerResponse);
        }

        [HttpDelete]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var offerToDelete = await _offerService.GetOfferById(id);

            if (offerToDelete == null)
            {
                return NotFound($"Offer With ID {id} Does Not Exist!");
            }

            if (!await _offerService.DeleteOffer(id))
            {
                return StatusCode(500, "Internal Error Occured While Deleting Offer!");
            }

            return NoContent();
        }
    }
}
