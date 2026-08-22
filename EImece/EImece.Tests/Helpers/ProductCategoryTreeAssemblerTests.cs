using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Tests.Helpers
{
    /// <summary>
    /// Unit tests for the storefront product-category tree assembly.
    /// Regression guard for the DTO refactor: the sidebar tree must keep showing
    /// per-category active-product counts, with parent nodes displaying the sum
    /// of their own plus all descendant category counts.
    /// </summary>
    [TestClass]
    public class ProductCategoryTreeAssemblerTests
    {
        private static StorefrontCategoryDto Cat(int id, int parentId, string name = null, int productCount = 0)
        {
            return new StorefrontCategoryDto
            {
                Id = id,
                ParentId = parentId,
                Name = name ?? $"Category-{id}",
                Position = id,
                IsActive = true,
                Lang = 1,
                ProductCount = productCount
            };
        }

        private static ProductCategoryTreeModel Find(List<ProductCategoryTreeModel> roots, int id)
        {
            foreach (var root in roots)
            {
                if (root.ProductCategory.Id == id) return root;
                var child = Find(root.Childrens, id);
                if (child != null) return child;
            }
            return null;
        }

        [TestMethod]
        public void Assemble_NullOrEmptyInput_ReturnsEmptyList()
        {
            Assert.AreEqual(0, ProductCategoryTreeAssembler.Assemble(null).Count);
            Assert.AreEqual(0, ProductCategoryTreeAssembler.Assemble(new List<StorefrontCategoryDto>()).Count);
        }

        [TestMethod]
        public void Assemble_RootOnly_KeepsItsOwnCount()
        {
            var roots = ProductCategoryTreeAssembler.Assemble(new List<StorefrontCategoryDto>
            {
                Cat(1, 0, productCount: 7)
            });

            Assert.AreEqual(1, roots.Count);
            Assert.AreEqual(7, roots[0].ProductCount);
            // admin count stays the raw per-category value (not accumulated — no children here anyway)
            Assert.AreEqual(7, roots[0].ProductCountAdmin);
            Assert.AreEqual(1, roots[0].TreeLevel);
        }

        [TestMethod]
        public void Assemble_ParentDisplaysSumOfAllDescendants()
        {
            // tree: 1 -> 2 -> 4 ; 1 -> 3   (own counts: 1=5, 2=2, 3=10, 4=8)
            var roots = ProductCategoryTreeAssembler.Assemble(new List<StorefrontCategoryDto>
            {
                Cat(1, 0, productCount: 5),
                Cat(2, 1, productCount: 2),
                Cat(3, 1, productCount: 10),
                Cat(4, 2, productCount: 8)
            });

            var node1 = Find(roots, 1);
            var node2 = Find(roots, 2);
            var node3 = Find(roots, 3);
            var node4 = Find(roots, 4);

            // leaf keeps its own count
            Assert.AreEqual(8, node4.ProductCount);
            // middle node = own + descendants
            Assert.AreEqual(10, node2.ProductCount); // 2 + 8
            // root = own + sum of ALL children totals (post-order accumulation)
            Assert.AreEqual(25, node1.ProductCount); // 5 + (2 + 8) + 10
        }

        [TestMethod]
        public void Assemble_ProductCountAdmin_StaysRawPerCategory()
        {
            var roots = ProductCategoryTreeAssembler.Assemble(new List<StorefrontCategoryDto>
            {
                Cat(1, 0, productCount: 5),
                Cat(2, 1, productCount: 2),
                Cat(3, 1, productCount: 10)
            });

            Assert.AreEqual(5, Find(roots, 1).ProductCountAdmin); // NOT accumulated
            Assert.AreEqual(17, Find(roots, 1).ProductCount);     // accumulated for display
            Assert.AreEqual(2, Find(roots, 2).ProductCountAdmin);
            Assert.AreEqual(10, Find(roots, 3).ProductCountAdmin);
        }

        [TestMethod]
        public void Assemble_CategoryWithoutProducts_ShowsZeroButStillAccumulatesChildren()
        {
            // parent has no own products but two children with products
            var roots = ProductCategoryTreeAssembler.Assemble(new List<StorefrontCategoryDto>
            {
                Cat(1, 0, productCount: 0),
                Cat(2, 1, productCount: 4),
                Cat(3, 1, productCount: 6)
            });

            var parent = Find(roots, 1);
            Assert.AreEqual(10, parent.ProductCount); // pure sum of children
            Assert.AreEqual(0, parent.ProductCountAdmin);
        }

        [TestMethod]
        public void Assemble_CategoryMissingFromCountsDictionary_DefaultsToZero()
        {
            // DTO arrives with default ProductCount (e.g. navigation projection without counts)
            var roots = ProductCategoryTreeAssembler.Assemble(new List<StorefrontCategoryDto>
            {
                Cat(1, 0)               // ProductCount defaults to 0
            });

            Assert.AreEqual(0, roots[0].ProductCount);
        }

        [TestMethod]
        public void Assemble_WiresHierarchyAndOrdering()
        {
            var roots = ProductCategoryTreeAssembler.Assemble(new List<StorefrontCategoryDto>
            {
                Cat(9, 1),              // child of 1 (out of order on purpose)
                Cat(1, 0),
                Cat(5, 1)               // second child of 1, higher Id -> later position
            });

            var root = roots.Single(r => r.ProductCategory.Id == 1);
            Assert.AreEqual(2, root.Childrens.Count);
            // ordered by Position (fixture sets Position = id): 5 then 9
            CollectionAssert.AreEqual(new[] { 5, 9 }, root.Childrens.Select(c => c.ProductCategory.Id).ToArray());

            // tree levels: root=1, children=2
            Assert.AreEqual(1, root.TreeLevel);
            Assert.AreEqual(2, root.Childrens[0].TreeLevel);

            // parent references wired both ways
            Assert.AreSame(root.ProductCategory, root.Childrens[0].ProductCategory.Parent);
            Assert.AreSame(root, root.Childrens[0].Parent);
        }

        [TestMethod]
        public void Assemble_MultipleRoots_AllReturnedAtLevelOne()
        {
            var roots = ProductCategoryTreeAssembler.Assemble(new List<StorefrontCategoryDto>
            {
                Cat(2, 0, productCount: 3),
                Cat(1, 0, productCount: 1)
            });

            CollectionAssert.AreEqual(new[] { 1, 2 }, roots.Select(r => r.ProductCategory.Id).ToArray());
            Assert.IsTrue(roots.All(r => r.TreeLevel == 1));
            Assert.AreEqual(1, roots[0].ProductCount);
            Assert.AreEqual(3, roots[1].ProductCount);
        }
    }
}
