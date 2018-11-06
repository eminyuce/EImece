using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
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
                         ApplicationConfigs.HttpProtocol));

            var desc = GeneralHelper.StripHtml(product.Description).ToStr(rssParams.Description);

            var pageLink = new Uri(link.ToLower());
            var ub = new UriBuilder(pageLink);
            if (!string.IsNullOrEmpty(rssParams.GetAnalyticsQueryString()))
            {
                ub.Query = rssParams.GetAnalyticsQueryString();
            }
            var si = new SyndicationItem(product.Name, desc, ub.Uri);
            si.PublishDate = product.UpdatedDate.Value.ToUniversalTime();

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
                         ApplicationConfigs.HttpProtocol));

            var desc = GeneralHelper.StripHtml(product.Description).ToStr(rssParams.Description);

            var pageLink = new Uri(link.ToLower());
            var ub = new UriBuilder(pageLink);
            if (!string.IsNullOrEmpty(rssParams.GetAnalyticsQueryString()))
            {
                ub.Query = rssParams.GetAnalyticsQueryString();
            }
            var si = new SyndicationItem(product.Name, desc, ub.Uri);
            si.PublishDate = product.UpdatedDate.Value.ToUniversalTime();

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
                         ApplicationConfigs.HttpProtocol));

            var desc = GeneralHelper.StripHtml(product.Description).ToStr(rssParams.Description);
            var pageLink = new Uri(link.ToLower());
            var ub = new UriBuilder(pageLink);
            if (!string.IsNullOrEmpty(rssParams.GetAnalyticsQueryString()))
            {
                ub.Query = rssParams.GetAnalyticsQueryString();
            }
            var si = new SyndicationItem(product.Name, desc, ub.Uri);
            si.PublishDate = product.UpdatedDate.Value.ToUniversalTime();

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

        public static double GetProductPrice(this Product product)
        {
            var categoryDiscount = product.ProductCategory.DiscountPercantage.HasValue ? product.ProductCategory.DiscountPercantage.Value : 0;
            return product.Price - (product.Discount) - (product.Price * (categoryDiscount / 100));
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
            if (obj == null)
                return;

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

            foreach (PropertyInfo p in obj.GetType().GetProperties(flags))
            {
                Type currentNodeType = p.PropertyType;
                if (currentNodeType == typeof(String))
                {
                    string currentValue = (string)p.GetValue(obj, null);
                    if (currentValue != null)
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

        #endregion

        public static String GetSeoUrl(this BaseEntity entity)
        {
            //return String.Format("{0}-{1}",
            //    GeneralHelper.GetUrlSeoString(entity.Name),
            //    GeneralHelper.Base64Encode(entity.Id));

            return String.Format("{0}-{1}",
               GeneralHelper.GetUrlSeoString(entity.Name),
               entity.Id);

        }
        public static String GetSeoTitle(this BaseEntity entity, int length = 50)
        {
            return GeneralHelper.Capitalize(GeneralHelper.TruncateAtWord(entity.Name, length));
        }
        public static String GetSeoDescription(this BaseContent entity, int length = 150)
        {
            var result = string.Format("{0}", GeneralHelper.GetDescriptionWithBody(entity.Description, length));
            if (String.IsNullOrEmpty(result))
            {
                var SettingService = DependencyResolver.Current.GetService<ISettingService>();
                result = SettingService.GetSettingByKey(ApplicationConfigs.SiteIndexMetaDescription).ToStr();
            }
            return result;
        }
        public static String GetSeoKeywords(this BaseContent entity, int length = 150)
        {
            var result = string.Format("{0}", entity.MetaKeywords.ToStr(255));
            if (String.IsNullOrEmpty(result))
            {
                //TODO: Missing keywords.
                var SettingService = DependencyResolver.Current.GetService<ISettingService>();
                result = SettingService.GetSettingByKey(ApplicationConfigs.SiteIndexMetaKeywords).ToStr();
            }
            return result;
        }
        public static String GetImageTag(this BaseContent entity)
        {
            String imageTag = "";
            if (entity != null && entity.MainImageId.HasValue && entity.MainImage != null && entity.MainImageId.Value != 0 && entity.ImageState)

            {
                String imagePath = GetFullPathImageUrlFromFileSystem(entity, false);
                imageTag = String.Format("<img src='{0}' alt='{1}'/>", imagePath, entity.Name).ToLower();

            }

            return imageTag;
        }
        public static String GetThumpImageTag(this BaseContent entity)
        {
            String imageTag = "";


            if (entity.MainImageId.HasValue && entity.MainImage != null && entity.MainImageId.Value != 0 && entity.ImageState)
            {
                String partThumb2 = GetFullPathImageUrlFromFileSystem(entity, true);
                imageTag = String.Format("<img src='{0}' alt='{1}'/>", partThumb2, entity.Name).ToLower();

            }

            return imageTag;
        }
        public static String GetFullPathImageUrlFromFileSystem(this BaseContent entity, bool isThump)
        {
            if (entity.MainImageId.HasValue && entity.MainImageId.Value != 0 && entity.ImageState)
            {
                String imagePath = ApplicationConfigs.UrlBase + entity.MainImage.FileName;
                if (isThump)
                {
                    String fileName = entity.MainImage.FileName;
                    String partThumb1 = Path.Combine(ApplicationConfigs.UrlBase, "thumbs");
                    String partThumb2 = Path.Combine(partThumb1, "thb" + fileName);
                    imagePath = partThumb2;
                }
                return imagePath;
            }
            return String.Empty;
        }
        public static String GetCroppedImageTag(this BaseContent entity, int width, int height)
        {
            String imageTag = "";
            if (entity.MainImageId.HasValue && entity.MainImageId.Value != 0 && entity.ImageState)
            {
                imageTag = GetCroppedImageTag(entity, entity.MainImageId.Value, width, height);
            }
            else
            {
                //  imageTag = "Test";
            }

            return imageTag;
        }
        public static String GetCroppedImageTag(this BaseEntity entity, int fileStorageId, int width = 0, int height = 0)
        {
            String imageTag = "";

            String imagePath = GetCroppedImageUrl(entity, fileStorageId, width, height);
            if (!String.IsNullOrEmpty(imagePath))
            {
                imageTag = String.Format("<img src='{0}' alt='{1}'   />",
                    imagePath, entity.Name, width, height).ToLower();
            }


            return imageTag;
        }
        public static String GetCroppedImageUrl(this BaseEntity entity, int fileStorageId, int width = 0, int height = 0)
        {
            if (fileStorageId > 0)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var imageSize = String.Format("w{0}h{1}", width, height);
                var imageId = String.Format("{0}-{1}.jpg", GeneralHelper.GetUrlSeoString(RemoveFileExtension(entity.Name)), fileStorageId);
                String imagePath = urlHelper.Action(ApplicationConfigs.ImageActionName, "Images", new { imageSize, id = imageId });
                return imagePath;
            }
            else
            {
                return String.Empty;
            }

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
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var imageId = String.Format("{0}.jpg", fileStorage.Id);
            String imagePath = urlHelper.Action(ApplicationConfigs.ImageActionName, "Images", new { area = "admin", id = imageId, width, height });
            return imagePath;
        }

        public static String GetDetailPageUrl(this BaseEntity entity, String action, String controller, String categoryName = "", String protocol = "")
        {
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            if (String.IsNullOrEmpty(categoryName))
            {
                return urlHelper.Action(action, controller, new { id = GetSeoUrl(entity) }, protocol);
            }
            else
            {
                return urlHelper.Action(action, controller, new { categoryName = GeneralHelper.GetUrlSeoString(categoryName), id = GetSeoUrl(entity) }, protocol);
            }

        }
    }
}
