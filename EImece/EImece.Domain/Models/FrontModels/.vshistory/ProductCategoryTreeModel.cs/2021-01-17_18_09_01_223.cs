using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using System.Collections.Generic;
using System.Text;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductCategoryTreeModel
    {
        public string CssClassName { get; set; }
        public int TreeLevel { get; set; }
        public ProductCategory ProductCategory { get; set; }
        public int ProductCount { get; set; }
        public int ProductCountAdmin { get; set; }
        public List<ProductCategoryTreeModel> Childrens { get; set; }

        public List<ProductCategoryTreeModel> AllChildrens
        {
            get
            {
                List<ProductCategoryTreeModel> allchildrens = new List<ProductCategoryTreeModel>();
                if (Childrens.IsNotEmpty())
                {
                    allchildrens.AddRange(Childrens);
                    foreach (var item in Childrens)
                    {
                        GetSubChildren(item, allchildrens);
                    }
                }
                return allchildrens;
            }
        }

        private void GetSubChildren(ProductCategoryTreeModel child, List<ProductCategoryTreeModel> allchildrens)
        {
            if (child.Childrens.IsNotEmpty())
            {
                foreach (var item in child.Childrens)
                {
                    if (item.Childrens.IsNotEmpty())
                    {
                        allchildrens.AddRange(item.Childrens);
                        GetSubChildren(item, allchildrens);
                    }
                }
            }
        }

        public ProductCategoryTreeModel Parent { get; set; }

        public string ProductCategoryName
        {
            get
            {
                return string.Format("{0}", ProductCategory.Name);
            }
        }

        public string AdminText
        {
            get
            {
                if (ProductCountAdmin > 0)
                {
                    return string.Format("{0} ({1})", ProductCategory.Name, ProductCountAdmin);
                }
                else
                {
                    return string.Format("{0}", ProductCategory.Name);
                }
            }
        }

        public string Text
        {
            get
            {
                if (ProductCount > 0)
                {
                    return string.Format("{0} ({1})", ProductCategory.Name, ProductCount);
                }
                else
                {
                    return string.Format("{0}", ProductCategory.Name);
                }
            }
        }

        public string TextWithArrow
        {
            get
            {
                if (ProductCount > 0)
                {
                    return string.Format("{2}{0} ({1})", ProductCategory.Name, ProductCount, ProduceArrow());
                }
                else
                {
                    return string.Format("{1}{0}", ProductCategory.Name, ProduceArrow());
                }
            }
        }

        public string ProduceArrow()
        {
            var builder = new StringBuilder();
            int count = TreeLevel - 1;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    builder.Append(" — ");
                }
                builder.Append("> ");
            }
            return builder.ToString();
        }
    }
}