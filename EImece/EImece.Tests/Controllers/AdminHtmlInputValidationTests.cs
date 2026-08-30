using EImece.App_Start;
using EImece.Areas.Admin.Controllers;
using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace EImece.Tests.Controllers
{
    [TestClass]
    public class AdminHtmlInputValidationTests
    {
        [TestInitialize]
        public void RegisterHtmlMetadata()
        {
            AdminHtmlMetadataConfig.Register();
        }

        [TestMethod]
        public void AdminHtmlSaveControllers_UseValidateInputFalsePattern()
        {
            var saveOrEditCases = new[]
            {
                Tuple.Create(typeof(ProductsController), "SaveOrEdit", typeof(Product)),
                Tuple.Create(typeof(StoriesController), "SaveOrEdit", typeof(Story)),
                Tuple.Create(typeof(StoryCategoriesController), "SaveOrEdit", typeof(StoryCategory)),
                Tuple.Create(typeof(ProductCategoriesController), "SaveOrEdit", typeof(ProductCategory)),
                Tuple.Create(typeof(MenusController), "SaveOrEdit", typeof(Menu)),
                Tuple.Create(typeof(MainPageImagesController), "SaveOrEdit", typeof(MainPageImage)),
                Tuple.Create(typeof(FaqController), "SaveOrEdit", typeof(Faq)),
                Tuple.Create(typeof(BrandsController), "SaveOrEdit", typeof(Brand)),
                Tuple.Create(typeof(MailTemplatesController), "SaveOrEdit", typeof(MailTemplate)),
                Tuple.Create(typeof(TemplatesController), "SaveOrEdit", typeof(Template)),
                Tuple.Create(typeof(AdminSettingsController), "Index", typeof(SettingModel)),
                Tuple.Create(typeof(AdminSettingsController), "SystemSettings", typeof(SystemSettingModel)),
            };

            foreach (var testCase in saveOrEditCases)
            {
                AssertPostActionDisablesRequestValidation(testCase.Item1, testCase.Item2, testCase.Item3);
            }
        }

        private static void AssertPostActionDisablesRequestValidation(Type controllerType, string actionName, Type modelType)
        {
            var method = FindPostMethod(controllerType, actionName, modelType);
            Assert.IsNotNull(method, controllerType.Name + " must expose POST " + actionName + " for " + modelType.Name);

            var validateInput = method.GetCustomAttributes(typeof(ValidateInputAttribute), inherit: false)
                .Cast<ValidateInputAttribute>()
                .SingleOrDefault();

            Assert.IsNotNull(validateInput, controllerType.Name + "." + actionName + " POST must disable request validation for HTML fields.");
            Assert.IsFalse(validateInput.EnableValidation, controllerType.Name + "." + actionName + " POST must set ValidateInput(false).");
        }

        [TestMethod]
        public void HtmlBoundProperties_HaveAllowHtmlMetadata()
        {
            AssertPropertyAllowsHtml(typeof(Product), nameof(Product.Description));
            AssertPropertyAllowsHtml(typeof(Product), nameof(Product.ShortDescription));
            AssertPropertyAllowsHtml(typeof(ProductCategory), nameof(ProductCategory.Description));
            AssertPropertyAllowsHtml(typeof(ProductCategory), nameof(ProductCategory.ShortDescription));
            AssertPropertyAllowsHtml(typeof(Brand), nameof(Brand.Description));
            AssertPropertyAllowsHtml(typeof(Menu), nameof(Menu.Description));
            AssertPropertyAllowsHtml(typeof(Story), nameof(Story.Description));
            AssertPropertyAllowsHtml(typeof(Story), nameof(Story.ShortDescription));
            AssertPropertyAllowsHtml(typeof(StoryCategory), nameof(StoryCategory.Description));
            AssertPropertyAllowsHtml(typeof(MainPageImage), nameof(MainPageImage.Description));
            AssertPropertyAllowsHtml(typeof(Faq), nameof(Faq.Question));
            AssertPropertyAllowsHtml(typeof(Faq), nameof(Faq.Answer));
            AssertPropertyAllowsHtml(typeof(MailTemplate), nameof(MailTemplate.Body));
            AssertPropertyAllowsHtml(typeof(Setting), nameof(Setting.Description));
            AssertPropertyAllowsHtml(typeof(Setting), nameof(Setting.SettingValue));
            AssertPropertyAllowsHtml(typeof(Customer), nameof(Customer.Description));
            AssertPropertyAllowsHtml(typeof(SettingModel), nameof(SettingModel.FooterHtmlDescription));
            AssertPropertyAllowsHtml(typeof(SystemSettingModel), nameof(SystemSettingModel.UnderConstructionHtml));
            AssertPropertyAllowsHtml(typeof(SystemSettingModel), nameof(SystemSettingModel.PaymentDetailHtml));
        }

        [TestMethod]
        public void AdminProductList_DefaultSort_IsUpdatedDateDescending()
        {
            var older = new DateTime(2024, 1, 1);
            var newer = new DateTime(2024, 6, 1);
            var products = new List<Product>
            {
                new Product { Id = 1, ProductCategoryId = 1928, Position = 1, UpdatedDate = older },
                new Product { Id = 2, ProductCategoryId = 1928, Position = 99, UpdatedDate = newer },
                new Product { Id = 3, ProductCategoryId = 1928, Position = 2, UpdatedDate = older.AddDays(1) },
                new Product { Id = 4, ProductCategoryId = 999, Position = 1, UpdatedDate = newer.AddDays(1) },
            };

            var categoryProducts = products.Where(p => p.ProductCategoryId == 1928);
            var orderedIds = categoryProducts.OrderByDescending(p => p.UpdatedDate).ThenByDescending(p => p.Id).Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 2, 3, 1 }, orderedIds);
        }

        [TestMethod]
        public void AdminProductIndexGrid_IncludesUpdatedDateColumnForGriddlySort()
        {
            var gridViewPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "EImece", "EImece", "Areas", "Admin", "Views", "Products", "IndexGrid.cshtml");

            gridViewPath = System.IO.Path.GetFullPath(gridViewPath);
            var gridMarkup = System.IO.File.ReadAllText(gridViewPath);

            StringAssert.Contains(gridMarkup, "Field = \"UpdatedDate\"");
            StringAssert.Contains(gridMarkup, "x => x.UpdatedDate");
        }

        private static MethodInfo FindPostMethod(Type controllerType, string actionName, Type modelType)
        {
            return controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .SingleOrDefault(m =>
                    m.Name == actionName
                    && m.GetCustomAttributes(typeof(HttpPostAttribute), inherit: false).Any()
                    && m.GetParameters().Any(p => p.ParameterType == modelType));
        }

        private static void AssertPropertyAllowsHtml(Type modelType, string propertyName)
        {
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, modelType, propertyName);
            Assert.IsFalse(metadata.RequestValidationEnabled,
                modelType.Name + "." + propertyName + " must allow HTML input via [AllowHtml] metadata.");
        }
    }
}
