using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.Extensions
{
    public static class EntityExtension
    {
        public static List<BaseEntity> DownCasting<T>(this List<T> items) where T : BaseEntity
        {
            var baseList = new List<BaseEntity>();
            items.ForEach(v => baseList.Add(v));
            return baseList;
        }
        public static String GetSeoUrl(this BaseEntity entity)
        {
            return String.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(entity.Name), entity.Id);
        }
        public static String GetSeoTitle(this BaseEntity entity, int length = 50)
        {
            return GeneralHelper.Capitalize(GeneralHelper.TruncateAtWord(entity.Name, length));
        }
        public static String GetSeoDescription(this BaseContent entity, int length = 150)
        {
            return string.Format("{0}", GeneralHelper.GetDescriptionWithBody(entity.Description, length));
        }
        public static String GetImageTag(this BaseContent entity)
        {
            String imageTag = "";
            if (entity.MainImageId.HasValue && entity.MainImage != null && entity.ImageState)
            {
                String imagePath = Settings.UrlBase + entity.MainImage.FileName;
                imageTag = String.Format("<img src='{0}' alt='{1}'/>", imagePath, entity.Name).ToLower();

            }

            return imageTag;
        }
        public static String GetCroppedImageTag(this BaseContent entity, int width, int height)
        {
            String imageTag = "";
            if (entity.MainImageId.HasValue && entity.ImageState)
            {
                imageTag = GetCroppedImageTag(entity, entity.MainImageId.Value, width, height);
            }
            else
            {

            }

            return imageTag;
        }
        public static String GetCroppedImageTag(this BaseEntity entity, int fileStorageId, int width = 0, int height = 0)
        {
            String imageTag = "";
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            String imagePath = GetCroppedImageUrl(fileStorageId, width, height);
            imageTag = String.Format("<img src='{0}' alt='{1}'/>", imagePath, entity.Name).ToLower();

            return imageTag;
        }
        public static String GetCroppedImageUrl(int fileStorageId, int width = 0, int height = 0)
        {
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            String imagePath = urlHelper.Action("Index", "Images", new { id = fileStorageId, width, height });
            return imagePath;
        }

    }
}
