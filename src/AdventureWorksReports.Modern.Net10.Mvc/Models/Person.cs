using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Net10.Mvc.Models;

[Table("Person", Schema = "Person")]
public class Person
{
    public int BusinessEntityID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}