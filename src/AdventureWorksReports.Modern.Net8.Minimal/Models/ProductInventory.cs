using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Net8.Minimal.Models;

[Table("ProductInventory", Schema = "Production")]
public class ProductInventory
{
    public int ProductId { get; set; }
    public short Quantity { get; set; }
    public int LocationId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }

    [ForeignKey(nameof(LocationId))]
    public Location Location { get; set; }
}