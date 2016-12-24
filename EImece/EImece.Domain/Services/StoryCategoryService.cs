using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class StoryCategoryService : BaseContentService<StoryCategory>, IStoryCategoryService
    {

   
        public override void SetCurrentRepository()
        {
            this.baseRepository = StoryCategoryRepository;
        }
    }
}
