/*
SCRIPT         : 429
AUTHOR         : Kevin Benson-White
PURPOSE        : Add sessionexceptionretry to BPAWorkQueue
*/

if exists (select 1 from information_schema.columns where table_name = 'BPAWorkQueue')
   and
   not exists (select 1 from information_schema.columns where table_name = 'BPAWorkQueue' and column_name = 'sessionexceptionretry')
begin
    alter table [BPAWorkQueue]
    add [sessionexceptionretry] [bit] null default(0);
end
go



-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('429', 
 GETUTCDATE(), 
 'db_upgradeR429.sql', 
 'Add sessionexceptionretry column to BPAWorkQueue', 
 0
);
