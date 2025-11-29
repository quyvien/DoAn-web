using DoAn_web.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DoAn_web.Areas.Admin2026.Controllers
{
    public class ProductsController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();

        // GET: Admin2026/Products
        public ActionResult Index(string searchString, decimal? minPrice, decimal? maxPrice, int? page)
        {
            var products = db.Products.Include(p => p.Category).Include(p => p.Color).Include(p => p.Storage).AsQueryable();


            if (!String.IsNullOrEmpty(searchString))
            {
                string searchUpper = searchString.ToUpper();
                products = products.Where(p =>
                    p.ProductName.ToUpper().Contains(searchUpper) ||
                    p.ProductDecription.ToUpper().Contains(searchUpper)
                );
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
                ViewBag.MinFilter = minPrice;
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
                ViewBag.MaxFilter = maxPrice;
            }

            ViewBag.CurrentFilter = searchString;

            // Sắp xếp sản phẩm mới nhất lên đầu
            products = products.OrderByDescending(p => p.ProductID);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin2026/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // GET: Admin2026/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            ViewBag.ColorID = new SelectList(db.Colors, "ColorID", "ColorName");
            ViewBag.StorageID = new SelectList(db.Storages, "StorageID", "StorageName");

            return View();
        }

        // POST: Admin2026/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm Quantity vào Bind để cho phép nhập số lượng ban đầu
        public ActionResult Create([Bind(Include = "ProductID,ProductName,ProductDecription,ProductPrice,CategoryID,Quantity,ColorID,StorageID")] Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                // 1. RÀNG BUỘC: KIỂM TRA TRÙNG TÊN SẢN PHẨM
                bool isExist = db.Products.Any(p => p.ProductName.ToLower() == product.ProductName.Trim().ToLower());
                if (isExist)
                {
                    ModelState.AddModelError("ProductName", "Tên sản phẩm này đã tồn tại! Vui lòng chọn tên khác.");
                    ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
                    return View(product);
                }

                // 2. XỬ LÝ ẢNH
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    // Thêm timestamp để tránh trùng tên file ảnh
                    var fileNameWithTime = DateTime.Now.Ticks + "_" + fileName;
                    var serverPath = Path.Combine(Server.MapPath("~/Content/Images/Products/"), fileNameWithTime);

                    // Kiểm tra thư mục tồn tại chưa, nếu chưa thì tạo
                    string directoryPath = Server.MapPath("~/Content/Images/Products/");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    ImageFile.SaveAs(serverPath);
                    product.ProductImage = "~/Content/Images/Products/" + fileNameWithTime;
                }
                else
                {
                    // Nếu không up ảnh, gán ảnh mặc định (tùy chọn)
                    //product.ProductImage = "~/Content/Images/Products/default.png";
                }

                // Mặc định đã bán = 0 khi tạo mới
                product.SoldQuantity = 0;

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.ColorID = new SelectList(db.Colors, "ColorID", "ColorName", product.ColorID);
            ViewBag.StorageID = new SelectList(db.Storages, "StorageID", "StorageName", product.StorageID);
            return View(product);
        }

        // GET: Admin2026/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
             ViewBag.ColorID = new SelectList(db.Colors, "ColorID", "ColorName", product.ColorID);
            ViewBag.StorageID = new SelectList(db.Storages, "StorageID", "StorageName", product.StorageID);
            return View(product);
        }

        // POST: Admin2026/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm tham số ImageFile để cho phép sửa ảnh
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDecription,ProductPrice,ProductImage,Quantity,SoldQuantity,ColorID,StorageID")] Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                // 1. RÀNG BUỘC: KIỂM TRA TRÙNG TÊN (Trừ chính nó ra)
                bool isExist = db.Products.Any(p => p.ProductName.ToLower() == product.ProductName.Trim().ToLower()
                                                 && p.ProductID != product.ProductID);
                if (isExist)
                {
                    ModelState.AddModelError("ProductName", "Tên sản phẩm đã bị trùng với một sản phẩm khác.");
                    ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
                    return View(product);
                }

                // 2. XỬ LÝ ẢNH KHI EDIT
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    // Nếu người dùng chọn ảnh mới -> Lưu ảnh mới
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var fileNameWithTime = DateTime.Now.Ticks + "_" + fileName;
                    var serverPath = Path.Combine(Server.MapPath("~/Content/Images/Products/"), fileNameWithTime);

                    ImageFile.SaveAs(serverPath);
                    product.ProductImage = "~/Content/Images/Products/" + fileNameWithTime;
                }
                else
                {
                    // Nếu không chọn ảnh mới -> Giữ nguyên ảnh cũ
                    // (Lưu ý: Form Edit phải có HiddenField cho ProductImage, nếu không thì phải load lại từ DB)
                    // Cách an toàn nhất để giữ ảnh cũ nếu Bind bị null:
                    if (string.IsNullOrEmpty(product.ProductImage))
                    {
                        var oldItem = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
                        if (oldItem != null) product.ProductImage = oldItem.ProductImage;
                    }
                }

                db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.ColorID = new SelectList(db.Colors, "ColorID", "ColorName", product.ColorID);
            ViewBag.StorageID = new SelectList(db.Storages, "StorageID", "StorageName", product.StorageID);
            return View(product);
        }

        // GET: Admin2026/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // POST: Admin2026/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);

            // 1. RÀNG BUỘC: KHÔNG CHO XÓA NẾU ĐÃ CÓ ĐƠN HÀNG MUA SẢN PHẨM NÀY
            // Kiểm tra trong bảng OrderDetails
            if (db.OrderDetails.Any(od => od.ProductID == id))
            {
                TempData["Error"] = "Không thể xóa sản phẩm này vì đã có trong lịch sử đơn hàng. Hãy ẩn nó đi hoặc xóa đơn hàng trước.";
                return RedirectToAction("Index");
            }

            // Xóa file ảnh cũ trên server cho sạch bộ nhớ (Tùy chọn)
            if (!string.IsNullOrEmpty(product.ProductImage))
            {
                string fullPath = Request.MapPath(product.ProductImage);
                if (System.IO.File.Exists(fullPath))
                {
                    try { System.IO.File.Delete(fullPath); } catch { /* Bỏ qua nếu lỗi xóa file */ }
                }
            }

            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
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