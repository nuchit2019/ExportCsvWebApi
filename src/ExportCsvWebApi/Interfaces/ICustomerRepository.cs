using ExportCsvWebApi.Models;

namespace ExportCsvWebApi.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
    }
}
