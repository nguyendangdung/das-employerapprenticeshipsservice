﻿CREATE PROCEDURE [employer_account].[GetAccountStats]
	@accountId BIGINT = 0	
AS
	SELECT
		(SELECT @accountId) AS AccountId,
		(SELECT COUNT(1) FROM [employer_account].[AccountHistory] WHERE AccountId = @accountId) AS PayeSchemeCount,
		(SELECT COUNT(1) FROM [employer_account].[EmployerAgreement] WHERE AccountId = @accountId) AS OrganisationCount,
		(SELECT COUNT(1) FROM [employer_account].[Membership] WHERE AccountId = @accountId) AS TeamMemberCount