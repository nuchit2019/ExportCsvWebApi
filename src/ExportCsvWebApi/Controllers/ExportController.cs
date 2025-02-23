using ExportCsvWebApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExportCsvWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICsvExportService _csvExportService;
        private readonly IWebHostEnvironment _environment;

        public ExportController(
            ICustomerRepository customerRepository,
            ICsvExportService csvExportService,
            IWebHostEnvironment environment)
        {
            _customerRepository = customerRepository;
            _csvExportService = csvExportService;
            _environment = environment;
        }

        [HttpGet("export-customers")]
        public async Task<IActionResult> ExportCustomersToCsv()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();
            var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "CustomerTemplate.csv");

            var csvData = await _csvExportService.ExportCustomersToCsvAsync(customers, templatePath);

            return File(csvData, "text/csv", "customers.csv");
        }
    }
}