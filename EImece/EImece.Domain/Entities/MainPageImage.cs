using Resources;
using System.ComponentModel.DataAnnotations;


namespace EImece.Domain.Entities
{
    public class MainPageImage : BaseContent
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Link))]
        public string Link { get; set; }



    }
}
