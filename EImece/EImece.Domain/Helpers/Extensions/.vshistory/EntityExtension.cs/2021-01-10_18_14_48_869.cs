using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
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
            String link = String.Format("{0}", product.GetDetailPageUrl("Detail", "Stories", categoryName,
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
                            SyndicationLink.CreateMediaEnclosureLink(new Uri(imageUrl), "image/jpeg", 100);
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
            String link = String.Format("{0}", product.GetDetailPageUrl("Detail", "Stories", categoryName,
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
            String link = String.Format("{0}", product.GetDetailPageUrl("Detail", "Products", product.ProductCategory.Name,
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

            if (!String.IsNullOrEmpty(product.ProductCategory.Name))
            {
                si.ElementExtensions.Add("category", String.Empty, product.ProductCategory.Name);
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
                            SyndicationLink.CreateMediaEnclosureLink(new Uri(imageUrl), "image/jpeg", 100);
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
            return string.Format("{0}-{1}.jpg", GeneralHelper.GetUrlSeoString(RemoveFileExtension(entity.Name)), GeneralHelper.ModifyId(fileStorageId));
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
            string imagePath = Constants.UrlBase + mainImage.FileName;
            String fullPath = Path.Combine(AppConfig.StorageRoot, mainImage.FileName);
            if (File.Exists(fullPath))
            {
                if (isThump)
                {
                    string fileName = mainImage.FileName;
                    string partThumb1 = Path.Combine(Constants.UrlBase, "thumbs");
                    string partThumb2 = Path.Combine(partThumb1, "thb" + fileName);
                    imagePath = partThumb2;
                }
                return imagePath;
            }
            return string.Empty;
        }
        public static string GetFullPathImageUrlFromFileSystem(this BaseContent entity, bool isThump)
        {
            try
            {
                if (entity != null && entity.MainImageId.HasValue && entity.MainImageId.Value != 0 && entity.ImageState)
                {
                   return GetFullPathImageUrlFromFileStorage(entity.MainImage,isThump);
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
                return $"<img src='{AppConfig.GetDefaultImage(width, height)}'    />";
            }

            return imageTag;
        }

        public static string GetCroppedImageTag(this BaseEntity entity, int fileStorageId, int width = 0, int height = 0)
        {
            string imageTag = "";
            if (entity != null && fileStorageId > 0)
            {
                string imagePath = GetCroppedImageUrl(entity, fileStorageId, width, height);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    imageTag = string.Format("<img src='{0}' alt='{1}'   />",
                        imagePath, entity.Name, width, height).ToLower();
                }
            }
            else
            {
                return $"<img src='{AppConfig.GetDefaultImage(width, height)}'    />";
            }

            return imageTag;
        }

        public static string GetCroppedImageUrl(this BaseEntity entity, int? fileStorageIdOptional, int width = 0, int height = 0, bool isFullPathImageUrl = false)
        {
            var fileStorageId = fileStorageIdOptional.HasValue ? fileStorageIdOptional.Value : 0;
            var result = GetCroppedImageUrl(entity, fileStorageId, width, height, isFullPathImageUrl);

            return result;
        }

        public static string GetCroppedImageUrl(this BaseEntity entity, int fileStorageId, int width = 0, int height = 0, bool isFullPathImageUrl = false)
        {
            if (entity != null && fileStorageId > 0)
            {
                if (AppConfig.IsImageFullSrcUnderMediaFolder && entity is BaseContent)
                {
                    var baseContentEntity = (BaseContent)entity;
                    var imagePath = GetFullPathImageUrlFromFileSystem(baseContentEntity, false);
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
                else if (AppConfig.IsImageFullSrcUnderMediaFolder && entity is ProductFile)
                {
                    var baseContentEntity = (ProductFile)entity;
                    var imagePath = GetFullPathImageUrlFromFileStorage(baseContentEntity.FileStorage, false);
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
                else
                {
                    var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                    var imageSize = $"w{width}h{height}";
                    if (isFullPathImageUrl)
                    {
                        return urlHelper.Action(Constants.ImageActionName, "Images", new { imageSize, id = entity.GetImageSeoUrl(fileStorageId), area = "" }, HttpContext.Current.Request.Url.Scheme);
                    }
                    else
                    {
                        return urlHelper.Action(Constants.ImageActionName, "Images", new { imageSize, id = entity.GetImageSeoUrl(fileStorageId), area = "" });
                    }
                }
               
            }
            return AppConfig.GetDefaultImage(width, height);
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
            if (fileStorage != null)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var imageId = string.Format("{0}.jpg", fileStorage.Id);
                String imagePath = urlHelper.Action(Constants.ImageActionName, "Images", new { area = "admin", id = imageId, width, height });
                return imagePath;
            }
            return "";
        }

        public static String GetDetailPageUrl(this BaseEntity entity, String action, String controller, String categoryName = "", String protocol = "")
        {
            if (entity != null)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                if (String.IsNullOrEmpty(categoryName))
                {
                    return urlHelper.Action(action, controller, new { id = GetSeoUrl(entity),area="" }, protocol);
                }
                else
                {
                    return urlHelper.Action(action, controller, new { categoryName = GeneralHelper.GetUrlSeoString(categoryName), id = GetSeoUrl(entity) }, protocol);
                }
            }
            return "";
        }
    }
}