namespace AdventureWorksReports.Legacy.NetFramework.Models.DTOs
{
    public class ProductReviewDto
    {
        public int ProductID { get; set; }
        public string ReviewerName { get; set; }
        public string EmailAddress { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
    }
}