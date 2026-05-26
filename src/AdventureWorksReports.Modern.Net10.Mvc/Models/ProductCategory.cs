using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Net10.Mvc.Models;

[Table("ProductCategory", Schema = "Production")]
public class ProductCategory
{
    public int ProductCategoryID { get; set; }
    public string Name { get; set; }
}