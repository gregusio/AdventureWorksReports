using AdventureWorksReports.Modern.Data;
using AdventureWorksReports.Modern.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksReports.Modern.Controllers;

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
        DateTime startDate = new DateTime(2011, 1, 1); 

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
}