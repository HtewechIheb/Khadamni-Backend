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
    [Route("/api/companies/")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;

        public CompaniesController(ICompanyRepository companyService)
        {
            _companyRepository = companyService;
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyRepository.GetCompanies();
            var companyDTOs = new List<CompanyDTO>();
            foreach (var company in companies)
            {
                var companyDTO = new CompanyDTO
                {
                    Id = company.Id,
                    Name = company.Name,
                    Address = company.Address,
                    Description = company.Description,
                    ContactNumber = company.ContactNumber,
                    Category = company.Category
                };

                companyDTOs.Add(companyDTO);
            }
            return Ok(companyDTOs);
        }

        [HttpGet]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            var company = await _companyRepository.GetCompanyById(id);

            if (company == null)
            {
                return NotFound($"Company With ID {id} Does Not Exist!");
            }

            var companyDTO = new CompanyDTO
            {
                Id = company.Id,
                Name = company.Name,
                Address = company.Address,
                Description = company.Description,
                ContactNumber = company.ContactNumber,
                Category = company.Category
            };

            return Ok(companyDTO);
        }

        [HttpGet]
        [Route("{id:long}/photo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> DownloadPhoto(long id)
        {
            var company = await _companyRepository.GetCompanyById(id);

            if (company == null)
            {
                return NotFound($"Company With ID {id} Does Not Exist!");
            }

            var byteStream = new MemoryStream(company.LogoFile);
            return new FileStreamResult(byteStream, new MediaTypeHeaderValue("application/octet-stream"))
            {
                FileDownloadName = company.LogoFileName
            };
        }

        [HttpDelete]
        [Route("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var companyToDelete = await _companyRepository.GetCompanyById(id);

            if (companyToDelete == null)
            {
                return NotFound($"Company With ID {id} Does Not Exist!");
            }

            if (!await _companyRepository.DeleteCompany(id))
            {
                return StatusCode(500, "Internal Error Occured While Deleting Company!");

            }

            return NoContent();
        }
    }
}
