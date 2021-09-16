/* THIS IS A DATABASE UPGRADE SCRIPT USED TO ADD A TABLE BPAAuditEvents FOR AUDIT LOGGING AND A NEW VIEW FOR VIEWING THAT DATA CONVENIENTLY.
    IT UPGRADES AN R27 database to R28
*/
CREATE TABLE [BPAAuditEvents] (
    [eventdatetime] [datetime] NOT NULL,
    [eventid] [int] IDENTITY (1, 1) NOT NULL ,
    [sCode] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
    [sNarrative] [varchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
    [gSrcUserID] [uniqueidentifier] NOT NULL ,
    [gTgtUserID] [uniqueidentifier] NULL ,
    [gTgtProcID] [uniqueidentifier] NULL ,
    [gTgtResourceID] [uniqueidentifier] NULL,
    [comments] [varchar] (512) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [oldXML] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [newXML] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)
create clustered index INDEX_BPAAuditEvents_eventdatetime on BPAAuditEvents(eventdatetime);
GO

create view vw_Audit as

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


/* DB Version */
insert into BPADBVersion values ('28',GETUTCDATE(),'db_upgradeR28.sql UTC','Database amendments - new audit log table and new view for log-viewing.')



