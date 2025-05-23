﻿using EImece.Domain.Helpers;
using EImece.Domain.Models.MigrationModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EImece.Domain.DbContext
{
    public class DbMigration
    {
        public static List<ProductImageExternalUrl> GetProductImages(string connectionString)
        {
            var result = new List<ProductImageExternalUrl>();
            string commandText = "[ewsiste].[ProductImageExternalUrl]";
            var parameterList = new List<SqlParameter>();
            var commandType = CommandType.StoredProcedure;

            DataSet dataSet = DatabaseUtility.ExecuteDataSet(new SqlConnection(connectionString), commandText, commandType, parameterList.ToArray());

            if (dataSet.Tables.Count > 0)
            {
                using (DataTable dt = dataSet.Tables[0])
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var productImage = GetProductImageFromDataRow(dr);
                        result.Add(productImage);
                    }
                }
            }

            return result;
        }

        private static ProductImageExternalUrl GetProductImageFromDataRow(DataRow dr)
        {
            return new ProductImageExternalUrl
            {
                ProductId = dr["ProductId"].ToInt(),
                ProductName = dr["ProductName"].ToStr(),
                ImageFullPath = dr["ImageFullPath"].ToStr(),
                EntityImageType = dr["EntityImageType"].ToStr()
            };
        }

        public static EntityImage GetImages(String connectionString)
        {
            var result = new EntityImage();
            String commandText = @"GetImages";
            var parameterList = new List<SqlParameter>();
            var commandType = CommandType.StoredProcedure;
            DataSet dataSet = DatabaseUtility.ExecuteDataSet(new SqlConnection(connectionString), commandText, commandType, parameterList.ToArray());
            if (dataSet.Tables.Count > 0)
            {
                result.EntityMainImages = new List<EntityMainImage>();
                using (DataTable dt = dataSet.Tables[0])
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var e = GetEntityMainImageFromDataRow(dr);
                        result.EntityMainImages.Add(e);
                    }
                }
                result.EntityMediaFiles = new List<EntityMediaFile>();
                using (DataTable dt = dataSet.Tables[1])
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var e = GetEntityMediaFileFromDataRow(dr);
                        result.EntityMediaFiles.Add(e);
                    }
                }
            }
            return result;
        }

        public static void InsertProductImages(string connectionString, ProductImageExternalUrl entityMainImage, Entities.FileStorage image)
        {
            var commandText = @"[ewsiste].[InsertProductImage]";
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("ProductId", entityMainImage.ProductId, SqlDbType.Int));
            parameterList.Add(DatabaseUtility.GetSqlParameter("ProductName", entityMainImage.ProductName, SqlDbType.NVarChar));
            parameterList.Add(DatabaseUtility.GetSqlParameter("EntityImageType", entityMainImage.EntityImageType, SqlDbType.NVarChar));
            parameterList.Add(DatabaseUtility.GetSqlParameter("ImageId", image.Id, SqlDbType.Int));
            var commandType = CommandType.StoredProcedure;
            DatabaseUtility.ExecuteScalar(new SqlConnection(connectionString), commandText, commandType, parameterList.ToArray());
        }

        private static EntityMediaFile GetEntityMediaFileFromDataRow(DataRow dr)
        {
            var item = new EntityMediaFile();
            item.CategoryName = dr["CategoryName"].ToStr();
            item.File_Type = dr["File_Type"].ToStr();
            item.Modul_Name = dr["Modul_Name"].ToStr();
            item.Mod = dr["Mod"].ToStr();
            item.Name = dr["Name"].ToStr();
            item.File_Path = dr["File_Path"].ToStr();
            item.File_Name = dr["File_Name"].ToStr();
            item.File_Desc = dr["File_Desc"].ToStr();
            item.File_Format = dr["File_Format"].ToStr();

            return item;
        }

        private static EntityMainImage GetEntityMainImageFromDataRow(DataRow dr)
        {
            var item = new EntityMainImage();

            item.EntityImageType = dr["EntityImageType"].ToStr();
            item.ImagePath = dr["ImagePath"].ToStr();
            item.ImagePath2 = dr["ImagePath2"].ToStr();
            item.Name = dr["Name"].ToStr();
            item.CategoryName = dr["CategoryName"].ToStr();
            return item;
        }
    }
}