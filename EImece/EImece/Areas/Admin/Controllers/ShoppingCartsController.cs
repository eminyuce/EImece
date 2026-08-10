using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ShoppingCartsController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IShoppingCartService ShoppingCartService { get; set; }

        // GET: Admin/ShoppingCarts
        public ActionResult Index(String search = "")
        {
            var items = ShoppingCartService.GetAdminPageList(search, CurrentLanguage);
            return View(items);
        }

        [HttpGet, ActionName("ExportExcel")]
        public ActionResult ExportExcel(string format = "excel", string search = "")
        {
            var items = ShoppingCartService.GetAdminPageList(search ?? "", CurrentLanguage);
            var result = items.Select(r =>
            {
                int itemCount;
                int quantity;
                decimal? total;
                string email;
                SummarizeCartJson(r.ShoppingCartJson, out itemCount, out quantity, out total, out email);
                return new
                {
                    r.Id,
                    r.Name,
                    r.OrderGuid,
                    r.UserId,
                    CustomerEmail = email,
                    ItemCount = itemCount,
                    Quantity = quantity,
                    TotalPrice = total,
                    r.IsActive,
                    r.CreatedDate,
                    r.UpdatedDate
                };
            });

            return DownloadFile(result, string.Format("shoppingcarts-{0}", GetCurrentLanguage), format);
        }

        private static void SummarizeCartJson(string json, out int itemCount, out int quantity, out decimal? total, out string email)
        {
            itemCount = 0;
            quantity = 0;
            total = null;
            email = "";
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }
            try
            {
                var token = JToken.Parse(json);
                var sessionItems = token["ShoppingCartItems"] as JArray;
                var seedItems = token["Items"] as JArray;
                if (sessionItems != null && sessionItems.Count > 0)
                {
                    itemCount = sessionItems.Count;
                    quantity = sessionItems.Sum(i => i.Value<int?>("Quantity") ?? 0);
                    var session = token.ToObject<ShoppingCartSession>();
                    if (session != null)
                    {
                        total = session.TotalPrice;
                        if (session.Customer != null)
                        {
                            email = session.Customer.Email ?? "";
                        }
                    }
                }
                else if (seedItems != null && seedItems.Count > 0)
                {
                    itemCount = seedItems.Count;
                    quantity = seedItems.Sum(i => i.Value<int?>("Quantity") ?? 0);
                }
                else
                {
                    var emailToken = token.SelectToken("Customer.Email");
                    if (emailToken != null)
                    {
                        email = emailToken.ToString();
                    }
                }
            }
            catch
            {
                // ignore malformed json rows in export
            }
        }

        public ActionResult Detail(int id)
        {
            var shoppingCart = ShoppingCartService.GetSingle(id);
            if (shoppingCart == null)
            {
                return HttpNotFound();
            }
            return View(shoppingCart);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            var item = ShoppingCartService.GetSingle(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            try
            {
                ShoppingCartService.DeleteById(id);
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, item);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
        }
    }
}