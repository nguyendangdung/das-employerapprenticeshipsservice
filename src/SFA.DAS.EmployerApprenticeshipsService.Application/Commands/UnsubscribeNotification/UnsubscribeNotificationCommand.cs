﻿using MediatR;

namespace SFA.DAS.EAS.Application.Commands.UnsubscribeNotification
{
    public class UnsubscribeNotificationCommand : IAsyncRequest
    {
        public string UserRef { get; set; }

        public long AccountId { get; set; }

        public string NotificationSettingUrl { get; set; }
    }
}
