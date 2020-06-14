namespace EImece.Domain.Models.Enums
{
    public enum NotificationStatus
    {
        NotTracked = -1,
        ServiceWorkerUpdated = 1,
        ServiceWorkerUnregister = 2,
        DebuggingSingnal = 4,//3
        NotificationDelivered = 8,
        NotificationClicked = 16, //9
    }
}