﻿using Dapper;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Models.Transfers;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EAS.Infrastructure.Data
{
    public class TransferRepository : BaseRepository, ITransferRepository
    {
        private readonly ILog _logger;
        private readonly float _allowancePercentage;

        public TransferRepository(LevyDeclarationProviderConfiguration configuration, ILog logger)
            : base(configuration.DatabaseConnectionString, logger)
        {
            _logger = logger;
            _allowancePercentage = configuration.TransferAllowancePercentage;
        }

        public async Task<decimal> GetTransferAllowance(long accountId)
        {
            var result = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@accountId", accountId, DbType.Int64);
                parameters.Add("@allowancePercentage", _allowancePercentage, DbType.Single);

                return await c.QuerySingleOrDefaultAsync<decimal?>(
                    sql: "[employer_financial].[GetAccountTransferAllowance]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });

            return result ?? 0;
        }

        public async Task CreateAccountTransfers(IEnumerable<AccountTransfer> transfers)
        {
            await WithTransaction(async (connection, transaction) =>
            {
                var accountTransfers = transfers as AccountTransfer[] ?? transfers.ToArray();

                try
                {
                    foreach (var transfer in accountTransfers)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@senderAccountId", transfer.SenderAccountId, DbType.Int64);
                        parameters.Add("@recieverAccountId", transfer.RecieverAccountId, DbType.Int64);
                        parameters.Add("@commitmentId", transfer.CommitmentId, DbType.Int64);
                        parameters.Add("@amount", transfer.Amount, DbType.Decimal);
                        parameters.Add("@type", transfer.Type, DbType.Int16);
                        parameters.Add("@transferDate", transfer.TransferDate, DbType.DateTime);

                        await connection.ExecuteAsync(
                            sql: "[employer_financial].[CreateAccountTransfer]",
                            param: parameters,
                            commandType: CommandType.StoredProcedure);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var transfer = accountTransfers.FirstOrDefault();

                    _logger.Error(ex, $"Failed to save transfers for account id {transfer?.SenderAccountId}");

                    throw;
                }
            });
        }
    }
}