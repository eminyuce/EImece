using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<List<TagCategory>> GetTagsByTagTypeAsync(EImeceLanguage language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await TagCategoryRepository.GetTagsByTagTypeAsync(language, cancellationToken).ConfigureAwait(false);
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

        public async Task DeleteTagCategoryByIdAsync(int tagCategoryId)
        {
            var tagCategory = GetTagCategoryById(tagCategoryId);
            var tagIdList = tagCategory.Tags.Select(r => r.Id).ToList();
            foreach (var tagId in tagIdList)
            {
                await TagService.DeleteTagByIdAsync(tagId).ConfigureAwait(false);
            }
            await DeleteEntityAsync(tagCategory).ConfigureAwait(false);
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

        public virtual new async Task DeleteBaseEntityAsync(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    await DeleteTagCategoryByIdAsync(id).ConfigureAwait(false);
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