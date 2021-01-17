using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace EImece.Domain.Services
{
    public class ProductService : BaseContentService<Product>, IProductService
    {
        private static readonly Logger ProductServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }

        [Inject]
        public IProductCommentRepository ProductCommentRepository { get; set; }

        [Inject]
        public IOrderProductRepository OrderProductRepository { get; set; }

        [Inject]
        public ITagService TagService { get; set; }

        [Inject]
        public IStoryService StoryService { get; set; }

        [Inject]
        public ITemplateService TemplateService { get; set; }

        [Inject]
        public IStoryRepository StoryRepository { get; set; }

        private IProductRepository ProductRepository { get; set; }

        public ProductService(IProductRepository repository) : base(repository)
        {
            ProductRepository = repository;
        }

        public List<Product> GetAdminPageList(int categoryId, string search, int lang)
        {
            return ProductRepository.GetAdminPageList(categoryId, search, lang);
        }

        public ProductIndexViewModel GetMainPageProducts(int page, int language)
        {
            ProductIndexViewModel result = null;
            var cacheKey = String.Format("GetMainPageProducts-{0}-{1}", page, language);

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new ProductIndexViewModel();
                int pageSize = AppConfig.RecordPerPage;
                result.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName);
                result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, language).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
                result.ProductMenu = MenuService.GetActiveBaseContentsFromCache(true, language).FirstOrDefault(r1 => r1.MenuLink.Equals("products-index", StringComparison.InvariantCultureIgnoreCase));

                var items = ProductRepository.GetActiveProducts(page, pageSize, language);
                result.Products = items;
                result.Tags = TagService.GetActiveBaseEntities(true, language);
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheMediumSeconds);
            }
            return result;
        }

        public void SaveProductTags(int id, int[] tags)
        {
            ProductTagRepository.SaveProductTags(id, tags);
        }

        public List<ProductTag> GetProductTagsByProductId(int productId)
        {
            return ProductTagRepository.GetAllByProductId(productId);
        }

        public ProductAdminModel GetProductAdminPage(int categoryId, String search, int lang, int productId)
        {
            var result = new ProductAdminModel();
            result.Products = this.GetAdminPageList(categoryId, search, lang);
            result.ProductCategoryTree = ProductCategoryService.BuildTree(null, lang);

            if (productId > 0)
            {
                result.Product = ProductRepository.GetProduct(productId);
            }
            else
            {
                result.Product = EntityFactory.GetBaseContentInstance<Product>();
                if (categoryId > 0)
                {
                    result.Product.ProductCategoryId = categoryId;
                    result.Product.ProductCategory = ProductCategoryService.GetSingle(categoryId);
                }
            }
            EImeceLanguage language = (EImeceLanguage)lang;
            result.TagCategories = TagCategoryService.GetTagsByTagType(language);

            return result;
        }

        public ProductDetailViewModel GetProductDetailViewModelById(int id)
        {
            ProductDetailViewModel result = null;

            result = new ProductDetailViewModel();
            var product = ProductRepository.GetProduct(id);
            if (product == null)
            {
                throw new ArgumentNullException("Product is null for id:" + id);
            }
            // if (product.MainImageId.HasValue)
            // {
            //     FileStorage fileStorage = null;
            //     product.MainImageBytes = FilesHelper.GetFileStorageFromCache(product.MainImageId.Value, out fileStorage);
            // }
            if (product.MainImageId.HasValue)
            {
                product.MainImageSrc = FilesHelper.GetImageSrcPath(product.MainImageId.Value);
            }
            else
            {
                product.MainImageSrc = new Tuple<string, string>("", "");
            }
            result.Contact = ContactUsFormViewModel.CreateContactUsFormViewModel("productDetail", id, EImeceItemType.Product);
            product.ProductComments = EntityFilterHelper.FilterProductComments(product.ProductComments);
            result.CargoDescription = SettingService.GetSettingObjectByKey(Constants.CargoDescription, product.Lang);
            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, product.Lang).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            result.ProductMenu = MenuService.GetActiveBaseContentsFromCache(true, product.Lang).FirstOrDefault(r1 => r1.MenuLink.Equals("products-index", StringComparison.InvariantCultureIgnoreCase));
            result.SocialMediaLinks = SettingService.CreateShareableSocialMediaLinks(product.DetailPageAbsoluteUrl, product.NameLong, product.ImageFullPath(1000, 0));
            result.Product = product;
            EntityFilterHelper.FilterProduct(result.Product);
            if (product.ProductCategory.TemplateId.HasValue)
            {
                result.Template = TemplateService.GetTemplate(product.ProductCategory.TemplateId.Value);
            }
            result.BreadCrumb = ProductCategoryService.GetBreadCrumb(product.ProductCategoryId, product.Lang);
            result.RelatedStories = new List<Story>();
            // if (product.ProductTags.Any())
            // {
            //    var tagIdList = product.ProductTags.Select(t => t.TagId).ToArray();
            // result.RelatedStories = StoryRepository.GetRelatedStories(tagIdList, 20, product.Lang, 0);
            // }
            int relatedProductTake = 20;
            result.RelatedProducts = new List<Product>();
            if (product.ProductTags.Any())
            {
                var tagIdList = product.ProductTags.Select(t => t.TagId).ToArray();
                result.RelatedProducts = this.GetRelatedProducts(tagIdList, relatedProductTake, product.Lang, id);
            }

            if (result.RelatedProducts.Count < 20)
            {
                relatedProductTake -= result.RelatedProducts.Count;
                result.RelatedProducts.AddRange(this.GetRandomProductsByCategoryId(product.ProductCategoryId, relatedProductTake, product.Lang, id));
            }

            result.RelatedProducts = result.RelatedProducts.OrderBy(x => Guid.NewGuid()).Take(relatedProductTake).OrderByDescending(r => r.UpdatedDate).ToList();

            return result;
        }

        private List<Product> GetRandomProductsByCategoryId(int productCategoryId, int relatedProductTake, int lang, int id)
        {
            List<Product> result = null;
            var cacheKey = string.Format("GetRandomProductsByCategoryId-{0}-{1}-{2}-{3}", productCategoryId, relatedProductTake,lang,id);
            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = ProductRepository.GetRandomProductsByCategoryId(productCategoryId, relatedProductTake*3, lang, id);
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheMediumSeconds);
            }
            return result;
        }

        private List<Product> GetRelatedProducts(int[] tagIdList, int relatedProductTake, int lang, int id)
        {
            List<Product> result = null;
            var cacheKey = string.Format("GetRelatedProducts-{0}-{1}-{2}-{3}", string.Join(",", tagIdList), relatedProductTake, lang, id);
            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = ProductRepository.GetRelatedProducts(tagIdList, relatedProductTake * 3, lang, id);
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheMediumSeconds);
            }
            return result;
        }

        public virtual new void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    DeleteProductById(id);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                ProductServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                ProductServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public void DeleteProductById(int id)
        {
            try
            {
                var product = ProductRepository.GetProduct(id);
                ProductCommentRepository.DeleteByWhereCondition(r => r.ProductId == id);
                ProductSpecificationRepository.DeleteByWhereCondition(r => r.ProductId == id);
                ProductTagRepository.DeleteByWhereCondition(r => r.ProductId == id);
                OrderProductRepository.DeleteByWhereCondition(r => r.ProductId == id);
                if (product.MainImageId.HasValue)
                {
                    FileStorageService.DeleteFileStorage(product.MainImageId.Value);
                }
                if (product.ProductFiles != null)
                {
                    var menuFiles = new List<ProductFile>(product.ProductFiles);
                    foreach (var file in menuFiles)
                    {
                        FileStorageService.DeleteUploadImageByFileStorage(id, MediaModType.Products, file.FileStorageId);
                    }
                    ProductFileRepository.DeleteByWhereCondition(r => r.ProductId == id);
                }
                DeleteEntity(product);
            }
            catch (Exception e)
            {
                ProductServiceLogger.Error(e, "DeleteProductById did not work for productId:" + id);
            }
        }

        public ProductsSearchViewModel SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting)
        {
            var r = new ProductsSearchViewModel();
            r.Search = search;
            if (!String.IsNullOrEmpty(search))
            {
                r.Products = ProductRepository.SearchProducts(pageIndex, pageSize, search, lang, sorting);
            }
            else
            {
                r.Products = new GenericRepository.PaginatedList<Product>(new List<Product>(), pageIndex, pageSize, 0);
            }

            r.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, lang).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            r.ProductMenu = MenuService.GetActiveBaseContentsFromCache(true, lang).FirstOrDefault(r1 => r1.MenuLink.Equals("products-index", StringComparison.InvariantCultureIgnoreCase));

            return r;
        }


        public void SaveProductSpecifications(List<ProductSpecification> specifications, int productId)
        {
            if (specifications.IsNotEmpty())
            {
                ProductSpecificationRepository.DeleteByWhereCondition(r => r.ProductId == productId);
                foreach (var item in specifications)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        ProductSpecificationRepository.Add(item);
                        ProductSpecificationRepository.Save();
                    }
                }
            }
        }

        public List<Product> GetActiveProducts(int? language)
        {
            return ProductRepository.GetActiveProducts(language);
        }

        public Rss20FeedFormatter GetProductsRss(RssParams rssParams)
        {
            var items = this.GetActiveProducts(rssParams.Language).Take(rssParams.Take).ToList();
            var builder = new UriBuilder(AppConfig.HttpProtocol, HttpContext.Current.Request.Url.Host);
            var url = String.Format("{0}", builder.Uri.ToString().TrimEnd('/'));

            String title = SettingService.GetSettingByKey(Constants.CompanyName);
            string lang = EnumHelper.GetEnumDescription((EImeceLanguage)rssParams.Language);

            var feed = new SyndicationFeed(title, "", new Uri(url))
            {
                Language = lang
            };

            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            String atomSelfHref = urlHelper.Action("products", "rss", new { rssParams.Take, rssParams.Language }, HttpContext.Current.Request.Url.Scheme);

            feed.Items = items.Select(s => s.GetProductSyndicationItem(url, rssParams));
            var formatter = new Rss20FeedFormatter(feed);
            formatter.SerializeExtensionsAsAtom = false;
            XNamespace atom = "http://www.w3.org/2005/Atom";
            formatter.Feed.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), atom.NamespaceName);
            formatter.Feed.ElementExtensions.Add(new XElement(atom + "link", new XAttribute("href", atomSelfHref.ToString()), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

            return formatter;
        }

        public ProductsSearchResult GetProductsSearchResult(
         string search,
         string filters,
         string page,
         int language)
        {
            int top = 10;
            int skip = 0;
            return ProductRepository.GetProductsSearchResult(search, filters, top, skip, language);
        }

        public void ParseTemplateAndSaveProductSpecifications(int productId, int templateId, int currentLanguage, HttpRequestBase request)
        {
            var template = TemplateService.GetTemplate(templateId);
            XDocument xdoc = XDocument.Parse(template.TemplateXml);
            var groups = xdoc.Root.Descendants("group");
            var Specifications = new List<ProductSpecification>();

            foreach (var group in groups)
            {
                var groupName = group.FirstAttribute.Value;
                int position = 1;
                foreach (XElement field in group.Elements())
                {
                    var p = new ProductSpecification();
                    p.GroupName = groupName;
                    p.ProductId = productId;
                    p.CreatedDate = DateTime.Now;
                    p.UpdatedDate = DateTime.Now;
                    p.Position = position++;
                    p.IsActive = true;
                    p.Lang = currentLanguage;
                    var name = field.Attribute("name");
                    var unit = field.Attribute("unit");
                    var values = field.Attribute("values");

                    var value = request.Unvalidated.Form.Get(name.Value);

                    //   var value = request.Form[name.Value];

                    if (name != null)
                    {
                        p.Name = name.Value;
                    }
                    if (unit != null)
                    {
                        p.Unit = unit.Value;
                    }

                    p.Value = value;
                    Specifications.Add(p);
                }
            }

            SaveProductSpecifications(Specifications, productId);
        }

        public void MoveProductsInTrees(int newCategoryId, String products)
        {
            if (!String.IsNullOrEmpty(products))
            {
                var productIdList = products.Split(',');
                foreach (var id in productIdList)
                {
                    var product = ProductRepository.GetProduct(id.ToInt());
                    product.ProductCategoryId = newCategoryId;
                    ProductRepository.Edit(product);
                }
                ProductRepository.Save();
            }
        }

        public Product GetProductById(int id)
        {
            return ProductRepository.GetProduct(id);
        }

        public List<Product> GetChildrenProducts(ProductCategory productCategory, List<ProductCategory> ChildrenProductCategories)
        {
            if (productCategory == null || ChildrenProductCategories.IsEmpty())
            {
                return new List<Product>();
            }
            var allCategoriesId = new List<int>();
            // GetChildren Category Id s
            int[] childrenCategoryId = ChildrenProductCategories.Select(r => r.Id).ToArray();
            allCategoriesId.AddRange(childrenCategoryId);
            var allActiveCategories = ProductCategoryService.GetActiveBaseContents(true, productCategory.Lang);
            foreach (var category in allActiveCategories)
            {
                foreach (var childrenId in childrenCategoryId)
                {
                    if (category.ParentId == childrenId)
                    {
                        allCategoriesId.Add(category.Id);
                    }
                }
            }
            return ProductRepository.GetChildrenProducts(allCategoriesId.ToArray());
        }

        public SimiliarProductTagsViewModel GetProductByTagId(int tagId, int pageIndex, int pageSize, int lang)
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = TagService.GetSingle(tagId);
            r.ProductTags = ProductTagRepository.GetProductsByTagId(tagId, pageIndex, pageSize, lang);
            r.StoryTags = StoryTagRepository.GetStoriesByTagId(tagId, 1, 10, lang);
            return r;
        }
        public SimiliarProductTagsViewModel GetProductByTagId(int tagId, int pageIndex, int pageSize, int lang, SortingType sorting)
        {
            var r = new SimiliarProductTagsViewModel();
            r.Tag = TagService.GetSingle(tagId);
            r.ProductTags = ProductTagRepository.GetProductsByTagId(tagId, pageIndex, pageSize, lang, sorting);
            r.StoryTags = StoryTagRepository.GetStoriesByTagId(tagId, 1, 10, lang);
            return r;
        }
    }
}