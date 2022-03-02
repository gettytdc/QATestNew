/*
SCRIPT         : 428
AUTHOR         : Tomasz Zelewski
PURPOSE        : Add BPDS_Get_Work_Queue_Compositions stored procedure
*/

if not exists (
		select *
		from sys.types
		where name = 'GuidIdTableType'
		)
	create TYPE [GuidIdTableType] as table ([id] [UNIQUEIDENTIFIER] not null primary key)

go

grant exec
	on TYPE::[GuidIdTableType]
	to [bpa_ExecuteSP_System]
go

if not exists (
		select *
		from sys.objects
		where type = 'P' and name = 'BPDS_Get_Work_Queue_Compositions'
		)
begin
	exec (
			'create proc [BPDS_Get_Work_Queue_Compositions] (@QueueIds as GuidIdTableType readonly)
                as
                begin
	                select wq.name, wq.id, [completed], [pending], [deferred], [locked], [exceptioned]
	                from BPAWorkQueue wq
	                inner join BPAWorkQueueItemAggregate wqagg on wq.ident = wqagg.queueIdent
	                where exists (
			                select 1
			                from @QueueIds ipq
			                where ipq.id = wq.id
			                )
                end'
			)
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
('428', 
 GETUTCDATE(), 
 'db_upgradeR428.sql', 
 'Add BPDS_Get_Work_Queue_Compositions stored procedure', 
 0
);
