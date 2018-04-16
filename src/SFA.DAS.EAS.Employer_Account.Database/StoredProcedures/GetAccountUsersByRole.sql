CREATE PROCEDURE [employer_account].[GetAccountUsersByRole]
	@accountId BIGINT,
	@roleId INT
AS
	SELECT * FROM [employer_account].[MembershipView] m 
	INNER JOIN [employer_account].[Account] a ON a.id=m.AccountId 
	WHERE a.Id = @accountId AND m.RoleId = @roleId
RETURN 0