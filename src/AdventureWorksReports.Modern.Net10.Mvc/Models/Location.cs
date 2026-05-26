using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorksReports.Modern.Net10.Mvc.Models;

[Table("Location", Schema = "Production")]
public class Location
{
    public int LocationId { get; set; }
    public string Name { get; set; }
}