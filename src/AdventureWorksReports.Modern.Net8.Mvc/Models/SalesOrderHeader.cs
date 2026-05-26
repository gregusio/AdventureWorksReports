using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Net8.Mvc.Models;

[Table("SalesOrderHeader", Schema = "Sales")]
public class SalesOrderHeader
{
    public int SalesOrderID { get; set; }
    public DateTime OrderDate { get; set; }
    
    public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; }
    public int? CustomerID { get; set; }
    
    [ForeignKey(nameof(CustomerID))]
    public Customer Customer { get; set; }
}