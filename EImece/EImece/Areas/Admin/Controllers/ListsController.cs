using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using EImece.Domain.Factories.IFactories;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class ListsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected IListService ListService { get; }
        protected IListItemService ListItemService { get; }
        protected IEntityFactory EntityFactory { get; }

        public ListsController(
            ISettingService settingService,
            IListService listService,
            IListItemService listItemService,
            IEntityFactory entityFactory)
            : base(settingService)
        {
            ListService = listService ?? throw new ArgumentNullException(nameof(listService));
            ListItemService = listItemService ?? throw new ArgumentNullException(nameof(listItemService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<List, bool>> whereLambda = r => r.Name.Contains(search);
            var tags = await ListService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(tags);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            Expression<Func<List, bool>> whereLambda = r => r.Name.Contains(search);
            var tags = await ListService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return new QueryableResult<List>(tags.AsQueryable());
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<List>();

            if (id == 0)
            {
            }
            else
            {
                content = await ListService.GetListByIdAsync(id, cancellationToken);
            }

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(List list, string itemText, String saveButton = null)
        {
            if (list == null)
            {
                throw new ArgumentException("list cannot be empty");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    list.Lang = CurrentLanguage;
                    list = await ListService.SaveOrEditEntityAsync(list);
                    var listItems = list.SetListItems(itemText);
                    await ListItemService.SaveListItemAsync(list.Id, listItems);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    RemoveModelState();
                    return View(list);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, list);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            return View(list);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            List List = await ListService.GetSingleAsync(id);
            if (List == null)
            {
                return HttpNotFound();
            }
            try
            {
                await ListService.DeleteListByIdAsync(id);
                SetSuccessMessage();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, List);
                SetErrorMessage();
                return RedirectToAction("Index");
            }
        }
    }
}
