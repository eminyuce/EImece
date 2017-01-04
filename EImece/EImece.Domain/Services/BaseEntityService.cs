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

        protected static readonly Logger BaseEntityServiceLogger = LogManager.GetCurrentClassLogger();

        private IBaseEntityRepository<T> baseEntityRepository { get; set; }
        protected BaseEntityService(IBaseEntityRepository<T> baseEntityRepository) :base(baseEntityRepository)
        {
            this.baseEntityRepository = baseEntityRepository;
        }
       
        public virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda,String search)
        {
            return baseEntityRepository.SearchEntities(whereLambda, search); 
        }

        public new virtual T SaveOrEditEntity(T entity)
        {
            entity.Lang = Settings.IsMainLanguageSet ? Settings.MainLanguage : entity.Lang;
            if(entity.Id > 0)
            {
                entity.UpdatedDate = DateTime.Now;
            }
            else
            {
                entity.UpdatedDate = DateTime.Now;
                entity.CreatedDate = DateTime.Now;
            }
            var tmp = baseEntityRepository.SaveOrEdit(entity);
            return entity;
        }

        public virtual void ChangeGridBaseEntityOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            try
            {
                foreach (OrderingItem item in values)
                {
                    var t = baseEntityRepository.GetSingle(item.Id);
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
                    baseEntityRepository.Edit(t);
                }
                baseEntityRepository.Save();
            }
            catch (Exception exception)
            {
                BaseEntityServiceLogger.Error(exception, "ChangeGridOrderingOrState<T> :" + String.Join(",", values), checkbox);
            }
        }
    }
}
