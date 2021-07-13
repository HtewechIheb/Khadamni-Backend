using Microsoft.AspNetCore.Mvc;
using Project_X.Contracts.Requests;
using Project_X.Contracts.Responses;
using Project_X.Models;
using Project_X.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Project_X.Controllers
{

    [ApiController]
    [Route("/api/companies/")]
    public class CompaniesController: ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetCompanies();
            var companyResponses = new List<CompanyResponse>();
            foreach(var company in companies)
            {
                var companyResponse = new CompanyResponse
                {
                    Id = company.Id,
                    Name = company.Name,
                    Address = company.Address,
                    Description = company.Description
                };

                companyResponses.Add(companyResponse);
            }
            return Ok(companyResponses);
        }

        [HttpGet]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var company = await _companyService.GetCompanyById(id);
            if(company != null)
            {
                var companyResponse = new CompanyResponse
                {
                    Id = company.Id,
                    Name = company.Name,
                    Address = company.Address,
                    Description = company.Description
                };

                return Ok(companyResponse);
            }
            else
            {
                return BadRequest($"Company With ID {id} Does Not Exist!");
            }
        }

        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Add([FromBody] AddCompanyRequest companyRequest)
        {
            var company = new Company
            {
                Name = companyRequest.Name,
                Address = companyRequest.Address,
                Description = companyRequest.Description
            };

            if (ModelState.IsValid)
            {
                if (await _companyService.AddCompany(company))
                {
                    var locationUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/companies/{company.Id}";
                    var companyResponse = new CompanyResponse
                    {
                        Id = company.Id,
                        Name = company.Name,
                        Address = company.Address,
                        Description = company.Description
                    };

                    return Created(locationUrl, companyResponse);
                }
                else
                {
                    return BadRequest("Internal Error Occured While Adding Company!");
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update([FromBody] UpdateCompanyRequest companyRequest, [FromRoute] long id)
        {
            var companyToUpdate = await _companyService.GetCompanyById(id);
            if(companyToUpdate != null)
            {
                companyToUpdate.Name = companyRequest.Name;
                companyToUpdate.Address = companyRequest.Address;
                companyToUpdate.Description = companyRequest.Description;

                if(await _companyService.UpdateCompany(companyToUpdate))
                {
                    var companyResponse = new CompanyResponse
                    {
                        Id = companyToUpdate.Id,
                        Name = companyToUpdate.Name,
                        Address = companyToUpdate.Address,
                        Description = companyToUpdate.Description
                    };

                    return Ok(companyResponse);
                }
                else
                {
                    return BadRequest("Internal Error Occured While Updating Company!");
                }
            }
            else
            {
                return BadRequest($"Company With ID {id} Does Not Exist!");
            }
        }

        [HttpDelete]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var companyToDelete = await _companyService.GetCompanyById(id);
            if (companyToDelete != null)
            {
                if (await _companyService.DeleteCompany(id))
                {
                    return NoContent();
                }
                else
                {
                    return BadRequest("Internal Error Occured While Deleting Company!");
                }
            }
            else
            {
                return BadRequest($"Company With ID {id} Does Not Exist!");
            }
        }
    }
}
