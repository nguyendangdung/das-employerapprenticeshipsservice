/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
:r .\AddLevyDeclarationCreatedDateDefault.sql

-- ONLY DO THIS FOR DB UPGRADES - CHECK THE SCHEMA EXISTS!
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'employer_financial')
BEGIN
	:r .\RemoveDuplicateLevyDeclarations.sql
END
