using EImece.Domain.Repositories.IRepositories;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public abstract class BaseController : Controller
    {
        [Inject]
        public IProductRepository ProductRepository { get; set; }

        [Inject]
        public IProductCategoryRepository ProductCategoryRepository { get; set; }

        [Inject]
        public IMenuRepository MenuRepository { get; set; }
    }
}