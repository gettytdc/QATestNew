/*
SCRIPT         : 411
PURPOSE        : Convert the primary key in the BPAInternalAuth table from a GUID to a auto-generated big int
AUTHOR         : Rob Cairns
*/

create table newTable(
    ID bigint identity(1,1),
    UserID uniqueidentifier,
    Token uniqueidentifier,
    Expiry datetime,
    LoggedInMode int default 0,
    ProcessId uniqueidentifier,
    Roles nvarchar(max) default '',
    IsWebService bit not null default 0,
    constraint PK_BPAInternalAuth_ID primary key (ID)
)
go

insert into newTable (UserID, Token, Expiry, LoggedInMode, ProcessId, Roles, IsWebService)
select UserID, Token, Expiry, LoggedInMode, ProcessId, Roles, IsWebService from BPAInternalAuth
go

drop table BPAInternalAuth;
go
exec sp_rename 'newTable', 'BPAInternalAuth';
go

alter table BPAInternalAuth
add constraint FK_BPAInternalAuth_BPAProcess
foreign key (ProcessId) references BPAProcess(ProcessId);
go

create nonclustered index INDEX_BPAInternalAuth_Token
on BPAInternalAuth(Token)

-- set db version.
insert into bpadbversion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'411'
	,getutcdate()
	,'db_upgradeR411.sql'
	,'Convert the primary key in the BPAInternalAuth table from a GUID to a auto-generated big int'
	,0
	);
