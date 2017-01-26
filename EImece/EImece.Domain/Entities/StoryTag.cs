using GenericRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class StoryTag : IEntity<int>
    {
        public int Id { get; set; }
        [ForeignKey("Story")]
        public int StoryId { get; set; }
        public int TagId { get; set; }

        public virtual Story Story { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
