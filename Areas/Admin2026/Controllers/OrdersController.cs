// File: /Areas/Admin2026/Controllers/OrdersController.cs

using DoAn_web.Models;
using DoAn_web.Services;
using PagedList;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Collections.Generic; // Cần thêm namespace này cho List<SelectListItem>
using System;
using System.Data;

namespace DoAn_web.Areas.Admin2026.Controllers
{
    [Authorize(Roles = "A")]
    public class OrdersController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();

        // Tải dữ liệu cần thiết cho Edit View (Customer, OrderDetails)
        private void LoadEditViewData(Order order)
        {
            // 🔥 CẢI TIẾN: Luôn tải lại Customer (cần thiết cho cả GET và POST thất bại)
            // Nếu order.Customer đã bị hủy do Bind trong POST, nó sẽ được tải lại ở đây.
            if (order.Customer == null)
            {
                order.Customer = db.Customers.Find(order.CustomerID);
            }

            // Tải Order Details (Sản phẩm trong đơn hàng)
            // Bao gồm cả Product để View hiển thị tên sản phẩm
            ViewBag.OrderDetails = db.OrderDetails.Include(od => od.Product)
                                                 .Where(od => od.OrderID == order.OrderID).ToList();
        }


        // GET: Admin2026/Orders/Edit/5 (Trang sửa trạng thái)
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // 1. Chỉ cần tìm Order
            Order order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();

            // 2. 🔥 DÙNG LoadEditViewData để tải Customer và OrderDetails
            LoadEditViewData(order);

            return View(order);
        }

        // POST: Admin2026/Orders/Edit/5 (Lưu thay đổi trạng thái)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderID,PaymentStatus,ShippingStatus")] Order order)
        {
            var validPaymentStatuses = new[] { "Chờ xử lý", "Chờ xác nhận chuyển khoản", "Đã thanh toán", OrderService.CancelledStatus };
            var validShippingStatuses = new[] { "Chờ lấy hàng", "Đang vận chuyển", "Đã giao thành công", OrderService.CancelledStatus };
            if (!validPaymentStatuses.Contains(order.PaymentStatus) || !validShippingStatuses.Contains(order.ShippingStatus))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Đọc và cập nhật trong cùng giao dịch để tránh hoàn kho hai lần khi hủy đồng thời.
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
            var orderDB = db.Orders.Include(o => o.OrderDetails).FirstOrDefault(o => o.OrderID == order.OrderID);

            if (orderDB != null)
            {
                if (OrderService.IsCancelled(order) && !OrderService.IsCancelled(orderDB) &&
                    !OrderService.CanCustomerCancel(orderDB))
                {
                    ModelState.AddModelError("", "Chỉ có thể hủy đơn chưa thanh toán và đang chờ lấy hàng.");
                    LoadEditViewData(orderDB);
                    return View(orderDB);
                }
                if (OrderService.IsCancelled(orderDB) && !OrderService.IsCancelled(order))
                {
                    ModelState.AddModelError("", "Không thể mở lại một đơn hàng đã hủy.");
                    LoadEditViewData(orderDB);
                    return View(orderDB);
                }

                if (order.ShippingStatus == "Đã giao thành công" && order.PaymentStatus != "Đã thanh toán")
                {
                    ModelState.AddModelError("ShippingStatus", "Không thể hoàn tất giao hàng khi Thanh toán chưa là 'Đã thanh toán'.");

                    // 🔥 BƯỚC 1: KHÔI PHỤC DỮ LIỆU HIỂN THỊ TỪ DB (Thêm đoạn này) 🔥
                    order.OrderDate = orderDB.OrderDate;       // Lấy lại ngày đặt
                    order.TotalAmount = orderDB.TotalAmount;   // Lấy lại tổng tiền
                    order.AddressDelivery = orderDB.AddressDelivery; // Lấy lại địa chỉ
                    order.CustomerID = orderDB.CustomerID;

                    // BƯỚC 2: Tải các quan hệ (Customer, OrderDetails)
                    LoadEditViewData(order);

                    return View(order);
                }

                    if (OrderService.IsCancelled(order))
                    {
                        OrderService.CancelOrder(db, orderDB);
                    }
                    else
                    {
                        orderDB.PaymentStatus = order.PaymentStatus;
                        orderDB.ShippingStatus = order.ShippingStatus;
                    }

                    db.SaveChanges();
                    transaction.Commit();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{order.OrderID} thành công!";
                return RedirectToAction("Index");
            }

            // Nếu đơn hàng không tồn tại, quay về Index
            return HttpNotFound();
            }
        }

        // ... Các Action khác (Index, Dispose) ...

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Index(string searchString)
        {
            // Tải orders, buôc tải Customer (Eager Loading)
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            //  FIX: SẮP XẾP THEO OrderID GIẢM DẦN (Lớn nhất -> Nhỏ nhất) 
            orders = orders.OrderByDescending(o => o.OrderID);

            // --- LOGIC TÌM KIẾM ---
            if (!string.IsNullOrEmpty(searchString))
            {
                string searchUpper = searchString.ToUpper();

                // Tìm kiếm theo Tên Khách hàng hoặc Mã Đơn hàng (OrderID)
                orders = orders.Where(o =>
                    o.Customer.CustomerName.ToUpper().Contains(searchUpper) ||
                    o.OrderID.ToString().Contains(searchUpper)
                );
            }
            ViewBag.CurrentFilter = searchString;

            // Thực thi query và trả về toàn bộ danh sách đã lọc (nếu có tìm kiếm)
            return View(orders.ToList());
        }
    }
}
