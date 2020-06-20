using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;

namespace EImece.Areas.Admin.Controllers
{
    public class TestAsyncController : Controller
    {
        private EImeceContext db = new EImeceContext();

        // GET: Admin/TestAsync
        public async Task<ActionResult> Index()
        {
            var productCategories = db.ProductCategories.Include(p => p.MainImage).Include(p => p.Template);
            return View(await productCategories.ToListAsync());
        }

        // GET: Admin/TestAsync/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductCategory productCategory = await db.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                return HttpNotFound();
            }
            return View(productCategory);
        }

        // GET: Admin/TestAsync/Create
        public ActionResult Create()
        {
            ViewBag.MainImageId = new SelectList(db.FileStorages, "Id", "FileName");
            ViewBag.TemplateId = new SelectList(db.Templates, "Id", "TemplateXml");
            return View();
        }

        // POST: Admin/TestAsync/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,ParentId,MainPage,TemplateId,DiscountPercantage,Description,ImageState,MetaKeywords,MainImageId,UpdateUserId,AddUserId,Name,EntityHash,CreatedDate,UpdatedDate,IsActive,Position,Lang")] ProductCategory productCategory)
        {
            if (ModelState.IsValid)
            {
                db.ProductCategories.Add(productCategory);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.MainImageId = new SelectList(db.FileStorages, "Id", "FileName", productCategory.MainImageId);
            ViewBag.TemplateId = new SelectList(db.Templates, "Id", "TemplateXml", productCategory.TemplateId);
            return View(productCategory);
        }

        // GET: Admin/TestAsync/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductCategory productCategory = await db.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                return HttpNotFound();
            }
            ViewBag.MainImageId = new SelectList(db.FileStorages, "Id", "FileName", productCategory.MainImageId);
            ViewBag.TemplateId = new SelectList(db.Templates, "Id", "TemplateXml", productCategory.TemplateId);
            return View(productCategory);
        }

        // POST: Admin/TestAsync/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ParentId,MainPage,TemplateId,DiscountPercantage,Description,ImageState,MetaKeywords,MainImageId,UpdateUserId,AddUserId,Name,EntityHash,CreatedDate,UpdatedDate,IsActive,Position,Lang")] ProductCategory productCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productCategory).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.MainImageId = new SelectList(db.FileStorages, "Id", "FileName", productCategory.MainImageId);
            ViewBag.TemplateId = new SelectList(db.Templates, "Id", "TemplateXml", productCategory.TemplateId);
            return View(productCategory);
        }

        // GET: Admin/TestAsync/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductCategory productCategory = await db.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                return HttpNotFound();
            }
            return View(productCategory);
        }

        // POST: Admin/TestAsync/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ProductCategory productCategory = await db.ProductCategories.FindAsync(id);
            db.ProductCategories.Remove(productCategory);
            await db.SaveChangesAsync();
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
