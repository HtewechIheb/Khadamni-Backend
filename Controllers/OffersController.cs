using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_X.Models;
using Project_X.Services;
using static Project_X.Shared.GlobalConstants;
using static Project_X.Shared.GlobalMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Project_X.Contracts.Responses;
using Project_X.Contracts.Requests;
using Project_X.Contracts.Enumerations;

namespace Project_X.Controllers
{
    [ApiController]
    [Route("/api/offers")]
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
        public async Task<IActionResult> GetAll()
        {
            var offers = await _offerService.GetOffers();
            var offerResponses = new List<OfferResponse>();
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

                offerResponses.Add(offerResponse);
            }
            return Ok(offerResponses);
        }

        [HttpGet]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var offer = await _offerService.GetOfferById(id);
            if (offer != null)
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

                return Ok(offerResponse);
            }
            else
            {
                return BadRequest($"Offer With ID {id} Does Not Exist!");
            }
        }

        [HttpPost]
        [Route("")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Add([FromForm] AddOfferRequest offerRequest)
        {
            if (ModelState.IsValid)
            {
                var associatedCompany = await _companyService.GetCompanyById(offerRequest.CompanyId);
                if (associatedCompany != null)
                {
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

                    if (await _offerService.AddOffer(offer))
                    {
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
                    else
                    {
                        return BadRequest("Internal Error Occured While Adding Offer!");
                    }
                }
                else
                {
                    return NotFound($"Company With Id {offerRequest.CompanyId} Does Not Exist!");
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

        [HttpPut]
        [Route("{id:long}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update([FromForm] UpdateOfferRequest offerRequest, [FromRoute] long id)
        {
            if (ModelState.IsValid)
            {
                var offerToUpdate = await _offerService.GetOfferById(id);
                var associatedCompany = await _companyService.GetCompanyById(offerRequest.CompanyId);

                if (offerToUpdate != null)
                {
                    using (MemoryStream photoFileStream = new MemoryStream())
                    {
                        offerToUpdate.Category = offerRequest.Category.ToString() ?? offerToUpdate.Category;
                        offerToUpdate.Title = offerRequest.Title ?? offerToUpdate.Title;
                        offerToUpdate.Description = offerRequest.Description ?? offerToUpdate.Description;
                        offerToUpdate.Spots = offerRequest.Spots ?? offerToUpdate.Spots;
                        offerToUpdate.Type = offerRequest.Type.ToString() ?? offerToUpdate.Type;
                        offerToUpdate.ExperienceLowerBound = offerRequest.ExperienceLowerBound ?? offerToUpdate.ExperienceLowerBound;
                        offerToUpdate.ExperienceUpperBound = offerRequest.ExperienceUpperBound ?? offerToUpdate.ExperienceUpperBound;
                        offerToUpdate.Company = associatedCompany ?? offerToUpdate.Company;
                    }


                    if (await _offerService.UpdateOffer(offerToUpdate))
                    {
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
                    else
                    {
                        return BadRequest("Internal Error Occured While Updating Offer!");
                    }
                }
                else
                {
                    return BadRequest($"Offer With ID {id} Does Not Exist!");
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

        [HttpDelete]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var offerToDelete = await _offerService.GetOfferById(id);
            if (offerToDelete != null)
            {
                if (await _offerService.DeleteOffer(id))
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Internal Error Occured While Deleting Offer!");
                }
            }
            else
            {
                return BadRequest($"Offer With ID {id} Does Not Exist!");
            }
        }
    }
}
