/*
SCRIPT         : 139
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Validation updates
*/

IF OBJECT_ID('chk') IS NOT NULL
    DROP PROC chk
GO

CREATE PROC chk @id int,@cat int,@typeid int,@msg nvarchar(255)
AS
    insert into BPAValCheck (checkid,catid,typeid,description)
        values (@id,@cat,@typeid,@msg)
GO

DECLARE @StageValidation int
SET @StageValidation=0
DECLARE @FlowValidation int
SET @FlowValidation=1
DECLARE @DocumentationControl int
SET @DocumentationControl=2

DECLARE @Error int
SET @Error=0
DECLARE @Warning int
SET @Warning=1
DECLARE @Advice int
SET @Advice=2

--New checks...

--Obsolete checks...
DELETE from BPAValCheck WHERE checkid=39
DELETE from BPAValCheck WHERE checkid=40
DELETE from BPAValCheck WHERE checkid=42
DELETE from BPAValCheck WHERE checkid=43

--Fix things...
UPDATE BPAValCheck SET description='Collection mismatch{0}: {1}' WHERE checkid=41
UPDATE BPAValCheck SET description='Value to be stored is not compatible with destination{0}' WHERE checkid=9

--set DB version
INSERT INTO BPADBVersion VALUES (
  '139',
  GETUTCDATE(),
  'db_upgradeR139.sql UTC',
  'Validation updates'
)

