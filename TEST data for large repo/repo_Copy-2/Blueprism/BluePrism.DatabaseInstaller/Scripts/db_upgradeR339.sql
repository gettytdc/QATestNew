/*
SCRIPT         : 339
AUTHOR         : Sarah McClean
PURPOSE        : Create new column 'Auth Type' in BPAUser to show the authentication type for each user 
*/

-- Add new column
alter table BPAUser
add authtype int not null default 0 --0 is the AuthMode.Unspecified
go

-- Now add the correct value for all existing users in the database on upgrade
declare @isnativeenvironment bit;
if exists (select 1 from BPASysConfig where ActiveDirectoryProvider is null or ActiveDirectoryProvider = '') set @isnativeenvironment = 1;

update BPAUser
set authtype = 4 --system
where systemusername = 'Scheduler'

update BPAUser
set authtype = 3 --anonymous
where systemusername = 'Anonymous Resource'

if @isnativeenvironment = 1 
    update BPAuser
    set authtype = 1 --native
    where authtype = 0
else
    update BPAuser
    set authtype = 2 --Active Directory
    where authtype = 0
go

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('339',
       getutcdate(),
       'db_upgradeR339.sql',
       'Add column authtype to BPAUser.',
       0);