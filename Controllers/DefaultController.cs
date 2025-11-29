using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAn_web.Models;
using DoAn_web.ViewModels;
using System.Data.Entity;
using System.Web.UI.WebControls.WebParts;

namespace DoAn_web.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
      
         private MyStore2026Entities db = new MyStore2026Entities();
        private void LoadFlashSalesToViewBag()
        {
           var now = DateTime.Now;  
            // lay cac danh sach Flashsale dang chay
            var activeFlashSales = db.FlashSaleItems
                                     .Where(f=> f.IsActive && f.StartDate<=now && f.EndDate>=now)
                                     .OrderBy(f=>f.EndDate)  /*lay cai het  han lam moc*/
                                     .ToList();
             // lay thong tin chi tiet cua sp FlashSale
             var flashsaleProducts =( from f in activeFlashSales
                                      join p in db.Products on f.ProductID equals p.ProductID
                                      select new Product
                                      {
                                          ProductID=p.ProductID,
                                          ProductName =p.ProductName,
                                          ProductImage =p.ProductImage,
                                          ProductPrice =p.ProductPrice,
                                          
                                      }).ToList();
            ViewBag.FlashSaleProducts = flashsaleProducts;// hien tho list sp
            ViewBag.FlashSales =  activeFlashSales;// hien thi gia
            if(activeFlashSales.Count>0)
            {
                ViewBag.FlashSalesEndTime = activeFlashSales.First().EndDate.ToString("yyyy-MM-dd HH:mm:ss");

            }
        }

        public ActionResult Index()
        {
            LoadFlashSalesToViewBag();
           // tao cai hop cho ViewModel
           var viewModel = new HomeViewModel();

            // lay 4 san pham iphone ra
            viewModel.Iphones=db.Products
                .Include(p=>p.Category)
                .Where(p=>p.Category.CategoryName=="Iphone")// loc
                .OrderByDescending(p=>p.ProductID)// lay sp moi nhat
                .Take(4)// lay 4 sp
                .ToList();

            // 4 sp ipad
                        viewModel.Ipads = db.Products
                .Include(p => p.Category)
                .Where(p => p.Category.CategoryName == "Ipad")// loc
                .OrderByDescending(p => p.ProductID)// lay sp moi nhat
                .Take(4)// lay 4 sp
                .ToList();
            // 4 sp mac
                        viewModel.Macs = db.Products
                .Include(p => p.Category)
                .Where(p => p.Category.CategoryName == "Mac")// loc
                .OrderByDescending(p => p.ProductID)// lay sp moi nhat
                .Take(4)// lay 4 sp
                    .ToList();
            // 4 sp watch
                        viewModel.Watches = db.Products
                .Include(p => p.Category)
                .Where(p => p.Category.CategoryName == "Watch")// loc
                .OrderByDescending(p => p.ProductID)// lay sp moi nhat
                .Take(4)// lay 4 sp
                .ToList();
            // 4 sp watch
            viewModel.PhuKiens = db.Products
            .Include(p => p.Category)
            .Where(p => p.Category.CategoryName == "PhuKien")// loc
            .OrderByDescending(p => p.ProductID)// lay sp moi nhat
            .Take(4)// lay 4 sp
            .ToList();
            //6 gui du lieu ve View
            return View(viewModel);




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
        private int GetCurrentCustomerID()
        {
            var username = User.Identity.Name;
           
            using (var db = new MyStore2026Entities())
            {
                var customer = db.Customers.FirstOrDefault(c => c.Username == username);
                return customer.CustomerID;
            }
        }

        // GET: /Default/OrderHistory
        [Authorize(Roles = "C")] // Chỉ Customer mới xem được
        public ActionResult OrderHistory()
        {
            int customerID = GetCurrentCustomerID(); // (Copy hàm này từ CartController sang hoặc gom vào BaseController)
            var orders = db.Orders.Where(o => o.CustomerID == customerID).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }
        public ActionResult CancelOrder(int id)
        {
            //Lấy id của người dùng đang đăng nhập
            int customerId = GetCurrentCustomerID();
            //Truy vấn db tìm cái đầu tiên nếu k thì trả về null
            var order = db.Orders.FirstOrDefault(o => o.OrderID == id && o.CustomerID == customerId);
            if (order != null && ((order.PaymentStatus == "Chờ xác nhận chuyển khoản" || order.PaymentStatus == "Chờ xác nhận chuyển khoảng" ||
                                                    order.PaymentStatus == "Chờ xử lý" ||
                                                    order.PaymentStatus == "Chưa thanh toán")) && order.ShippingStatus != "Đang vận chuyển" && order.ShippingStatus != "Đã giao thành công")
            {
                order.PaymentStatus = "Đã Huỷ !";
                order.ShippingStatus = "Đã Huỷ !";

                db.SaveChanges();
                //Cập nhật 
                TempData["SuccessMessage"] = "Order #" + id + "has been cancelled !";
            }
            else
            {
                TempData["ErrorMessage"] = "Cannot cancel this order !";
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
        
    }
}