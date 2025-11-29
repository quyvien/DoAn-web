using DoAn_web.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace DoAn_web.Areas.Admin2026.Controllers
{
    public class CustomersController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();

        // 1. DANH SÁCH KHÁCH HÀNG (Kèm tìm kiếm)
        public ActionResult Index(string searchString)
        {
            var customers = db.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Tìm theo Tên, SĐT hoặc Email
                customers = customers.Where(c => c.CustomerName.Contains(searchString)
                                              || c.CustomerPhone.Contains(searchString)
                                              || c.CustomerEmail.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;
            return View(customers.ToList()); // Nếu muốn phân trang thì thêm PagedList sau
        }

        // 2. XEM CHI TIẾT (Kèm lịch sử đơn hàng)
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Customer customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();

            // Lấy danh sách đơn hàng của khách này để hiển thị bên dưới
            ViewBag.OrderHistory = db.Orders.Where(o => o.CustomerID == id).OrderByDescending(o => o.OrderDate).ToList();

            return View(customer);
        }

        // 3. SỬA THÔNG TIN (GET)
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Customer customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();
            return View(customer);
        }

        // 3. SỬA THÔNG TIN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Username")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // 4. XÓA KHÁCH HÀNG (Có kiểm tra đơn hàng)
        // GET: Admin2026/Customers/Delete/5
        // Hàm này chỉ để HIỂN THỊ trang xác nhận, chưa xóa gì cả
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Customer customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();

            // Kiểm tra ngay: Nếu đã có đơn hàng thì không cho vào trang xóa luôn
            bool hasOrders = db.Orders.Any(o => o.CustomerID == id);
            if (hasOrders)
            {
                TempData["Error"] = "Không thể xóa! Khách hàng này đang có dữ liệu đơn hàng.";
                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // POST: Admin2026/Customers/Delete/5
        // Hàm này mới thực sự XÓA dữ liệu
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);

            // Tìm tài khoản User liên kết để xóa luôn (nếu có)
            var userAccount = db.Users.FirstOrDefault(u => u.Username == customer.Username);
            if (userAccount != null)
            {
                db.Users.Remove(userAccount);
            }

            db.Customers.Remove(customer);
            db.SaveChanges();

            TempData["Success"] = "Đã xóa khách hàng thành công!";
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}