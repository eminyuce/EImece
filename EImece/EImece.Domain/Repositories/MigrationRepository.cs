using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.MigrationModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EImece.Domain.Repositories
{
    public class MigrationRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductRepository ProductRepository { get; set; }

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IProductFileRepository ProductFileRepository { get; set; }
        [Inject]
        public IFileStorageRepository FileStorageRepository { get; set; }

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

        public String SiteUrl { get; set; }

        public MigrationRepository()
        {

        }

        public EntityImage GetImages()
        {
            return DbMigration.GetImages(this.ProductRepository.GetDbContext().Database.Connection.ConnectionString);
        }

        public void MigrateImages(int currentLanguage)
        {
            var images = GetImages();
            this.FilesHelper.CurrentLanguage = currentLanguage;
            // var products = ProductService.GetAll().Where(r => images.EntityMainImages.Any(l => l.Name.Trim().Contains(r.Name.Trim().ToLower()))).ToList();
            foreach (var entityMainImage in images.EntityMainImages)
            {
                string imageFullPath = String.Format("{0}/{1}", this.SiteUrl, entityMainImage.ImagePath.Replace("thb", "").Replace("~/", ""));
                var imageDictionary = new Dictionary<String, String>();

                try
                {


                    var imageBytes = DownloadHelper.GetImageFromUrl(imageFullPath, imageDictionary);
                    var imageBitmap = this.FilesHelper.ByteArrayToBitmap(imageBytes);
                    if (entityMainImage.EntityImageType.Equals("ProductMainImage", StringComparison.InvariantCultureIgnoreCase))
                    {

                        String ext = entityMainImage.ImagePath.Substring(entityMainImage.ImagePath.Length - 4);
                        string fileName = GeneralHelper.GetUrlSeoString(entityMainImage.Name) + ext;
                        string mimeType = MimeMapping.GetMimeMapping(fileName);
                        FileStorage image = this.FilesHelper.SaveFileFromByteArray(imageBytes,
                            fileName,
                            mimeType,
                            imageBitmap.Width,
                            imageBitmap.Height,
                            EImeceImageType.ProductMainImage, null);

                        //var product = products.FirstOrDefault(r => r.Name.Trim().ToLower().Equals(entityMainImage.Name.Trim().ToLower()));
                        //if (product != null)
                        //{
                        //    if (image != null)
                        //    {
                        //        product.MainImage = image;
                        //        product.MainImageId = image.Id;
                        //        product.ImageState = true;

                        //    }
                        //}
                        if (image != null)
                        {
                            object[] parameters = { entityMainImage.Name.Trim().ToLower(), image.Id };
                            this.ProductRepository.GetDbContext().Database.ExecuteSqlCommand("update Products set MainImageId=@p1, [UpdatedDate]=getdate() where Name=@p0", parameters);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, ex.Message + " imageFullPath:" + imageFullPath + " ProductName:" + entityMainImage.Name);

                }
            }

            //foreach (var product in products)
            //{
            //    ProductService.SaveOrEditEntity(product);
            //}


        }

    }
}

