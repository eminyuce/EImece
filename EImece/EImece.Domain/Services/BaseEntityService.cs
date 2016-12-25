using EImece.Domain.Entities;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories.IRepositories;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public abstract class BaseEntityService<T> : BaseService<T> where T : BaseEntity
    {

        [Inject]
        public ISettingRepository SettingRepository { get; set; }
        [Inject]
        public IFileStorageRepository FileStorageRepository { get; set; }
        [Inject]
        public IProductSpecificationRepository ProductSpecificationRepository { get; set; }
        [Inject]
        public IStoryFileRepository StoryFileRepository { get; set; }
        [Inject]
        public ISubscriberRepository SubscriberRepository { get; set; }
        [Inject]
        public ITagCategoryRepository TagCategoryRepository { get; set; }
        [Inject]
        public ITagRepository TagRepository { get; set; }

        public virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda,String search)
        {
            var menus = baseRepository.GetAll();
            if (!String.IsNullOrEmpty(search))
            {
                menus = menus.Where(whereLambda);
            }
            return menus.OrderBy(r => r.Position).ThenByDescending(r => r.Id).ToList();
        }



        protected static readonly Logger BaseEntityServiceLogger = LogManager.GetCurrentClassLogger();
        public virtual void ChangeGridBaseEntityOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            try
            {
                foreach (OrderingItem item in values)
                {
                    var t = baseRepository.GetSingle(item.Id);
                    var baseContent = t as BaseEntity;
                    if (baseContent != null)
                    {
                        if (String.IsNullOrEmpty(checkbox))
                        {
                            baseContent.Position = item.Position;
                        }
                        else if (checkbox.Equals("state", StringComparison.InvariantCultureIgnoreCase))
                        {
                            baseContent.IsActive = item.IsActive;
                        }
                        else if (checkbox.Equals("mainpage", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (baseContent is Product)
                            {
                                var product = baseContent as Product;
                                product.MainPage = item.IsActive;
                            }
                        }
                        else if (checkbox.Equals("imagestate", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (baseContent is BaseContent)
                            {
                                var product = baseContent as BaseContent;
                                product.ImageState = item.IsActive;
                            }
                        }

                    }
                    baseRepository.Edit(t);
                }
                baseRepository.Save();
            }
            catch (Exception exception)
            {
                BaseEntityServiceLogger.Error(exception, "ChangeGridOrderingOrState<T> :" + String.Join(",", values), checkbox);
            }
        }
    }
}
