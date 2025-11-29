using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn_web.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; } // gia thuc te
        public decimal OriginalPrice { get; set; } // gia theo phan tram
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice
        {
            get {  return UnitPrice* Quantity; }
        }

    }
}