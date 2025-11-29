using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAn_web.Models; // Đổi tên này nếu namespace của bạn khác

namespace DoAn_web.Areas.Admin2026.Controllers
{
    public class ProductDetailController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();

        // Trang này vừa hiện danh sách, vừa có form thêm mới luôn cho tiện
        public ActionResult Index(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            // Gửi ID và Tên sản phẩm sang View để hiển thị tiêu đề
            ViewBag.CurrentProductID = id;
            ViewBag.CurrentProductName = product.ProductName;

            // Lấy danh sách thông số của sản phẩm này, sắp xếp theo thứ tự
            var details = db.ProductDetails.Where(d => d.ProductID == id).OrderBy(d => d.OrderIndex).ToList();
            return View(details);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductDetail detail)
        {
            if (ModelState.IsValid)
            {
                db.ProductDetails.Add(detail);
                db.SaveChanges();
            }
            // Thêm xong thì load lại chính trang danh sách của sản phẩm đó
            return RedirectToAction("Index", new { id = detail.ProductID });
        }

        public ActionResult Delete(int id)
        {
            var detail = db.ProductDetails.Find(id);
            int productID = detail.ProductID; // Lưu lại ID để quay về đúng chỗ

            db.ProductDetails.Remove(detail);
            db.SaveChanges();

            return RedirectToAction("Index", new { id = productID });
        }
    }
}