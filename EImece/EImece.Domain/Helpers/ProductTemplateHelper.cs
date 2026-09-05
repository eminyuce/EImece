using EImece.Domain.Entities;
using System;

namespace EImece.Domain.Helpers
{
    public static class ProductTemplateHelper
    {
        /// <summary>
        /// True when the product's category has an active template with non-empty XML.
        /// </summary>
        public static bool HasUsableProductXmlTemplate(Product product)
        {
            if (product == null || product.ProductCategory == null)
            {
                return false;
            }

            return HasUsableProductXmlTemplate(product.ProductCategory);
        }

        public static bool HasUsableProductXmlTemplate(ProductCategory category)
        {
            if (category == null)
            {
                return false;
            }

            if (!category.TemplateId.HasValue || category.TemplateId.Value <= 0)
            {
                return false;
            }

            var template = category.Template;
            if (template == null)
            {
                // Template nav not loaded — TemplateId alone is not enough to show Specs UI.
                return false;
            }

            if (!template.IsActive)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(template.TemplateXml);
        }
    }
}
