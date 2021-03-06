﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EAS.Application.Commands.AuditCommand;
using SFA.DAS.EAS.Application.Commands.CreateAccount;
using SFA.DAS.EAS.Application.Exceptions;
using SFA.DAS.EAS.Application.Factories;
using SFA.DAS.EAS.Application.Hashing;
using SFA.DAS.EAS.Application.Queries.GetUserByRef;
using SFA.DAS.EAS.Application.Validation;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Domain.Models.AccountTeam;
using SFA.DAS.EAS.Domain.Models.PAYE;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Events.Messages;
using SFA.DAS.Messaging.Interfaces;
using IGenericEventFactory = SFA.DAS.EAS.Application.Factories.IGenericEventFactory;
using SFA.DAS.HashingService;

namespace SFA.DAS.EAS.Application.UnitTests.Commands.CreateAccountCommandTests
{
    public class WhenICallCreateAccount
    {
        private Mock<IAccountRepository> _accountRepository;
        private CreateAccountCommandHandler _handler;
        private Mock<IMessagePublisher> _messagePublisher;
        private Mock<IMediator> _mediator;
        private Mock<IValidator<CreateAccountCommand>> _validator;
        private Mock<IHashingService> _hashingService;
        private Mock<IPublicHashingService> _externalhashingService;
        private Mock<IGenericEventFactory> _genericEventFactory;
        private Mock<IAccountEventFactory> _accountEventFactory;
        private Mock<IRefreshEmployerLevyService> _refreshEmployerLevyService;
        private Mock<IMembershipRepository> _mockMembershipRepository;
        
        private const long ExpectedAccountId = 12343322;
        private const long ExpectedLegalEntityId = 2222;
        private const string ExpectedHashString = "123ADF23";
        private const string ExpectedPublicHashString = "SCUFF";
        
        private User _user;

        [SetUp]
        public void Arrange()
        {
            _accountRepository = new Mock<IAccountRepository>();
            _accountRepository.Setup(x => x.GetPayeSchemesByAccountId(ExpectedAccountId)).ReturnsAsync(new List<PayeView> { new PayeView { LegalEntityId = ExpectedLegalEntityId } });
            _accountRepository.Setup(x => x.CreateAccount(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<short>(), It.IsAny<short?>(), It.IsAny<string>())).ReturnsAsync(new CreateAccountResult { AccountId = ExpectedAccountId, LegalEntityId = 0L, EmployerAgreementId = 0L });
            
            _messagePublisher = new Mock<IMessagePublisher>();
            _mediator = new Mock<IMediator>();

            _user = new User { Id = 33, UserRef = "ABC123"};

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetUserByRefQuery>()))
                .ReturnsAsync(new GetUserByRefResponse {User = _user});

            _validator = new Mock<IValidator<CreateAccountCommand>>();
            _validator.Setup(x => x.ValidateAsync(It.IsAny<CreateAccountCommand>())).ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            _hashingService = new Mock<IHashingService>();
            _hashingService.Setup(x => x.HashValue(ExpectedAccountId)).Returns(ExpectedHashString);

            _externalhashingService = new Mock<IPublicHashingService>();
            _externalhashingService.Setup(x => x.HashValue(ExpectedAccountId)).Returns(ExpectedPublicHashString);

            _genericEventFactory = new Mock<IGenericEventFactory>();
            _accountEventFactory = new Mock<IAccountEventFactory>();

            _refreshEmployerLevyService = new Mock<IRefreshEmployerLevyService>();
            _mockMembershipRepository=new Mock<IMembershipRepository>();
            _mockMembershipRepository.Setup(r => r.GetCaller(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new MembershipView() { FirstName = "Caller", LastName = "Full Name" }));

            _handler = new CreateAccountCommandHandler(
                _accountRepository.Object, 
                _messagePublisher.Object, 
                _mediator.Object, 
                _validator.Object, 
                _hashingService.Object,
                _externalhashingService.Object,
                _genericEventFactory.Object,
                _accountEventFactory.Object,
                _refreshEmployerLevyService.Object,
                _mockMembershipRepository.Object);
        }

        [Test]
        public async Task ThenTheIdHashingServiceIsCalledAfterTheAccountIsCreated()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { PayeReference = "123/abc,456/123", AccessToken = "123rd", RefreshToken = "45YT" };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert
            _hashingService.Verify(x => x.HashValue(ExpectedAccountId), Times.Once);
        }

        [Test]
        public async Task ThenTheIdPublicHashingServiceIsCalledAfterTheAccountIsCreated()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { PayeReference = "123/abc,456/123", AccessToken = "123rd", RefreshToken = "45YT" };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert
            _externalhashingService.Verify(x => x.HashValue(ExpectedAccountId), Times.Once);
        }



        [Test]
        public async Task ThenTheAccountIsUpdatedWithTheHashes()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { PayeReference = "123/abc,456/123", AccessToken = "123rd", RefreshToken = "45YT" };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert
            _accountRepository.Verify(x => x.UpdateAccountHashedIds(ExpectedAccountId, ExpectedHashString, ExpectedPublicHashString), Times.Once);
        }

        [Test]
        public async Task ThenTheHashedIdIsReturnedInTheResponse()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { PayeReference = "123/abc,456/123", AccessToken = "123rd", RefreshToken = "45YT" };

            //Act
            var actual = await _handler.Handle(createAccountCommand);

            //Assert
            Assert.IsAssignableFrom<CreateAccountCommandResponse>(actual);
            Assert.AreEqual(ExpectedHashString, actual.HashedAccountId);
        }

        [Test]
        public void ThenTheValidatorIsCalledAndAInvalidRequestExceptionIsThrownWhenInvalid()
        {
            //Assert
            _validator.Setup(x => x.ValidateAsync(It.IsAny<CreateAccountCommand>())).ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

            //Act
            Assert.ThrowsAsync<InvalidRequestException>(async () => await _handler.Handle(new CreateAccountCommand()));

            //Assert
            _validator.Verify(x => x.ValidateAsync(It.IsAny<CreateAccountCommand>()), Times.Once);
        }

        [Test]
        public async Task WillCallRepositoryToCreateNewAccount()
        {
            const int accountId = 23;

            var cmd = new CreateAccountCommand
            {
                ExternalUserId = Guid.NewGuid().ToString(),
                OrganisationReferenceNumber = "QWERTY",
                OrganisationName = "Qwerty Corp",
                OrganisationAddress = "Innovation Centre, Coventry, CV1 2TT",
                OrganisationDateOfInception = DateTime.Today.AddDays(-1000),
                Sector = "Sector",
                PayeReference = "120/QWERTY",
                AccessToken = Guid.NewGuid().ToString(),
                RefreshToken = Guid.NewGuid().ToString(),
                OrganisationStatus = "active",
                EmployerRefName = "Paye Scheme 1"
            };
            
            _accountRepository.Setup(x => x.CreateAccount(_user.Id, cmd.OrganisationReferenceNumber, cmd.OrganisationName, cmd.OrganisationAddress, cmd.OrganisationDateOfInception, cmd.PayeReference, cmd.AccessToken, cmd.RefreshToken, cmd.OrganisationStatus, cmd.EmployerRefName, (short)cmd.OrganisationType, cmd.PublicSectorDataSource, cmd.Sector)).ReturnsAsync(new CreateAccountResult { AccountId = accountId, LegalEntityId = 0L, EmployerAgreementId = 0L });

            var expectedHashedAccountId = "DJRR4359";
            _hashingService.Setup(x => x.HashValue(accountId)).Returns(expectedHashedAccountId);

            var expectedPublicHashedAccountId = "SCUFF";
            _externalhashingService.Setup(x => x.HashValue(accountId)).Returns(expectedPublicHashedAccountId);

            await _handler.Handle(cmd);

            _accountRepository.Verify(x => x.CreateAccount(_user.Id, cmd.OrganisationReferenceNumber, cmd.OrganisationName, cmd.OrganisationAddress, cmd.OrganisationDateOfInception, cmd.PayeReference, cmd.AccessToken, cmd.RefreshToken, cmd.OrganisationStatus, cmd.EmployerRefName, (short)cmd.OrganisationType, cmd.PublicSectorDataSource, cmd.Sector));
            _refreshEmployerLevyService.Verify(x=>x.QueueRefreshLevyMessage(accountId,cmd.PayeReference));
            
        }

        [Test]
        public async Task ThenIfTheCommandIsValidTheCreateAuditCommandIsCalledForEachComponent()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand
            {
                PayeReference = "123/abc,456/123",
                AccessToken = "123rd",
                RefreshToken = "45YT",
                OrganisationType = OrganisationType.CompaniesHouse,
                OrganisationName = "OrgName",
                EmployerRefName = "123AB",
                ExternalUserId = "4566",
                OrganisationAddress = "Address",
                OrganisationDateOfInception = new DateTime(2017, 01, 30),
                OrganisationReferenceNumber = "TYG56",
                OrganisationStatus = "Active",
                PublicSectorDataSource = 2,
                Sector = "Sector"
            };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert
            _mediator.Verify(
                x => x.SendAsync(It.Is<CreateAuditCommand>(c =>
                    c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("AccountId") && y.NewValue.Equals(ExpectedAccountId.ToString())) != null &&
                    c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("AccountId") && y.NewValue.Equals(ExpectedAccountId.ToString())) != null
                    )));
        }

        [Test]
        public async Task ThenAnOrganisationCodeIsGeneratedIfOneIsNotSupplied()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { PayeReference = "123/abc,456/123", AccessToken = "123rd", RefreshToken = "45YT", OrganisationStatus = "active" };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert

            _accountRepository.Verify(x => x.CreateAccount(It.IsAny<long>(), It.Is<string>(cd => !string.IsNullOrEmpty(cd)), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<short>(), It.IsAny<short?>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ThenTheMessageIsAddedToTheAddPayeSchemeQueue()
        {
            //Arrange
            var  expectedPayeRef = "123/abc";
           
            var createAccountCommand = new CreateAccountCommand { PayeReference = expectedPayeRef, AccessToken = "123rd", RefreshToken = "45YT", OrganisationStatus = "active", ExternalUserId = _user.UserRef };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert
            _messagePublisher.Verify(x => x.PublishAsync(It.Is<PayeSchemeAddedMessage>(
                c => c.PayeScheme.Equals(expectedPayeRef) &&
                c.AccountId.Equals(ExpectedAccountId) &&
                c.CreatorUserRef.Equals(_user.UserRef)
                )), Times.Once());
        }

        [Test]
        public async Task ThenTheMessageIsAddedToTheAccountCreatedQueue()
        {
            //Arrange
            var createAccountCommand = new CreateAccountCommand { PayeReference = "123EDC", AccessToken = "123rd", RefreshToken = "45YT", OrganisationStatus = "active" };

            //Act
            await _handler.Handle(createAccountCommand);

            //Assert
            _messagePublisher.Verify(x=>x.PublishAsync(It.Is<AccountCreatedMessage>(c=>c.AccountId.Equals(ExpectedAccountId))),Times.Once);
        }
    }
}
