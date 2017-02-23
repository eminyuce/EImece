using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Resources;

namespace EImece.Domain.Entities
{
    public class StoryFile : BaseEntity
    {
        public int StoryId { get; set; }
        public int FileStorageId { get; set; }

        public  FileStorage FileStorage { get; set; }
    }
}
