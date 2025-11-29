using DoAn_web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Data.Entity;



namespace DoAn_web.Controllers
{
    public class ProductController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();
        // GET: Product
        public ActionResult Index(string searchString)
        {
            // Tạo truy vấn lấy tất cả sản phẩm
            var products = from p in db.Products
                           select p;
            // Nếu có chuỗi tìm kiếm, lọc sản phẩm
            if (!String.IsNullOrEmpty(searchString))
            {
                string searchUpper = searchString.ToUpper();
                products = products.Where(p => p.ProductName.ToUpper().Contains(searchUpper) ||
                                               p.ProductDecription.ToUpper().Contains(searchUpper));

            }
            // Truyền chuỗi tìm kiếm hiện tại về View để hiển thị lại trong ô tìm kiếm
            ViewBag.CurrentFilter = searchString;
            return View(products.ToList());
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // dung id de tim sp trong database
            var product = db.Products
                    .Include("Category")  // liên kết danh mục
                    .Include("Color")     //  Lấy bảng Màu
                    .Include("Storage")   // Lấy bảng GB
                    .FirstOrDefault(p => p.ProductID == id);

            // neu ko tim thay thi bao loi
            if (product == null) 
                return HttpNotFound();
            // coi coi sp nay co dang sale hay khom
            var activeFlashSale = db.FlashSaleItems.FirstOrDefault(f =>
                                   f.ProductID == id &&
                                   f.IsActive == true &&
                                   f.StartDate <= DateTime.Now &&
                                   f.EndDate >= DateTime.Now);
            // gui nay vo view
            ViewBag.FlashSale = activeFlashSale;
            // lấy 3 sp phụ kiện
            var comboAccessories = db.Products
                             .Include(p => p.Category)
                             .Where(p => p.Category.CategoryName == "PhuKien")
                             .OrderByDescending(p => p.ProductID)
                             .Take(3)   // lấy 3–4 cái tuỳ bạn
                             .ToList();
            ViewBag.ComboAccessories = comboAccessories;

            // cùng Category, khác ProductID hiện tại, lấy tối đa 4 sp
            var related = db.Products
                    .Where(p => p.CategoryID == product.CategoryID
                             && p.ProductID != product.ProductID)
                    .OrderByDescending(p => p.ProductID)
                    .Take(4)
                    .ToList();

            ViewBag.RelatedProducts = related;

            return View(product);

        }
        public ActionResult LiveSearch( string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return new EmptyResult(); // tra ve cchuoi rong neu khong co du lieu

            var products = db.Products
                              .Where(p => p.ProductName.ToUpper().Contains(searchString.ToUpper()))
                              .Take(5)
                              .ToList();
            return PartialView("_LiveSearchResults", products);                    
        }














        public ActionResult Iphon17()
        {
            return View();
        }
        public ActionResult IphoneAir()
        {
            return View();
        }
        public ActionResult MacbookAirM3()
        {
            return View();
        }
        public ActionResult MacbookAirM4()
        {
            return View();
        }
        public ActionResult WatchUltra2()
        {
            return View();
        }
        public ActionResult WatchSeGPS()
        {
            return View();
        }
        public ActionResult IpadAirM3()
        {
            return View();
        }
        public ActionResult IpadA16()
        {
            return View();
        }
        
    }
}