using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class BrowserNotification : BaseEntity
    {
        public int NotificationType { get; set; }
        [NotMapped]
        public NotificationType NotificationTypeEnum
        {
            get { return (NotificationType)NotificationType; }
            set { NotificationType = (int) value; }
        }


        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public string RedirectionUrl { get; set; }

        [NotMapped]
        public bool IsSend { get; set; }
    }
}
