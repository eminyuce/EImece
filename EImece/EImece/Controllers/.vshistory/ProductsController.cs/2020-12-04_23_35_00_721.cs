﻿using EImece.Domain;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.ProductsControllerRoutingPrefix)]
    public class ProductsController : BaseController
    {
        private readonly IProductCommentService productCommentService;

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        public ProductsController(IProductCommentService ProductCommentService)
        {
            this.productCommentService = ProductCommentService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        //       [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        //       public ActionResult Index(int page = 1)
        //       {
        //           var products = ProductService.GetMainPageProducts(page, CurrentLanguage);
        //           return View(products);
        //       }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult AdvancedSearchProducts(String search = "", string filters = "", String page = "")
        {
            var products = ProductService.GetProductsSearchResult(search, filters, page, CurrentLanguage);
            return View(products);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public ActionResult Detail(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var productId = id.GetId();
            var product = ProductService.GetProductDetailViewModelById(productId);
            ViewBag.SeoId = product.Product.GetSeoUrl();

            return View(product);
        }

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        [Route("etiket/{id}")]
        public ActionResult Tag(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var tagId = id.GetId();
            int pageIndex = 1;
            int pageSize = 20;
            SimiliarProductTagsViewModel products = ProductService.GetProductByTagId(tagId, pageIndex, pageSize, CurrentLanguage);
            ViewBag.SeoId = products.Tag.GetSeoUrl();
            return View(products);
        }

        public ActionResult SearchProducts(String search, int page = 1, int sorting = 0)
        {
            if (String.IsNullOrEmpty(search))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int pageSize = AppConfig.ProductDefaultRecordPerPage;
            var products = ProductService.SearchProducts(page, pageSize, search, CurrentLanguage, (SortingType)sorting);
            products.RecordPerPage = pageSize;
            products.Page = page;
            products.Sorting = (SortingType)sorting;
            return View(products);
        }

        [HttpPost]
        public ActionResult Review(ProductComment productComment)
        {
            if (productComment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var users = ApplicationDbContext.Users.AsQueryable();
            var user = users.Where(u => u.UserName.Equals(productComment.Email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            productComment.UserId = user == null ? "" : user.Id;
            productComment.CreatedDate = DateTime.Now;
            productComment.UpdatedDate = DateTime.Now;
            productComment.IsActive = false;
            productComment.Position = 1;
            productComment.Lang = CurrentLanguage;
            productCommentService.SaveOrEditEntity(productComment);
            return RedirectToAction("Detail", new { id = productComment.SeoUrl });
        }
    }
}