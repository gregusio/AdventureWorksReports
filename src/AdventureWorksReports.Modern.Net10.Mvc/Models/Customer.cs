using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Models;

[Table("Customer", Schema = "Sales")]
public class Customer
{
    public int CustomerID { get; set; }
    public int? PersonID { get; set; }

    [ForeignKey(nameof(PersonID))]
    public Person Person { get; set; }
}