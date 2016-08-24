﻿using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerApprenticeshipsService.Application.Messages;
using SFA.DAS.EmployerApprenticeshipsService.Application.Validation;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Attributes;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Data;
using SFA.DAS.Messaging;

namespace SFA.DAS.EmployerApprenticeshipsService.Application.Commands.RefreshEmployerLevyData
{
    public class RefreshEmployerLevyDataCommandHandler : AsyncRequestHandler<RefreshEmployerLevyDataCommand>
    {
        [QueueName]
        public string refresh_employer_levy { get; set; }

        private readonly IValidator<RefreshEmployerLevyDataCommand> _validator;
        private readonly IDasLevyRepository _dasLevyRepository;
        private readonly IMessagePublisher _messagePublisher;

        public RefreshEmployerLevyDataCommandHandler(IValidator<RefreshEmployerLevyDataCommand> validator, IDasLevyRepository dasLevyRepository, IMessagePublisher messagePublisher)
        {
            _validator = validator;
            _dasLevyRepository = dasLevyRepository;
            _messagePublisher = messagePublisher;
        }

        protected override async Task HandleCore(RefreshEmployerLevyDataCommand message)
        {
            var result = _validator.Validate(message);

            if (!result.IsValid())
            {
                throw new InvalidRequestException(result.ValidationDictionary);
            }

            bool sendLevyDataChanged = false;
            foreach (var employerLevyData in message.EmployerLevyData)
            {
                foreach (var dasDeclaration in employerLevyData.Declarations.Declarations)
                {
                    var declaration = await _dasLevyRepository.GetEmployerDeclaration(dasDeclaration.Id, employerLevyData.EmpRef);

                    if (declaration == null)
                    {
                        await _dasLevyRepository.CreateEmployerDeclaration(dasDeclaration, employerLevyData.EmpRef, message.AccountId);
                        sendLevyDataChanged = true;
                    }
                }

                foreach (var fraction in employerLevyData.Fractions.Fractions)
                {
                    var dasFraction = await _dasLevyRepository.GetEmployerFraction(fraction.DateCalculated, employerLevyData.EmpRef);

                    if (dasFraction == null)
                    {
                        await _dasLevyRepository.CreateEmployerFraction(fraction, employerLevyData.EmpRef);
                        sendLevyDataChanged = true;
                    }
                }
                
            }

            if (sendLevyDataChanged)
            {
                await _messagePublisher.PublishAsync(new EmployerRefreshLevyQueueMessage { AccountId = message.AccountId });
            }

        }
    }
}
