/*
SCRIPT         : 397
PURPOSE        : Add Exception details to BPASession
*/

ALTER TABLE [BPASession]
ADD [terminationreason] tinyint,
	[exceptiontype] nvarchar(50),
	[exceptionmessage] NVARCHAR(MAX)

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('397', 
 GETUTCDATE(), 
 'db_upgradeR397.sql', 
 'Add Exception details to BPASession', 
 0
);
