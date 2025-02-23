using ExportCsvWebApi.Models;
using ExportCsvWebApi.Data;
using Microsoft.EntityFrameworkCore;
using ExportCsvWebApi.Interfaces;

namespace ExportCsvWebApi.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }
    }
}
