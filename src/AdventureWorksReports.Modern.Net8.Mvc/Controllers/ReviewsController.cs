using AdventureWorksReports.Modern.Net8.Mvc.Data;
using AdventureWorksReports.Modern.Net8.Mvc.Models;
using AdventureWorksReports.Modern.Net8.Mvc.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksReports.Modern.Net8.Mvc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly AdventureWorksContext _context;

    public ReviewsController(AdventureWorksContext context)
    {
        _context = context;
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkInsertReviews([FromBody] List<ProductReviewDto> reviews)
    {
        if (reviews == null || reviews.Count == 0)
        {
            return BadRequest("No reviews provided.");
        }

        var newReviews = reviews.Select(review => new ProductReview
        {
            ProductID = review.ProductID,
            ReviewerName = review.ReviewerName,
            EmailAddress = review.EmailAddress,
            ReviewDate = DateTime.Now,
            Rating = review.Rating,
            Comments = review.Comments
        }).ToList();

        await _context.ProductReviews.AddRangeAsync(newReviews);
        await _context.SaveChangesAsync();

        return Created("/api/reviews/bulk", newReviews.Count);
    }

    [HttpDelete("delete-bulk")]
    public async Task<IActionResult> ClearTestData()
    {
        string sqlCommand = "TRUNCATE TABLE Production.ProductReview";

        int deletedCount = await _context.Database.ExecuteSqlRawAsync(sqlCommand);

        return Ok(new { Deleted = deletedCount, Message = "Success" });
    }
}