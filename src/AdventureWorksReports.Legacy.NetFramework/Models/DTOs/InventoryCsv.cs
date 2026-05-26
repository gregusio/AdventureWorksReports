using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.NetFramework.Models.DTOs
{
    public class InventoryCsv
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string LocationName { get; set; }
        public short Quantity { get; set; }
    }
}