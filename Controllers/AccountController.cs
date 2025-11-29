using DoAn_web.Models;
using DoAn_web.ViewModels;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
// QUAN TRỌNG: Thêm thư viện này để bắt lỗi Validation chi tiết
using System.Data.Entity.Validation;

namespace DoAn_web.Controllers
{
    public class AccountController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra Username đã tồn tại chưa
                var checkUser = db.Users.FirstOrDefault(u => u.Username == model.Username);
                if (checkUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
                    return View(model);
                }

                try
                {
                    // 2. BƯỚC 1: Tạo và LƯU User trước
                    var user = new User();
                    user.Username = model.Username;
                    user.UserRole = "C";
                    user.Password = model.Password; // Lưu pass thường (bài tập)

                    db.Users.Add(user);
                    db.SaveChanges(); // <--- Lưu User để lấy Username

                    // 3. BƯỚC 2: Tạo và Lưu Customer
                    var customer = new Customer();
                    customer.CustomerName = model.CustomerName;
                    customer.CustomerPhone = model.CustomerPhone;
                    customer.CustomerEmail = model.CustomerEmail;
                    customer.CustomerAddress = model.CustomerAddress;
                    customer.Username = model.Username; // Username đã an toàn tồn tại

                    db.Customers.Add(customer);
                    db.SaveChanges(); // <--- Lưu Customer

                    // Thành công thì chuyển hướng
                    return RedirectToAction("Login");
                }
                // --- ĐOẠN CODE QUAN TRỌNG ĐỂ SỬA LỖI VALIDATION ---
                catch (DbEntityValidationException ex)
                {
                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                        {
                            // Hiện lỗi cụ thể ra màn hình (Ví dụ: Cột Email không được để trống)
                            ModelState.AddModelError("", "Lỗi tại dòng " + validationError.PropertyName + ": " + validationError.ErrorMessage);
                        }
                    }
                    return View(model);
                }
                catch (Exception ex)
                {
                    // Các lỗi khác (Lỗi kết nối DB, lỗi hệ thống...)
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    return View(model);
                }
            }

            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Username, string Password, string ReturnUrl)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ViewBag.LoginError = "Vui lòng nhập Username và Mật khẩu.";
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Username == Username);

            if (user != null)
            {
                if (user.Password == Password)
                {
                    var ticket = new FormsAuthenticationTicket(
                        1,
                        user.Username,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(30),
                        true,
                        user.UserRole.Trim()
                    );

                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    Response.Cookies.Add(authCookie);

                    if (!String.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Default");
                    }
                }
            }

            ViewBag.LoginError = "Username hoặc mật khẩu không chính xác.";
            return View();
        }

        // Action Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Default");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}