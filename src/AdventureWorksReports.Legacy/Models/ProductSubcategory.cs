using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.Models
{
    [Table("ProductSubcategory", Schema = "Production")]
    public class ProductSubcategory
    {
        public int ProductSubcategoryID { get; set; }
        public int ProductCategoryID { get; set; }

        [ForeignKey(nameof(ProductCategoryID))]
        public ProductCategory ProductCategory { get; set; }
    }
}