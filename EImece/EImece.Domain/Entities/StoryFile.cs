namespace EImece.Domain.Entities
{
    public class StoryFile : BaseEntity
    {
        public int StoryId { get; set; }
        public int FileStorageId { get; set; }

        public FileStorage FileStorage { get; set; }
    }
}