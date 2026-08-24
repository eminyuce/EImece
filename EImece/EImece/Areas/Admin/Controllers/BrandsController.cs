using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using EImece.Domain.Factories.IFactories;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class BrandsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected IBrandService BrandService { get; }
        protected IEntityFactory EntityFactory { get; }
        protected FilesHelper FilesHelper { get; }

        public BrandsController(
            ISettingService settingService,
            IBrandService brandService,
            IEntityFactory entityFactory,
            FilesHelper filesHelper)
            : base(settingService)
        {
            BrandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            FilesHelper = filesHelper ?? throw new ArgumentNullException(nameof(filesHelper));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            var brands = await BrandService.GetAdminPageListAsync(search, CurrentLanguage);
            return View(brands);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            var brands = await BrandService.GetAdminPageListAsync(search, CurrentLanguage);
            return new QueryableResult<Brand>(brands.AsQueryable());
        }

        //
        // GET: /Brand/Create

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<Brand>();

            if (id == 0)
            {
            }
            else
            {
                content = await BrandService.GetBaseContentAsync(id, cancellationToken);
            }

            return View(content);
        }

        //
        // POST: /Brand/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, Brand brand, int[] tags = null, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (brand == null)
                {
                    return HttpNotFound();
                }

                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(postedImage,
                        brand.ImageHeight,
                        brand.ImageWidth,
                        EImeceImageType.BrandMainImage, brand);

                    brand.Lang = CurrentLanguage;
                    brand = await BrandService.SaveOrEditEntityAsync(brand);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }
                    else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    }
                }
                else
                {
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, brand);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.StackTrace);
            }

            RemoveModelState();

            return View(brand);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            Brand Brand = await BrandService.GetBaseContentAsync(id, cancellationToken);
            if (Brand == null)
            {
                return HttpNotFound();
            }
            try
            {
                await BrandService.DeleteBrandByIdAsync(id);
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, Brand);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(format);
        }

        private async Task<ActionResult> DownloadFileAsync(string format = "excel")
        {
            String search = "";
            var brands = await BrandService.GetAdminPageListAsync(search, CurrentLanguage);

            var result = from r in brands
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             Description = r.Description,
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("brands-{0}", GetCurrentLanguage), format);
        }
    }
}
