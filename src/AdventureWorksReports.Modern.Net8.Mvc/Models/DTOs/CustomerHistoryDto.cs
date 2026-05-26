namespace AdventureWorksReports.Modern.Net8.Mvc.Models.DTOs;

public record CustomerHistoryDto
{
    public int CustomerId { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public decimal TotalPurchases { get; init; }
    public ICollection<OrderDto> Orders { get; init; } 
}