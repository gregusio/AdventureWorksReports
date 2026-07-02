using AdventureWorksReports.Legacy.NetFramework.Data;
using AdventureWorksReports.Legacy.NetFramework.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace AdventureWorksReports.Legacy.NetFramework.Controllers
{
    [RoutePrefix("api/reports")]
    public class ReportsController : ApiController
    {
        [HttpGet]
        [Route("monthly-profit")]
        public async Task<IHttpActionResult> GetMonthlyProfit()
        {
            using (var db = new AdventureWorksContext())
            {
                DateTime startDate = new DateTime(2013, 1, 1);

                var report = await db.SalesOrderDetails
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

        [HttpGet]
        [Route("customer-history")]
        public async Task<IHttpActionResult> GetCustomerHistory()
        {
            using (var db = new AdventureWorksContext())
            {
                var report = await db.SalesOrderHeaders
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
        }

        [HttpGet]
        [Route("inventory-csv")]
        public HttpResponseMessage ExportInventoryCsv()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            response.Content = new PushStreamContent(async (outputStream, httpContent, transportContext) =>
            {
                using (var db = new AdventureWorksContext())
                {
                    using (var writer = new StreamWriter(outputStream))
                    {
                        await writer.WriteLineAsync("ProductName,LocationName,Quantity");

                        var query = db.ProductInventories
                            .Select(pi => new
                            {
                                ProductName = pi.Product.Name,
                                LocationName = pi.Location.Name,
                                Quantity = pi.Quantity
                            })
                            .AsNoTracking();

                        foreach (var row in query)
                        {
                            var cleanName = row.ProductName != null ? row.ProductName.Replace(",", "") : "";
                            await writer.WriteLineAsync($"{cleanName},{row.LocationName},{row.Quantity}");
                        }
                    }
                }
            }, "text/csv");

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "inventory.csv"
            };

            return response;
        }
    }
}