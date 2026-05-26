using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Net8.Minimal.Models;

[Table("ProductSubcategory", Schema = "Production")]
public class ProductSubcategory
{
    public int ProductSubcategoryID { get; set; }
    public int ProductCategoryID { get; set; }

    [ForeignKey(nameof(ProductCategoryID))]
    public ProductCategory ProductCategory { get; set; }
}