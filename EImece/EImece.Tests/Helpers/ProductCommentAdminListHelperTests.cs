using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ProductCommentAdminListHelperTests
    {
        [TestMethod]
        public void ApplyAdminFilters_SearchesByProductName()
        {
            var comments = CreateComments();

            var result = ProductCommentAdminListHelper.ApplyAdminFilters(comments, lang: 1, productId: null, search: "Pamuk", ratings: null).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(c => c.Product.Name.Contains("Pamuk")));
        }

        [TestMethod]
        public void ApplyAdminFilters_SearchesByClientAndReviewFields()
        {
            var comments = CreateComments();

            var byEmail = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "ayse@eimece.test", null).ToList();
            var bySubject = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "Kaliteli", null).ToList();
            var byReview = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "Beden", null).ToList();

            Assert.AreEqual(1, byEmail.Count);
            Assert.AreEqual("Ayşe", byEmail[0].Name);
            Assert.AreEqual(1, bySubject.Count);
            Assert.AreEqual("Kaliteli ürün", bySubject[0].Subject);
            Assert.AreEqual(1, byReview.Count);
            Assert.AreEqual("Beden tam oldu", byReview[0].Subject);
        }

        [TestMethod]
        public void ApplyAdminFilters_FiltersByStarRating()
        {
            var comments = CreateComments();

            var fiveStars = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "", new[] { 5 }).ToList();
            var threeStars = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "", new[] { 3 }).ToList();

            Assert.AreEqual(1, fiveStars.Count);
            Assert.AreEqual(5, fiveStars[0].Rating);
            Assert.AreEqual(1, threeStars.Count);
            Assert.AreEqual(3, threeStars[0].Rating);
        }

        [TestMethod]
        public void ApplyAdminFilters_FiltersByMultipleStarRatings()
        {
            var comments = CreateComments();

            var result = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "", new[] { 4, 5 }).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(c => c.Rating == 4 || c.Rating == 5));
            Assert.IsFalse(result.Any(c => c.Rating == 3));
        }

        [TestMethod]
        public void ApplyAdminFilters_WithoutProductId_ReturnsAllLanguageMatchesSortedByUpdatedDate()
        {
            var comments = CreateComments();

            var result = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "", null).ToList();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Rahat kumaş", result[0].Subject);
            Assert.AreEqual("Kaliteli ürün", result[1].Subject);
            Assert.AreEqual("Beden tam oldu", result[2].Subject);
        }

        [TestMethod]
        public void ApplyAdminFilters_WithProductId_LimitsToThatProduct()
        {
            var comments = CreateComments();

            var result = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, 10, "", null).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(c => c.ProductId == 10));
        }

        [TestMethod]
        public void ApplyAdminFilters_FiltersByUpdatedDateRangeInclusive()
        {
            var comments = CreateComments();
            var start = new DateTime(2026, 8, 1);
            var end = new DateTime(2026, 8, 2);

            var result = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "", null, start, end).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(c => c.UpdatedDate.Date >= start && c.UpdatedDate.Date <= end));
            Assert.IsFalse(result.Any(c => c.Subject == "Rahat kumaş"));
        }

        [TestMethod]
        public void ApplyAdminFilters_FiltersByStartDateOnly()
        {
            var comments = CreateComments();
            var start = new DateTime(2026, 8, 3);

            var result = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "", null, start, null).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Rahat kumaş", result[0].Subject);
        }

        [TestMethod]
        public void ApplyAdminFilters_SwapsInvertedDateRange()
        {
            var comments = CreateComments();
            var start = new DateTime(2026, 8, 2);
            var end = new DateTime(2026, 8, 1);

            var result = ProductCommentAdminListHelper.ApplyAdminFilters(comments, 1, null, "", null, start, end).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(c => c.UpdatedDate.Date >= end && c.UpdatedDate.Date <= start));
        }

        private static IQueryable<ProductComment> CreateComments()
        {
            var tulum = new Product { Id = 10, Name = "Organik Pamuk Bebek Tulumu", ProductCode = "TULUM-01", NameShort = "Tulum" };
            var body = new Product { Id = 20, Name = "Bebek Body", ProductCode = "BODY-01", NameShort = "Body" };
            var now = new DateTime(2026, 8, 1, 12, 0, 0);

            return new List<ProductComment>
            {
                new ProductComment
                {
                    Id = 1,
                    Lang = 1,
                    ProductId = 10,
                    Product = tulum,
                    Name = "Ayşe",
                    Email = "ayse@eimece.test",
                    Subject = "Kaliteli ürün",
                    Review = "Çok beğendik",
                    Rating = 5,
                    UpdatedDate = now.AddDays(1)
                },
                new ProductComment
                {
                    Id = 2,
                    Lang = 1,
                    ProductId = 10,
                    Product = tulum,
                    Name = "Mehmet",
                    Email = "mehmet@eimece.test",
                    Subject = "Beden tam oldu",
                    Review = "Beden tam oldu, teşekkürler",
                    Rating = 4,
                    UpdatedDate = now
                },
                new ProductComment
                {
                    Id = 3,
                    Lang = 1,
                    ProductId = 20,
                    Product = body,
                    Name = "Elif",
                    Email = "elif@eimece.test",
                    Subject = "Rahat kumaş",
                    Review = "Yumuşak duruyor",
                    Rating = 3,
                    UpdatedDate = now.AddDays(2)
                },
                new ProductComment
                {
                    Id = 4,
                    Lang = 2,
                    ProductId = 10,
                    Product = tulum,
                    Name = "Anna",
                    Email = "anna@eimece.test",
                    Subject = "English only",
                    Review = "Great",
                    Rating = 5,
                    UpdatedDate = now.AddDays(3)
                }
            }.AsQueryable();
        }
    }
}
