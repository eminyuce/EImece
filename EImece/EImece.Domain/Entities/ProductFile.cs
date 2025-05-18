using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class ProductFile : BaseEntity
    {
        public int FileStorageId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public FileStorage FileStorage { get; set; }
        public Product Product { get; set; }

        public string ImageFullPath(int width, int height, bool isThump = false)
        {
            var request = HttpContext.Current.Request;
            var baseurl = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/');
            var result = this.GetCroppedImageUrl(FileStorage.Id, width, height, true, isThump);
            if (!result.Contains(baseurl))
            {
                result = baseurl + result;
            }
            return result;
        }
    }
}