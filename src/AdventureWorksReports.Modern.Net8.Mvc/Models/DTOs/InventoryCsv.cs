namespace AdventureWorksReports.Modern.Net8.Mvc.Models.DTOs;

public record InventoryCsvDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; }
    public string LocationName { get; init; }
    public short Quantity { get; init; }
}