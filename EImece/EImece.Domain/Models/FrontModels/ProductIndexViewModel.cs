using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Entities;
using GenericRepository;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductIndexViewModel
    {
        public PaginatedList<Product> Products { get; set; }
    }
}
