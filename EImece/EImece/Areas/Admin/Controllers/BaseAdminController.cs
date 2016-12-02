using EImece.Domain.Repositories.IRepositories;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public abstract class BaseAdminController : Controller
    {
        [Inject]
        public IProductRepository ProductRepository { get; set; }
    }
}