/*
SCRIPT         : 138
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
EXEC chk 128,@StageValidation,@Error,"Expression{0} uses Exception function but is not in a Recovery section"

--Obsolete checks...
DELETE from BPAValCheck WHERE checkid = 0
DELETE from BPAValCheck WHERE checkid = 1
DELETE from BPAValCheck WHERE checkid = 2
DELETE from BPAValCheck WHERE checkid = 14
DELETE from BPAValCheck WHERE checkid = 32
DELETE from BPAValCheck WHERE checkid = 34
DELETE from BPAValCheck WHERE checkid = 35
DELETE from BPAValCheck WHERE checkid = 36
DELETE from BPAValCheck WHERE checkid = 37
DELETE from BPAValCheck WHERE checkid = 44
DELETE from BPAValCheck WHERE checkid = 46
DELETE from BPAValCheck WHERE checkid = 47
DELETE from BPAValCheck WHERE checkid = 48
DELETE from BPAValCheck WHERE checkid = 49
DELETE from BPAValCheck WHERE checkid = 50
DELETE from BPAValCheck WHERE checkid = 59
DELETE from BPAValCheck WHERE checkid = 71
DELETE from BPAValCheck WHERE checkid = 73
DELETE from BPAValCheck WHERE checkid = 74
DELETE from BPAValCheck WHERE checkid = 75
DELETE from BPAValCheck WHERE checkid = 76
DELETE from BPAValCheck WHERE checkid = 87
DELETE from BPAValCheck WHERE checkid = 88
DELETE from BPAValCheck WHERE checkid = 89
DELETE from BPAValCheck WHERE checkid = 93

--Fix things...
UPDATE BPAValCheck SET typeid=@Warning WHERE checkid = 43

--set DB version
INSERT INTO BPADBVersion VALUES (
  '138',
  GETUTCDATE(),
  'db_upgradeR138.sql UTC',
  'Validation updates'
)

