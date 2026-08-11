using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ProductSortHelperTests
    {
        [TestMethod]
        public void OrderByStorefrontDefault_SortsByPositionThenMainPageThenCampaignThenUpdatedDate()
        {
            var older = new DateTime(2024, 1, 1);
            var newer = new DateTime(2024, 6, 1);
            var products = new List<Product>
            {
                new Product { Id = 1, Position = 2, MainPage = true, IsCampaign = true, UpdatedDate = newer },
                new Product { Id = 2, Position = 1, MainPage = false, IsCampaign = false, UpdatedDate = older },
                new Product { Id = 3, Position = 1, MainPage = true, IsCampaign = false, UpdatedDate = older },
                new Product { Id = 4, Position = 1, MainPage = true, IsCampaign = true, UpdatedDate = older },
                new Product { Id = 5, Position = 1, MainPage = true, IsCampaign = true, UpdatedDate = newer },
            };

            var orderedIds = products.OrderByStorefrontDefault().Select(p => p.Id).ToList();

            CollectionAssert.AreEqual(new List<int> { 5, 4, 3, 2, 1 }, orderedIds);
        }
    }
}
