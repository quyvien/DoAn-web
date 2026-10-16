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
    [Authorize(Roles = "A")]
    public class ProductsController : Controller
    {
        private const int MaxImageSize = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif" };
        private MyStore2026Entities db = new MyStore2026Entities();

        private void LoadProductOptions(Product product = null)
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product?.CategoryID);
            ViewBag.ColorID = new SelectList(db.Colors, "ColorID", "ColorName", product?.ColorID);
            ViewBag.StorageID = new SelectList(db.Storages, "StorageID", "StorageName", product?.StorageID);
        }

        private void ValidateProductInput(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError("ProductName", "Vui lòng nhập tên sản phẩm.");
            }

            if (product.ProductPrice < 0)
            {
                ModelState.AddModelError("ProductPrice", "Giá sản phẩm không được âm.");
            }

            if (product.Quantity < 0)
            {
                ModelState.AddModelError("Quantity", "Số lượng sản phẩm không được âm.");
            }
        }

        private bool TrySaveProductImage(HttpPostedFileBase imageFile, out string virtualPath, out string error)
        {
            virtualPath = null;
            error = null;

            if (imageFile == null || imageFile.ContentLength == 0)
            {
                return true;
            }

            var extension = Path.GetExtension(imageFile.FileName);
            if (imageFile.ContentLength > MaxImageSize)
            {
                error = "Ảnh sản phẩm không được vượt quá 5 MB.";
                return false;
            }

            if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
            {
                error = "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc GIF.";
                return false;
            }

            try
            {
                using (var image = System.Drawing.Image.FromStream(imageFile.InputStream, true, true))
                {
                    if (image.Width <= 0 || image.Height <= 0)
                    {
                        throw new ArgumentException();
                    }
                }

                if (imageFile.InputStream.CanSeek)
                {
                    imageFile.InputStream.Position = 0;
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is OutOfMemoryException)
            {
                error = "Tệp tải lên không phải là ảnh hợp lệ.";
                return false;
            }

            var directoryPath = Server.MapPath("~/Content/Images/Products/");
            Directory.CreateDirectory(directoryPath);
            var safeFileName = Guid.NewGuid().ToString("N") + extension.ToLowerInvariant();
            imageFile.SaveAs(Path.Combine(directoryPath, safeFileName));
            virtualPath = "~/Content/Images/Products/" + safeFileName;
            return true;
        }

        // GET: Admin2026/Products
        public ActionResult Index(string searchString, decimal? minPrice, decimal? maxPrice, int? page)
        {
            var products = db.Products.Include(p => p.Category).Include(p => p.Color).Include(p => p.Storage).AsQueryable();


            if (!String.IsNullOrEmpty(searchString))
            {
                string searchUpper = searchString.ToUpper();
                products = products.Where(p =>
                    (p.ProductName != null && p.ProductName.ToUpper().Contains(searchUpper)) ||
                    (p.ProductDecription != null && p.ProductDecription.ToUpper().Contains(searchUpper))
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
            LoadProductOptions();
            return View();
        }

        // POST: Admin2026/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm Quantity vào Bind để cho phép nhập số lượng ban đầu
        public ActionResult Create([Bind(Include = "ProductID,ProductName,ProductDecription,ProductPrice,CategoryID,Quantity,ColorID,StorageID")] Product product, HttpPostedFileBase ImageFile)
        {
            ValidateProductInput(product);
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

                string imagePath;
                string imageError;
                if (!TrySaveProductImage(ImageFile, out imagePath, out imageError))
                {
                    ModelState.AddModelError("ImageFile", imageError);
                    LoadProductOptions(product);
                    return View(product);
                }

                if (imagePath != null)
                {
                    product.ProductImage = imagePath;
                }

                // Mặc định đã bán = 0 khi tạo mới
                product.SoldQuantity = 0;

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            LoadProductOptions(product);
            return View(product);
        }

        // GET: Admin2026/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            LoadProductOptions(product);
            return View(product);
        }

        // POST: Admin2026/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm tham số ImageFile để cho phép sửa ảnh
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDecription,ProductPrice,Quantity,ColorID,StorageID")] Product product, HttpPostedFileBase ImageFile)
        {
            ValidateProductInput(product);
            if (ModelState.IsValid)
            {
                // 1. RÀNG BUỘC: KIỂM TRA TRÙNG TÊN (Trừ chính nó ra)
                bool isExist = db.Products.Any(p => p.ProductName.ToLower() == product.ProductName.Trim().ToLower()
                                                 && p.ProductID != product.ProductID);
                if (isExist)
                {
                    ModelState.AddModelError("ProductName", "Tên sản phẩm đã bị trùng với một sản phẩm khác.");
                    LoadProductOptions(product);
                    return View(product);
                }

                var existingProduct = db.Products.Find(product.ProductID);
                if (existingProduct == null) return HttpNotFound();

                string imagePath;
                string imageError;
                if (!TrySaveProductImage(ImageFile, out imagePath, out imageError))
                {
                    ModelState.AddModelError("ImageFile", imageError);
                    product.ProductImage = existingProduct.ProductImage;
                    LoadProductOptions(product);
                    return View(product);
                }

                existingProduct.CategoryID = product.CategoryID;
                existingProduct.ProductName = product.ProductName;
                existingProduct.ProductDecription = product.ProductDecription;
                existingProduct.ProductPrice = product.ProductPrice;
                existingProduct.Quantity = product.Quantity;
                existingProduct.ColorID = product.ColorID;
                existingProduct.StorageID = product.StorageID;
                if (imagePath != null)
                {
                    existingProduct.ProductImage = imagePath;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            LoadProductOptions(product);
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
            if (product == null) return HttpNotFound();

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
                var imageDirectory = Path.GetFullPath(Server.MapPath("~/Content/Images/Products/"))
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var fullPath = Path.GetFullPath(Request.MapPath(product.ProductImage));
                if (fullPath.StartsWith(imageDirectory, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
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
