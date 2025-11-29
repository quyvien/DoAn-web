using DoAn_web.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace DoAn_web.Areas.Admin2026.Controllers
{
    public class UsersController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();

        // 1. DANH SÁCH TÀI KHOẢN
        public ActionResult Index()
        {
            // Chỉ hiển thị danh sách User
            return View(db.Users.ToList());
        }

        // 2. TẠO TÀI KHOẢN MỚI (Ví dụ tạo thêm Admin)
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Password,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên
                if (db.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
                    return View(user);
                }

                db.Users.Add(user);
                db.SaveChanges();
                TempData["Success"] = "Đã tạo tài khoản mới thành công!";
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // 3. SỬA QUYỀN HOẶC ĐỔI MẬT KHẨU
        public ActionResult Edit(string username) // Lưu ý: User dùng Username làm khóa chính (theo code cũ của bạn)
        {
            if (string.IsNullOrEmpty(username)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            User user = db.Users.Find(username); // Find theo Username
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,Password,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // 4. XÓA TÀI KHOẢN
        // GET: Admin2026/Users/Delete/username
        public ActionResult Delete(string username)
        {
            if (string.IsNullOrEmpty(username)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            User user = db.Users.Find(username);
            if (user == null) return HttpNotFound();

            // 🔥 RÀNG BUỘC 1: KHÔNG ĐƯỢC XÓA CHÍNH MÌNH 🔥
            // User.Identity.Name là tên người đang đăng nhập hiện tại
            if (user.Username == User.Identity.Name)
            {
                TempData["Error"] = "Bạn không thể xóa chính tài khoản đang đăng nhập!";
                return RedirectToAction("Index");
            }

            // 🔥 RÀNG BUỘC 2: KIỂM TRA DỮ LIỆU KHÁCH HÀNG LIÊN QUAN 🔥
            // Tìm xem User này có phải là Khách hàng không
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer != null)
            {
                // Nếu là khách hàng, kiểm tra xem họ có đơn hàng nào không?
                bool hasOrders = db.Orders.Any(o => o.CustomerID == customer.CustomerID);
                if (hasOrders)
                {
                    TempData["Error"] = $"Không thể xóa User '{username}' vì khách hàng này đã có đơn hàng trong hệ thống. Hãy chỉ khóa tài khoản thay vì xóa.";
                    return RedirectToAction("Index");
                }
            }

            return View(user);
        }

        // POST: Admin2026/Users/Delete/username
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string username)
        {
            User user = db.Users.Find(username);

            // Kiểm tra lại ràng buộc 1 lần nữa ở Back-end để chắc chắn (phòng trường hợp hack vượt qua UI)
            if (user.Username == User.Identity.Name)
            {
                TempData["Error"] = "Không thể xóa chính mình!";
                return RedirectToAction("Index");
            }

            // Xử lý xóa Customer liên kết (nếu có và không có đơn hàng)
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer != null)
            {
                // Kiểm tra đơn hàng lần cuối
                if (db.Orders.Any(o => o.CustomerID == customer.CustomerID))
                {
                    TempData["Error"] = "Lỗi: Tài khoản này có đơn hàng, không thể xóa.";
                    return RedirectToAction("Index");
                }

                // Xóa thông tin khách hàng trước
                db.Customers.Remove(customer);
            }

            // Cuối cùng mới xóa User
            db.Users.Remove(user);
            db.SaveChanges();

            TempData["Success"] = "Đã xóa tài khoản và dữ liệu liên quan thành công!";
            return RedirectToAction("Index");
        }
    }
}