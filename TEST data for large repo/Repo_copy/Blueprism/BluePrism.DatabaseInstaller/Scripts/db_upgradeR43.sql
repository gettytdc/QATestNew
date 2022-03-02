/*
SCRIPT         : 43
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : addwebservices
CREATION DATE  : 18 Apr 2006
AUTHOR         : PJW
PURPOSE        : Copies existing (pre Version 2.0) processes into the audit log table in order that they appear in the process history feature of system manager.
NOTES          : 
*/


--only proceed if we are upgrading from Version 1.1;
--we do not do anything if version 2.0 is already installed.
IF (SELECT COUNT(eventdatetime) FROM bpaauditevents) = 0
BEGIN

    --declare and initialise our variables
    DECLARE @ProcessID uniqueidentifier
    DECLARE @ProcName varchar(256)
    DECLARE C1 CURSOR FOR SELECT ProcessID, Name FROM BPAProcess
    OPEN C1

    --Loop through each process and create an event in the audit table
    FETCH NEXT FROM C1 INTO @ProcessID, @ProcName
    WHILE (@@FETCH_STATUS = 0)
        BEGIN
            INSERT INTO BPAAuditEvents (eventdatetime, sCode, gSrcUserID, gTgtProcID, sNarrative, newXML, EditSummary)
                        SELECT GETDATE(), 'P008', '00000000-0000-0000-0000-000000000000', @ProcessID, 'The process ''' + @ProcName + ''' was inherited from a version older than Automate 2.0.0',  ProcessXML, 'Process inherited from a version older than Automate 2.0.0' FROM BPAProcess where ProcessID = @ProcessID
    
            FETCH NEXT FROM C1 INTO @ProcessID, @ProcName
        END
        
    --tidy up before ending
    CLOSE C1
    DEALLOCATE C1
    
END --SECTION TO BE COMPLETED ONLY FOR 1.1 UPGRADES
GO



--set DB version
INSERT INTO BPADBVersion VALUES (
  '43',
  GETUTCDATE(),
  'db_upgradeR43.sql UTC',
  'Copying of existing processes into audit log table in order that they appear in the process history feature.'
)
GO

