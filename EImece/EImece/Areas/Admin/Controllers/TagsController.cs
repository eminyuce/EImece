﻿using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class TagsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            var result = TagService.GetAdminPageList(search);
            return View(result);
        }


        //
        // GET: /Tag/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tag content = TagService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }
        private List<SelectListItem> GetCategoriesSelectList()
        {

           List<TagCategory> tagCategories = TagCategoryService.GetAll().OrderBy(r => r.Position).ToList();
           return  tagCategories.Select(r => new SelectListItem()
            {
                Text =

                String.Format("{0} -- {1}", r.Name.ToStr(), (EImeceTagType)r.TagType)
                ,
                Value = r.Id.ToStr()
            }).ToList();
        }
        //
        // GET: /Tag/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = new Tag();
            ViewBag.Categories = GetCategoriesSelectList();

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
            }
            else
            {
                content = TagService.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
            }


            return View(content);
        }

        //
        // POST: /Tag/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Tag Tag)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    TagService.SaveOrEditEntity(Tag);
                    int contentId = Tag.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, Tag);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator."+ex.Message);
            }
            ViewBag.Categories = GetCategoriesSelectList();
            return View(Tag);
        }



        //
        // GET: /Tag/Delete/5
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tag content = TagService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }


            return View(content);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            Tag Tag = TagService.GetSingle(id);
            if (Tag == null)
            {
                return HttpNotFound();
            }
            try
            {
                TagService.DeleteEntity(Tag);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, Tag);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(Tag);

        }
    }
}