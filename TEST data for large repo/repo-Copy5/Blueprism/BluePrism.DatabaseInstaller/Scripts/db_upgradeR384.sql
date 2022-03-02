/*
SCRIPT		: 384
STORY		: bp-641
PURPOSE		: Adds functionality to save the tree state for a user (which nodes are expanded)
AUTHOR		: Matthew Nethercott
*/

create table BPAGroupUserPref
(
    UserId uniqueidentifier not null,
    GroupId uniqueidentifier not null,
    TreeType smallint not null,
    [Timestamp] datetime not null default GetUTCDate(),

    primary key (UserId, GroupId),

    constraint FK_BPAGroupUserPref_BPAGroup
        foreign key (GroupId)
        references BPAGroup (id)
        on delete cascade    
);

create type GroupIdParameterTable
    as table
    (
        GroupId uniqueidentifier
    );
go

-- set db version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'384'
	,getutcdate()
	,'db_upgradeR384.sql'
	,'Adds functionality to save the tree state for a user (which nodes are expanded)'
	,0
);
