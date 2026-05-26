using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Net8.Mvc.Models;

[Table("SalesOrderDetail", Schema = "Sales")]
public class SalesOrderDetail
{
    public int SalesOrderID { get; set; }
    public int SalesOrderDetailID { get; set; }
    public decimal LineTotal { get; set; }
    public int ProductID { get; set; }

    [ForeignKey(nameof(SalesOrderID))]
    public SalesOrderHeader SalesOrderHeader { get; set; }
   
    [ForeignKey(nameof(ProductID))]
    public Product Product { get; set; }
}