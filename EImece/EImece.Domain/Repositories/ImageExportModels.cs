namespace EImece.Domain.Repositories
{
    /// <summary>
    /// Read models used by the image export feature. Repositories return these;
    /// services map them into export metadata structures.
    /// </summary>
    public class ProductImageInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public int? MainImageId { get; set; }
        public int ProductCategoryId { get; set; }
    }

    public class ProductFileImageInfo
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int FileStorageId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
    }

    public class CategoryImageInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MainImageId { get; set; }
        public int ParentId { get; set; }
    }

    public class MenuImageInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MainImageId { get; set; }
        public string MenuLink { get; set; }
        public string Link { get; set; }
    }

    public class MenuFileImageInfo
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int FileStorageId { get; set; }
        public string MenuName { get; set; }
    }

    public class StoryImageInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MainImageId { get; set; }
        public int StoryCategoryId { get; set; }
    }

    public class StoryFileImageInfo
    {
        public int Id { get; set; }
        public int StoryId { get; set; }
        public int FileStorageId { get; set; }
    }

    public class BrandImageInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MainImageId { get; set; }
    }
}
