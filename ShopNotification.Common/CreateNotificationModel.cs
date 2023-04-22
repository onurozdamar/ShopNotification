using System;
namespace ShopNotification.Common
{
    public class CreateNotificationModel
    {
        public int UserId { get; set; }

        public string Message { get; set; }

        public NotificationType NotificationType { get; set; }
    }

    public enum NotificationType
    {
        Normal,
        Emergency
    }
}

