using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Application.Hashing;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.EAS.Application.Commands.SendTransferConnectionInvitation
{
    public class SendTransferConnectionInvitationCommandHandler : IAsyncRequestHandler<SendTransferConnectionInvitationCommand, long>
    {
        private readonly IEmployerAccountRepository _employerAccountRepository;
        private readonly IPublicHashingService _publicHashingService;
        private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly INotificationsApi _notificationsApi;

        public SendTransferConnectionInvitationCommandHandler(
            IEmployerAccountRepository employerAccountRepository,
            IPublicHashingService publicHashingService,
            ITransferConnectionInvitationRepository transferConnectionInvitationRepository,
            IUserRepository userRepository,
            IMembershipRepository membershipRepository,
            INotificationsApi notificationsApi)
        {
            _employerAccountRepository = employerAccountRepository;
            _publicHashingService = publicHashingService;
            _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
            _userRepository = userRepository;
            _notificationsApi = notificationsApi;
            _membershipRepository = membershipRepository;
        }

        public async Task<long> Handle(SendTransferConnectionInvitationCommand message)
        {
            var receiverAccountId = _publicHashingService.DecodeValue(message.ReceiverAccountPublicHashedId);

            var senderUser = await _userRepository.GetUserById(message.UserId.Value);
            var senderAccount = await _employerAccountRepository.GetAccountById(message.AccountId.Value);
            var receiverAccount = await _employerAccountRepository.GetAccountById(receiverAccountId);
            var transferConnectionInvitation = senderAccount.SendTransferConnectionInvitation(receiverAccount, senderUser);

            var accountOwnersTask = _membershipRepository.GetAccountUsersByRole(receiverAccountId, Role.Owner);

            await _transferConnectionInvitationRepository.Add(transferConnectionInvitation);

            await NotifyAllAccountOwners(message, accountOwnersTask, receiverAccount);

            return transferConnectionInvitation.Id;
        }

        private async Task NotifyAllAccountOwners(SendTransferConnectionInvitationCommand message, Task<IEnumerable<User>> accountOwnersTask,
            Domain.Data.Entities.Account.Account receiverAccount)
        {
            var accountOwners = await accountOwnersTask;
            foreach (var owner in accountOwners)
            {
                await _notificationsApi.SendEmail(CreateEmail(owner, receiverAccount.Name, message.NotificationLink));
            }
        }

        private Email CreateEmail(User user, string accountName, string notificationLink)
        {
            return new Email
            {
                RecipientsAddress = user.Email,
                TemplateId = "SendTransferConnectionInvitationNotification",
                ReplyToAddress = "noreply@sfa.gov.uk",
                Subject = "ReceiverConnectionRequestToReview",
                SystemId = "x",
                Tokens = new Dictionary<string, string>
                {
                    { "name", user.FirstName },
                    { "account_name", accountName },
                    { "link_notification_page", notificationLink ?? string.Empty }
                }
            };
        }
    }
}