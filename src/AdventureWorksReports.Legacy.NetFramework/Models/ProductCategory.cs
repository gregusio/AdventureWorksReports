using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.Models
{
    [Table("ProductCategory", Schema = "Production")]
    public class ProductCategory
    {
        public int ProductCategoryID { get; set; }
        public string Name { get; set; }
    }
}