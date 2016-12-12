using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using Ninject;
using SharkDev.Web.Controls.TreeView.Model;
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

        [Inject]
        public IProductCategoryRepository ProductCategoryRepository { get; set; }


        [Inject]
        public IMenuRepository MenuRepository { get; set; }


        protected List<Node> CreateTreeViewDataList()
        {
            List<Node> _lstTreeNodes = new List<Node>();
            var productCategories = ProductCategoryRepository.GetAll().ToList();
            foreach (var p in productCategories)
            {
                _lstTreeNodes.Add(new Node() { Id = p.Id.ToStr(), Term = p.Name, ParentId = p.ParentId.HasValue ? p.ParentId.Value.ToStr() : String.Empty });
            }

            return _lstTreeNodes;
        }
    }
}