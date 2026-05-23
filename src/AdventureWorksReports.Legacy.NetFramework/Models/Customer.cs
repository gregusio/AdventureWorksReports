using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.Models
{
    [Table("Customer", Schema = "Sales")]
    public class Customer
    {
        public int CustomerID { get; set; }
        public int? PersonID { get; set; }

        [ForeignKey(nameof(PersonID))]
        public Person Person { get; set; }
    }
}