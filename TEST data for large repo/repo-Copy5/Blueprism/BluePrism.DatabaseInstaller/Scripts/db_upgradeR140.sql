/*
SCRIPT         : 140
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
EXEC chk 129,@DocumentationControl,@Warning,"Action stage has no description"
EXEC chk 130,@DocumentationControl,@Advice,"Application element with dynamic attributes has no notes"
EXEC chk 131,@DocumentationControl,@Advice,"No preconditions are defined"
EXEC chk 132,@DocumentationControl,@Advice,"No postconditions are defined"
EXEC chk 133,@DocumentationControl,@Advice,"No description given for parameter '{0}'"
EXEC chk 134,@StageValidation,@Error,"Referenced subprocess does not exist"

--Obsolete checks...

--Fix things...

--set DB version
INSERT INTO BPADBVersion VALUES (
  '140',
  GETUTCDATE(),
  'db_upgradeR140.sql UTC',
  'Validation updates'
)

