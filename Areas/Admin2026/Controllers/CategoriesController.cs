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
            if (ModelState.IsValid)
            {
                bool Check = db.Categories.Any(c=>c.CategoryName.ToLower()==category.CategoryName.Trim().ToLower());
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
            if (ModelState.IsValid)
            {
                // ktra coi co trung ten ko
                bool CheckTen = db.Categories.Any(c => c.CategoryName.ToLower() == category.CategoryName.Trim().ToLower()
                                                  && c.CategoryID != category.CategoryID );
                if (CheckTen)
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục này đã được sử dụng!error... ");
                    return View(category);
                }
            }
            db.Entry(category).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
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
            // kt coi co sp ko
            if (category.Products.Count > 0)
            {
                TempData["Error"] = "Không thể xóa danh mục này vì đang có sản phẩm bên trong!error...";
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
