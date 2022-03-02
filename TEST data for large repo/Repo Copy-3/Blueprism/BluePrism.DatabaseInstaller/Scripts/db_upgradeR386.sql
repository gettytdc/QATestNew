/*
SCRIPT        : 385
STORY		  : BP-1461
PURPOSE       : Adding a composite index to more fields on the BPAWorkQueue table so that it is more performant when sorting and filtering
AUTHOR		  : Neal Callaghan / Vinu Thomas
*/

ALTER TABLE [BPAWorkQueue] DROP CONSTRAINT [INDEX_WorkQueueName]
GO
SET ANSI_PADDING ON
GO


CREATE UNIQUE NONCLUSTERED INDEX [INDEX_WorkQueueName] ON [BPAWorkQueue] 
(
[name] ASC
)
INCLUDE
(
id,keyfield,running,maxattempts,encryptid, processid,snapshotconfigurationid,resourcegroupid,requiredFeature
)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'386'
	,getutcdate()
	,'db_upgradeR386.sql'
	,'Adding a composite index to more fields on the BPAWorkQueue table so that it is more performant when sorting and filtering'
	,0
	);
