using GenericRepository;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class StoryTag : IEntity<int>
    {
        public int Id { get; set; }

        [ForeignKey("Story")]
        public int StoryId { get; set; }

        public int TagId { get; set; }

        public Story Story { get; set; }
        public Tag Tag { get; set; }
    }
}