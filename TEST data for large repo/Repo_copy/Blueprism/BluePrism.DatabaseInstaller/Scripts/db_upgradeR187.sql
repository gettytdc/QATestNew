/*
SCRIPT         : 187
AUTHOR         : GM
PURPOSE        : Adds additional dashboard tiles
*/

-- Create new data source for Queue Volumes
if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_QueueVolumesNow')
   exec(N'create procedure BPDS_QueueVolumesNow as begin set nocount on; end');
GO

alter procedure BPDS_QueueVolumesNow
    @BPQueueName nvarchar(max) = null,
    @ExcludePending nvarchar(max) = 'False',
    @ExcludeDeferred nvarchar(max) = 'False',
    @ExcludeComplete nvarchar(max) = 'False',
    @ExcludeExceptions nvarchar(max) = 'False' as

if @ExcludePending not in ('True', 'False')
    raiserror('@ExcludePending must be either True or False', 11, 1);
else if @ExcludeDeferred not in ('True', 'False')
    raiserror('@ExcludeDeferred must be either True or False', 11, 1);
else if @ExcludeComplete not in ('True', 'False')
    raiserror('@ExcludeComplete must be either True or False', 11, 1);
else if @ExcludeExceptions not in ('True', 'False')
    raiserror('@ExcludeExceptions must be either True or False', 11, 1);
else
begin
    declare @ColumnNames nvarchar(max);
    select @ColumnNames = ISNULL(@ColumnNames + ',', '') + QUOTENAME(ItemStatus)
    from (
    select 'Pending' as ItemStatus where @ExcludePending='False' union
    select 'Deferred' as ItemStatus where @ExcludeDeferred='False' union
    select 'Complete' as ItemStatus where @ExcludeComplete='False' union
    select 'Exceptions' as ItemStatus where @ExcludeExceptions='False') as StatusNarrs;

    declare @WhereClause nvarchar(max);
    set @WhereClause = ISNULL(' and q.name = ''' + @BPQueueName + '''', '');

    declare @SQLQuery nvarchar(max);
    set @SQLQuery = 'with results as (
    select
        q.name,
        case
            when i.state = 1 then ''Pending''
            when i.state = 3 then ''Deferred''
            when i.state = 4 then ''Complete''
            when i.state = 5 then ''Exceptions''
        end as state,
        COUNT(*) as Number
    from BPAWorkQueue q
        inner join BPAWorkQueueItem i on i.queueident=q.ident
    where i.state in (1,3,4,5)' + @WhereClause + '
    group by q.name, i.state)
    select name, ' + @ColumnNames + ' from results pivot (SUM(Number) for state in (' + @ColumnNames + ')) as number';

    exec(@SQLQuery);
end
return;
GO

-- Register as system data source
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_QueueVolumesNow', 1, 'QueueVolumesNow.htm');

 -- Add to system data source executor's role
GRANT EXECUTE ON OBJECT::BPDS_QueueVolumesNow TO bpa_ExecuteSP_DataSource_bpSystem;

-- Add new tile - Queue Volumes Now
insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Queue Volumes Now',1,'Queue volumes by status',0,'<Chart type="4" plotByRow="false"><Procedure name="BPDS_QueueVolumesNow" /></Chart>');

 -- Add new tile - Pending Queue Volumes
insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Pending Queue Volumes',1,'Pending Queue Volumes',0,'<Chart type="6" plotByRow="false"><Procedure name="BPDS_QueueVolumesNow"><Param name="@ExcludeDeferred">True</Param><Param name="@ExcludeComplete">True</Param><Param name="@ExcludeExceptions">True</Param></Procedure></Chart>');

-- Add new tile - Largest 10 database tables
insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Largest Database Tables (Column)',1,'The largest ten tables in the database (Mb)',0,'<Chart type="3" plotByRow="false"><Procedure name="BPDS_LargestTables"><Param name="@NumberOfTables">10</Param></Procedure></Chart>');

 -- Re-create default dashboard
delete from BPADashboardTile where dashid='00000000-0000-0000-0000-000000000000';

insert into BPADashboardTile (dashid, tileid, displayorder, width, height)
select '00000000-0000-0000-0000-000000000000', t.id, 1, 1, 1 from BPATile t where t.name in ('Workforce Availability');

insert into BPADashboardTile (dashid, tileid, displayorder, width, height)
select '00000000-0000-0000-0000-000000000000', t.id, 2, 1, 1 from BPATile t where t.name in ('Total Automations');

insert into BPADashboardTile (dashid, tileid, displayorder, width, height)
select '00000000-0000-0000-0000-000000000000', t.id, 3, 1, 1 from BPATile t where t.name in ('Queue Volumes Now');

insert into BPADashboardTile (dashid, tileid, displayorder, width, height)
select '00000000-0000-0000-0000-000000000000', t.id, 4, 3, 1 from BPATile t where t.name in ('Largest Database Tables (Column)');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '187',
  GETUTCDATE(),
  'db_upgradeR187.sql UTC',
  'Adds additional dashboard tiles'
);
