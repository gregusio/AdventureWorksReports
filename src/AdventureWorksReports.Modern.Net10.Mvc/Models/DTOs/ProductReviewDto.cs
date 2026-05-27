namespace AdventureWorksReports.Modern.Net10.Mvc.Models.DTOs;

public record ProductReviewDto
(
    int ProductID,
    string ReviewerName,
    string EmailAddress,
    int Rating,
    string Comments
);