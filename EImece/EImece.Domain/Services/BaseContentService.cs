using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using GenericRepository.EntityFramework.Enums;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
   
    public abstract class BaseContentService<T> : BaseEntityService<T> where T : BaseContent
    {
        private static readonly Logger BaseContentServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ITagCategoryService TagCategoryService { get; set; }


        [Inject]
        public IFileStorageService FileStorageService { get; set; }

        public IBaseContentRepository<T> BaseContentRepository { get; set; }
        protected BaseContentService(IBaseContentRepository<T> baseContentRepository) :base(baseContentRepository) 
        {
            this.BaseContentRepository = baseContentRepository;
        }
        public virtual T GetBaseContent(int id)
        {
            var item = BaseContentRepository.GetBaseContent(id);
            if (item.MainImageId.HasValue && item.MainImageId > 0)
            {
                var imageSize = FilesHelper.GetThumbnailImageSize(item.MainImage);
                item.ImageHeight = imageSize.Item2;
                item.ImageWidth = imageSize.Item1;
            }
           

            return item;
        }
        public virtual new List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int language) 
        {
            return BaseContentRepository.SearchEntities(whereLambda, search, language);
        }
        public virtual List<T> GetActiveBaseContents(bool ?isActive, int language)
        {
            return BaseContentRepository.GetActiveBaseContents(isActive, language);
        }
        public virtual new void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    var item = GetBaseContent(id);
                    if (item.MainImageId.HasValue)
                    {
                        FileStorageService.DeleteFileStorage(item.MainImageId.Value);
                    }
                    BaseContentRepository.Delete(item);
                }
                BaseContentRepository.Save();
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                BaseContentServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                BaseContentServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }
    }
}
