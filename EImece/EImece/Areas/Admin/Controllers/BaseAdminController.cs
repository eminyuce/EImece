using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
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
        public ISettingService SettingService { get; set; }
        [Inject]
        public IProductService ProductService { get; set; }
        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }
        [Inject]
        public IMenuService MenuService { get; set; }
        [Inject]
        public IStoryService StoryService { get; set; }
        [Inject]
        public IStoryCategoryService StoryCategoryService { get; set; }
        [Inject]
        public ITagService TagService { get; set; }
        [Inject]
        public ITagCategoryService TagCategoryService { get; set; }
        [Inject]
        public ISubsciberService SubsciberService { get; set; }
        [Inject]
        public IFileStorageService FileStorageService { get; set; }


        protected List<Node> CreateMenuTreeViewDataList()
        {
            List<Node> _lstTreeNodes = new List<Node>();
            var menus = MenuService.GetAll().OrderBy(r => r.Position).ToList();
            foreach (var p in menus)
            {
                _lstTreeNodes.Add(new Node() { Id = p.Id.ToStr(), Term = p.Name, ParentId = p.ParentId > 0 ? p.ParentId.ToStr() : "" });
            }

            return _lstTreeNodes;
        }

        protected List<Node> CreateProductCategoryTreeViewDataList()
        {
            List<Node> _lstTreeNodes = new List<Node>();
            var productCategories = ProductCategoryService.GetAll().OrderBy(r => r.Position).ToList();
            foreach (var p in productCategories)
            {
                _lstTreeNodes.Add(new Node() { Id = p.Id.ToStr(), Term = p.Name, ParentId = p.ParentId > 0 ? p.ParentId.ToStr() : "" });
            }

            return _lstTreeNodes;
        }

        protected int SelectedLanguage
        {
            get
            {
                if (Session["SelectedLanguage"] != null)
                    return Session["SelectedLanguage"].ToInt(1);
                else
                {
                    return 1;
                };
            }
            set
            {
                Session["SelectedLanguage"] = value;
            }
        }
    }
}