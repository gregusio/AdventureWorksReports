using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Models;

[Table("Product", Schema = "Production")]
public class Product
{
    public int ProductID { get; set; }
    public string Name { get; set; }
    public int? ProductSubcategoryID { get; set; }

    [ForeignKey(nameof(ProductSubcategoryID))]
    public ProductSubcategory ProductSubcategory { get; set; }
}