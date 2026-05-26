namespace AdventureWorksReports.Modern.Net10.Mvc.Models.DTOs;

public record MonthlyCategoryProfitDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string CategoryName { get; init; }
    public decimal TotalProfit { get; init; }
}