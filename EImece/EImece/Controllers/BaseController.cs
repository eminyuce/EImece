using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
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
        public IMainPageImageService MainPageImageService { get; set; }
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
        [Inject]
        public ITemplateService TemplateService { get; set; }
        [Inject]
        public IEmailSender EmailSender { get; set; }
        private FilesHelper _filesHelper { get; set; }
        [Inject]
        public FilesHelper FilesHelper
        {
            get
            {
                _filesHelper.Init(Settings.DeleteURL, Settings.DeleteType, Settings.StorageRoot, Settings.UrlBase, Settings.TempPath, Settings.ServerMapPath);
                return _filesHelper;
            }
            set
            {
                _filesHelper = value;
            }
        }


    }
}