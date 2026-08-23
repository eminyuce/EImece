using AutoMapper;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMapProductCategory();
            CreateMapAddress();
            CreateMapAppLog();
            CreateMapBrand();
            CreateMapBrowserNotification();
            CreateMapBrowserNotificationFeedBack();
            CreateMapBrowserSubscriber();
            CreateMapBrowserSubscription();
            CreateMapCoupon();
            CreateMapCustomer();
            CreateMapFaq();
            CreateMapFileStorage();
            CreateMapFileStorageTag();
            CreateMapList();
            CreateMapListItem();
            CreateMapMailTemplate();
            CreateMapMainPageImage();
            CreateMapMenu();
            CreateMapMenuFile();
            CreateMapOrder();
           // CreateMapOrderProduct();
            CreateMapProduct();
            CreateMapProductComment();
            CreateMapProductFile();
            CreateMapProductSpecification();
            CreateMapProductTag();
            CreateMapSetting();
            CreateMapShoppingCart();
            CreateMapShortUrl();
            CreateMapStory();
            CreateMapStoryCategory();
            CreateMapStoryFile();
            CreateMapStoryTag();
            CreateMapSubscriber();
            CreateMapTag();
            CreateMapTagCategory();
            CreateMapTemplate();
            CreateMapStorefrontDtos();
        }

        // Helper to safely get UrlHelper only when HttpContext is available
        private UrlHelper GetUrlHelper()
        {
            return HttpContext.Current?.Request?.RequestContext != null
                ? new UrlHelper(HttpContext.Current.Request.RequestContext)
                : null;
        }

        private void CreateMapTag()
        {
            CreateMap<Tag, TagDto>()
                .ForMember(d => d.DetailPageRelativeUrlForProducts,
                    o => o.MapFrom(s => GetUrlHelper() != null
                        ? GetUrlHelper().Action("Tag", "products", new { id = s.GetSeoUrl() })
                        : string.Empty))
                .ForMember(d => d.DetailPageRelativeUrlForStories,
                    o => o.MapFrom(s => GetUrlHelper() != null
                        ? GetUrlHelper().Action("tag", "stories", new { id = s.GetSeoUrl() })
                        : string.Empty));
        }

        private void CreateMapStoryCategory()
        {
            CreateMap<StoryCategory, StoryCategoryDto>()
                .ForMember(d => d.DetailPageAbsoluteUrl,
                    o => o.MapFrom(s => GetUrlHelper() != null
                        ? GetUrlHelper().Action("categories", "stories", new { id = s.GetSeoUrl() }, EImece.Domain.AppConfig.HttpProtocol)
                        : string.Empty))
                .ForMember(d => d.DetailPageRelativeUrl,
                    o => o.MapFrom(s => GetUrlHelper() != null
                        ? GetUrlHelper().Action("categories", "stories", new { id = s.GetSeoUrl() })
                        : string.Empty));
        }

        private void CreateMapProduct()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.ModifiedId, o => o.MapFrom(s => EImece.Domain.Helpers.GeneralHelper.ModifyId(s.Id)));
        }

        private void CreateMapProductCategory()
        {
            CreateMap<ProductCategory, ProductCategoryDto>()
                .ForMember(d => d.MainImageUrl,
                    o => o.MapFrom(s => HttpContext.Current != null
                        ? s.GetCroppedImageUrl(s.MainImageId, 100, 100, false, false)
                        : string.Empty))
                .ForMember(d => d.MainImageThumbnailUrl,
                    o => o.MapFrom(s => HttpContext.Current != null
                        ? s.GetCroppedImageUrl(s.MainImageId, 100, 100, true, false)
                        : string.Empty))
                .ForMember(d => d.DetailPageUrl,
                    o => o.MapFrom(s => HttpContext.Current != null ? s.GetDetailPageUrl("Category", "productCategories", "", "", "") : string.Empty))
                .ForMember(d => d.SeoUrl,
                    o => o.MapFrom(s => s.GetSeoUrl()))
                .ForMember(d => d.DiscountPercentage,
                    o => o.MapFrom(s => s.DiscountPercantage))
                .ForMember(d => d.Children, o => o.Ignore());
        }

        // Remaining basic maps...
        private void CreateMapTemplate() => CreateMap<Template, TemplateDto>();
        private void CreateMapTagCategory() => CreateMap<TagCategory, TagCategoryDto>();
        private void CreateMapSubscriber() => CreateMap<Subscriber, SubscriberDto>();
        private void CreateMapStoryTag() => CreateMap<StoryTag, StoryTagDto>();
        private void CreateMapStoryFile() => CreateMap<StoryFile, StoryFileDto>();
        private void CreateMapStory() => CreateMap<Story, StoryDto>();
        private void CreateMapShortUrl() => CreateMap<ShortUrl, ShortUrlDto>();
        private void CreateMapShoppingCart() => CreateMap<ShoppingCart, ShoppingCartDto>();
        private void CreateMapSetting() => CreateMap<Setting, SettingDto>();
        private void CreateMapProductTag() => CreateMap<ProductTag, ProductTagDto>();
        private void CreateMapProductSpecification() => CreateMap<ProductSpecification, ProductSpecificationDto>();
        private void CreateMapProductFile() => CreateMap<ProductFile, ProductFileDto>();
        private void CreateMapProductComment() => CreateMap<ProductComment, ProductCommentDto>();
        private void CreateMapOrderProduct() => CreateMap<OrderProduct, OrderProductDto>()
            .ForMember(d => d.Product, o => o.Ignore())
            .ForMember(d => d.ProductSpecObjItems, o => o.Ignore())
            .ForMember(d => d.ProductSpecColorItem, o => o.Ignore());

        private void CreateMapOrder() => CreateMap<Order, OrderDto>()
            .ForMember(d => d.OrderProducts, o => o.Ignore())
            .ForMember(d => d.ShippingAddress, o => o.Ignore())
            .ForMember(d => d.BillingAddress, o => o.Ignore())
            .ForMember(d => d.Customer, o => o.Ignore());
        private void CreateMapMenuFile() => CreateMap<MenuFile, MenuFileDto>();
        private void CreateMapMenu() => CreateMap<Menu, MenuDto>().ForMember(d => d.Childrens, o => o.Ignore());
        private void CreateMapMainPageImage() => CreateMap<MainPageImage, MainPageImageDto>();
        private void CreateMapMailTemplate() => CreateMap<MailTemplate, MailTemplateDto>();
        private void CreateMapListItem() => CreateMap<ListItem, ListItemDto>();
        private void CreateMapList() => CreateMap<List, ListDto>();
        private void CreateMapFileStorageTag() => CreateMap<FileStorageTag, FileStorageTagDto>();
        private void CreateMapFileStorage() => CreateMap<FileStorage, FileStorageDto>();
        private void CreateMapFaq() => CreateMap<Faq, FaqDto>();
        private void CreateMapCustomer() => CreateMap<Customer, CustomerDto>();
        private void CreateMapCoupon() => CreateMap<Coupon, CouponDto>();
        private void CreateMapBrowserSubscription() => CreateMap<BrowserSubscription, BrowserSubscriptionDto>();
        private void CreateMapBrowserSubscriber() => CreateMap<BrowserSubscriber, BrowserSubscriberDto>();
        private void CreateMapBrowserNotificationFeedBack() => CreateMap<BrowserNotificationFeedBack, BrowserNotificationFeedBackDto>();
        private void CreateMapBrowserNotification() => CreateMap<BrowserNotification, BrowserNotificationDto>();
        private void CreateMapBrand() => CreateMap<Brand, BrandDto>();
        private void CreateMapAppLog() => CreateMap<AppLog, AppLogDto>();
        private void CreateMapAddress() => CreateMap<Address, AddressDto>();

        private void CreateMapStorefrontDtos()
        {
            CreateMap<Product, StorefrontProductCardDto>()
                .ForMember(d => d.ProductCategoryName, o => o.MapFrom(s => s.ProductCategory != null ? s.ProductCategory.Name : string.Empty))
                .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.Name : string.Empty));
            CreateMap<Product, StorefrontProductDetailDto>()
                .ForMember(d => d.ProductCategoryName, o => o.MapFrom(s => s.ProductCategory != null ? s.ProductCategory.Name : string.Empty))
                .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.Name : string.Empty));
            CreateMap<ProductCategory, StorefrontCategoryDto>()
                .ForMember(d => d.DiscountPercentage, o => o.MapFrom(s => s.DiscountPercantage.HasValue ? (int?)s.DiscountPercantage.Value : null));
            CreateMap<Story, StorefrontStoryCardDto>()
                .ForMember(d => d.StoryCategoryName, o => o.MapFrom(s => s.StoryCategory != null ? s.StoryCategory.Name : string.Empty));
            CreateMap<Story, StorefrontStoryDetailDto>()
                .ForMember(d => d.StoryCategoryName, o => o.MapFrom(s => s.StoryCategory != null ? s.StoryCategory.Name : string.Empty));
            CreateMap<Menu, StorefrontMenuDto>()
                .ForMember(d => d.Url, o => o.MapFrom(s => s.Link));
            CreateMap<Menu, StorefrontPageDto>();
            CreateMap<Brand, StorefrontBrandDto>();
            CreateMap<Tag, StorefrontTagDto>()
                .ForMember(d => d.TagCategoryName, o => o.MapFrom(s => s.TagCategory != null ? s.TagCategory.Name : string.Empty));
            CreateMap<ProductSpecification, StorefrontProductSpecificationDto>()
                .ForMember(d => d.Order, o => o.MapFrom(s => s.Position));
            CreateMap<ProductComment, StorefrontProductCommentDto>()
                .ForMember(d => d.Comment, o => o.MapFrom(s => s.Review));
        }
    }
}