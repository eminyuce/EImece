using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace EImece.Domain.Services
{
    public class TagCategoryService : BaseEntityService<TagCategory>, ITagCategoryService
    {
        private static readonly Logger TagCategoryServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public ITagService TagService { get; set; }

        private ITagCategoryRepository TagCategoryRepository { get; set; }

        public TagCategoryService(ITagCategoryRepository repository) : base(repository)
        {
            TagCategoryRepository = repository;
        }

        public List<TagCategory> GetTagsByTagType(EImeceLanguage language)
        {
            return TagCategoryRepository.GetTagsByTagType(language);
        }

        public void DeleteTagCategoryById(int tagCategoryId)
        {
            var tagCategory = GetTagCategoryById(tagCategoryId);
            var tagIdList = tagCategory.Tags.Select(r => r.Id).ToList();
            foreach (var tagId in tagIdList)
            {
                TagService.DeleteTagById(tagId);
            }
            DeleteEntity(tagCategory);
        }

        public TagCategory GetTagCategoryById(int tagCategoryId)
        {
            return TagCategoryRepository.GetTagCategoryById(tagCategoryId);
        }

        public virtual new void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    DeleteTagCategoryById(id);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                TagCategoryServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                TagCategoryServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }
    }
}