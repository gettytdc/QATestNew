-- Upgrade script.
insert into BPAValCheck([checkid], 
                        [catid], 
                        [typeid], 
                        [description], 
                        [enabled])
values(147, 
       1, 
       0,
       'The data type ({1}) for data stage ''{0}'' does not match its associated environment variable data type ({2}).',
       1);

-- Set DB version.
insert into BPADBVersion([dbversion], 
                         [scriptrundate], 
                         [scriptname], 
                         [description], 
                         [timezoneoffset])
values('368',
       getutcdate(),
       'db_upgradeR368.sql',
       'Add validation message for clsDataStage to handle miss-match between stage data type and env variable data type.',
       0);