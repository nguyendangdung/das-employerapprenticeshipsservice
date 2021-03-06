﻿using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.EAS.Account.API.IntegrationTests.TestUtils.DataHelper.Dtos;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;

namespace SFA.DAS.EAS.Account.API.IntegrationTests.TestUtils.DataHelper
{
    class DbDirectRepository : BaseRepository
    {
        public DbDirectRepository(EmployerApprenticeshipsServiceConfiguration configuration, ILog logger)
            : base(configuration.DatabaseConnectionString, logger)
        {
            // Just call base
        }

        public Task<string> GetHashIdForAccount(string accountName)
        {
            return GetValueAsync<string>($"SELECT HashedId FROM [employer_account].Account WHERE Name = '{accountName}'");
        }

        public Task<EmployerAccountOutput> GetAccountDetailsAsync(string accountName)
        {
            return GetValueFromRowAsync($"SELECT Id, HashedId, PublicHashedId FROM [employer_account].Account WHERE Name = '{accountName}'",
                reader => new EmployerAccountOutput
                {
                    AccountId = reader.GetInt64(0),
                    HashedAccountId = reader.IsDBNull(1) ? null : reader.GetString(1),
                    PublicHashedAccountId = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
        }

        private async Task<TValueType> GetValueAsync<TValueType>(string sql)
        {
            return await WithConnection(async conn =>
            {
                var value = await conn.ExecuteScalarAsync<TValueType>(sql, commandType: CommandType.Text);
                return value;
            });
        }

        private async Task<TValueType> GetValueFromRowAsync<TValueType>(string sql, Func<IDataReader, TValueType> rowProcessor)
        {
            return await WithConnection(async conn =>
            {
                var reader = await conn.ExecuteReaderAsync(sql, commandType: CommandType.Text);
                if (reader.Read())
                {
                    return rowProcessor(reader);
                }

                return default(TValueType);
            });
        }
    }
}