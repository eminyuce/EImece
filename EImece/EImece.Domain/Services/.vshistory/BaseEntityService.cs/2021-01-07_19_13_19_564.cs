using EImece.Domain.Entities;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EImece.Domain.Services
{
    public abstract class BaseEntityService<T> : BaseService<T> where T : BaseEntity
    {
        private const string STATE = "state";
        private const string MAIN_PAGE = "mainpage";
        private const string IMAGE_STATE = "imagestate";
        private static readonly Logger BaseEntityServiceLogger = LogManager.GetCurrentClassLogger();

        private IBaseEntityRepository<T> baseEntityRepository { get; set; }

        protected BaseEntityService(IBaseEntityRepository<T> baseEntityRepository) : base(baseEntityRepository)
        {
            this.baseEntityRepository = baseEntityRepository;
        }

        protected BaseEntityService(IBaseEntityRepository<T> baseEntityRepository, bool IsCachingActivated) : base(baseEntityRepository)
        {
            this.IsCachingActivated = IsCachingActivated;
        }

        public virtual List<T> GetActiveBaseEntities(bool? isActive, int? language)
        {
            return baseEntityRepository.GetActiveBaseEntities(isActive, language);
        }

        public virtual List<T> GetActiveBaseEntitiesFromCache(bool? isActive, int? language)
        {
            List<T> result = null;
            String cacheKey = String.Format(this.GetType().FullName + "-GetActiveBaseEntitiesFromCache-{0}-{1}", isActive, language);

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = baseEntityRepository.GetActiveBaseEntities(isActive, language);
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheLongSeconds);
            }
            return result;
        }

        public virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int? language)
        {
            return baseEntityRepository.SearchEntities(whereLambda, search, language);
        }

        public new virtual T SaveOrEditEntity(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentException("entity cannot be null");
            }
            if (entity.Id > 0)
            {
                entity.UpdatedDate = DateTime.Now;
            }
            else
            {
                entity.UpdatedDate = DateTime.Now;
                entity.CreatedDate = DateTime.Now;
            }
            var tmp = baseEntityRepository.SaveOrEdit(entity);
            this.MemoryCacheProvider.ClearAll();
            return entity;
        }

        public virtual void ChangeGridBaseEntityOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            if (values == null)
            {
                throw new ArgumentException("values cannot be null");
            }

            foreach (OrderingItem item in values)
            {
                var t = baseEntityRepository.GetSingle(item.Id);
                var baseContent = t as BaseEntity;
                if (baseContent != null)
                {
                    try
                    {
                        if (String.IsNullOrEmpty(checkbox))
                        {
                            baseContent.Position = item.Position;
                        }
                        else if (checkbox.Equals(STATE, StringComparison.InvariantCultureIgnoreCase))
                        {
                            baseContent.IsActive = item.IsActive;
                        }
                        else if (checkbox.Equals(MAIN_PAGE, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (baseContent is Product)
                            {
                                var product = baseContent as Product;
                                product.MainPage = item.IsActive;
                            }
                            else if (baseContent is Story)
                            {
                                var story = baseContent as Story;
                                story.MainPage = item.IsActive;
                            }
                            else if (baseContent is ProductCategory)
                            {
                                var story = baseContent as ProductCategory;
                                story.MainPage = item.IsActive;
                            }
                        }
                        else if (checkbox.Equals(IMAGE_STATE, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (baseContent is BaseContent)
                            {
                                var product = baseContent as BaseContent;
                                product.ImageState = item.IsActive;
                            }
                        }
                        else if (checkbox.Equals("IsCampaign", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (baseContent is Product)
                            {
                                var product = baseContent as Product;
                                product.IsCampaign = item.IsActive;
                            }
                        }
                        baseEntityRepository.Edit(t);
                        baseEntityRepository.Save();
                    }
                    catch (Exception exception)
                    {
                        BaseEntityServiceLogger.Error(exception, "ChangeGridOrderingOrState<T> :" + item.Id, checkbox);
                    }
                }
            }
        }
    }
}