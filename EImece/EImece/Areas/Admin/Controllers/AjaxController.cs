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
        // GET: Admin/Ajax

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