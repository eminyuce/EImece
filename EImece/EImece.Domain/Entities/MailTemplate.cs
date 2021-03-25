using Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class MailTemplate : BaseEntity
    {
        [AllowHtml]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MailSubjectPropertyRequiredErrorMessage))]
        [StringLength(500, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MailSubjectPropertyErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MailSubject))]
        public String Subject { get; set; }

        [AllowHtml]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MailBodyPropertyRequiredErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MailBody))]
        [DataType(DataType.MultilineText)]
        public String Body { get; set; }

        public String UpdateUserId { get; set; }
        public String AddUserId { get; set; }
        public bool TrackWithBitly { get; set; }
        public bool TrackWithMlnk { get; set; }
    }
}