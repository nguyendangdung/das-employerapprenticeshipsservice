﻿DECLARE @sqlStatement NVARCHAR(4000) = '
	CREATE PROCEDURE #CreateAccount
		@accountId BIGINT,
		@accountHashedId NVARCHAR(100),
		@accountPublicHashedId NVARCHAR(100),
		@userId BIGINT,
		@legalEntityCode NVARCHAR(50), 
		@legalEntityName NVARCHAR(100),
		@legalEntityRegisteredAddress NVARCHAR(256),
		@legalEntityDateOfIncorporation DATETIME,
		@legalEntityStatus NVARCHAR(50),
		@legalEntitySource TINYINT,
		@payeRef NVARCHAR(16),
		@payeName VARCHAR(500)
	AS
	BEGIN
		IF (NOT EXISTS (SELECT 1 FROM [employer_account].[Account] WHERE Id = @accountId))
		BEGIN
			DECLARE @now DATETIME = GETDATE()
			DECLARE @legalEntityId BIGINT
			DECLARE @employerAgreementId BIGINT

			SET IDENTITY_INSERT [employer_account].[Account] ON
			INSERT INTO [employer_account].[Account] (Id, HashedId, PublicHashedId, Name, CreatedDate)
			VALUES (@accountId, @accountHashedId, @accountPublicHashedId, @legalEntityName, GETDATE())
			SET IDENTITY_INSERT [employer_account].[Account] OFF
		
			INSERT INTO [employer_account].[Membership] (AccountId, UserId, RoleId)
			VALUES (@accountId, @userId, 1)

			EXEC [employer_account].[CreateLegalEntity] @legalEntityCode, @legalEntityName, @legalEntityRegisteredAddress, @legalEntityDateOfIncorporation, @legalEntityStatus, @legalEntitySource, NULL, NULL, @legalEntityId OUTPUT
			EXEC [employer_account].[CreateEmployerAgreement] @legalEntityId, @accountId, @employerAgreementId OUTPUT
			EXEC [employer_account].[SignEmployerAgreement] @employerAgreementId, @userId, ''Test User'', @now
			EXEC [employer_account].[CreateAccountEmployerAgreement] @accountId, @employerAgreementId		
			EXEC [employer_account].[CreatePaye] @payeRef, ''accessToken'', ''refreshToken'', @payeName
			EXEC [employer_account].[CreateAccountHistory] @accountId, @payeRef, @now
		END
	END'

EXEC sp_executesql @sqlStatement

IF (NOT EXISTS (SELECT 1 FROM [employer_account].[EmployerAgreementTemplate] WHERE PartialViewName = '_Agreement_V1'))
BEGIN 
	SET IDENTITY_INSERT [employer_account].[EmployerAgreementTemplate] ON
	INSERT INTO [employer_account].[EmployerAgreementTemplate] (Id, PartialViewName, CreatedDate)
	VALUES (1, '_Agreement_V1', GETDATE()) 
	SET IDENTITY_INSERT [employer_account].[EmployerAgreementTemplate] OFF
END

DECLARE @userRef UNIQUEIDENTIFIER = '87df36f4-78ad-47c7-84d7-900ef4c39920'

IF (NOT EXISTS (SELECT 1 FROM [employer_account].[User] WHERE UserRef = @userRef))
BEGIN
	EXECUTE [employer_account].[UpsertUser] @userRef, 'test@account.com', 'Test', 'Account' 
END

DECLARE @userId BIGINT = (SELECT Id FROM [employer_account].[User] WHERE UserRef = @userRef)

EXECUTE #CreateAccount 1, 'JRML7V', 'LDMVWV', @userId, '00445790', 'Tesco Plc', 'Tesco House, Shire Park, Kestrel Way, Welwyn Garden City, AL7 1GA', '1947-11-27 00:00:00.000', 'active', 1, '222/ZZ00002', 'NA'
EXECUTE #CreateAccount 2, '84VBNV', 'BDXBDV', @userId, 'SC171417', 'SAINSBURY''S LIMITED', 'No 2 Lochrin Square, 96 Fountainbridge, Edinburgh, EH3 9QA', '1997-01-16 00:00:00.000', 'active', 1, '123/SFZZ029', 'NA'
EXECUTE #CreateAccount 3, 'JLVKPM', 'XWBVWN', @userId, '07297044', 'DINE CONTRACT CATERING LIMITED', '1st Floor The Centre, Birchwood Park, Warrington, Lancashire, WA3 6YN', '2010-06-28 00:00:00.000', 'active', 1, '101/ZZR00016', 'NA'

DROP PROCEDURE #CreateAccount