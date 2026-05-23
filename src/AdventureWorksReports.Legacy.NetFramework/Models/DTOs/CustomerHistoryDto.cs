using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdventureWorksReports.Legacy.Models.DTOs
{
    public class CustomerHistoryDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal TotalPurchases { get; set; }
        public ICollection<OrderDto> Orders { get; set; }
    }
}