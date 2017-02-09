using EImece.Domain.Entities;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class StoryCategoryViewModel
    {
        public StoryCategory StoryCategory { get; set; }

        public PaginatedList<Story> Stories { get; set; }
    }
}
