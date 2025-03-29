namespace EImece.Domain.Models.MigrationModels
{
    public class ProductImageExternalUrl
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageFullPath { get; set; }
        public string EntityImageType { get; set; }
    }
}