using EImece.Domain.Entities;
using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductsSearchViewModel
    {
        public String Search { get; set; }
        public PaginatedList<Product> Products { get; set; }
 
    }
}
