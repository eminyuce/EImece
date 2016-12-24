using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.GenericRepositories;
using EImece.Domain.Models.Enums;
using System.Linq.Expressions;
using NLog;
using GenericRepository.EntityFramework.Enums;

namespace EImece.Domain.Repositories
{
    public class TagCategoryRepository : BaseEntityRepository<TagCategory>, ITagCategoryRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public TagCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
 
      
        public List<TagCategory> GetTagsByTagType(EImeceTagType tagType, EImeceLanguage language)
        {
            try
            {
                Expression<Func<TagCategory, object>> includeProperty1 = r => r.Tags;
                Expression<Func<TagCategory, bool>> match = r2 => r2.IsActive && r2.Lang == (int)language  && r2.TagType == (int)tagType;
                Expression<Func<TagCategory, int>> keySelector = t => t.Position;
                Expression<Func<TagCategory, object>>[] includeProperties = { includeProperty1 };
                var result = this.FindAllIncluding(match, null, null, keySelector, OrderByType.Ascending, includeProperties);

                return result;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, exception.Message);
                throw exception;
            }
        }
    }

}
