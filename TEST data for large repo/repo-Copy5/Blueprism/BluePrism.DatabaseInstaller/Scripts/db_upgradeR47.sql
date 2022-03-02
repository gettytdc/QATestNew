/*
SCRIPT         : 47
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 10 Sep 2006
AUTHOR         : PJW
PURPOSE        : Add improved view for audit log viewer
NOTES          : 
*/


--Creates the new view
CREATE VIEW vw_Audit_improved AS

    SELECT  TOP 100 PERCENT
        "BPAAuditEvents"."eventdatetime" as [Event Datetime], "BPAAuditEvents"."eventid" as [Event ID],
        "BPAAuditEvents"."sCode" as Code, s."username" as [By User], "BPAAuditEvents"."sNarrative" as Narrative,
        "BPAAuditEvents"."comments" as Comments, t."username" as [Target User], p."name" as [Target Process],
        r."Name" as [Target Resource]
        
    FROM    "BPAAuditEvents" "BPAAuditEvents"

    LEFT OUTER JOIN "BPAProcess" p
        ON "BPAAuditEvents"."gTgtProcID"= p."processid" 
    LEFT OUTER JOIN "BPAUser" s
        ON "BPAAuditEvents"."gSrcUserID"= s."userid"
    LEFT OUTER JOIN "BPAUser" t
        ON "BPAAuditEvents"."gTgtUserID"= t."userid"
    LEFT OUTER JOIN "BPAResource" r
        ON "BPAAuditEvents"."gTgtResourceID" = r."ResourceID"

    ORDER BY eventdatetime
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '47',
  GETUTCDATE(),
  'db_upgradeR47.sql UTC',
  'Add improved view for audit log viewer'
)
GO

