## 🚦 **ขั้นตอนการสร้าง Project ExportCsvWebApi แบบ Step by Step**

---

## 🎯 **วัตถุประสงค์**
สร้าง API ด้วย **C# WebAPI (.NET 8)** สำหรับ **Export ข้อมูลเป็นไฟล์ CSV** โดยใช้ **Template CSV** พร้อมการเชื่อมต่อฐานข้อมูล **In-Memory Database**

---

## 🧱 **โครงสร้าง Project Structure**

```
ExportCsvWebApi/
├─ ExportCsvWebApi.sln
└─ src/
   ├─ ExportCsvWebApi/
   │   ├─ Controllers/
   │   │    └─ ExportController.cs
   │   ├─ Models/
   │   │    └─ Customer.cs
   │   ├─ Repositories/
   │   │    └─ CustomerRepository.cs
   │   ├─ Services/
   │   │    └─ CsvExportService.cs
   │   ├─ Data/
   │   │    └─ AppDbContext.cs
   │   ├─ Interfaces/
   │   │    ├─ ICustomerRepository.cs
   │   │    └─ ICsvExportService.cs
   │   ├─ Templates/
   │   │    └─ CustomerTemplate.csv
   │   ├─ appsettings.json
   │   ├─ Program.cs
   │   └─ ExportCsvWebApi.csproj
```

---

## 📦 **ขั้นตอนการสร้างโปรเจค**

```bash
# 1. สร้าง Solution และ Project
mkdir ExportCsvWebApi
cd ExportCsvWebApi
dotnet new sln -n ExportCsvWebApi
mkdir src
cd src
dotnet new webapi -n ExportCsvWebApi

cd ..
dotnet sln add src/ExportCsvWebApi

# 2. ติดตั้ง NuGet Packages ที่จำเป็น
cd src/ExportCsvWebApi 
dotnet add package CsvHelper
dotnet add package Microsoft.EntityFrameworkCore.InMemory

# 3. สร้างโฟลเดอร์สำหรับจัดระเบียบโค้ด
mkdir Controllers Models Repositories Services Data Interfaces Templates

# 4. สร้าง Template สำหรับการ Export CSV
echo "JANAWAT Report\nDate: {{date}}\nCreate by: {{username}}\nId,Name,Email,CreatedAt" > Templates/CustomerTemplate.csv
```

---

## 🧑‍💻 **ขั้นตอนการเขียนโค้ดแบบ Step by Step**

### 1. **Model: `Customer.cs`**
```csharp
namespace ExportCsvWebApi.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

---

### 2. **Database Context: `AppDbContext.cs`**
```csharp
using ExportCsvWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ExportCsvWebApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow },
            new Customer { Id = 2, Name = "Jane Smith", Email = "jane@example.com", CreatedAt = DateTime.UtcNow }
        );
    }
}
```

---

### 3. **Interfaces: `ICustomerRepository.cs` และ `ICsvExportService.cs`**
```csharp
// Interfaces/ICustomerRepository.cs
using ExportCsvWebApi.Models;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
}

// Interfaces/ICsvExportService.cs
using ExportCsvWebApi.Models;

public interface ICsvExportService
{
    Task<byte[]> ExportCustomersToCsvAsync(IEnumerable<Customer> customers, string templatePath);
}
```

---

### 4. **Repository: `CustomerRepository.cs`**
```csharp
using ExportCsvWebApi.Interfaces;
using ExportCsvWebApi.Models;
using ExportCsvWebApi.Data;
using Microsoft.EntityFrameworkCore;

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
```

---

### 5. **Service: `CsvExportService.cs`**
```csharp
using CsvHelper;
using CsvHelper.Configuration;
using ExportCsvWebApi.Interfaces;
using ExportCsvWebApi.Models;
using System.Globalization;
using System.Text;

public class CsvExportService : ICsvExportService
{
    public async Task<byte[]> ExportCustomersToCsvAsync(IEnumerable<Customer> customers, string templatePath)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        // Render Template
        var template = await File.ReadAllTextAsync(templatePath);
        var renderedTemplate = template
            .Replace("{{date}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
            .Replace("{{username}}", "admin"); // Replace with actual username if needed

        await writer.WriteLineAsync(renderedTemplate);
        csvWriter.WriteRecords(customers);
        await writer.FlushAsync();

        return memoryStream.ToArray();
    }
}
```

---

### 6. **Controller: `ExportController.cs`**
```csharp
using ExportCsvWebApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
```

---

### 7. **ตั้งค่าใน `Program.cs`**
```csharp
using ExportCsvWebApi.Data;
using ExportCsvWebApi.Interfaces;
using ExportCsvWebApi.Repositories;
using ExportCsvWebApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// In-Memory Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("CustomerDb"));

// Register Services
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICsvExportService, CsvExportService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
```

---

## 🚀 **ทดสอบการทำงาน**
1. รันโปรเจคด้วยคำสั่ง
```sh
dotnet run
```

2. เปิด **Swagger** และเรียก API:
```
GET /api/export/export-customers
```

3. ไฟล์ **customers.csv** จะถูกดาวน์โหลดพร้อมข้อมูลและ Header ตาม Template ที่กำหนด 😊
