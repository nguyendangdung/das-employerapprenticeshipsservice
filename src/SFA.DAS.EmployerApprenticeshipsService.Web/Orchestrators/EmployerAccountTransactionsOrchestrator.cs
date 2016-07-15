﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MediatR;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetEmployerAccount;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerApprenticeshipsService.Domain;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.Orchestrators
{
    public class EmployerAccountTransactionsOrchestrator
    {
        private readonly IMediator _mediator;

        public EmployerAccountTransactionsOrchestrator(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }

        public async Task<TransactionViewResult>  GetAccountTransactions(int accountId)
        {
            var employerAccountResult = await _mediator.SendAsync(new GetEmployerAccountQuery { Id = accountId });
            if (employerAccountResult == null)
            {
                return new TransactionViewResult();
            }

            var data = await _mediator.SendAsync(new GetEmployerAccountTransactionsQuery {AccountId = accountId});
            
            return new TransactionViewResult
            {
                Account = employerAccountResult.Account,
                Model = new TransactionViewModel
                {
                    Data = data.Data
                }
                
            };
            
        }
    }

    public class TransactionViewResult
    {
        public Account Account { get; set; }
        public TransactionViewModel Model { get; set; }
    }

    public class TransactionViewModel   
    {
        public AggregationData Data { get; set; }
    }
}