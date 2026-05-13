using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.Models
{
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
}