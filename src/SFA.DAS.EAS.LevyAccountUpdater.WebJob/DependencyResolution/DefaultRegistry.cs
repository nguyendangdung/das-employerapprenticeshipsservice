﻿using SFA.DAS.EAS.LevyAccountUpdater.WebJob.Updater;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Interfaces;
using StructureMap;
using StructureMap.Graph;

namespace SFA.DAS.EAS.LevyAccountUpdater.WebJob.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                });

            For<IConfiguration>().Use<EmployerApprenticeshipsServiceConfiguration>();
            For<IAccountUpdater>().Use<AccountUpdater>();
        }
    }
}
