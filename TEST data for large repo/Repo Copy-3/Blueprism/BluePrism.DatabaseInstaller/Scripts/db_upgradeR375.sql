/*
SCRIPT        : 375
PURPOSE       : Alter work queue update stored procedure - usp_SetTargetSessionsForMultipleWorkQueues - to get all queue data in bulk.
AUTHOR		  : Gary Chadwick & Rob Cairns
*/

create type TargetSessionDetails as table(
	queueId int primary key clustered not null,
	targetSessionAmount int not null
)
go

create procedure usp_SetTargetSessionsForMultipleWorkQueues
(@tvpTargetSessionDetails TargetSessionDetails ReadOnly)
as
set nocount on

update BPQ
set BPQ.targetsessions = tsd.targetSessionAmount 
from @tvpTargetSessionDetails tsd 
inner join BPAWorkQueue BPQ on BPQ.ident = tsd.queueId
go

-- set db version.
insert into bpadbversion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'375'
	,getutcdate()
	,'db_upgradeR375.sql'
	,'Alter work queue update stored procedure - usp_SetTargetSessionsForMultipleWorkQueues - to get all queue data in bulk.'
	,0
	);
