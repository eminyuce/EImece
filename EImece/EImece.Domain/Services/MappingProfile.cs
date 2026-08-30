using AutoMapper;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;

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
        }

        private void CreateMapTag()
        {
            CreateMap<Tag, TagDto>()
                .ForMember(d => d.DetailPageRelativeUrlForProducts,
                    o => o.MapFrom(s => s.GetDetailPageUrl("Tag", "Products", "", "", "")))
                .ForMember(d => d.DetailPageRelativeUrlForStories,
                    o => o.MapFrom(s => s.GetDetailPageUrl("Tag", "Stories", "", "", "")));
        }

        private void CreateMapStoryCategory()
        {
            CreateMap<StoryCategory, StoryCategoryDto>()
                .ForMember(d => d.DetailPageAbsoluteUrl,
                    o => o.MapFrom(s => s.GetDetailPageUrl("Categories", "Stories", "", EImece.Domain.AppConfig.HttpProtocol, "")))
                .ForMember(d => d.DetailPageRelativeUrl,
                    o => o.MapFrom(s => s.GetDetailPageUrl("Categories", "Stories", "", "", "")));
        }

        private void CreateMapProduct()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.DiscountPercentage, o => o.MapFrom(s => s.DiscountPercentage))
                .ForMember(d => d.ModifiedId, o => o.MapFrom(s => EImece.Domain.Helpers.GeneralHelper.ModifyId(s.Id)))
                .ForMember(d => d.ProductNameStr, o => o.MapFrom(s => !string.IsNullOrEmpty(s.NameShort) ? s.NameShort : (!string.IsNullOrEmpty(s.NameLong) ? s.NameLong : s.Name)))
                .ForMember(d => d.PriceWithDiscount, o => o.MapFrom(s => s.PriceWithDiscount))
                .ForMember(d => d.IsBuyableState, o => o.MapFrom(s => s.State != null && s.Price > 0 && (s.State == EImece.Domain.Models.Enums.ProductState.ProductInStock.ToString())));
        }

        private void CreateMapProductCategory()
        {
            CreateMap<ProductCategory, ProductCategoryDto>()
                .ForMember(d => d.MainImageUrl,
                    o => o.MapFrom(s => s.GetCroppedImageUrl(s.MainImageId, 100, 100, false, false)))
                .ForMember(d => d.MainImageThumbnailUrl,
                    o => o.MapFrom(s => s.GetCroppedImageUrl(s.MainImageId, 100, 100, true, false)))
                .ForMember(d => d.DetailPageUrl,
                    o => o.MapFrom(s => s.GetDetailPageUrl("Category", "ProductCategories", "", "", "")))
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
        private void CreateMapStory() => CreateMap<Story, StoryDto>()
            .ForMember(d => d.DetailPageUrl, o => o.MapFrom(s => s.GetDetailPageUrl("Detail", "Stories", "no_category", "", "")));
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
        private void CreateMapCustomer() => CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.IsSameAsShippingAddress, opt => opt.Ignore())
            .ForMember(d => d.Captcha, opt => opt.Ignore());
        private void CreateMapCoupon() => CreateMap<Coupon, CouponDto>();
        private void CreateMapBrowserSubscription() => CreateMap<BrowserSubscription, BrowserSubscriptionDto>();
        private void CreateMapBrowserSubscriber() => CreateMap<BrowserSubscriber, BrowserSubscriberDto>();
        private void CreateMapBrowserNotificationFeedBack() => CreateMap<BrowserNotificationFeedBack, BrowserNotificationFeedBackDto>();
        private void CreateMapBrowserNotification() => CreateMap<BrowserNotification, BrowserNotificationDto>();
        private void CreateMapBrand() => CreateMap<Brand, BrandDto>();
        private void CreateMapAppLog() => CreateMap<AppLog, AppLogDto>();
        private void CreateMapAddress() => CreateMap<Address, AddressDto>();
    }
}