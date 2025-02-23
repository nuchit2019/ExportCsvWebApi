using ExportCsvWebApi.Models;

namespace ExportCsvWebApi.Interfaces
{
    public interface ICsvExportService
    {
        Task<byte[]> ExportCustomersToCsvAsync(IEnumerable<Customer> customers, string templatePath);
    }
} 