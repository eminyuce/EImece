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
        public ISettingRepository SettingRepository { get; set; }
        [Inject]
        public IProductRepository ProductRepository { get; set; }
        [Inject]
        public IProductCategoryRepository ProductCategoryRepository { get; set; }
        [Inject]
        public IMenuRepository MenuRepository { get; set; }
        [Inject]
        public IFileStorageRepository FileStorageRepository { get; set; }
        [Inject]
        public IFileStorageTagRepository FileStorageTagRepository { get; set; }
        [Inject]
        public IProductSpecificationRepository ProductSpecificationRepository { get; set; }
        [Inject]
        public IProductTagRepository ProductTagRepository { get; set; }
        [Inject]
        public IStoryCategoryRepository StoryCategoryRepository { get; set; }
        [Inject]
        public IStoryFileRepository StoryFileRepository { get; set; }
        [Inject]
        public IStoryRepository StoryRepository { get; set; }
        [Inject]
        public IStoryTagRepository StoryTagRepository { get; set; }
        [Inject]
        public ISubscriberRepository SubscriberRepository { get; set; }
        [Inject]
        public ITagCategoryRepository TagCategoryRepository { get; set; }
        [Inject]
        public ITagRepository TagRepository { get; set; }



        protected List<Node> CreateMenuTreeViewDataList()
        {
            List<Node> _lstTreeNodes = new List<Node>();
            var menus = MenuRepository.GetAll().ToList();
            foreach (var p in menus)
            {
                _lstTreeNodes.Add(new Node() { Id = p.Id.ToStr(), Term = p.Name, ParentId = p.ParentId.ToStr() });
            }

            return _lstTreeNodes;
        }

        protected List<Node> CreateProductCategoryTreeViewDataList()
        {
            List<Node> _lstTreeNodes = new List<Node>();
            var productCategories = ProductCategoryRepository.GetAll().ToList();
            foreach (var p in productCategories)
            {
                _lstTreeNodes.Add(new Node() { Id = p.Id.ToStr(), Term = p.Name, ParentId = p.ParentId.ToStr() });
            }

            return _lstTreeNodes;
        }
    }
}