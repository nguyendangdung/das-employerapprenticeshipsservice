﻿CREATE PROCEDURE [employer_financial].[ProcessPaymentDataTransactions]
	
AS

--- Process Levy Payments ---
INSERT INTO [employer_financial].TransactionLine
select mainUpdate.* from
	(
	select 
			x.AccountId as AccountId,
			DATEFROMPARTS(DatePart(yyyy,GETDATE()),DatePart(MM,GETDATE()),DATEPART(dd,GETDATE())) as DateAdded,
			null as submissionId,
			Max(pe.CompletionDateTime) as TransactionDate,
			3 as TransactionType,
			null as LevyDeclared,
			Sum(x.Amount) * -1 Amount,
			null as empref,
			PeriodEnd,
			UkPrn
		FROM 
			[employer_financial].[Payment] x
		inner join
			[employer_financial].[PeriodEnd] pe on pe.PeriodEndId = x.PeriodEnd
		where
			fundingsource = 1
		Group by
			UkPrn,PeriodEnd,accountId
	) mainUpdate
	inner join (
		select accountid,ukprn,periodend from [employer_financial].Payment where FundingSource = 1
	EXCEPT
		select accountid,ukprn,periodend from [employer_financial].transactionline where TransactionType = 3
	) dervx on dervx.accountId = mainUpdate.accountId and dervx.PeriodEnd = mainUpdate.PeriodEnd and dervx.Ukprn = mainUpdate.ukprn

--- Process SFA Co-Funded Payments ---
INSERT INTO [employer_financial].TransactionLine
select mainUpdate.* from
	(
	select 
			x.AccountId as AccountId,
			DATEFROMPARTS(DatePart(yyyy,GETDATE()),DatePart(MM,GETDATE()),DATEPART(dd,GETDATE())) as DateAdded,
			null as submissionId,
			Max(pe.CompletionDateTime) as TransactionDate,
			4 as TransactionType,
			null as LevyDeclared,
			Sum(x.Amount) * -1 Amount,
			null as empref,
			PeriodEnd,
			UkPrn
		FROM 
			[employer_financial].[Payment] x
		inner join
			[employer_financial].[PeriodEnd] pe on pe.PeriodEndId = x.PeriodEnd
		where
			fundingsource = 2
		Group by
			UkPrn,PeriodEnd,accountId
	) mainUpdate
	inner join (
		select accountid,ukprn,periodend from [employer_financial].Payment where FundingSource = 2
	EXCEPT
		select accountid,ukprn,periodend from [employer_financial].transactionline where TransactionType = 4
	) dervx on dervx.accountId = mainUpdate.accountId and dervx.PeriodEnd = mainUpdate.PeriodEnd and dervx.Ukprn = mainUpdate.ukprn


--- Porcess Employer Co-Funded Payments ---
INSERT INTO [employer_financial].TransactionLine
select mainUpdate.* from
	(
	select 
			x.AccountId as AccountId,
			DATEFROMPARTS(DatePart(yyyy,GETDATE()),DatePart(MM,GETDATE()),DATEPART(dd,GETDATE())) as DateAdded,
			null as submissionId,
			Max(pe.CompletionDateTime) as TransactionDate,
			5 as TransactionType,
			null as LevyDeclared,
			Sum(x.Amount) * -1 Amount,
			null as empref,
			PeriodEnd,
			UkPrn
		FROM 
			[employer_financial].[Payment] x
		inner join
			[employer_financial].[PeriodEnd] pe on pe.PeriodEndId = x.PeriodEnd
		where
			fundingsource = 3
		Group by
			UkPrn,PeriodEnd,accountId
	) mainUpdate
	inner join (
		select accountid,ukprn,periodend from [employer_financial].Payment where FundingSource = 3
	EXCEPT
		select accountid,ukprn,periodend from [employer_financial].transactionline where TransactionType = 5
	) dervx on dervx.accountId = mainUpdate.accountId and dervx.PeriodEnd = mainUpdate.PeriodEnd and dervx.Ukprn = mainUpdate.ukprn