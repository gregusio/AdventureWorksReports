using AdventureWorksReports.Modern.Net10.Mvc.Data;
using AdventureWorksReports.Modern.Net10.Mvc.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksReports.Modern.Net10.Mvc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AdventureWorksContext _context;

    public ReportsController(AdventureWorksContext context)
    {
        _context = context;
    }

    [HttpGet("monthly-profit")]
    public async Task<IActionResult> GetMonthlyProfit()
    {
        DateTime startDate = new DateTime(2013, 1, 1);

        var report = await _context.SalesOrderDetails
            .Where(detail => detail.SalesOrderHeader.OrderDate >= startDate)
            .GroupBy(detail => new
            {
                Year = detail.SalesOrderHeader.OrderDate.Year,
                Month = detail.SalesOrderHeader.OrderDate.Month,
                CategoryName = detail.Product.ProductSubcategory.ProductCategory.Name
            })
            .Select(group => new MonthlyCategoryProfitDto
            {
                Year = group.Key.Year,
                Month = group.Key.Month,
                CategoryName = group.Key.CategoryName,
                TotalProfit = group.Sum(x => x.LineTotal)
            })
            .OrderByDescending(dto => dto.Year)
            .ThenByDescending(dto => dto.Month)
            .ThenBy(dto => dto.CategoryName)
            .AsNoTracking()
            .ToListAsync();

        return Ok(report);
    }

    [HttpGet("customer-history")]
    public async Task<IActionResult> GetCustomerHistory()
    {
        var report = await _context.SalesOrderHeaders
            .SelectMany(header => header.SalesOrderDetails)
            .GroupBy(detail => new
            {
                detail.SalesOrderHeader.CustomerID,
                detail.SalesOrderHeader.Customer.Person.FirstName,
                detail.SalesOrderHeader.Customer.Person.LastName
            })
            .Select(group => new CustomerHistoryDto
            {
                CustomerId = group.Key.CustomerID ?? 0,
                FirstName = group.Key.FirstName,
                LastName = group.Key.LastName,
                TotalPurchases = group.Sum(x => x.LineTotal),
                Orders = group.Select(detail => new OrderDto
                {
                    OrderId = detail.SalesOrderHeader.SalesOrderID,
                    OrderDate = detail.SalesOrderHeader.OrderDate,
                    TotalAmount = detail.LineTotal
                })
                .ToList()
            })
            .OrderByDescending(dto => dto.TotalPurchases)
            .ThenBy(dto => dto.FirstName)
            .ThenBy(dto => dto.LastName)
            .AsNoTracking()
            .Take(10_000)
            .ToListAsync();

        return Ok(report);
    }

    [HttpGet("inventory-csv")]
    public async Task ExportInventoryCsv()
    {
        Response.ContentType = "text/csv";
        Response.Headers.Append("Content-Disposition", "attachment; filename=\"inventory.csv\"");

        await using var writer = new StreamWriter(Response.Body);

        await writer.WriteLineAsync("ProductName,LocationName,Quantity");

        var inventoryStream = _context.ProductInventories
            .Select(pi => new
            {
                ProductName = pi.Product.Name,
                LocationName = pi.Location.Name,
                Quantity = pi.Quantity
            })
            .AsNoTracking()
            .AsAsyncEnumerable();

        await foreach (var row in inventoryStream)
        {
            var cleanName = row.ProductName.Replace(",", "");

            await writer.WriteLineAsync($"{cleanName},{row.LocationName},{row.Quantity}");
        }
    }
}
