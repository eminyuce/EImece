namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Minimal projection for product specification template — only XML required for ProdSpecs helper.
    /// Query: SELECT TemplateXml FROM Templates WHERE Id=@id
    /// </summary>
    public class TemplateXmlDto
    {
        public string TemplateXml { get; set; }
    }
}
