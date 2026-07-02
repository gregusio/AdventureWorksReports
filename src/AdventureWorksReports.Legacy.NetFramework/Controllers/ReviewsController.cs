using AdventureWorksReports.Legacy.NetFramework.Data;
using AdventureWorksReports.Legacy.NetFramework.Models;
using AdventureWorksReports.Legacy.NetFramework.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace AdventureWorksReports.Legacy.NetFramework.Controllers
{
    [RoutePrefix("api/reviews")]
    public class ReviewsController : ApiController
    {
        [HttpPost]
        [Route("bulk")]
        public async Task<IHttpActionResult> BulkInsertReviews([FromBody] List<ProductReviewDto> reviews)
        {
            using (var db = new AdventureWorksContext())
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

                db.ProductReviews.AddRange(newReviews);
                await db.SaveChangesAsync();

                return Content(HttpStatusCode.Created, newReviews.Count);
            }
        }

        [HttpDelete]
        [Route("delete-bulk")]
        public async Task<IHttpActionResult> ClearTestData()
        {
            using (var db = new AdventureWorksContext()) // Podmień na nazwę Twojego DbContextu
            {
                string sqlCommand = "TRUNCATE TABLE Production.ProductReview";

                int deletedCount = await db.Database.ExecuteSqlCommandAsync(sqlCommand);

                return Ok(new { Deleted = deletedCount, Message = "Success" });
            }
        }
    }
}