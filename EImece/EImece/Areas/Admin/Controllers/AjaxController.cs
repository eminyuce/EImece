using EImece.Domain.GenericRepositories;
using EImece.Domain.Models.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class AjaxController : BaseAdminController
    {

        [HttpPost]
        public ActionResult StoryGridItem(List<String> values)
        {
            BaseEntityRepository.DeleteBaseEntity(StoryRepository, values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult StoryCategoryGridItem(List<String> values)
        {
            BaseEntityRepository.DeleteBaseEntity(StoryCategoryRepository, values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        // GET: Admin/Ajax
        [HttpPost]
        public ActionResult DeleteSubscriberGridItem(List<String> values)
        {
            BaseEntityRepository.DeleteBaseEntity(SubscriberRepository, values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteProductGridItem(List<String> values)
        {
            BaseEntityRepository.DeleteBaseEntity(ProductRepository, values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeProductGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            BaseEntityRepository.ChangeGridBaseEntityOrderingOrState(ProductRepository, values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
    }
}