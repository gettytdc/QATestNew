
if database_principal_id('bpa_ExecuteSP_System') is not null
begin
    grant execute on object::usp_getmappedadusers to bpa_ExecuteSP_System;
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
('417', 
 getutcdate(), 
 'db_upgradeR417.sql', 
 'Grant execute permissions to stored procedure usp_getmappedadusers', 
 0
);
