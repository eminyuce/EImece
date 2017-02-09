using EImece.Domain.Entities;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public  class StoryIndexViewModel
    {
        public PaginatedList<Story> Stories { get; set; }
        public List<StoryCategory> StoryCategories { get; set; }
    }
}
