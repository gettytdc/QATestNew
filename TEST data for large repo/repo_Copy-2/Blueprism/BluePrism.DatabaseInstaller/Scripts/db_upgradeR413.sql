/*
SCRIPT         : 413
AUTHOR         : Gareth Davidson
PURPOSE        : Delete expired tokens
*/


if not exists(select *
              from sys.objects
              where type = 'p' and object_id = object_id('usp_clearinternalauthtokens'))
begin
    exec(N'create procedure usp_clearinternalauthtokens as begin set nocount on; end')
end
go

alter procedure usp_clearinternalauthtokens
as
    delete from bpainternalauth where expiry < getutcdate();
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
('413',
 getutcdate(), 
 'db_upgradeR413.sql', 
 'Delete expired tokens', 
 0
);
