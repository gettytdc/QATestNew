/*
SCRIPT         : 173
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 13 Oct 2015
AUTHOR         : GM
PURPOSE        : Adds clustered indexes to BPACredentialsProcesses, BPACredentialsResources and BPAProcessMITemplate
*/

if object_id('UNQ_BPACredentialsProcesses') is null
      alter table BPACredentialsProcesses
            add constraint UNQ_BPACredentialsProcesses
            unique clustered(credentialid, processid);

if object_id('UNQ_BPACredentialsResources') is null
      alter table BPACredentialsResources
            add constraint UNQ_BPACredentialsResources
            unique clustered(credentialid, resourceid);

if object_id('PK_BPAProcessMITemplate') is null
      alter table BPAProcessMITemplate
            add constraint PK_BPAProcessMITemplate
            primary key(templatename, processid);

--set DB version
INSERT INTO BPADBVersion VALUES (
  '173',
  GETUTCDATE(),
  'db_upgradeR173.sql UTC',
  'Adds clustered indexes to BPACredentialsProcesses, BPACredentialsResources and BPAProcessMITemplate'
);
