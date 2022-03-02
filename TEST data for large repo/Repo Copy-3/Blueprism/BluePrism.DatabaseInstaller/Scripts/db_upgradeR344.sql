/*
SCRIPT         : 344
AUTHOR         : Tomasz Zelewski
PURPOSE        : Insert missing [BPAUserRolePerm] entries between Decipher permissions and System Administrators
*/

declare @systemAdminRoleId int = (
		select top 1 [id]
		from [BPAUserRole]
		where [name] = 'System Administrators'
		)

insert into [BPAUserRolePerm] (
	[permid]
	,[userroleid]
	)
select p.id
	,@systemAdminRoleId
from [BPAPerm] p
join [BPAUserRolePerm] urp on p.id = urp.permid
where p.requiredfeature = 'DocumentProcessing'
	and p.id not in (
		select p.id
		from [BPAPerm] p
		join [BPAUserRolePerm] r on p.id = r.permid
		where p.requiredfeature = 'DocumentProcessing'
			and r.userroleid = @systemAdminRoleId
		)

-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'344'
	,getutcdate()
	,'db_upgradeR344.sql'
	,'Insert missing BPAUserRolePerm entries for Decipher permissions'
	,0
	);