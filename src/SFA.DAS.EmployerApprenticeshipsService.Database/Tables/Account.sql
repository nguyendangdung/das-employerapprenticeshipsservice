﻿CREATE TABLE [account].[Account]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
	[HashedAccountId] NVARCHAR(100) NULL,
    [Name] NVARCHAR(100) NOT NULL 
)
GO

CREATE INDEX [IX_Account_HashedAccountId] ON [account].[Account] ([HashedAccountId])
