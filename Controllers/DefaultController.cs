using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAn_web.Models;
using DoAn_web.ViewModels;
using DoAn_web.Services;
using System.Data.Entity;
using System.Data;

namespace DoAn_web.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
      
         private MyStore2026Entities db = new MyStore2026Entities();
        private void LoadFlashSalesToViewBag()
        {
            var now = DateTime.Now;
            var activeFlashSales = db.FlashSaleItems
                                     .AsNoTracking()
                                     .Include(f => f.Product)
                                     .Where(f => f.IsActive && f.StartDate <= now && f.EndDate >= now &&
                                                 f.SoldQuantity < f.SaleQuantityLimit)
                                     .OrderBy(f => f.EndDate)
                                     .ToList();

            ViewBag.FlashSaleProducts = activeFlashSales.Select(f => f.Product).ToList();
            ViewBag.FlashSales = activeFlashSales;
            if (activeFlashSales.Count > 0)
            {
                ViewBag.FlashSalesEndTime = activeFlashSales.First().EndDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public ActionResult Index()
        {
            LoadFlashSalesToViewBag();
            var viewModel = new HomeViewModel
            {
                Iphones = GetNewestProducts("Iphone"),
                Ipads = GetNewestProducts("Ipad"),
                Macs = GetNewestProducts("Mac"),
                Watches = GetNewestProducts("Watch"),
                PhuKiens = GetNewestProducts("PhuKien")
            };

            return View(viewModel);
        }

        private List<Product> GetNewestProducts(string categoryName)
        {
            // Giới hạn ngay tại SQL: trang chủ chỉ tải tối đa 20 sản phẩm.
            return db.Products.AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Category.CategoryName == categoryName)
                .OrderByDescending(p => p.ProductID)
                .Take(4)
                .ToList();
        }
       
        

        public ActionResult TrangIphone()
        {
           
            //  Lọc database:
            // CHỈ lấy các sản phẩm có CategoryName == "iPhone"
            var iphones = db.Products
                            .Include(p => p.Category)
                            .Where(p => p.Category.CategoryName == "iPhone")
                            .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                            .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(iphones);
        }
        private int? GetCurrentCustomerID()
        {
            var username = User.Identity.Name;
            return db.Customers
                .Where(c => c.Username == username)
                .Select(c => (int?)c.CustomerID)
                .FirstOrDefault();
        }

        // GET: /Default/OrderHistory
        [Authorize(Roles = "C")] // Chỉ Customer mới xem được
        public ActionResult OrderHistory()
        {
            int? customerID = GetCurrentCustomerID();
            if (!customerID.HasValue) return new HttpStatusCodeResult(403);

            var orders = db.Orders.AsNoTracking()
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .Where(o => o.CustomerID == customerID.Value)
                .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID).ToList();
            return View(orders);
        }
        [HttpPost]
        [Authorize(Roles = "C")]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id)
        {
            int? customerId = GetCurrentCustomerID();
            if (!customerId.HasValue) return new HttpStatusCodeResult(403);

            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var order = db.Orders.Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.OrderID == id && o.CustomerID == customerId.Value);

                if (!OrderService.CanCustomerCancel(order))
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại hoặc không còn có thể hủy.";
                    return RedirectToAction("OrderHistory");
                }

                OrderService.CancelOrder(db, order);
                db.SaveChanges();
                transaction.Commit();
                TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{id} và hoàn lại tồn kho.";
            }
            return RedirectToAction("OrderHistory");
                

        }
        





        public ActionResult TrangMac()
        {
            LoadFlashSalesToViewBag();
            var Mac = db.Products
                            .Include(p => p.Category)
                            .Where(p => p.Category.CategoryName == "Mac")
                            .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                            .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(Mac);
        }
        public ActionResult TrangWatch()
        {
            
            var Watch = db.Products
                            .Include(p => p.Category)
                            .Where(p => p.Category.CategoryName == "Watch")
                            .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                            .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(Watch);
        }
        public ActionResult TrangIpad()
        {
           
            var Ipads = db.Products
                           .Include(p => p.Category)
                           .Where(p => p.Category.CategoryName == "ipad")
                           .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                           .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(Ipads);
        }
        public ActionResult TrangMussic()
        {
            return View();
        }
        public ActionResult TrangCamera()
        {
            
            var Camera = db.Products
                            .Include(p => p.Category)
                            .Where(p => p.Category.CategoryName == "Camera")
                            .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                            .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(Camera);
        }
        public ActionResult TrangAccessory()
        {
            return View();
        }
        public ActionResult PhuKien()
        {
            LoadFlashSalesToViewBag();
            var PhuKien = db.Products
                            .Include(p => p.Category)
                            .Where(p => p.Category.CategoryName == "PhuKien")
                            .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                            .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(PhuKien);
        }
        public ActionResult AmThanh()
        {
            LoadFlashSalesToViewBag();
            var AmThanh = db.Products
                            .Include(p => p.Category)
                            .Where(p => p.Category.CategoryName == "AmThanh")
                            .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                            .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(AmThanh);
        }
        public ActionResult Camera()
        {
            LoadFlashSalesToViewBag();
            var Camera = db.Products
                            .Include(p => p.Category)
                            .Where(p => p.Category.CategoryName == "Camera")
                            .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                            .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(Camera);
        }
        public ActionResult GiaDung()
        {
            LoadFlashSalesToViewBag();
            var GiaDung = db.Products
                            .Include(p => p.Category)
                            .Where(p => p.Category.CategoryName == "GiaDung")
                            .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                            .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(GiaDung);
        }
        public ActionResult MayLuot()
        {
            LoadFlashSalesToViewBag();
            var MayLuot = db.Products
                            .Include(p => p.Category)
                            .Where(p => p.Category.CategoryName == "MayLuot")
                            .OrderByDescending(p => p.ProductID) // Lấy sp mới nhất
                            .ToList();

            //  Gửi danh sách ĐÃ LỌC này đến View
            return View(MayLuot);
        }
        public ActionResult TinTuc()
        {
            return View();
        }
        public ActionResult DichVu()
        {
            return View();
        }
        public ActionResult UserSighUp()
        {
            return View();
        }
        public ActionResult Address()
        {
            return View();
        }
        public ActionResult Confirm()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        
    }
}
