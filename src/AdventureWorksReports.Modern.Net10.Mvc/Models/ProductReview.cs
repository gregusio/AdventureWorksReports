using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Net10.Mvc.Models;

[Table("ProductReview", Schema = "Production")]
public class ProductReview
{
    public int ProductReviewID { get; set; }
    public int ProductID { get; set; }
    public string ReviewerName { get; set; }
    public DateTime ReviewDate { get; set; }
    public string EmailAddress { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; }
    
    [ForeignKey(nameof(ProductID))]
    public Product Product { get; set; }
}
