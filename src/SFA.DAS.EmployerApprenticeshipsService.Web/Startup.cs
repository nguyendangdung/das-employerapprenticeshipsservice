﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Configuration.FileStorage;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.OidcMiddleware;

[assembly: OwinStartup(typeof(SFA.DAS.EmployerApprenticeshipsService.Web.Startup))]

namespace SFA.DAS.EmployerApprenticeshipsService.Web
{
    public class Startup
    {
        private const string ServiceName = "SFA.DAS.EmployerApprenticeshipsService";
        public void Configuration(IAppBuilder app)
        {
            var config = GetConfigurationObject();

            if (config.Identity.UseFake)
            {
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies",
                    LoginPath = new PathString("/home/FakeUserSignIn")
                });
            }
            else
            {
                JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies"
                });

                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "TempState",
                    AuthenticationMode = AuthenticationMode.Passive
                });

                var constants = new Constants(config.Identity.BaseAddress);
                app.UseCodeFlowAuthentication(new OidcMiddlewareOptions
                {
                    ClientId = config.Identity.ClientId, 
                    ClientSecret = config.Identity.ClientSecret, 
                    Scopes = "openid",
                    BaseUrl = constants.BaseAddress,
                    TokenEndpoint = constants.TokenEndpoint(),
                    UserInfoEndpoint = constants.UserInfoEndpoint(),
                    AuthorizeEndpoint = constants.AuthorizeEndpoint(),
                    AuthenticatedCallback = identity => { identity.AddClaim(new Claim("CustomClaim", "new claim added")); }
                });
            }


        }

        private static EmployerApprenticeshipsServiceConfiguration GetConfigurationObject()
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(
               configurationRepository,
               new ConfigurationOptions(ServiceName, environment, "1.0"));

            var config = configurationService.Get<EmployerApprenticeshipsServiceConfiguration>();

            return config;
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
                configurationRepository =
                    new AzureTableStorageConfigurationRepository(
                        CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
            }
            return configurationRepository;
        }
    }


    public class Constants
    {

        public Constants(string baseAddress)
        {
            this.BaseAddress = baseAddress;
        }
        public string BaseAddress { get; set; }

        public string AuthorizeEndpoint() => BaseAddress + "/Login/dialog/appl/oidctest/wflow/authorize";
        public string LogoutEndpoint() => BaseAddress + "/connect/endsession";
        public string TokenEndpoint() =>  BaseAddress + "/Login/rest/appl/oidctest/wflow/token";
        public string UserInfoEndpoint() =>  BaseAddress + "/Login/rest/appl/oidctest/wflow/userinfo";
        public string IdentityTokenValidationEndpoint() =>  BaseAddress + "/connect/identitytokenvalidation";
        public string TokenRevocationEndpoint() => BaseAddress + "/connect/revocation";
    }
}
