﻿using System;
using System.Linq;
using System.Web;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using SFA.DAS.EAS.Application.Extensions;
using SFA.DAS.EAS.Domain.Models.Authorization;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.Helpers;
using SFA.DAS.HashingService;
using Z.EntityFramework.Plus;

namespace SFA.DAS.EAS.Web.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private static readonly string Key = typeof(AuthorizationContext).FullName;

        private readonly EmployerAccountDbContext _db;
        private readonly HttpContextBase _httpContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IHashingService _hashingService;

        public AuthorizationService(EmployerAccountDbContext db, HttpContextBase httpContext, IAuthenticationService authenticationService, IConfigurationProvider configurationProvider, IHashingService hashingService)
        {
            _db = db;
            _httpContext = httpContext;
            _authenticationService = authenticationService;
            _configurationProvider = configurationProvider;
            _hashingService = hashingService;
        }

        public IAuthorizationContext GetAuthorizationContext()
        {
            if (_httpContext.Items.Contains(Key))
            {
                return _httpContext.Items[Key] as AuthorizationContext;
            }

            var accountId = GetAccountId();
            var userExternalId = GetUserExternalId();

            var accountContextQuery = accountId == null ? null : _db.Accounts
                .Where(a => a.Id == accountId.Value)
                .ProjectTo<AccountContext>(_configurationProvider)
                .Future();

            var userContextQuery = userExternalId == null ? null : _db.Users
                .Where(u => u.ExternalId == userExternalId)
                .ProjectTo<UserContext>(_configurationProvider)
                .Future();

            var membershipContextQuery = accountId == null || userExternalId == null ? null : _db.Memberships
                .Where(m => m.Account.Id == accountId && m.User.ExternalId == userExternalId)
                .ProjectTo<MembershipContext>(_configurationProvider)
                .Future();

            var accountContext = accountContextQuery?.SingleOrDefault();
            var userContext = userContextQuery?.SingleOrDefault();
            var membershipContext = membershipContextQuery?.SingleOrDefault();

            var authorizationContext = new AuthorizationContext
            {
                AccountContext = accountContext,
                UserContext = userContext,
                MembershipContext = membershipContext
            };

            _httpContext.Items[Key] = authorizationContext;

            return authorizationContext;
        }

        public void ValidateMembership()
        {
            var authorizationContext = GetAuthorizationContext();

            if (authorizationContext?.MembershipContext == null)
            {
                throw new UnauthorizedAccessException();
            }
        }

        private long? GetAccountId()
        {
            if (!_httpContext.Request.RequestContext.RouteData.Values.TryGetValue(ControllerConstants.AccountHashedIdRouteKeyName, out var accountHashedId))
            {
                return null;
            }

            if (!_hashingService.TryDecodeValue(accountHashedId.ToString(), out var accountId))
            {
                throw new UnauthorizedAccessException();
            }

            return accountId;
        }

        private Guid? GetUserExternalId()
        {
            if (!_authenticationService.IsUserAuthenticated())
            {
                return null;
            }

            if (!_authenticationService.TryGetClaimValue(ControllerConstants.UserExternalIdClaimKeyName, out var userExternalIdClaimValue))
            {
                throw new UnauthorizedAccessException();
            }

            if (!Guid.TryParse(userExternalIdClaimValue, out var userExternalId))
            {
                throw new UnauthorizedAccessException();
            }

            return userExternalId;
        }
    }
}