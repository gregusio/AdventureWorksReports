using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.Models.DTOs
{
    public class MonthlyCategoryProfitDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string CategoryName { get; set; }
        public decimal TotalProfit { get; set; }
    }
}