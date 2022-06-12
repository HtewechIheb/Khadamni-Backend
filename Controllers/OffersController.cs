using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_X.DTO.Requests;
using Project_X.DTO.Responses;
using Project_X.Models;
using Project_X.Repositories.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Controllers
{
    [ApiController]
    [Route("/api/offers/")]
    public class OffersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOfferRepository _offerRepository;
        private readonly ICompanyRepository _companyRepository;

        public OffersController(UserManager<AppUser> userManager, IOfferRepository offerService, ICompanyRepository companyService)
        {
            _userManager = userManager;
            _offerRepository = offerService;
            _companyRepository = companyService;
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetAll()
        {
            var offers = await _offerRepository.GetOffers();
            var offerDTOs = new List<OfferDTO>();
            foreach (var offer in offers)
            {
                var offerDTO = new OfferDTO
                {
                    Id = offer.Id,
                    Industry = offer.Industry,
                    Title = offer.Title,
                    Description = offer.Description,
                    Spots = offer.Spots,
                    Salary = offer.Salary,
                    Degree = offer.Degree,
                    Gender = offer.Gender,
                    Skills = offer.Skills.Split("/"),
                    Type = offer.Type,
                    MinimumExperience = offer.MinimumExperience,
                    RecommendedExperience = offer.RecommendedExperience
                };

                offerDTOs.Add(offerDTO);
            }
            return Ok(offerDTOs);
        }

        [HttpGet]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var offer = await _offerRepository.GetOfferById(id);

            if (offer == null)
            {
                return NotFound($"Offer With ID {id} Does Not Exist!");
            }

            var offerDTO = new OfferDTO
            {
                Id = offer.Id,
                Industry = offer.Industry,
                Title = offer.Title,
                Description = offer.Description,
                Spots = offer.Spots,
                Salary = offer.Salary,
                Degree = offer.Degree,
                Gender = offer.Gender,
                Skills = offer.Skills.Split("/"),
                Type = offer.Type,
                MinimumExperience = offer.MinimumExperience,
                RecommendedExperience = offer.RecommendedExperience
            };

            return Ok(offerDTO);
        }

        [HttpGet]
        [Route("companies/{id}/offers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetByCompany([FromRoute] long id)
        {
            var company = await _companyRepository.GetCompanyById(id);

            if(company == null)
            {
                return NotFound($"Company With Id {id} Does Not Exist!");
            }

            var offers = company.Offers.ToList();
            var offersDTO = new List<OfferDTO>();
            foreach(var offer in offers)
            {
                var offerDTO = new OfferDTO
                {
                    Id = offer.Id,
                    Industry = offer.Industry,
                    Title = offer.Title,
                    Description = offer.Description,
                    Spots = offer.Spots,
                    Salary = offer.Salary,
                    Degree = offer.Degree,
                    Gender = offer.Gender,
                    Skills = offer.Skills.Split("/"),
                    Type = offer.Type,
                    MinimumExperience = offer.MinimumExperience,
                    RecommendedExperience = offer.RecommendedExperience
                };

                offersDTO.Add(offerDTO);
            }

            return Ok(offersDTO);
        }

        [HttpPost]
        [Route("")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Add([FromForm] AddOfferDTO addOfferDTO)
        {
            if (!ModelState.IsValid)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorDTO);
            }

            var loggedInUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var associatedCompany = loggedInUser.Company;

            var offer = new Offer
            {
                Industry = addOfferDTO.Industry,
                Title = addOfferDTO.Title,
                Description = addOfferDTO.Description,
                Spots = addOfferDTO.Spots,
                Salary = addOfferDTO.Salary,
                Degree = addOfferDTO.Degree,
                Gender = addOfferDTO.Gender,
                Skills = string.Join("/", addOfferDTO.Skills),
                Type = addOfferDTO.Type,
                MinimumExperience = addOfferDTO.MinimumExperience,
                RecommendedExperience = addOfferDTO.RecommendedExperience,
                Company = associatedCompany
            };

            if (!await _offerRepository.AddOffer(offer))
            {
                return StatusCode(500, "Internal Error Occured While Adding Offer!");
            }

            var locationUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/offers/{offer.Id}";
            var offerDTO = new OfferDTO
            {
                Id = offer.Id,
                Industry = offer.Industry,
                Title = offer.Title,
                Description = offer.Description,
                Spots = offer.Spots,
                Salary = offer.Salary,
                Degree = offer.Degree,
                Gender = offer.Gender,
                Skills = offer.Skills.Split("/"),
                Type = offer.Type,
                MinimumExperience = offer.MinimumExperience,
                RecommendedExperience = offer.RecommendedExperience,
                CompanyId = offer.Company.Id
            };

            return Created(locationUrl, offerDTO);
        }

        [HttpPut]
        [Route("{id:long}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update([FromForm] UpdateOfferDTO updateOfferDTO, [FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                var errorDTO = new ErrorDTO
                {
                    Errors = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))
                };

                return BadRequest(errorDTO);
            }

            var offerToUpdate = await _offerRepository.GetOfferById(id);

            if (offerToUpdate == null)
            {
                return NotFound($"Offer With ID {id} Does Not Exist!");
            }

            offerToUpdate.Industry = updateOfferDTO.Industry.ToString() ?? offerToUpdate.Industry;
            offerToUpdate.Title = updateOfferDTO.Title ?? offerToUpdate.Title;
            offerToUpdate.Description = updateOfferDTO.Description ?? offerToUpdate.Description;
            offerToUpdate.Spots = updateOfferDTO.Spots ?? offerToUpdate.Spots;
            offerToUpdate.Salary = updateOfferDTO.Salary ?? offerToUpdate.Salary;
            offerToUpdate.Degree = updateOfferDTO.Degree ?? offerToUpdate.Degree;
            offerToUpdate.Gender = updateOfferDTO.Gender ?? offerToUpdate.Gender;
            offerToUpdate.Skills = updateOfferDTO.Skills is null ? string.Join("/", updateOfferDTO.Skills) : offerToUpdate.Skills;
            offerToUpdate.Type = updateOfferDTO.Type.ToString() ?? offerToUpdate.Type;
            offerToUpdate.MinimumExperience = updateOfferDTO.MinimumExperience ?? offerToUpdate.MinimumExperience;
            offerToUpdate.RecommendedExperience = updateOfferDTO.RecommendedExperience ?? offerToUpdate.RecommendedExperience;

            if (!await _offerRepository.UpdateOffer(offerToUpdate))
            {
                return StatusCode(500, "Internal Error Occured While Updating Offer!");
            }

            var offerDTO = new OfferDTO
            {
                Id = offerToUpdate.Id,
                Industry = offerToUpdate.Industry,
                Title = offerToUpdate.Title,
                Description = offerToUpdate.Description,
                Spots = offerToUpdate.Spots,
                Salary = offerToUpdate.Salary,
                Degree = offerToUpdate.Degree,
                Gender = offerToUpdate.Gender,
                Skills = offerToUpdate.Skills.Split("/"),
                Type = offerToUpdate.Type,
                MinimumExperience = offerToUpdate.MinimumExperience,
                RecommendedExperience = offerToUpdate.RecommendedExperience
            };

            return Ok(offerDTO);
        }

        [HttpDelete]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var offerToDelete = await _offerRepository.GetOfferById(id);

            if (offerToDelete == null)
            {
                return NotFound($"Offer With ID {id} Does Not Exist!");
            }

            if (!await _offerRepository.DeleteOffer(id))
            {
                return StatusCode(500, "Internal Error Occured While Deleting Offer!");
            }

            return NoContent();
        }
    }
}
