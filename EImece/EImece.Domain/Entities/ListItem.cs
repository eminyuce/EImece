using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class ListItem : BaseEntity
    {
        [ForeignKey("List")]
        public int ListId { get; set; }

        public string Value { get; set; }

        public List List { get; set; }
    }
}