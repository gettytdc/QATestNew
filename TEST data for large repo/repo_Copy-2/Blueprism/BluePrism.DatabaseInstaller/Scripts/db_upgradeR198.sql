/*
SCRIPT         : 198
AUTHOR         : SW
PURPOSE        : Adds new "locked item" table for queues
EQUIVALENTS    : v4.2 = R197;
*/

create table BPACaseLock (
    id bigint not null,
    locktime datetime not null,
    sessionid uniqueidentifier null,
    lockid uniqueidentifier not null,
    constraint PK_BPACaseLock
        primary key clustered (id),
    constraint FK_CaseLock_WorkQueueItem
        foreign key (id)
            references BPAWorkQueueItem (ident),
    constraint FK_CaseLock_Session
        foreign key (sessionid)
            references BPASession (sessionid)
);

create nonclustered index Index_BPACaseLock_sessionid
  on BPACaseLock(sessionid);

create nonclustered index Index_BPACaseLock_lockid
  on BPACaseLock(lockid);

GO

drop index Index_BPAWorkQueueItem_locked on BPAWorkQueueItem;
GO

drop index Index_BPAWorkQueueItem_lastupdated on BPAWorkQueueItem;
GO

alter table BPAWorkQueueItem
  drop column queuepositiondate, lastupdated, state, locked;
GO

-- Option 1: lastupdated no longer registers 'locked'

alter table BPAWorkQueueItem add
  lastupdated as coalesce(completed,exception,loaded) persisted;
GO


-- Option 2: replace with a view which works just like before
--           except that lastupdated is not persisted/indexed

if not exists(select * from sys.views where name = 'BPVWorkQueueItem')
  exec (N'create view BPVWorkQueueItem as select 1 as placeholder');
GO

alter view BPVWorkQueueItem as
select
     it.ident
    ,it.id
    ,it.queueid
    ,it.keyvalue
    ,it.status
    ,it.attempt
    ,it.loaded
    ,lk.locktime as locked
    ,it.completed
    ,it.exception
    ,it.exceptionreason
    ,it.deferred
    ,it.worktime
    ,it.data
    ,it.queueident
    ,coalesce(lk.sessionid,it.sessionid) as sessionid
    ,it.priority
    ,case
        when it.exception is null and it.completed is null and lk.locktime is null and (it.deferred is null or it.deferred<getutcdate())
            then it.loaded
        else convert(datetime,'99991231',0) end as queuepositiondate
    ,it.prevworktime
    ,it.attemptworktime
    ,it.finished
    ,case
        when it.exception is not null then 5
        when it.completed is not null then 4
        when (it.deferred is not null and it.deferred > getutcdate()) then 3
        when lk.locktime is not null then 2
        else 1
     end as state
    ,coalesce(it.completed,it.exception,lk.locktime,it.loaded) as lastupdated
    ,it.exceptionreasonvarchar
    ,it.exceptionreasontag
    ,it.encryptid
  from BPAWorkQueueItem it
    left join BPACaseLock lk on it.ident = lk.id;
GO

-- Create a 'Get Tag IDs' proc to get the IDs relating to a particular
-- tag with optional wildcards

if not exists (select * from sys.procedures where name='usp_gettagids' and type='P')
  exec(N'create procedure usp_gettagids as select 1');
GO

alter procedure usp_gettagids
    @tag nvarchar(255)
as
    declare @tags table(id int not null);

    if @tag is not null
        if @tag like '%[_%]%'
            insert into @tags
            select t.id from BPATag t where t.tag like @tag;
        else
            insert into @tags
            select t.id from BPATag t where t.tag = @tag;

    -- If we have tags, return them. Otherwise, return null to indicate
    -- that the tag didn't exist
    if exists (select * from @tags)
        select * from @tags;
    else
        select null;
GO

-- Implement Get Next as a stored proc
if not exists (select * from sys.procedures where name='usp_getnextcase' and type='P')
  exec(N'create procedure usp_getnextcase as select 1');
GO

alter procedure usp_getnextcase
    @queuename nvarchar(255),
    @keyfilter nvarchar(255) = null,
    @sess uniqueidentifier = null,
    @ontag1 nvarchar(255)  = null,
    @ontag2 nvarchar(255)  = null,
    @ontag3 nvarchar(255)  = null,
    @ontag4 nvarchar(255)  = null,
    @ontag5 nvarchar(255)  = null,
    @ontag6 nvarchar(255)  = null,
    @ontag7 nvarchar(255)  = null,
    @ontag8 nvarchar(255)  = null,
    @ontag9 nvarchar(255)  = null,
    @offtag1 nvarchar(255) = null,
    @offtag2 nvarchar(255) = null,
    @offtag3 nvarchar(255) = null,
    @offtag4 nvarchar(255) = null,
    @offtag5 nvarchar(255) = null,
    @offtag6 nvarchar(255) = null,
    @offtag7 nvarchar(255) = null,
    @offtag8 nvarchar(255) = null,
    @offtag9 nvarchar(255) = null
as
    declare @sql nvarchar(max);
    declare @params nvarchar(max);

    -- This lock id ensures we can identify the lock record in
    -- BPACaseLock after we've created it
    declare @lockid uniqueidentifier;
    set @lockid = newid();

    -- Get the tags first. Note that we don't need to worry about
    -- 'virtual' tags for get next. At the moment, the only virtual tag
    -- is an exception tag: if it has an exception on it, it is not
    -- eligible for the get next call

    if object_id('tempdb..#ontags') is not null drop table #ontags;
    if object_id('tempdb..#offtags')is not null drop table #offtags;

    create table #ontags (id int);
    create table #offtags (id int);

    if @ontag1 is not null insert into #ontags exec usp_gettagids @tag=@ontag1;
    if @ontag2 is not null insert into #ontags exec usp_gettagids @tag=@ontag2;
    if @ontag3 is not null insert into #ontags exec usp_gettagids @tag=@ontag3;
    if @ontag4 is not null insert into #ontags exec usp_gettagids @tag=@ontag4;
    if @ontag5 is not null insert into #ontags exec usp_gettagids @tag=@ontag5;
    if @ontag6 is not null insert into #ontags exec usp_gettagids @tag=@ontag6;
    if @ontag7 is not null insert into #ontags exec usp_gettagids @tag=@ontag7;
    if @ontag8 is not null insert into #ontags exec usp_gettagids @tag=@ontag8;
    if @ontag9 is not null insert into #ontags exec usp_gettagids @tag=@ontag9;

    -- If there are any nulls in #ontags - that implies the tag doesn't exist;
    -- if the tag doesn't exist, no work case can have it applied, ergo there are
    -- no cases with that tag, might as well exit now with nothing
    if exists (select 1 from #ontags where id is null) return;

    if @offtag1 is not null insert into #offtags exec usp_gettagids @tag=@offtag1;
    if @offtag2 is not null insert into #offtags exec usp_gettagids @tag=@offtag2;
    if @offtag3 is not null insert into #offtags exec usp_gettagids @tag=@offtag3;
    if @offtag4 is not null insert into #offtags exec usp_gettagids @tag=@offtag4;
    if @offtag5 is not null insert into #offtags exec usp_gettagids @tag=@offtag5;
    if @offtag6 is not null insert into #offtags exec usp_gettagids @tag=@offtag6;
    if @offtag7 is not null insert into #offtags exec usp_gettagids @tag=@offtag7;
    if @offtag8 is not null insert into #offtags exec usp_gettagids @tag=@offtag8;
    if @offtag9 is not null insert into #offtags exec usp_gettagids @tag=@offtag9;

    -- if there are nulls in #offtags, we can exclude that restriction from the
    -- search - there are no cases with any tags which don't exist in the database
    delete from #offtags where id is null;

    set @sql = '
    insert into BPACaseLock (id,locktime,sessionid,lockid)
    select top 1
         i.ident
        ,getutcdate()
        ,case when @sess is null then i.sessionid else @sess end
        ,@lockid
      from BPAWorkQueueItem i
        join BPAWorkQueue q on i.queueident = q.ident
        left join BPACaseLock l on l.id = i.ident
      where
        q.name = @queuename
        and q.running = 1
        and i.finished is null and (i.deferred is null or i.deferred < getutcdate()) /* ie. pending */
        and l.id is null /* ie. and not locked... */
        and (@keyfilter is null or i.keyvalue = @keyfilter)
    ';
    if exists (select 1 from #ontags) begin
        set @sql = @sql + '
        and (
          select count(*)
            from BPAWorkQueueItemTag it
              join #ontags ot on it.tagid = ot.id
          where it.queueitemident = i.ident
        ) = (select count(*) from #ontags)
        ';
    end
    if exists (select 1 from #offtags) begin
        set @sql = @sql + '
        and not exists (
          select 1
            from BPAWorkQueueItemTag it
              join #offtags ot on it.tagid = ot.id
          where it.queueitemident = i.ident
        )
        ';
    end
    set @sql = @sql + '
        order by i.priority, i.loaded, i.ident;
    ';

    -- The params that we need to pass for the above sql
    set @params = '
        @sess uniqueidentifier,
        @queuename nvarchar(255),
        @keyfilter nvarchar(255),
        @lockid uniqueidentifier
    ';

    -- Actually call the SQL to create the lock record
    exec sp_executesql @sql,@params,@sess=@sess,@queuename=@queuename,@keyfilter=@keyfilter,@lockid=@lockid;

    -- Join the lock record and get the queue / case details to return
    select i.encryptid, i.id, i.ident, i.keyvalue, i.data, i.status, i.attempt
      from BPAWorkQueueItem i
        join BPAWorkQueue q on i.queueident = q.ident
        join BPACaseLock l on i.ident = l.id
      where l.lockid = @lockid;

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '198',
  GETUTCDATE(),
  'db_upgradeR198.sql UTC',
  'Adds new "locked item" table for queues'
);
