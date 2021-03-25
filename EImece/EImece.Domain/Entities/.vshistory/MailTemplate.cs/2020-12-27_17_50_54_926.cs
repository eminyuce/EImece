using Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class MailTemplate : BaseEntity
    {
        [AllowHtml]
        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MailSubjectPropertyRequiredErrorMessage))]
        [StringLength(500, ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MailSubjectPropertyErrorMessage))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MailSubject))]
        public String Subject { get; set; }

        [AllowHtml]
        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MailBodyPropertyRequiredErrorMessage))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MailBody))]
        [DataType(DataType.MultilineText)]
        public String Body { get; set; }

        public String UpdateUserId { get; set; }
        public String AddUserId { get; set; }
        public bool TrackWithBitly { get; set; }
        public bool TrackWithMlnk { get; set; }
    }
}