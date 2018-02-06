﻿using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;
using System.Threading.Tasks;

namespace SFA.DAS.EAS.Infrastructure.Data
{
    public class TransferRepository : BaseRepository, ITransferRepository
    {
        public TransferRepository(EmployerApprenticeshipsServiceConfiguration configuration, ILog logger)
            : base(configuration.DatabaseConnectionString, logger)
        {
        }

        public Task<decimal> GetTransferBalance(string hashedAccountId)
        {
            throw new System.NotImplementedException();
        }
    }
}
