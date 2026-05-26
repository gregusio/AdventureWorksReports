namespace AdventureWorksReports.Modern.Net8.Mvc.Models.DTOs;

public record OrderDto
{
    public int OrderId { get; init; }
    public DateTime OrderDate { get; init; }
    public decimal TotalAmount { get; init; }
}