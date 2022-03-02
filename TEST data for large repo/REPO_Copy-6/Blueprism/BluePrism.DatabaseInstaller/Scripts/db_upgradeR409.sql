/*
SCRIPT         : 409
AUTHOR         : Gary Crosbie
PURPOSE        : Grant execute permissions to user defined types and procedures
*/

if database_principal_id('bpa_ExecuteSP_System') is not null
begin
    grant exec on type::GroupIdParameterTable TO bpa_ExecuteSP_System
    grant exec on type::ActiveDirectoryUserTableType TO bpa_ExecuteSP_System
    grant exec on type::TargetSessionDetails TO bpa_ExecuteSP_System
    grant execute on object::usp_SetTargetSessionsForMultipleWorkQueues to bpa_ExecuteSP_System
end

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('409',
 getutcdate(), 
 'db_upgradeR409.sql', 
 'Grant execute permissions to user defined types and procedures', 
 0
);
