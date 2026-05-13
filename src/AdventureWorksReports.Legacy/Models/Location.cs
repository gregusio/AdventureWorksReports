using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.Models
{
    [Table("Location", Schema = "Production")]
    public class Location
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
    }
}