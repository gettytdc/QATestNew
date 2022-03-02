/*
SCRIPT         : 359
PURPOSE        : Add new columns to BPASysConfig for enabling offine help and storing a base url for hosting help documents offline
*/

if not exists ( select 1
                from INFORMATION_SCHEMA.columns
                where table_name = 'BPASysConfig' and column_name = 'enableofflinehelp')
begin
    alter table BPASysConfig add enableofflinehelp bit null;
end
go

if not exists ( select 1
                from INFORMATION_SCHEMA.columns
                where table_name = 'BPASysConfig' and column_name = 'offlinehelpbaseurl')
begin
    alter table BPASysConfig add offlinehelpbaseurl NVARCHAR(2083) null;
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
('359', 
 GETUTCDATE(), 
 'db_upgradeR359.sql', 
 'Add new columns to BPASysConfig for enabling offine help and storing a base url for hosting help documents offline', 
 0
);