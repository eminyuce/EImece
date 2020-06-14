using GenericRepository;

namespace EImece.Domain.Entities
{
    public class FileStorageTag : IEntity<int>
    {
        public int Id { get; set; }
        public int FileStorageId { get; set; }
        public int TagId { get; set; }

        public Tag Tag { get; set; }
        public FileStorage FileStorage { get; set; }
    }
}