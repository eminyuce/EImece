using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.MigrationModels
{
    public class EntityImage
    {
        public List<EntityMainImage> EntityMainImages { get; set; }
        public List<EntityMediaFile> EntityMediaFiles { get; set; }
    }
}
