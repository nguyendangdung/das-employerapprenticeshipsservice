﻿using System;
using System.Configuration;
using System.Linq;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Configuration.FileStorage;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using StructureMap;
using StructureMap.Pipeline;

namespace SFA.DAS.EmployerApprenticeshipsService.Domain.DepedencyResolution
{
    
    public class ConfigurationPolicy<T> : ConfiguredInstancePolicy
    {
        private const string ServiceName = "SFA.DAS.EmployerApprenticeshipsService";
        protected override void apply(Type pluginType, IConfiguredInstance instance)
        {
            
            var serviceConfigurationParamater = instance?.Constructor?.GetParameters().FirstOrDefault(x => x.ParameterType == typeof(T));

            if (serviceConfigurationParamater != null)
            {
                var environment = Environment.GetEnvironmentVariable("DASENV");
                if (string.IsNullOrEmpty(environment))
                {
                    environment = CloudConfigurationManager.GetSetting("EnvironmentName");
                }

                var configurationRepository = GetConfigurationRepository();
                var configurationService = new ConfigurationService(configurationRepository,
                    new ConfigurationOptions(ServiceName, environment, "1.0"));

                var result = configurationService.Get<EmployerApprenticeshipsServiceConfiguration>();
                if (result != null)
                {
                    instance.Dependencies.AddForConstructorParameter(serviceConfigurationParamater, result);
                }
            }
            
        }
        
        private static IConfigurationRepository GetConfigurationRepository()
        {
            IConfigurationRepository configurationRepository;
            if (bool.Parse(ConfigurationManager.AppSettings["LocalConfig"]))
            {
                configurationRepository = new FileStorageConfigurationRepository();
            }
            else
            {
                configurationRepository = new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
            }
            return configurationRepository;
        }
        
    }
}