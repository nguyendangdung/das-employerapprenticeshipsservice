-- Store the original data 
IF (NOT EXISTS (SELECT *  
					FROM INFORMATION_SCHEMA.TABLES  
					WHERE TABLE_SCHEMA = 'employer_financial'  
					AND  TABLE_NAME = 'LevyDeclarationNonUnique')) 
BEGIN 
	CREATE TABLE [employer_financial].[LevyDeclarationNonUnique]
	(
		[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
		[AccountId] BIGINT NOT NULL DEFAULT 0,
		[empRef] NVARCHAR(50) NOT NULL, 
		[LevyDueYTD] DECIMAL(18, 4) NULL DEFAULT 0, 
		[LevyAllowanceForYear] DECIMAL(18, 4) NULL DEFAULT 0, 
		[SubmissionDate] DATETIME NULL, 
		[SubmissionId] BIGINT NOT NULL DEFAULT 0,
		[PayrollYear] NVARCHAR(10) NULL,
		[PayrollMonth] TINYINT NULL,
		[CreatedDate] DATETIME NOT NULL,
		[EndOfYearAdjustment] BIT NOT NULL DEFAULT 0,
		[EndOfYearAdjustmentAmount] DECIMAL(18,4) NULL,
		[DateCeased] DATETIME NULL,
		[InactiveFrom] DATETIME NULL,
		[InactiveTo] DATETIME NULL,
		[HmrcSubmissionId] BIGINT NULL,
		[NoPaymentForPeriod] BIT DEFAULT 0
	)


	SET IDENTITY_INSERT [employer_financial].[LevyDeclarationNonUnique] ON 

	INSERT [employer_financial].[LevyDeclarationNonUnique] ([Id]
			,[AccountId]
			,[empRef]
			,[LevyDueYTD]
			,[LevyAllowanceForYear]
			,[SubmissionDate]
			,[SubmissionId]
			,[PayrollYear]
			,[PayrollMonth]
			,[CreatedDate]
			,[EndOfYearAdjustment]
			,[EndOfYearAdjustmentAmount]
			,[DateCeased]
			,[InactiveFrom]
			,[InactiveTo]
			,[HmrcSubmissionId]
			,[NoPaymentForPeriod]
	)
	SELECT 	[Id]
			,[AccountId]
			,[empRef]
			,[LevyDueYTD]
			,[LevyAllowanceForYear]
			,[SubmissionDate]
			,[SubmissionId]
			,[PayrollYear]
			,[PayrollMonth]
			,[CreatedDate]
			,[EndOfYearAdjustment]
			,[EndOfYearAdjustmentAmount]
			,[DateCeased]
			,[InactiveFrom]
			,[InactiveTo]
			,[HmrcSubmissionId]
			,[NoPaymentForPeriod]
			FROM [employer_financial].[LevyDeclaration]

	SET IDENTITY_INSERT [employer_financial].[LevyDeclarationNonUnique] OFF 

	-- Clean up the duplicates
	DELETE ld FROM [employer_financial].[LevyDeclaration] ld
	INNER JOIN (

		SELECT ldd.EmpRef, ldd.[SubmissionId], Min(ldd.Id) AS minId 
		FROM [employer_financial].[LevyDeclaration] ldd
		INNER JOIN (
			SELECT [empRef], [SubmissionId] 
			FROM employer_financial.LevyDeclaration 
			GROUP BY [empRef], [SubmissionId] HAVING COUNT(*) > 1
		) d 
		ON d.empRef = ldd.[empRef] AND d.[SubmissionId] = ldd.[SubmissionId]
		GROUP BY ldd.[empRef], ldd.[SubmissionId]

	) dupes
		ON dupes.[empRef] = ld.[empRef]
		AND dupes.[SubmissionId] = ld.[SubmissionId]
		AND dupes.minId != ld.Id
END 


