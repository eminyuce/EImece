namespace EImece.Domain.Models.MigrationModels
{
    public class EntityMainImage
    {
        public string CategoryName { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public string ImagePath2 { get; set; }
        public string EntityImageType { get; set; }

        public override string ToString()
        {
            return "Name:" + Name + " ImagePath:" + ImagePath + "  ImagePath2:" + ImagePath2 + "  EntityImageType:" + EntityImageType;
        }
    }
}