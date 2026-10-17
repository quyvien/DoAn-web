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
    [Authorize(Roles = "A")]
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
        public ActionResult Create([Bind(Include = "ProductID,SpecName,SpecValue,OrderIndex")] ProductDetail detail)
        {
            if (!db.Products.Any(p => p.ProductID == detail.ProductID)) return HttpNotFound();
            if (string.IsNullOrWhiteSpace(detail.SpecName))
                ModelState.AddModelError("SpecName", "Vui lòng nhập tên thông số.");
            if (string.IsNullOrWhiteSpace(detail.SpecValue))
                ModelState.AddModelError("SpecValue", "Vui lòng nhập giá trị thông số.");
            if (detail.OrderIndex < 0)
                ModelState.AddModelError("OrderIndex", "Thứ tự không được âm.");

            if (ModelState.IsValid)
            {
                detail.SpecName = detail.SpecName.Trim();
                detail.SpecValue = detail.SpecValue.Trim();
                db.ProductDetails.Add(detail);
                db.SaveChanges();
            }
            else
            {
                TempData["Error"] = "Thông số không hợp lệ. Vui lòng kiểm tra lại dữ liệu.";
            }
            // Thêm xong thì load lại chính trang danh sách của sản phẩm đó
            return RedirectToAction("Index", new { id = detail.ProductID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var detail = db.ProductDetails.Find(id);
            if (detail == null) return HttpNotFound();
            int productID = detail.ProductID; // Lưu lại ID để quay về đúng chỗ

            db.ProductDetails.Remove(detail);
            db.SaveChanges();

            return RedirectToAction("Index", new { id = productID });
        }
    }
}
