using AdventureWorksReports.Modern.Net8.Minimal.Data;
using AdventureWorksReports.Modern.Net8.Minimal.Models;
using AdventureWorksReports.Modern.Net8.Minimal.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksReports.Modern.Net8.Minimal.Endpoints;

public static class ReviewsEndpoints
{
    public static void MapReviewsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews");

        group.MapPost("/bulk", async (AdventureWorksContext context, List<ProductReviewDto> reviews) =>
        {
            if (reviews == null || reviews.Count == 0)
            {
                return Results.BadRequest("No reviews provided.");
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

            await context.ProductReviews.AddRangeAsync(newReviews);
            await context.SaveChangesAsync();

            return Results.Created("/api/reviews/bulk", newReviews.Count);
        });

        group.MapDelete("/delete-bulk", async (AdventureWorksContext context) =>
        {
            string sqlCommand = "TRUNCATE TABLE Production.ProductReview";

            int deletedCount = await context.Database.ExecuteSqlRawAsync(sqlCommand);

            return Results.Ok(new { Deleted = deletedCount, Message = "Success" });
        });
    }
}

