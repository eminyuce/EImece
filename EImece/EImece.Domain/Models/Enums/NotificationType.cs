﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.Enums
{
    public enum NotificationType
    {
        NotTracked = -1,
        ServiceWorkerUpdated = 1,
        ServiceWorkerUnregister = 2,
        DebuggingSingnal = 4,//3
        NotificationDelivered = 8,
        NotificationClicked = 16, //9

    }
}