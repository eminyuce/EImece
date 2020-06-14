using System.Collections.Generic;

namespace EImece.Domain.Models.MigrationModels
{
    public class EntityImage
    {
        public List<EntityMainImage> EntityMainImages { get; set; }
        public List<EntityMediaFile> EntityMediaFiles { get; set; }
    }
}