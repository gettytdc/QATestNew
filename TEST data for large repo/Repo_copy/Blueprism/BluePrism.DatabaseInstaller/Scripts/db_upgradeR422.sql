/*
STORY:      BP-3210
PURPOSE:   Change authenticationServerUserId column in BPAUser table to uniqueidentifier data type and add a unique constraints on authServerUserId columns

*/

alter table BPAUser
drop column authenticationServerUserId

alter table BPAUser
add authenticationServerUserId uniqueidentifier null

create unique nonclustered index UQ_BPAUser_authenticationServerUserId
on BPAUser(authenticationServerUserId)
where authenticationServerUserId is not null


-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('422',
 getutcdate(), 
 'db_upgradeR422.sql', 
 'Change authenticationServerUserId column in BPAUser table to uniqueidentifier data type and add a unique constraint', 
 0
);
