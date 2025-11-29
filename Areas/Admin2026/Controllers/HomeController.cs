using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DoAn_web.Models;
using DoAn_web.ViewModels;

namespace DoAn_web.Areas.Admin2026.Controllers
{
    public class HomeController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();

        // GET: Admin2026/Home
        public ActionResult Index()
        {
            var model = new DashboardViewModel();

            // --- 1. THỐNG KÊ SỐ LIỆU TỔNG QUÁT ---

            // Đếm số đơn hàng (Chỉ trừ những đơn Đã hủy)
            model.TotalOrders = db.Orders.Count(o => o.ShippingStatus != "Đã hủy");

            // Đếm tổng sản phẩm đang có
            model.TotalProducts = db.Products.Count();

            // Đếm tổng khách hàng
            model.TotalCustomers = db.Customers.Count();

            // Tính tổng doanh thu
            // Logic: Cộng tiền tất cả đơn hàng chưa bị hủy (bao gồm cả chưa thanh toán và đã thanh toán)
            // Để số liệu khớp với danh sách Order
            model.TotalRevenue = db.Orders
                .Where(o => o.ShippingStatus != "Đã hủy")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;


            // --- 2. DỮ LIỆU BIỂU ĐỒ (Doanh thu 12 tháng năm nay) ---

            var currentYear = DateTime.Now.Year;

            // Lấy dữ liệu từ DB: Gom nhóm theo Tháng
            var revenueData = db.Orders
                .Where(o => o.OrderDate.Year == currentYear && o.ShippingStatus != "Đã hủy")
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                }).ToList();

            // Tạo danh sách dữ liệu đầy đủ cho 12 tháng (để tránh bị thiếu tháng không có đơn)
            var labels = new List<string>();
            var dataPoints = new List<decimal>();

            for (int month = 1; month <= 12; month++)
            {
                labels.Add("Tháng " + month);

                // Tìm xem tháng này có tiền không
                var record = revenueData.FirstOrDefault(r => r.Month == month);
                if (record != null)
                {
                    dataPoints.Add(record.Total);
                }
                else
                {
                    dataPoints.Add(0); // Không có đơn thì là 0đ
                }
            }

            // Gửi dữ liệu biểu đồ sang View qua ViewBag
            ViewBag.ChartLabels = labels;
            ViewBag.ChartData = dataPoints;

            var soldData = db.OrderDetails
                                 .GroupBy(od => od.Product.Category.CategoryName)
                                 .Select(g => new {
                                     Name = g.Key,
                                     SoldQty = g.Sum(od => od.Quantity) // Dùng Sum để cộng tổng số lượng bán
                                 }).ToList();

            ViewBag.PieLabels = soldData.Select(x => x.Name).ToList();
            ViewBag.PieData = soldData.Select(x => x.SoldQty).ToList();
            return View(model);
        }
    }
}