using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.NetFramework.Models
{
    [Table("Person", Schema = "Person")]
    public class Person
    {
        public int BusinessEntityID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}