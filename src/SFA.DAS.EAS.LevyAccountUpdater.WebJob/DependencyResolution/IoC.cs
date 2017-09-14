// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IoC.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Infrastructure.DependencyResolution;
using SFA.DAS.EAS.Infrastructure.EnvironmentInfo;
using StructureMap;

namespace SFA.DAS.EAS.LevyAccountUpdater.WebJob.DependencyResolution {
    public static class IoC
    {
        private const string ServiceName = "SFA.DAS.EmployerApprenticeshipsService";

        public static IContainer Initialize()
        {
            return new Container(c =>
            {
                c.Policies.Add(new ConfigurationPolicy<EmployerApprenticeshipsServiceConfiguration>(ServiceName, new ConfigurationInfo<EmployerApprenticeshipsServiceConfiguration>()));
                c.Policies.Add(new MessagePolicy<EmployerApprenticeshipsServiceConfiguration>(ServiceName));
                c.AddRegistry<DefaultRegistry>();
            });
        }
    }
}