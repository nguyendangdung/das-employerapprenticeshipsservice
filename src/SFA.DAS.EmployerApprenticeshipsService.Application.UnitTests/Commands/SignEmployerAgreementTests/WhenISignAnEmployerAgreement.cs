﻿using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerApprenticeshipsService.Application.Commands.SignEmployerAgreement;
using SFA.DAS.EmployerApprenticeshipsService.Domain;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Data;
using SFA.DAS.TimeProvider;

namespace SFA.DAS.EmployerApprenticeshipsService.Application.UnitTests.Commands.SignEmployerAgreementTests
{
    [TestFixture]
    public class WhenISignAnEmployerAgreement
    {
        private Mock<IMembershipRepository> _membershipRepository;
        private Mock<IEmployerAgreementRepository> _agreementRepository;
        private SignEmployerAgreementCommandHandler _handler;
        private SignEmployerAgreementCommand _command;
        private MembershipView _owner;

        [SetUp]
        public void Setup()
        {
            _membershipRepository = new Mock<IMembershipRepository>();
            _agreementRepository = new Mock<IEmployerAgreementRepository>();
            _handler = new SignEmployerAgreementCommandHandler(_membershipRepository.Object, _agreementRepository.Object);

            _command = new SignEmployerAgreementCommand
            {
                AccountId = 1,
                AgreementId = 2,
                ExternalUserId = Guid.NewGuid().ToString()
            };

            _owner = new MembershipView
            {
                UserId = 1,
                RoleId = (short) Role.Owner,
                FirstName = "Fred",
                LastName = "Bloggs"
            };

            _membershipRepository.Setup(x => x.GetCaller(_command.AccountId, _command.ExternalUserId))
                .ReturnsAsync(_owner);
        }

        [Test]
        public void ThenAnInvalidCommandThrowsExcption()
        {
            _command = new SignEmployerAgreementCommand();

            var exception = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(_command));

            Assert.That(exception.ErrorMessages.Count, Is.EqualTo(3));
        }

        [Test]
        public void ThenNonMemberOfAccountThrowsExcption()
        {
            _membershipRepository.Setup(x => x.GetCaller(_command.AccountId, _command.ExternalUserId))
                .ReturnsAsync(null);

            var exception = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(_command));

            Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void ThenNonOwnerOfAccountThrowsExcption()
        {
            _membershipRepository.Setup(x => x.GetCaller(_command.AccountId, _command.ExternalUserId))
                .ReturnsAsync(new MembershipView
                {
                    RoleId = (short)Role.Viewer
                });

            var exception = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(_command));

            Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void ThenAgreementNotFoundThrowsExcption()
        {
            _agreementRepository.Setup(x => x.GetEmployerAgreement(_command.AgreementId))
                .ReturnsAsync(null);

            var exception = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(_command));

            Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void ThenAgreementIsAlreadySignedThrowsExcption()
        {
            _agreementRepository.Setup(x => x.GetEmployerAgreement(_command.AgreementId))
                .ReturnsAsync(new EmployerAgreementView
                {
                    Status = EmployerAgreementStatus.Signed
                });

            var exception = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(_command));

            Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void ThenAgreementHasExpiredThrowsExcption()
        {
            _agreementRepository.Setup(x => x.GetEmployerAgreement(_command.AgreementId))
                .ReturnsAsync(new EmployerAgreementView
                {
                    Status = EmployerAgreementStatus.Pending,
                    ExpiredDate = DateTimeProvider.Current.UtcNow.AddDays(-1)
                });

            var exception = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(_command));

            Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task ThenAgreementIsSigned()
        {
            _agreementRepository.Setup(x => x.GetEmployerAgreement(_command.AgreementId))
                .ReturnsAsync(new EmployerAgreementView
                {
                    Status = EmployerAgreementStatus.Pending
                });

            await _handler.Handle(_command);

            _agreementRepository.Verify(x => x.SignAgreement(_command.AgreementId, _owner.UserId, $"{_owner.FirstName} {_owner.LastName}"), Times.Once);
        }
    }
}