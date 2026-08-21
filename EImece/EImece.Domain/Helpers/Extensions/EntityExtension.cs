using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.Extensions
{
    public static class EntityExtension
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static SyndicationItem GetStorySyndicationItem(this Story product, string categoryName, string url, RssParams rssParams)
        {
            String link = String.Format("{0}", product.GetDetailPageUrl(Constants.DetailAction, Constants.StoriesAction, categoryName,
                         AppConfig.HttpProtocol));

            var desc = GeneralHelper.StripHtml(product.Description).ToStr(rssParams.Description);

            var pageLink = new Uri(link.ToLower());
            var ub = new UriBuilder(pageLink);
            if (!string.IsNullOrEmpty(rssParams.GetAnalyticsQueryString()))
            {
                ub.Query = rssParams.GetAnalyticsQueryString();
            }
            var si = new SyndicationItem(product.Name, desc, ub.Uri);
            si.PublishDate = product.UpdatedDate.ToUniversalTime();

            if (!String.IsNullOrEmpty(categoryName))
            {
                si.ElementExtensions.Add("category", String.Empty, categoryName);
            }

            if (product.MainImageId.HasValue)
            {
                String imageSrc = product.GetCroppedImageUrl(product.MainImageId.Value, rssParams.Width, rssParams.Height);
                if (!String.IsNullOrEmpty(imageSrc))
                {
                    string imageUrl = String.Format("{0}{1}", url, imageSrc);

                    try
                    {
                        SyndicationLink imageLink =
                            SyndicationLink.CreateMediaEnclosureLink(new Uri(imageUrl), MediaTypeNames.Image.Jpeg, 100);
                        si.Links.Add(imageLink);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, e.Message + " : " + String.Format("url={0} imageSrc={1}", url, imageSrc));
                    }
                }
            }

            return si;
        }

        public static SyndicationItem GetStorySyndicationItemFull(this Story product, string categoryName, string url, RssParams rssParams)
        {
            String link = String.Format("{0}", product.GetDetailPageUrl(Constants.DetailAction, Constants.StoriesAction, categoryName,
                         AppConfig.HttpProtocol));

            var desc = GeneralHelper.StripHtml(product.Description).ToStr(rssParams.Description);

            var pageLink = new Uri(link.ToLower());
            var ub = new UriBuilder(pageLink);
            if (!string.IsNullOrEmpty(rssParams.GetAnalyticsQueryString()))
            {
                ub.Query = rssParams.GetAnalyticsQueryString();
            }
            var si = new SyndicationItem(product.Name, desc, ub.Uri);
            si.PublishDate = product.UpdatedDate.ToUniversalTime();

            if (!String.IsNullOrEmpty(categoryName))
            {
                si.ElementExtensions.Add("category", String.Empty, categoryName);
            }
            si.SetGuid(link.ToLower(), true);
            String imageUrlSrcHtml = "";
            if (product.MainImageId.HasValue)
            {
                String imageSrc = product.GetCroppedImageUrl(product.MainImageId.Value, rssParams.Width, rssParams.Height);
                if (!String.IsNullOrEmpty(imageSrc))
                {
                    string imageUrl = String.Format("{0}{1}", url, imageSrc);
                    imageUrlSrcHtml = String.Format("<div><img src='{0}'  /></div>", imageUrl);
                }
            }

            si.SetCDataHtml(imageUrlSrcHtml + product.Description);
            return si;
        }

        public static SyndicationItem GetProductSyndicationItem(this Product product, string url, RssParams rssParams)
        {
            String link = String.Format("{0}", product.GetDetailPageUrl(Constants.DetailAction, "Products", product.ProductCategory.Name,
                         AppConfig.HttpProtocol));

            var desc = GeneralHelper.StripHtml(product.Description).ToStr(rssParams.Description);
            var pageLink = new Uri(link.ToLower());
            var ub = new UriBuilder(pageLink);
            if (!string.IsNullOrEmpty(rssParams.GetAnalyticsQueryString()))
            {
                ub.Query = rssParams.GetAnalyticsQueryString();
            }
            var si = new SyndicationItem(product.ProductNameStr, desc, ub.Uri);
            si.PublishDate = product.UpdatedDate.ToUniversalTime();

            if (!String.IsNullOrEmpty(product.ProductCategory.Name))
            {
                si.ElementExtensions.Add("category", String.Empty, product.ProductCategory.Name);
            }
            if (product.Brand !=null && !string.IsNullOrEmpty(product.Brand.Name))
            {
                si.ElementExtensions.Add("brand", String.Empty, product.Brand.Name);
            }
            if (product.Brand != null && !string.IsNullOrEmpty(product.Brand.Name))
            {
                si.ElementExtensions.Add("price", String.Empty, product.PriceWithDiscount);
            }


            si.SetGuid(link, true);
            if (product.MainImageId.HasValue)
            {
                String imageSrc = product.GetCroppedImageUrl(product.MainImageId.Value, rssParams.Width, rssParams.Height);
                if (!String.IsNullOrEmpty(imageSrc))
                {
                    string imageUrl = String.Format("{0}{1}", url, imageSrc);

                    try
                    {
                        SyndicationLink imageLink =
                            SyndicationLink.CreateMediaEnclosureLink(new Uri(imageUrl), MediaTypeNames.Image.Jpeg, 100);
                        si.Links.Add(imageLink);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, e.Message + " : " + String.Format("url={0} imageSrc={1}", url, imageSrc));
                    }
                }
            }

            return si;
        }

        public static List<BaseEntity> DownCasting<T>(this List<T> items) where T : BaseEntity
        {
            var baseList = new List<BaseEntity>();
            items.ForEach(v => baseList.Add(v));
            return baseList;
        }

        #region trimAllString

        public static void TrimAllStrings<T>(this T obj)
        {
            try
            {
                if (obj == null)
                    return;

                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

                foreach (PropertyInfo p in obj.GetType().GetProperties(flags))
                {
                    Type currentNodeType = p.PropertyType;
                    if (currentNodeType == typeof(String))
                    {
                        string currentValue = (string)p.GetValue(obj, null);
                        if (currentValue != null && p.CanWrite)
                        {
                            p.SetValue(obj, currentValue.Trim(), null);
                        }
                    }
                    // see http://stackoverflow.com/questions/4444908/detecting-native-objects-with-reflection
                    else if (currentNodeType != typeof(object) && Type.GetTypeCode(currentNodeType) == TypeCode.Object)
                    {
                        if (p.GetIndexParameters().Length == 0)
                        {
                            p.GetValue(obj, null).TrimAllStrings();
                        }
                        else
                        {
                            p.GetValue(obj, new Object[] { 0 }).TrimAllStrings();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        #endregion trimAllString

        public static string GetSeoUrl(this BaseEntity entity)
        {
            return string.Format("{0}-{1}",
               GeneralHelper.GetUrlSeoString(entity.Name),
             GeneralHelper.ModifyId(entity.Id));
        }

        public static string GetImageSeoUrl(this BaseEntity entity, int fileStorageId)
        {
            if (entity == null) return string.Empty;
            return string.Format("{0}-{1}.jpg", GeneralHelper.GetUrlSeoString(RemoveFileExtension(entity.Name)), GeneralHelper.ModifyId(fileStorageId));
        }

        public static string GetImageSeoUrl(this EImece.Domain.Models.DTOs.Storefront.StorefrontProductCardDto entity, int fileStorageId)
        {
            if (entity == null) return string.Empty;
            return string.Format("{0}-{1}.jpg", GeneralHelper.GetUrlSeoString(RemoveFileExtension(entity.Name)), GeneralHelper.ModifyId(fileStorageId));
        }

        public static string GetImageSeoUrl(this EImece.Domain.Models.DTOs.Storefront.StorefrontProductFileDto entity, int fileStorageId)
        {
            if (entity == null) return string.Empty;
            string name = !string.IsNullOrEmpty(entity.Title) ? entity.Title : (!string.IsNullOrEmpty(entity.FileName) ? entity.FileName : "image");
            return string.Format("{0}-{1}.jpg", GeneralHelper.GetUrlSeoString(RemoveFileExtension(name)), GeneralHelper.ModifyId(fileStorageId));
        }

        public static String GetSeoTitle(this BaseEntity entity, int length = 50)
        {
            string value = GeneralHelper.TruncateAtWord(entity.Name, length);
            if (string.IsNullOrEmpty(value))
            {
                value = entity.Name;
            }
            return GeneralHelper.Capitalize(value);
        }

        public static String GetProductSeoTitle(this Product entity, int length = 50)
        {
            string name = string.IsNullOrEmpty(entity.NameLong) ? entity.Name : entity.NameLong;
            string value = GeneralHelper.TruncateAtWord(name, length);
            if (string.IsNullOrEmpty(value))
            {
                value = name;
            }
            return GeneralHelper.Capitalize(value);
        }

        public static string GetSeoDescription(this BaseContent entity, int length = 150)
        {
            var result = string.Format("{0}", GeneralHelper.GetDescriptionWithBody(entity.Description, length));
            if (string.IsNullOrEmpty(result))
            {
                var SettingService = DependencyResolver.Current.GetService<ISettingService>();
                result = SettingService.GetSettingByKey(Constants.SiteIndexMetaDescription).ToStr();
            }
            if (string.IsNullOrEmpty(result) && entity != null)
            {
                result = entity.Name.ToStr(length);
            }
            return result;
        }

        public static string GetSeoKeywords(this BaseContent entity, int length = 150)
        {
            var result = string.Format("{0}", entity.MetaKeywords.ToStr(255));
            if (string.IsNullOrEmpty(result))
            {
                //TODO: Missing keywords.
                var SettingService = DependencyResolver.Current.GetService<ISettingService>();
                result = SettingService.GetSettingByKey(Constants.SiteIndexMetaKeywords).ToStr();
            }
            return result;
        }

        public static string GetImageTag(this BaseContent entity)
        {
            string imageTag = "";
            if (entity != null && entity.MainImageId.HasValue && entity.MainImage != null && entity.MainImageId.Value != 0 && entity.ImageState)

            {
                string imagePath = GetFullPathImageUrlFromFileSystem(entity, false);
                imageTag = string.Format("<img src='{0}' alt='{1}'/>", imagePath, entity.Name).ToLower();
            }

            return imageTag;
        }

        public static string GetThumpImageTag(this BaseContent entity)
        {
            string imageTag = "";

            if (entity != null && entity.MainImageId.HasValue && entity.MainImage != null && entity.MainImageId.Value != 0 && entity.ImageState)
            {
                string partThumb2 = GetFullPathImageUrlFromFileSystem(entity, true);
                imageTag = string.Format("<img src='{0}' alt='{1}'/>", partThumb2, entity.Name).ToLower();
            }

            return imageTag;
        }

        public static string GetFullPathImageUrlFromFileStorage(FileStorage mainImage, bool isThump)
        {
            if (mainImage == null)
            {
                return String.Empty;
            }

            if (FilesHelper.IsSeedPlaceholderMedia(mainImage))
            {
                // Force callers to use GetCroppedImageUrl / default placeholder instead of
                // static /media/images seed JPEGs that used to embed filenames as pixels.
                return string.Empty;
            }

            if (mainImage.FileName.Equals(FilesHelper.EXTERNAL_IMAGE))
            {
                return mainImage.FileUrl;
            }

            string imagePath = Constants.UrlBase + mainImage.FileName;

            if (isThump)
            {
                string fileName = mainImage.FileName;
                string partThumb1 = Path.Combine(Constants.UrlBase, "thumbs");
                imagePath = Path.Combine(partThumb1, "thb" + fileName);
            }
            return imagePath;
        }

        public static string GetFullPathImageUrlFromFileSystem(this BaseContent entity, bool isThump)
        {
            try
            {
                if (entity != null && entity.MainImageId.HasValue && entity.MainImageId.Value != 0 && entity.ImageState)
                {
                    return GetFullPathImageUrlFromFileStorage(entity.MainImage, isThump);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return String.Empty;
        }

        public static string GetCroppedImageTag(this BaseContent entity, int width, int height)
        {
            string imageTag = "";
            if (entity != null && entity.MainImageId.HasValue && entity.MainImageId.Value != 0 && entity.ImageState)
            {
                imageTag = GetCroppedImageTag(entity, entity.MainImageId.Value, width, height);
            }
            else
            {
                return BuildImageTag(new ImageTagArgs
                {
                    Src = AppConfig.GetDefaultImage(width, height),
                    Alt = "Default image",
                    Width = width,
                    Height = height,
                    Lazy = true
                });
            }

            return imageTag;
        }

        public static string GetCroppedImageTag(this BaseEntity entity, int fileStorageId, int width = 0, int height = 0)
        {
            return GetCroppedImageTag(entity, fileStorageId, width, height, lazy: true, fetchPriority: null, sizes: null);
        }

        public static string GetCroppedImageTag(this BaseEntity entity, int fileStorageId, int width, int height, bool lazy, string fetchPriority = null, string sizes = null)
        {
            if (entity != null && fileStorageId > 0)
            {
                string imagePath = GetCroppedImageUrl(entity, fileStorageId, width, height);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string srcset = GetResponsiveImageSrcSet(entity, fileStorageId, width, height);
                    return BuildImageTag(new ImageTagArgs
                    {
                        Src = imagePath,
                        Alt = entity.Name,
                        Width = width,
                        Height = height,
                        Lazy = lazy,
                        FetchPriority = fetchPriority,
                        SrcSet = srcset,
                        Sizes = sizes
                    });
                }
            }

            return BuildImageTag(new ImageTagArgs
            {
                Src = AppConfig.GetDefaultImage(width, height),
                Alt = "Default image",
                Width = width,
                Height = height,
                Lazy = lazy
            });
        }

        /// <summary>
        /// Builds a comma-separated srcset for resized image variants near the requested display size.
        /// </summary>
        public static string GetResponsiveImageSrcSet(this BaseEntity entity, int fileStorageId, int width, int height)
        {
            if (entity == null || fileStorageId <= 0)
            {
                return string.Empty;
            }

            int baseWidth;
            if (width > 0)
            {
                baseWidth = width;
            }
            else if (height > 0)
            {
                baseWidth = height;
            }
            else
            {
                baseWidth = 400;
            }
            int baseHeight = height > 0 ? height : 0;
            var widths = new[] { baseWidth, baseWidth * 2 };
            var parts = new List<string>();
            foreach (var w in widths)
            {
                int h = baseHeight > 0 ? (int)Math.Round(baseHeight * ((double)w / baseWidth)) : 0;
                string url = GetCroppedImageUrl(entity, fileStorageId, w, h);
                if (!string.IsNullOrEmpty(url))
                {
                    parts.Add(string.Format("{0} {1}w", url, w));
                }
            }
            return string.Join(", ", parts);
        }

        private sealed class ImageTagArgs
        {
            public string Src { get; set; }
            public string Alt { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public bool Lazy { get; set; }
            public string FetchPriority { get; set; }
            public string SrcSet { get; set; }
            public string Sizes { get; set; }
        }

        private static string BuildImageTag(ImageTagArgs args)
        {
            var attrs = new List<string>
            {
                string.Format("src='{0}'", args.Src),
                string.Format("alt='{0}'", HttpUtility.HtmlAttributeEncode(args.Alt ?? string.Empty))
            };

            if (args.Width > 0)
            {
                attrs.Add(string.Format("width='{0}'", args.Width));
            }
            if (args.Height > 0)
            {
                attrs.Add(string.Format("height='{0}'", args.Height));
            }
            if (!string.IsNullOrEmpty(args.SrcSet))
            {
                attrs.Add(string.Format("srcset='{0}'", args.SrcSet));
                string sizesValue = args.Sizes;
                if (string.IsNullOrEmpty(sizesValue))
                {
                    sizesValue = string.Format("{0}px", args.Width > 0 ? args.Width : 300);
                }
                attrs.Add(string.Format("sizes='{0}'", sizesValue));
            }
            if (!string.IsNullOrEmpty(args.FetchPriority))
            {
                attrs.Add(string.Format("fetchpriority='{0}'", args.FetchPriority));
            }
            attrs.Add(args.Lazy ? "loading='lazy'" : "loading='eager'");
            attrs.Add("decoding='async'");

            return string.Format("<img {0} />", string.Join(" ", attrs));
        }

        public static string GetCroppedImageUrl(this BaseEntity entity, int? fileStorageIdOptional, int width = 0, int height = 0, bool isFullPathImageUrl = false, bool isThump = false)
        {
            var fileStorageId = fileStorageIdOptional.HasValue ? fileStorageIdOptional.Value : 0;
            var result = GetCroppedImageUrl(entity, fileStorageId, width, height, isFullPathImageUrl, isThump);

            return result;
        }

        public static string GetCroppedImageUrl(this BaseEntity entity, int fileStorageId, int width = 0, int height = 0, bool isFullPathImageUrl = false, bool isThump=false)
        {
            NormalizeImageDimensions(ref width, ref height);

            if (entity == null || fileStorageId <= 0)
            {
                return AppConfig.GetDefaultImage(width, height);
            }

            // When explicit dimensions are requested, prefer resize proxy (or static thumb below)
            // so clients never download full-resolution originals for small display slots.
            bool preferResizedProxy = width > 0 || height > 0;
            if (AppConfig.IsImageFullSrcUnderMediaFolder && !preferResizedProxy)
            {
                var mediaFolderUrl = TryGetMediaFolderImageUrl(entity, isThump, width, height);
                if (mediaFolderUrl != null)
                {
                    return mediaFolderUrl;
                }
            }

            // Prefer prebuilt thumb when it exists and can cover the requested display size.
            // Layout can still show it at 100x100 via width/height/CSS; browser downscales.
            if (preferResizedProxy)
            {
                var staticThumbUrl = TryGetStaticThumbnailUrl(entity, fileStorageId, width, height, isFullPathImageUrl);
                if (!string.IsNullOrEmpty(staticThumbUrl))
                {
                    return staticThumbUrl;
                }
            }

            return BuildResizeProxyImageUrl(entity, fileStorageId, width, height, isFullPathImageUrl);
        }

        /// <summary>
        /// Coerce a zero dimension when the other side is set (avoids /images/w0h500 and /images/w610h0).
        /// Leaves 0×0 unchanged so callers can still request the uncropped original.
        /// </summary>
        internal static void NormalizeImageDimensions(ref int width, ref int height)
        {
            if (width < 0)
            {
                width = 0;
            }
            if (height < 0)
            {
                height = 0;
            }
            if (width == 0 && height > 0)
            {
                width = height;
            }
            else if (height == 0 && width > 0)
            {
                height = width;
            }
        }

        private static string BuildResizeProxyImageUrl(BaseEntity entity, int fileStorageId, int width, int height, bool isFullPathImageUrl)
        {
            var imageSize = $"w{width}h{height}";
            var imageId = entity.GetImageSeoUrl(fileStorageId);
            if (HttpContext.Current == null)
            {
                // Async service continuations (ConfigureAwait(false)) lose HttpContext; build the known route.
                var relative = $"/images/{imageSize}/{imageId}";
                if (!isFullPathImageUrl)
                {
                    return relative;
                }

                var baseUrl = GetAbsoluteApplicationBaseUrl(AppConfig.HttpProtocolForImages);
                return string.IsNullOrEmpty(baseUrl) ? relative : baseUrl + relative;
            }

            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            if (isFullPathImageUrl)
            {
                return urlHelper.Action(Constants.ImageActionName,
                    "Images", new { imageSize, id = imageId, area = "" },
                    AppConfig.HttpProtocolForImages);
            }

            return urlHelper.Action(Constants.ImageActionName, "Images", new { imageSize, id = imageId, area = "" });
        }

        private static string TryGetMediaFolderImageUrl(BaseEntity entity, bool isThump, int width, int height)
        {
            if (entity is BaseContent baseContentEntity)
            {
                var imagePath = GetFullPathImageUrlFromFileSystem(baseContentEntity, isThump);
                return GetImagePathOrDefaultImage(width, height, imagePath);
            }
            if (entity is ProductFile productFile)
            {
                var imagePath = GetFullPathImageUrlFromFileStorage(productFile.FileStorage, isThump);
                return GetImagePathOrDefaultImage(width, height, imagePath);
            }
            if (entity is StoryFile storyFile)
            {
                var imagePath = GetFullPathImageUrlFromFileStorage(storyFile.FileStorage, isThump);
                return GetImagePathOrDefaultImage(width, height, imagePath);
            }
            if (entity is FileStorage fileStorage)
            {
                var imagePath = GetFullPathImageUrlFromFileStorage(fileStorage, isThump);
                return GetImagePathOrDefaultImage(width, height, imagePath);
            }

            return null;
        }

        /// <summary>
        /// Returns a static /media/images/thumbs/thb… URL when the thumb file exists and is large enough
        /// for the requested display size; otherwise null (caller uses ImagesController resize proxy).
        /// </summary>
        private static string TryGetStaticThumbnailUrl(BaseEntity entity, int fileStorageId, int width, int height, bool isFullPathImageUrl)
        {
            try
            {
                var fileStorage = ResolveFileStorageForImageUrl(entity, fileStorageId);
                if (fileStorage == null || string.IsNullOrWhiteSpace(fileStorage.FileName))
                {
                    return null;
                }

                // Seed demo thumbs still contain burned-in filenames — force resize proxy.
                if (FilesHelper.IsSeedPlaceholderMedia(fileStorage))
                {
                    return null;
                }

                if (!FilesHelper.CanServeRequestFromThumbnail(width, height, fileStorage.Width, fileStorage.Height))
                {
                    return null;
                }

                if (!FilesHelper.ThumbnailFileExists(fileStorage.FileName))
                {
                    return null;
                }

                var relative = FilesHelper.GetThumbnailPublicUrl(fileStorage.FileName);
                if (string.IsNullOrEmpty(relative))
                {
                    return null;
                }

                if (!isFullPathImageUrl)
                {
                    return relative;
                }

                var baseUrl = GetAbsoluteApplicationBaseUrl(AppConfig.HttpProtocolForImages);
                if (string.IsNullOrEmpty(baseUrl))
                {
                    return relative;
                }

                return baseUrl.TrimEnd('/') + relative;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "TryGetStaticThumbnailUrl fallback to resize proxy for fileStorageId={0}", fileStorageId);
                return null;
            }
        }

        private static FileStorage ResolveFileStorageForImageUrl(BaseEntity entity, int fileStorageId)
        {
            if (entity is FileStorage asFileStorage && asFileStorage.Id == fileStorageId)
            {
                return asFileStorage;
            }

            if (entity is BaseContent asContent
                && asContent.MainImage != null
                && asContent.MainImageId.GetValueOrDefault() == fileStorageId)
            {
                return asContent.MainImage;
            }

            if (entity is ProductFile asProductFile
                && asProductFile.FileStorage != null
                && asProductFile.FileStorage.Id == fileStorageId)
            {
                return asProductFile.FileStorage;
            }

            if (entity is StoryFile asStoryFile
                && asStoryFile.FileStorage != null
                && asStoryFile.FileStorage.Id == fileStorageId)
            {
                return asStoryFile.FileStorage;
            }

            var fileStorageService = DependencyResolver.Current != null
                ? DependencyResolver.Current.GetService<IFileStorageService>()
                : null;
            return fileStorageService != null ? fileStorageService.GetFileStorage(fileStorageId) : null;
        }

        private static string GetImagePathOrDefaultImage(int width, int height, string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                return imagePath;
            }
            else
            {
                if (width == 0 && height == 0)
                {
                    width = 800;
                    height = 600;
                }
                imagePath = $"/images/defaultimage/w{width}h{height}/default.jpg";
            }

            return imagePath;
        }

        /// <summary>
        /// Get the extension from the given filename
        /// </summary>
        /// <param name="fileName">the given filename ie:abc.123.txt</param>
        /// <returns>the extension ie:txt</returns>
        private static string RemoveFileExtension(string fileName)
        {
            string ext = string.Empty;
            int fileExtPos = fileName.LastIndexOf(".", StringComparison.Ordinal);
            if (fileExtPos >= 0)
            {
                ext = fileName.Substring(fileExtPos, fileName.Length - fileExtPos);
                return fileName.Replace(ext, "");
            }

            return fileName;
        }

        public static String GetAdminCroppedImageUrl(this FileStorage fileStorage, int width = 0, int height = 0)
        {
            if (HttpContext.Current == null)
                return "";
            if (fileStorage != null)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var imageId = string.Format("{0}.jpg", fileStorage.Id);
                String imagePath = urlHelper.Action(Constants.ImageActionName, "Images", new { area = "admin", id = imageId, width, height });
                return imagePath;
            }
            return "";
        }

        public static String GetDetailPageUrl_OLD(this BaseEntity entity, String action, String controller, String categoryName = "", String protocol = "", String authorName = "")
        {
            if (HttpContext.Current == null)
                return "";
            if (entity != null)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                if (!String.IsNullOrEmpty(authorName))
                {
                    return urlHelper.Action(action, controller, new { id = authorName, area = "" }, protocol);
                }
                else if (String.IsNullOrEmpty(categoryName))
                {
                    return urlHelper.Action(action, controller, new { id = GetSeoUrl(entity), area = "" }, protocol);
                }
                else if (!String.IsNullOrEmpty(categoryName))
                {
                    return urlHelper.Action(action, controller, new { categoryName = GeneralHelper.GetUrlSeoString(categoryName), id = GetSeoUrl(entity), area = "" }, protocol);
                }
            }
            return "";
        }

        public static String GetDetailPageUrl(this BaseEntity entity, String action, String controller, String categoryName = "", String protocol = "", String authorName = "")
        {
            string path = "";

            if (entity != null)
            {
                path = BuildDetailRelativePathWithoutHttpContext(entity, action, controller, categoryName, authorName);
            }

            string domain = AppConfig.Domain;
            if (string.IsNullOrEmpty(domain))
            {
                if (HttpContext.Current == null)
                {
                    return path ?? string.Empty;
                }

                // Authority keeps the port (e.g. localhost:81); Host alone drops it
                domain = HttpContext.Current.Request.Url.Authority;
            }

            var httpProtocol = string.IsNullOrEmpty(protocol) ? AppConfig.HttpProtocol : protocol;
            if (string.IsNullOrEmpty(httpProtocol))
            {
                httpProtocol = AppConfig.HttpProtocol;
            }

            return $"{httpProtocol}://{domain}{path}";
        }

        /// <summary>
        /// Absolute site base URL (scheme + authority + app path), safe when HttpContext.Current is null
        /// after ConfigureAwait(false) continuations.
        /// </summary>
        public static string GetAbsoluteApplicationBaseUrl(string protocol = null)
        {
            var context = HttpContext.Current;
            if (context?.Request != null)
            {
                var request = context.Request;
                return request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/');
            }

            var domain = AppConfig.Domain;
            if (string.IsNullOrEmpty(domain))
            {
                return string.Empty;
            }

            var scheme = string.IsNullOrEmpty(protocol) ? AppConfig.HttpProtocol : protocol;
            return $"{scheme}://{domain.TrimEnd('/')}";
        }

        private static string BuildDetailRelativePathWithoutHttpContext(
            BaseEntity entity,
            string action,
            string controller,
            string categoryName,
            string authorName)
        {
            if (entity == null)
            {
                return string.Empty;
            }

            var seoId = !string.IsNullOrEmpty(authorName) ? authorName : GetSeoUrl(entity);
            var categorySeo = string.IsNullOrEmpty(categoryName)
                ? string.Empty
                : GeneralHelper.GetUrlSeoString(categoryName);

            if (string.Equals(controller, "Products", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, Constants.DetailAction, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(categorySeo))
            {
                return $"/{Constants.ProductsControllerRoutingPrefix}/{categorySeo}/{seoId}";
            }

            if (string.Equals(controller, "ProductCategories", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, "Category", StringComparison.OrdinalIgnoreCase))
            {
                return $"/{Constants.ProductsCategoriesControllerRoutingPrefix}/pc/{seoId}";
            }

            if (string.Equals(controller, "Stories", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, Constants.DetailAction, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(categorySeo))
            {
                return $"/{Constants.StoriesCategoriesControllerRoutingPrefix}/{categorySeo}/{seoId}";
            }

            if (string.Equals(controller, "Stories", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, "Categories", StringComparison.OrdinalIgnoreCase))
            {
                return $"/{Constants.StoriesCategoriesControllerRoutingPrefix}/sc/{seoId}";
            }

            if (string.Equals(controller, "Products", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, "Tag", StringComparison.OrdinalIgnoreCase))
            {
                return $"/{Constants.ProductsControllerRoutingPrefix}/t/{seoId}";
            }

            if (string.Equals(controller, "Stories", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, "Tag", StringComparison.OrdinalIgnoreCase))
            {
                return $"/{Constants.StoriesCategoriesControllerRoutingPrefix}/t/{seoId}";
            }

            if (string.Equals(controller, "Pages", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, Constants.DetailAction, StringComparison.OrdinalIgnoreCase))
            {
                return $"/{Constants.PagesControllerRoutingPrefix}/{seoId}";
            }

            if (string.Equals(controller, "Payment", StringComparison.OrdinalIgnoreCase)
                && string.Equals(action, "BuyNow", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(categorySeo))
            {
                return $"/b/{categorySeo}/{seoId}";
            }

            if (!string.IsNullOrEmpty(categorySeo))
            {
                return $"/{controller}/{action}/{categorySeo}/{seoId}";
            }

            return $"/{controller}/{action}/{seoId}";
        }

    }
}