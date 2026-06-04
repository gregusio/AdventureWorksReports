using AdventureWorksReports.Modern.Net8.Minimal.Data;
using AdventureWorksReports.Modern.Net8.Minimal.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksReports.Modern.Net8.Minimal.Endpoints;

public static class ReportsEndpoints
{
    public static void MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports");

        group.MapGet("/monthly-profit", async (AdventureWorksContext context) =>
        {
            DateTime startDate = new DateTime(2013, 1, 1);

            var report = await context.SalesOrderDetails
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

            return Results.Ok(report);
        });

        group.MapGet("/customer-history", async (AdventureWorksContext context) =>
        {
            var report = await context.SalesOrderHeaders
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

            return Results.Ok(report);
        });

        group.MapGet("/inventory-csv", async (AdventureWorksContext context, HttpResponse response) =>
        {
            response.ContentType = "text/csv";
            response.Headers.Append("Content-Disposition", "attachment; filename=\"inventory.csv\"");

            await using var writer = new StreamWriter(response.Body);

            await writer.WriteLineAsync("ProductName,LocationName,Quantity");

            var inventoryStream = context.ProductInventories
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
        });
    }
}
