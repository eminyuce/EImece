using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class MainPageViewModel
    {
        public List<MainPageImage> MainPageImages { get; set; }
        public List<Product> MainPageProducts { get; set; }

    }
}
