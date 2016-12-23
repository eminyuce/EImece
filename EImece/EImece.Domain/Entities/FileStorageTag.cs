using GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class FileStorageTag : IEntity<int>
    {
        public int Id { get; set; }
        public int FileStorageId { get; set; }
        public int TagId { get; set; }
    }
}
