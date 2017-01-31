using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public ActionResult Index(int id = 0, String search = "")
        {
            int lang = Settings.MainLanguage;
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList();
            var products = ProductService.GetAdminPageList(id, search, lang);
            return View(products);
        }
        public ActionResult SaveOrEditProductSpecs(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product content = ProductService.GetProductById(id).Product;
            ViewBag.Template = TemplateService.GetSingle(content.ProductCategory.TemplateId);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }
        [HttpPost]
        public ActionResult SaveOrEditProductSpecs(int id,int templateId)
        {
            var template = TemplateService.GetSingle(templateId);
            XDocument xdoc = XDocument.Parse(template.TemplateXml);
            var groups = xdoc.Root.Descendants("group");
            var Specifications = new List<ProductSpecification>();

            foreach (var group in groups)
            {
                var groupName = group.FirstAttribute.Value;
                foreach (XElement field in group.Elements())
                {

                    var p = new  ProductSpecification();
                    p.GroupName = groupName;
                    p.ProductId = id;
                    p.CreatedDate = DateTime.Now;
                    p.UpdatedDate = DateTime.Now;
                    p.Position = 1;
                    p.IsActive = true;
                    p.Lang = CurrentLanguage;
                    var name = field.Attribute("name");
                    var unit = field.Attribute("unit");
                    var values = field.Attribute("values");


                    var value = Request.Form[name.Value];

                    if (name != null)
                    {
                        p.Name = name.Value;
                    }
                    if (unit != null)
                    {
                        p.Unit = unit.Value;
                    }

                    p.Value = value;
                    Specifications.Add(p);

                }
            }

            ProductService.SaveProductSpecifications(Specifications);
            return RedirectToAction("SaveOrEditProductSpecs", new { id });
        }
        //
        // GET: /Product/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product content = ProductService.GetBaseContent(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }

        //
        // GET: /Product/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = EntityFactory.GetBaseContentInstance<Product>();
            var productCategory = EntityFactory.GetBaseContentInstance<ProductCategory>();
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList();

            if (id == 0)
            {
                content.ProductCategoryId = 0;
            }
            else
            {

                content = ProductService.GetBaseContent(id);
                productCategory = ProductCategoryService.GetSingle(content.ProductCategoryId);
            }
            ViewBag.ProductCategory = productCategory;
            return View(content);
        }

        //
        // POST: /Product/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Product product, int[] tags = null, HttpPostedFileBase productImage = null)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    if (product.ProductCategoryId == 0)
                    {
                        ModelState.AddModelError("ProductCategoryId", "You should select category from category tree.");
                    }
                    else
                    {

                       FilesHelper.SaveFileFromHttpPostedFileBase(
                            productImage,
                            product.ImageHeight,
                            product.ImageWidth,
                            EImeceImageType.ProductMainImage,
                             product);


                        ProductService.SaveOrEditEntity(product);
                        int contentId = product.Id;

                        if (tags != null)
                        {
                            ProductService.SaveProductTags(product.Id, tags);
                        }
                        return RedirectToAction("Index");
                    }


                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, product);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.StackTrace);
            }
            ViewBag.Tree = ProductCategoryService.CreateProductCategoryTreeViewDataList();
            ViewBag.ProductCategory = ProductCategoryService.GetSingle(product.ProductCategoryId);
            if (product.MainImageId.HasValue)
            {
                product.MainImage = FileStorageService.GetSingle(product.MainImageId.Value);
            }
            return View(product);
        }



        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product content = ProductService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }


            return View(content);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {

            Product product = ProductService.GetSingle(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                ProductService.DeleteProductById(id);
                return RedirectToAction("Index", new { categoryId = product.ProductCategoryId });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, product);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(product);

        }
        public ActionResult Media(int id)
        {
            return RedirectToAction("Index", "Media", new { contentId = id, mod = MediaModType.Products, imageType = EImeceImageType.ProductGallery });
        }

    }
}