/*
SCRIPT         : 322
AUTHOR         : DPoole
PURPOSE        : Add WQA snap shot data flags for including data sent to data gateways
*/

-- Add iswqasnapshotdata flag
alter table BPADataPipelineOutputConfig add iswqasnapshotdata bit not null default 0;
GO

-- Add senttodatagateways flag
alter table BPMIQueueSnapshot add senttodatagateways bit not null default 0;
GO

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('322',
       getutcdate(),
       'db_upgradeR322.sql',
       'Add WQA snap shot data flag columns',
       0);