using EImece.Domain.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class BrowserNotification : BaseEntity
    {
        public int NotificationType { get; set; }

        // Needed for Admin panel — admin notification editor binds the enum directly for the type dropdown.
        [NotMapped]
        public NotificationType NotificationTypeEnum
        {
            get { return (NotificationType)NotificationType; }
            set { NotificationType = (int)value; }
        }

        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public string RedirectionUrl { get; set; }

        // Needed for Admin panel — admin notification queue shows transient send status without persisting it.
        [NotMapped]
        public bool IsSend { get; set; }
    }
}