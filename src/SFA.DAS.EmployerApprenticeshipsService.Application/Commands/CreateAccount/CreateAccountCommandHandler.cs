﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Audit.Types;
using SFA.DAS.EAS.Application.Commands.AuditCommand;
using SFA.DAS.EAS.Application.Commands.PublishGenericEvent;
using SFA.DAS.EAS.Application.Exceptions;
using SFA.DAS.EAS.Application.Factories;
using SFA.DAS.EAS.Application.Hashing;
using SFA.DAS.EAS.Application.Queries.GetUserByRef;
using SFA.DAS.EAS.Application.Validation;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Domain.Models.Audit;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Events.Messages;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.HashingService;
using Entity = SFA.DAS.Audit.Types.Entity;

namespace SFA.DAS.EAS.Application.Commands.CreateAccount
{
    //TODO this needs changing to be a facade and calling individual commands for each component
    public class CreateAccountCommandHandler : IAsyncRequestHandler<CreateAccountCommand, CreateAccountCommandResponse>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IMediator _mediator;
        private readonly IValidator<CreateAccountCommand> _validator;
        private readonly IHashingService _hashingService;
        private readonly IHashingService _publicHashingService;
        private readonly IGenericEventFactory _genericEventFactory;
        private readonly IAccountEventFactory _accountEventFactory;
        private readonly IRefreshEmployerLevyService _refreshEmployerLevyService;
        private readonly IMembershipRepository _membershipRepository;

        public CreateAccountCommandHandler(
            IAccountRepository accountRepository, 
            IMessagePublisher messagePublisher, 
            IMediator mediator, 
            IValidator<CreateAccountCommand> validator, 
            IHashingService hashingService,
            IPublicHashingService publicHashingService,
            IGenericEventFactory genericEventFactory, 
            IAccountEventFactory accountEventFactory, 
            IRefreshEmployerLevyService refreshEmployerLevyService,
            IMembershipRepository membershipRepository)
        {
            _accountRepository = accountRepository;
            _messagePublisher = messagePublisher;

            _mediator = mediator;
            _validator = validator;
            _hashingService = hashingService;
            _publicHashingService = publicHashingService;
            _genericEventFactory = genericEventFactory;
            _accountEventFactory = accountEventFactory;
            _refreshEmployerLevyService = refreshEmployerLevyService;
            _membershipRepository = membershipRepository;
        }

        public async Task<CreateAccountCommandResponse> Handle(CreateAccountCommand message)
        {
            await ValidateMessage(message);

            var userResponse = await _mediator.SendAsync(new GetUserByRefQuery { UserRef = message.ExternalUserId });

            if (string.IsNullOrEmpty(message.OrganisationReferenceNumber))
            {
                message.OrganisationReferenceNumber = Guid.NewGuid().ToString();
            }

            var createAccountResult = await _accountRepository.CreateAccount(userResponse.User.Id, message.OrganisationReferenceNumber, message.OrganisationName, message.OrganisationAddress, message.OrganisationDateOfInception, message.PayeReference, message.AccessToken, message.RefreshToken, message.OrganisationStatus, message.EmployerRefName, (short)message.OrganisationType, message.PublicSectorDataSource, message.Sector);
            
            var hashedAccountId = _hashingService.HashValue(createAccountResult.AccountId);
            var publicHashedAccountId = _publicHashingService.HashValue(createAccountResult.AccountId);

            await _accountRepository.UpdateAccountHashedIds(createAccountResult.AccountId, hashedAccountId, publicHashedAccountId);

            await RefreshLevy(createAccountResult, message.PayeReference);

            var caller = await _membershipRepository.GetCaller(createAccountResult.AccountId, message.ExternalUserId);

            var createdByName = caller.FullName();
            await PublishAddPayeSchemeMessage(message.PayeReference, createAccountResult.AccountId, createdByName, userResponse.User.UserRef);

            await PublishAccountCreatedMessage(createAccountResult.AccountId, createdByName, message.ExternalUserId);

            await NotifyAccountCreated(hashedAccountId);

            await CreateAuditEntries(message, createAccountResult, hashedAccountId, userResponse.User);

            await PublishLegalEntityAddedMessage(createAccountResult.AccountId, createAccountResult.LegalEntityId,
                createAccountResult.EmployerAgreementId, message.OrganisationName, createdByName, message.ExternalUserId);

            await PublishAgreementCreatedMessage(createAccountResult.AccountId, createAccountResult.LegalEntityId,
                createAccountResult.EmployerAgreementId, message.OrganisationName, createdByName, message.ExternalUserId);

            return new CreateAccountCommandResponse
            {
                HashedAccountId = hashedAccountId
            };
        }

        private async Task PublishAgreementCreatedMessage(long accountId, long legalEntityId, long employerAgreementId, string organisationName, string userName, string userRef)
        {
            await _messagePublisher.PublishAsync(new AgreementCreatedMessage(accountId, employerAgreementId, organisationName, legalEntityId, userName, userRef));
        }

        private async Task PublishLegalEntityAddedMessage(long accountId, long legalEntityId, long employerAgreementId, string organisationName, string userName, string userRef)
        {
            await _messagePublisher.PublishAsync(new LegalEntityAddedMessage(accountId, employerAgreementId, organisationName, legalEntityId, userName, userRef));
        }

        private async Task NotifyAccountCreated(string hashedAccountId)
        {
            var accountEvent = _accountEventFactory.CreateAccountCreatedEvent(hashedAccountId);

            var genericEvent = _genericEventFactory.Create(accountEvent);

            await _mediator.SendAsync(new PublishGenericEventCommand { Event = genericEvent });
        }

        private async Task RefreshLevy(CreateAccountResult returnValue, string empref)
        {
            await _refreshEmployerLevyService.QueueRefreshLevyMessage(returnValue.AccountId, empref);
        }

        private async Task PublishAddPayeSchemeMessage(string empref, long accountId, string createdByName, string userRef)
        {
                await _messagePublisher.PublishAsync(new PayeSchemeAddedMessage(empref, accountId, createdByName, userRef));
        }

        private async Task PublishAccountCreatedMessage(long accountId, string createdByName, string userRef)
        {
            await _messagePublisher.PublishAsync(new AccountCreatedMessage(accountId, createdByName, userRef));
        }

        private async Task ValidateMessage(CreateAccountCommand message)
        {
            var validationResult = await _validator.ValidateAsync(message);

            if (!validationResult.IsValid())
                throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        private async Task CreateAuditEntries(CreateAccountCommand message, CreateAccountResult returnValue, string hashedAccountId, User user)
        {
            //Account
            await _mediator.SendAsync(new CreateAuditCommand
            {
                EasAuditMessage = new EasAuditMessage
                {
                    Category = "CREATED",
                    Description = $"Account {message.OrganisationName} created with id {returnValue.AccountId}",
                    ChangedProperties = new List<PropertyUpdate>
                    {
                        PropertyUpdate.FromLong("AccountId", returnValue.AccountId),
                        PropertyUpdate.FromString("HashedId", hashedAccountId),
                        PropertyUpdate.FromString("Name", message.OrganisationName),
                        PropertyUpdate.FromDateTime("CreatedDate", DateTime.UtcNow),
                    },
                    AffectedEntity = new Entity { Type = "Account", Id = returnValue.AccountId.ToString() },
                    RelatedEntities = new List<Entity>()
                }
            });
            //LegalEntity
            var changedProperties = new List<PropertyUpdate>
            {
                PropertyUpdate.FromLong("Id", returnValue.LegalEntityId),
                PropertyUpdate.FromString("Name", message.OrganisationName),
                PropertyUpdate.FromString("Code", message.OrganisationReferenceNumber),
                PropertyUpdate.FromString("RegisteredAddress", message.OrganisationAddress),
                PropertyUpdate.FromString("OrganisationType", message.OrganisationType.ToString()),
                PropertyUpdate.FromString("PublicSectorDataSource", message.PublicSectorDataSource.ToString()),
                PropertyUpdate.FromString("Sector", message.Sector)
            };
            if (message.OrganisationDateOfInception != null)
            {
                changedProperties.Add(PropertyUpdate.FromDateTime("DateOfIncorporation", message.OrganisationDateOfInception.Value));
            }

            await _mediator.SendAsync(new CreateAuditCommand
            {
                EasAuditMessage = new EasAuditMessage
                {
                    Category = "CREATED",
                    Description = $"Legal Entity {message.OrganisationName} created of type {message.OrganisationType} with id {returnValue.LegalEntityId}",
                    ChangedProperties = changedProperties,
                    AffectedEntity = new Entity { Type = "LegalEntity", Id = returnValue.LegalEntityId.ToString() },
                    RelatedEntities = new List<Entity>()
                }
            });

            //EmployerAgreement 
            await _mediator.SendAsync(new CreateAuditCommand
            {
                EasAuditMessage = new EasAuditMessage
                {
                    Category = "CREATED",
                    Description = $"Employer Agreement Created for {message.OrganisationName} legal entity id {returnValue.LegalEntityId}",
                    ChangedProperties = new List<PropertyUpdate>
                    {
                        PropertyUpdate.FromLong("Id", returnValue.EmployerAgreementId),
                        PropertyUpdate.FromLong("LegalEntityId", returnValue.LegalEntityId),
                        PropertyUpdate.FromString("TemplateId", hashedAccountId),
                        PropertyUpdate.FromInt("StatusId", 2),
                    },
                    RelatedEntities = new List<Entity> { new Entity { Id = returnValue.EmployerAgreementId.ToString(), Type = "LegalEntity" } },
                    AffectedEntity = new Entity { Type = "EmployerAgreement", Id = returnValue.EmployerAgreementId.ToString() }
                }
            });

            //AccountEmployerAgreement Account Employer Agreement
            await _mediator.SendAsync(new CreateAuditCommand
            {
                EasAuditMessage = new EasAuditMessage
                {
                    Category = "CREATED",
                    Description = $"Employer Agreement Created for {message.OrganisationName} legal entity id {returnValue.LegalEntityId}",
                    ChangedProperties = new List<PropertyUpdate>
                    {
                        PropertyUpdate.FromLong("AccountId", returnValue.AccountId),
                        PropertyUpdate.FromLong("EmployerAgreementId", returnValue.EmployerAgreementId),
                    },
                    RelatedEntities = new List<Entity>
                    {
                        new Entity { Id = returnValue.EmployerAgreementId.ToString(), Type = "LegalEntity" },
                        new Entity { Id = returnValue.AccountId.ToString(), Type = "Account" }
                    },
                    AffectedEntity = new Entity { Type = "AccountEmployerAgreement", Id = returnValue.EmployerAgreementId.ToString() }
                }
            });

            //Paye 
            await _mediator.SendAsync(new CreateAuditCommand
            {
                EasAuditMessage = new EasAuditMessage
                {
                    Category = "CREATED",
                    Description = $"Paye scheme {message.PayeReference} added to account {returnValue.AccountId}",
                    ChangedProperties = new List<PropertyUpdate>
                    {
                        PropertyUpdate.FromString("Ref", message.PayeReference),
                        PropertyUpdate.FromString("AccessToken", message.AccessToken),
                        PropertyUpdate.FromString("RefreshToken", message.RefreshToken),
                        PropertyUpdate.FromString("Name", message.EmployerRefName)
                    },
                    RelatedEntities = new List<Entity> { new Entity { Id = returnValue.AccountId.ToString(), Type = "Account" } },
                    AffectedEntity = new Entity { Type = "Paye", Id = message.PayeReference }
                }
            });

            //Membership Account
            await _mediator.SendAsync(new CreateAuditCommand
            {
                EasAuditMessage = new EasAuditMessage
                {
                    Category = "CREATED",
                    Description = $"User {message.ExternalUserId} added to account {returnValue.AccountId} as owner",
                    ChangedProperties = new List<PropertyUpdate>
                    {
                        PropertyUpdate.FromLong("AccountId", returnValue.AccountId),
                        PropertyUpdate.FromString("UserId", message.ExternalUserId),
                        PropertyUpdate.FromString("RoleId", Role.Owner.ToString()),
                        PropertyUpdate.FromDateTime("CreatedDate", DateTime.UtcNow)
                    },
                    RelatedEntities = new List<Entity>
                    {
                        new Entity { Id = returnValue.AccountId.ToString(), Type = "Account" },
                        new Entity { Id = user.Id.ToString(), Type = "User" }
                    },
                    AffectedEntity = new Entity { Type = "Membership", Id = message.ExternalUserId }
                }
            });
        }
    }
}