using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Pure (no DB) assembly of the storefront/admin product-category tree.
    /// Seeds each node with its own active-product count, then accumulates descendant
    /// counts into parents post-order — matching the legacy entity-based builder.
    /// </summary>
    public static class ProductCategoryTreeAssembler
    {
        public static List<ProductCategoryTreeModel> Assemble(List<StorefrontCategoryDto> categories)
        {
            var list = (categories ?? new List<StorefrontCategoryDto>()).Select(r => new ProductCategoryTreeModel()
            {
                ProductCategory = r,
                // Seed the model-level counts from the DTO BEFORE the tree walk:
                // views render Model.ProductCount; the tree walk then adds descendants into parents.
                ProductCount = r != null ? r.ProductCount : 0,
                // raw per-category count, captured before accumulation
                ProductCountAdmin = r != null ? r.ProductCount : 0
            }).ToList();

            List<ProductCategoryTreeModel> returnList = new List<ProductCategoryTreeModel>();

            int level = 1;
            //find top levels items
            var topLevels = list.Where(a => a.ProductCategory != null && a.ProductCategory.ParentId == 0)
                                .OrderBy(r => r.ProductCategory.Position).ToList();
            topLevels.ForEach(r => r.TreeLevel = level);
            returnList.AddRange(topLevels);
            foreach (var i in topLevels)
            {
                AttachChildren(list, i, level);
            }
            return returnList;
        }

        /// <summary>
        /// Recursively attaches children of <paramref name="current"/> and accumulates their
        /// (already complete) subtree product counts into it, post-order.
        /// </summary>
        public static void AttachChildren(List<ProductCategoryTreeModel> list, ProductCategoryTreeModel current, int level)
        {
            var childs = list.Where(a => a.ProductCategory != null && current.ProductCategory != null && a.ProductCategory.ParentId == current.ProductCategory.Id)
                             .OrderBy(r => r.ProductCategory.Position).ToList();
            current.Childrens = new List<ProductCategoryTreeModel>();
            level = level + 1;
            childs.ForEach(r => r.TreeLevel = level);

            current.Childrens.AddRange(childs);
            foreach (var i in childs)
            {
                i.ProductCategory.Parent = current.ProductCategory;
                i.Parent = current;
                AttachChildren(list, i, level);
                current.ProductCount += i.ProductCount;
            }
        }
    }
}
