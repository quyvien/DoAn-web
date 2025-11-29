using DoAn_web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn_web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }        // Tổng số đơn hàng
        public int TotalProducts { get; set; }      // Tổng số sản phẩm
        public int TotalCustomers { get; set; }     // Tổng số khách hàng
        public decimal TotalRevenue { get; set; }   // Tổng doanh thu

        // Danh sách đơn hàng mới nhất (để hiện bảng nhỏ)
        public List<Order> RecentOrders { get; set; }
    }
}