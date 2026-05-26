using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.NetFramework.Models.DTOs
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}