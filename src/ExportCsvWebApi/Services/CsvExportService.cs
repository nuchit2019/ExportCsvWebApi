using CsvHelper.Configuration;
using CsvHelper;
using ExportCsvWebApi.Models;
using System.Globalization;
using System.Text;
using ExportCsvWebApi.Interfaces;

namespace ExportCsvWebApi.Services
{
    public class CsvExportService : ICsvExportService
    {
        public async Task<byte[]> ExportCustomersToCsvAsync(IEnumerable<Customer> customers, string templatePath)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // อ่าน Template และแทนที่ข้อมูลใน Template
            var template = await File.ReadAllTextAsync(templatePath);
            var renderedTemplate = template
                .Replace("{{date}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
                .Replace("{{username}}", "admin"); // สามารถปรับให้รับค่าจากระบบ Authentication ได้

            // เขียน Header จาก Template ลงไฟล์
            await writer.WriteLineAsync(renderedTemplate);

            // เขียนข้อมูลจาก Customers ลงใน CSV
            csvWriter.WriteRecords(customers);
            await writer.FlushAsync();

            return memoryStream.ToArray();
        }
    }
}