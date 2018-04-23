-- This script was created as there was already a fixup script that had not fully worked in Production
-- It had failed to set the default value for the LevyDeclaration -> DateCreated column
IF EXISTS(select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'LevyDeclaration')
BEGIN
	IF NOT EXISTS (
		SELECT *
		  FROM sys.all_columns c
		  JOIN sys.tables t ON t.object_id = c.object_id
		  JOIN sys.schemas s ON s.schema_id = t.schema_id
		  JOIN sys.default_constraints d on c.default_object_id = d.object_id
		WHERE t.name = 'LevyDeclaration'
		  AND c.name = 'CreatedDate'
		  AND s.name = 'employer_financial')
	BEGIN
		-- There is not a constraint on this column
		IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LevyDeclaration' AND COLUMN_NAME = 'CreatedDate')
		BEGIN
			ALTER TABLE [employer_financial].[LevyDeclaration] 
			ADD CONSTRAINT LevyDeclaration_CreatedDate_Default DEFAULT GetDate() FOR CreatedDate
		END
	END
END
