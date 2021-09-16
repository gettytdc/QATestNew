/*
SCRIPT         : 168
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 10 Mar 2014
AUTHOR         : CG
PURPOSE        : Add web service publishing support to BPAProcess table.
NOTES          : 
*/


alter table BPAProcess drop constraint FK_BPAProcess_DefaultRealTimeStatsView
drop table BPAUserViewPreferencePerProcess;
drop table BPARealTimeStatsView;
alter table BPAProcess add 
  wspublishname varchar(255) null;
alter table BPAProcess drop column DefaultRealTimeStatsView;
insert into BPAProcessAttribute (AttributeID, AttributeName) values (4, 'PublishWS')

GO


--set DB version
INSERT INTO BPADBVersion VALUES (
  '168',
  GETUTCDATE(),
  'db_upgradeR168.sql UTC',
  'Add web service publishing support to BPAProcess table'
)


