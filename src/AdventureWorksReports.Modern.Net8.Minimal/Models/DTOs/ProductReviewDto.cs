namespace AdventureWorksReports.Modern.Net8.Minimal.Models.DTOs;

public record ProductReviewDto
(
    int ProductID,
    string ReviewerName,
    string EmailAddress,
    int Rating,
    string Comments
);