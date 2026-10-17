using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DoAn_web.Models;


namespace DoAn_web.Areas.Admin2026.Controllers
{
    [Authorize(Roles = "A")]
    public class CategoriesController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();

        // GET: Admin2026/Categories
        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        // GET: Admin2026/Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Admin2026/Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin2026/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Vui lòng nhập tên danh mục.");
            }

            if (ModelState.IsValid)
            {
                category.CategoryName = category.CategoryName.Trim();
                bool Check = db.Categories.Any(c => c.CategoryName == category.CategoryName);
                if (Check)
                {
                    // neu co roi thi thong bao loi
                    ModelState.AddModelError("CategoryName", "Tên danh mục này đã tồn tại");
                    return View(category);
                }
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            


            return View(category);
        }

        // GET: Admin2026/Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin2026/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Vui lòng nhập tên danh mục.");
            }

            if (ModelState.IsValid)
            {
                category.CategoryName = category.CategoryName.Trim();
                // ktra coi co trung ten ko
                bool CheckTen = db.Categories.Any(c => c.CategoryName == category.CategoryName
                                                  && c.CategoryID != category.CategoryID );
                if (CheckTen)
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục này đã được sử dụng!error... ");
                    return View(category);
                }

                var existingCategory = db.Categories.Find(category.CategoryID);
                if (existingCategory == null) return HttpNotFound();
                existingCategory.CategoryName = category.CategoryName;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin2026/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin2026/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();
            // kt coi co sp ko
            if (category.Products.Count > 0)
            {
                TempData["Error"] = "Không thể xóa danh mục này vì đang có sản phẩm bên trong.";
                return RedirectToAction("Index");
            }
            db.Categories.Remove(category);
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
