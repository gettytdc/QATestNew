/*
SCRIPT         : 400
AUTHOR         : Rowland Hill
PURPOSE        : Add column  description  to table BPAWebApiCustomOutputParameter
*/


if exists (select * from information_schema.columns where table_name = 'BPAWebApiCustomOutputParameter')
   and
   not exists (select * from information_schema.columns where table_name = 'BPAWebApiCustomOutputParameter' and column_name = 'description')
begin
    alter table BPAWebApiCustomOutputParameter 
    add description nvarchar (max) not null default ''
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
('400', 
 GETUTCDATE(), 
 'db_upgradeR400.sql', 
 'Add column  description  to table BPAWebApiCustomOutputParameter', 
 0
);