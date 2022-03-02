/*
SCRIPT         : 40
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : correctcollation
CREATION DATE  : Feb 2006
AUTHOR         : Denis Dennehy
PURPOSE        : Corrects the collation in multiple columns to be the database default
NOTES          : The collation for new rows must not be set in any future scripts
*/


--modify columns
Alter table [BPAStatus] Alter Column [type] [varchar] (10) COLLATE DATABASE_DEFAULT NOT NULL    
GO

Alter table [BPAStatus] Alter Column [description] [varchar] (20) COLLATE DATABASE_DEFAULT NULL 
GO

Alter table [BPASysConfig] Alter Column [maxnumconcproc] [varchar] (100) COLLATE DATABASE_DEFAULT NULL  
GO

Alter table [BPAUser] Alter Column [username] [varchar] (20) COLLATE DATABASE_DEFAULT NULL  
GO

Alter table [BPAUser] Alter Column [password] [varchar] (100) COLLATE DATABASE_DEFAULT NULL 
GO

Alter table [BPAUser] Alter Column [useremail] [varchar] (60) COLLATE DATABASE_DEFAULT NULL 
GO

IF object_id('PK_BPAStatistics') IS NULL
   BEGIN
     Alter table [BPAStatistics] Alter Column [name] [varchar] (50) COLLATE DATABASE_DEFAULT NULL;
     Alter table [BPAStatistics] Alter Column [datatype] [varchar] (32) COLLATE DATABASE_DEFAULT NULL;
     Alter table [BPAStatistics] Alter Column [value_text] [varchar] (255) COLLATE DATABASE_DEFAULT NULL;
   END
GO

Alter table [BPAProcessBackup] Alter Column [name] [varchar] (128) COLLATE DATABASE_DEFAULT NULL    
GO

Alter table [BPAProcessBackup] Alter Column [description] [varchar] (1000) COLLATE DATABASE_DEFAULT NULL    
GO

Alter table [BPAProcessBackup] Alter Column [version] [varchar] (20) COLLATE DATABASE_DEFAULT NULL  
GO


declare @InError bit
set     @InError =0
begin transaction 
-- add a temp column
exec ('Alter table [BPAProcessBackup] add [____temp] [text]')

-- copy data to temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAProcessBackup] set [____temp] =[processxml]')

-- drop origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAProcessBackup] drop column [processxml]')

-- create origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAProcessBackup] add [processxml] [text] ')

-- Copy data back to origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAProcessBackup] set [processxml] = [____temp] ')

-- drop temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('alter table [BPAProcessBackup] drop column [____temp]')

if @@error<>0 set @InError =1
if @@error = 0
    commit transaction
else
    rollback transaction


GO

Alter table [BPAAuditEvents] Alter Column [sCode] [varchar] (10) COLLATE DATABASE_DEFAULT NOT NULL  
GO

Alter table [BPAAuditEvents] Alter Column [sNarrative] [varchar] (500) COLLATE DATABASE_DEFAULT NOT NULL    
GO

Alter table [BPAAuditEvents] Alter Column [comments] [varchar] (512) COLLATE DATABASE_DEFAULT NULL  
GO


declare @InError bit
set     @InError =0
begin transaction 
-- add a temp column
exec ('Alter table [BPAAuditEvents] add [____temp] [text]')

-- copy data to temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAAuditEvents] set [____temp] =[oldXML]')


-- drop origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAAuditEvents] drop column [oldXML]')

-- create origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAAuditEvents] add [oldXML] [text] ')

-- Copy data back to origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAAuditEvents] set [oldXML] = [____temp] ')

-- drop temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('alter table [BPAAuditEvents] drop column [____temp]')

if @@error<>0 set @InError =1
if @@error = 0
    commit transaction
else
    rollback transaction


GO


declare @InError bit
set     @InError =0
begin transaction 
-- add a temp column
exec ('Alter table [BPAAuditEvents] add [____temp] [text]')

-- copy data to temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAAuditEvents] set [____temp] =[newXML]')

-- drop origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAAuditEvents] drop column [newXML]')

-- create origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAAuditEvents] add [newXML] [text] ')

-- Copy data back to origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAAuditEvents] set [newXML] = [____temp] ')

-- drop temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('alter table [BPAAuditEvents] drop column [____temp]')

if @@error<>0 set @InError =1
if @@error = 0
    commit transaction
else
    rollback transaction


GO



Alter table [BPARole] Alter Column [RoleName] [varchar] (100) COLLATE DATABASE_DEFAULT NOT NULL 
GO

Alter table [BPAPermission] Alter Column [Name] [varchar] (100) COLLATE DATABASE_DEFAULT NOT NULL   
GO

Alter table [BPADBVersion] Alter Column [dbversion] [varchar] (50) COLLATE DATABASE_DEFAULT NOT NULL    
GO

Alter table [BPADBVersion] Alter Column [scriptname] [varchar] (50) COLLATE DATABASE_DEFAULT NULL   
GO

Alter table [BPADBVersion] Alter Column [description] [varchar] (200) COLLATE DATABASE_DEFAULT NULL 
GO

Alter table [BPAProcess] Alter Column [name] [varchar] (128) COLLATE DATABASE_DEFAULT NULL  
GO

Alter table [BPAProcess] Alter Column [description] [varchar] (1000) COLLATE DATABASE_DEFAULT NULL  
GO

Alter table [BPAProcess] Alter Column [version] [varchar] (20) COLLATE DATABASE_DEFAULT NULL    
GO


declare @InError bit
set     @InError =0
begin transaction 
-- add a temp column
exec ('Alter table [BPAProcess] add [____temp] [text]')

-- copy data to temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAProcess] set [____temp] =[processxml]')

-- drop origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAProcess] drop column [processxml]')

-- create origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAProcess] add [processxml] [text] ')

-- Copy data back to origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAProcess] set [processxml] = [____temp] ')

-- drop temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('alter table [BPAProcess] drop column [____temp]')

if @@error<>0 set @InError =1
if @@error = 0
    commit transaction
else
    rollback transaction


GO

Alter table [BPARecent] Alter Column [name] [varchar] (128) COLLATE DATABASE_DEFAULT NULL   
GO

Alter table [BPAReport] Alter Column [name] [varchar] (128) COLLATE DATABASE_DEFAULT NULL   
GO

Alter table [BPAReport] Alter Column [description] [varchar] (1000) COLLATE DATABASE_DEFAULT NULL   
GO

Alter table [BPAResource] Alter Column [name] [varchar] (128) COLLATE DATABASE_DEFAULT NULL 
GO

Alter table [BPAResource] Alter Column [status] [varchar] (10) COLLATE DATABASE_DEFAULT NULL    
GO

Alter table [BPAResource] Alter Column [availability] [varchar] (16) COLLATE DATABASE_DEFAULT NULL  
GO

Alter table [BPAResourceUnit] Alter Column [name] [varchar] (50) COLLATE DATABASE_DEFAULT NULL  
GO


declare @InError bit
set     @InError =0
begin transaction 
-- add a temp column
exec ('Alter table [BPAResourceUnit] add [____temp] [text]')

-- copy data to temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAResourceUnit] set [____temp] =[capabilities]')

-- drop origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAResourceUnit] drop column [capabilities]')

-- create origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPAResourceUnit] add [capabilities] [text] ')

-- Copy data back to origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPAResourceUnit] set [capabilities] = [____temp] ')

-- drop temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('alter table [BPAResourceUnit] drop column [____temp]')

if @@error<>0 set @InError =1
if @@error = 0
    commit transaction
else
    rollback transaction


GO

Alter table [BPAScenario] Alter Column [scenariotext] [varchar] (1000) COLLATE DATABASE_DEFAULT NULL    
GO

Alter table [BPAScenario] Alter Column [scenarionotes] [varchar] (1000) COLLATE DATABASE_DEFAULT NULL   
GO

Alter table [BPAScenarioLink] Alter Column [scenarioname] [varchar] (50) COLLATE DATABASE_DEFAULT NULL  
GO

Alter table [BPASession] Alter Column [runningosusername] [varchar] (50) COLLATE DATABASE_DEFAULT NULL  
GO


declare @InError bit
set     @InError =0
begin transaction 
-- add a temp column
exec ('Alter table [BPASession] add [____temp] [text]')

-- copy data to temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPASession] set [____temp] =[sessionstatexml]')

-- drop origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPASession] drop column [sessionstatexml]')

-- create origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('Alter table [BPASession] add [sessionstatexml] [text] ')

-- Copy data back to origional column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('update [BPASession] set [sessionstatexml] = [____temp] ')

-- drop temp column
if @@error<>0 set @InError =1
if @@error = 0
    exec ('alter table [BPASession] drop column [____temp]')

if @@error<>0 set @InError =1
if @@error = 0
    commit transaction
else
    rollback transaction


GO

Alter table [BPASessionLog] Alter Column [message] [varchar] (2000) COLLATE DATABASE_DEFAULT NULL   
GO

--drop and recreate all views with an incorrect collation column
DROP VIEW vw_Audit
GO

CREATE VIEW vw_Audit AS

    SELECT  TOP 100 PERCENT
        "BPAAuditEvents"."eventdatetime" as eventdatetime, "BPAAuditEvents"."eventid",
        "BPAAuditEvents"."sCode", s."username" as [source user], "BPAAuditEvents"."sNarrative",
        "BPAAuditEvents"."comments", t."username" as [target user], "BPAProcess"."name",
        r."Name" as [target resource]
        
    FROM    "BPAAuditEvents" "BPAAuditEvents"

    LEFT OUTER JOIN "BPAProcess" "BPAProcess"
        ON "BPAAuditEvents"."gTgtProcID"="BPAProcess"."processid"   
    LEFT OUTER JOIN "BPAUser" s
        ON "BPAAuditEvents"."gSrcUserID"= s."userid"
    LEFT OUTER JOIN "BPAUser" t
        ON "BPAAuditEvents"."gTgtUserID"= t."userid"
    LEFT OUTER JOIN "BPAResource" r
        ON "BPAAuditEvents"."gTgtResourceID" = r."ResourceID"

    ORDER BY eventdatetime
GO

DROP VIEW vwBPAUptime
GO

CREATE VIEW vwBPAUptime AS

SELECT DISTINCT 
    c.[year],
    c.[quarter],
    c.[month],
    c.[week],
    c.[day],
    c.[date],
    h.[hour],
    m.[minute],
    DATEADD(MINUTE, 60 * h.[hour] + m.[minute], c.[date]) AS [time], 
    CASE ISNULL(CAST(SessionId AS VARCHAR(36)), '') 
        WHEN '' THEN 0 
        ELSE 10  -- SEGMENT
    END AS uptime,
    CASE ISNULL(CAST(SessionId AS VARCHAR(36)), '') 
        WHEN '' THEN 10 -- SEGMENT
        ELSE 0 
    END AS downtime,
    r.[name] AS resource,
    DATEADD(HOUR, 8, c.[date]) AS [start],
    DATEADD(HOUR, 18, c.[date]) AS [end],
    10 AS segment

FROM vwBPACalendar c 
CROSS JOIN BPAClock h
CROSS JOIN BPAClock m
CROSS JOIN BPAResource r
LEFT OUTER JOIN BPASession s ON (
    s.StartDateTime <= (DATEADD(MINUTE, 60 * h.[hour] + m.[minute] + 10, c.[date])) -- SEGMENT
    AND s.EndDateTime >= (DATEADD(MINUTE, 60 * h.[hour] + m.[minute], c.[date]))
    AND r.ResourceId = s.StarterResourceId
)

WHERE h.[hour] >= 8 AND h.[hour] < 18 -- WORKING HOURS
AND m.[minute] % 10 = 0  -- SEGMENT
AND DATENAME(WEEKDAY, c.[date]) NOT IN ('SATURDAY', 'SUNDAY') -- WORKING DAYS

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '40',
  GETUTCDATE(),
  'db_upgradeR40.sql UTC',
  'Database amendments - Correct collation on columns.'
)
GO
