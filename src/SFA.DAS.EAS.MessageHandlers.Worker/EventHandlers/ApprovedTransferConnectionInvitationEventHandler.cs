﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.EmployerAccounts.Events.Messages;
using SFA.DAS.Messaging;
using SFA.DAS.Messaging.AzureServiceBus.Attributes;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.EAS.MessageHandlers.Worker.EventHandlers
{
    [TopicSubscription("Task_ApprovedTransferConnectionInvitation")]
    public class ApprovedTransferConnectionInvitationEventHandler : MessageProcessor<ApprovedTransferConnectionInvitationEvent>
    {
        private readonly EmployerApprenticeshipsServiceConfiguration _employerApprenticeshipsServiceConfiguration;

        public const string UrlFormat = "/accounts/{0}/transfers";

        private readonly EmployerAccountDbContext _db;
        private readonly ILog _log;
        private readonly INotificationsApi _notificationsApi;

        public ApprovedTransferConnectionInvitationEventHandler(IMessageSubscriberFactory subscriberFactory, ILog log, EmployerAccountDbContext dbContext, INotificationsApi notificationsApi, EmployerApprenticeshipsServiceConfiguration employerApprenticeshipsServiceConfiguration) : base(subscriberFactory, log)
        {
            _db = dbContext;
            _notificationsApi = notificationsApi;
            _employerApprenticeshipsServiceConfiguration = employerApprenticeshipsServiceConfiguration;
            _log = log;
        }

        protected override async Task ProcessMessage(ApprovedTransferConnectionInvitationEvent messageContent)
        {
            var users = _db.Users
                .Where(user =>
                    user.UserAccountSettings.Any(us => 
                        us.AccountId == messageContent.SenderAccountId && us.ReceiveNotifications) &&
                    user.Memberships.Any(ms =>
                        ms.AccountId == messageContent.SenderAccountId && ms.Role == Role.Owner));

            foreach (var owner in users)
            {
                try
                {
                    await _notificationsApi.SendEmail(CreateEmail(owner, messageContent.ReceiverAccountName,
                        messageContent.SenderAccountHashedId));
                }
                catch (Exception ex)
                {
                    _log.Error(ex, $"Unable to send approved transfer invitation notification to userId {owner.Id} for Receiver Account Id {messageContent.ReceiverAccountId} ");
                }
            }
        }

        private Email CreateEmail(User user, string accountName, string senderExternalId)
        {
            return new Email
            {
                RecipientsAddress = user.Email,
                TemplateId = "TransferConnectionRequestApproved",
                ReplyToAddress = "noreply@sfa.gov.uk",
                Tokens = new Dictionary<string, string>
                {
                    {"name", user.FirstName},
                    {"account_name", accountName},
                    {
                        "link_notification_page",
                        $"{_employerApprenticeshipsServiceConfiguration.DashboardUrl}{string.Format(UrlFormat, senderExternalId)}"
                    }
                }
            };
        }
    }
}
