using Microsoft.AspNetCore.Mvc;
using Project_X.Contracts.Requests;
using Project_X.Contracts.Responses;
using Project_X.Models;
using Project_X.Services;
using static Project_X.Shared.GlobalConstants;
using static Project_X.Shared.GlobalMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Net.Http.Headers;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
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
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var company = await _companyService.GetCompanyById(id);
            
            if (company == null)
            {
                return NotFound($"Company With ID {id} Does Not Exist!");
            }
                
            var companyResponse = new CompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                Address = company.Address,
                Description = company.Description
            };

            return Ok(companyResponse);
        }

        [HttpPost]
        [Route("")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Add([FromForm] AddCompanyRequest companyRequest)
        {
            if (ModelState.IsValid)
            {
                using(MemoryStream photoFileStream = new MemoryStream())
                {
                    await companyRequest.PhotoFile.CopyToAsync(photoFileStream);

                    var company = new Company
                    {
                        Name = companyRequest.Name,
                        Address = companyRequest.Address,
                        Description = companyRequest.Description,
                        PhotoFile = photoFileStream.ToArray(),
                        PhotoFileName = GenerateFileName(ResumePrefix, companyRequest.PhotoFile.FileName)
                    };

                    if (!await _companyService.AddCompany(company))
                    {
                        return StatusCode(500, "Internal Error Occured While Adding Company!");
                    }
                    
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
        [Route("{id:long}/photo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> DownloadPhoto(long id)
        {
            var company = await _companyService.GetCompanyById(id);

            if (company == null)
            {
                return NotFound($"Company With ID {id} Does Not Exist!");
            }
                
            var byteStream = new MemoryStream(company.PhotoFile);
            return new FileStreamResult(byteStream, new MediaTypeHeaderValue("application/octet-stream"))
            {
                FileDownloadName = company.PhotoFileName
            };
        }

        [HttpPut]
        [Route("{id:long}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update([FromForm] UpdateCompanyRequest companyRequest, [FromRoute] long id)
        {
            if (ModelState.IsValid)
            {
                var companyToUpdate = await _companyService.GetCompanyById(id);
                
                if (companyToUpdate == null)
                {
                    return NotFound($"Company With ID {id} Does Not Exist!");
                }
                
                using (MemoryStream photoFileStream = new MemoryStream())
                {
                    companyToUpdate.Name = companyRequest.Name ?? companyToUpdate.Name;
                    companyToUpdate.Address = companyRequest.Address ?? companyToUpdate.Address;
                    companyToUpdate.Description = companyRequest.Description ?? companyToUpdate.Description;
                    if (companyRequest.PhotoFile != null)
                    {
                        await companyRequest.PhotoFile.CopyToAsync(photoFileStream);
                        companyToUpdate.PhotoFile = photoFileStream.ToArray();
                        companyToUpdate.PhotoFileName = GenerateFileName(ResumePrefix, companyRequest.PhotoFile.FileName);
                    }
                }

                if (!await _companyService.UpdateCompany(companyToUpdate))
                {
                    return StatusCode(500, "Internal Error Occured While Updating Company!");
                }
                
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
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var companyToDelete = await _companyService.GetCompanyById(id);

            if (companyToDelete == null)
            {
                return NotFound($"Company With ID {id} Does Not Exist!");
            }

            if (!await _companyService.DeleteCompany(id))
            {
                return StatusCode(500, "Internal Error Occured While Deleting Company!");

            }
            
            return NoContent();
        }
    }
}
