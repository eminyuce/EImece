using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.MigrationModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
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
                _filesHelper.InitFilesMediaFolder();

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
                try
                {
                    if (entityMainImage.EntityImageType.Equals("ProductMainImage", StringComparison.InvariantCultureIgnoreCase))
                    {
                        FileStorage image = InsertImage(entityMainImage.ImagePath, entityMainImage.Name);
                        FileStorage image2 = InsertImage(entityMainImage.ImagePath2, entityMainImage.Name);
                        if (image != null && image2 != null)
                        {
                            object[] parameters = { currentLanguage, entityMainImage.Name, image.Id, image2.Id, image2.FileName, entityMainImage.CategoryName, };
                            var result = this.ProductRepository.GetDbContext().Database.ExecuteSqlCommand(@"exec InsertProductFiles
                                @Lang=@p0,@Name=@p1,@fileStorageId=@p2,@MainProductImageId=@p3,@fileName=@p4,@CategoryName=@p5", parameters);
                            Logger.Info("InsertProductFiles:" + result + " parameters:" + String.Join(", ", parameters));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, ex.Message + " entityMainImage:" + entityMainImage);
                }
            }

            foreach (var entityMediaImage in images.EntityMediaFiles)
            {
                try
                {
                    if (entityMediaImage.Mod.Equals("product", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (FilesHelper.IsImage(entityMediaImage.File_Format))
                        {
                            FileStorage image = InsertImage(entityMediaImage.File_Path, entityMediaImage.Name);
                            if (image != null)
                            {
                                object[] parameters = { currentLanguage, entityMediaImage.Name, image.Id, 1, image.FileName, entityMediaImage.CategoryName, 1 };
                                var result = this.ProductRepository.GetDbContext().Database.ExecuteSqlCommand(@"exec InsertProductFiles
                                @Lang=@p0,@Name=@p1,@fileStorageId=@p2,@MainProductImageId=@p3,@fileName=@p4,@CategoryName=@p5, @SkipMainImage=@p6", parameters);
                                Logger.Info("Media InsertProductFiles:" + result + " parameters:" + String.Join(", ", parameters));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, ex.Message + " entityMainImage:" + entityMediaImage);
                }
            }
        }

        private FileStorage InsertImage(String imagePath, String name)
        {
            string imageFullPath = String.Format("{0}/{1}", this.SiteUrl, imagePath.Replace("thb", "").Replace("~/", ""));
            try
            {
                var imageDictionary = new Dictionary<String, String>();
                var imageBytes = DownloadHelper.GetImageFromUrl(imageFullPath, imageDictionary);
                var imageBitmap = this.FilesHelper.ByteArrayToBitmap(imageBytes);
                String ext = imagePath.Substring(imagePath.Length - 4);
                string fileName = GeneralHelper.GetUrlSeoString(name) + ext;
                string mimeType = MimeMapping.GetMimeMapping(fileName);
                FileStorage image = this.FilesHelper.SaveFileFromByteArray(imageBytes,
                    fileName,
                    mimeType,
                    imageBitmap.Width,
                    imageBitmap.Height,
                    EImeceImageType.ProductMainImage, null);
                return image;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message + " imageFullPath: " + imageFullPath + " name:" + name);
                return null;
            }
        }
    }
}