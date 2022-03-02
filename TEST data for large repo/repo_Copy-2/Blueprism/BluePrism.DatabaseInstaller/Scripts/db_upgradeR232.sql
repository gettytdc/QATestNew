/*
SCRIPT         : 232
AUTHOR         : DJM
PURPOSE        : usp_getnextcase stored procedure optimisation
*/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
 
ALTER procedure usp_getnextcase
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

    -- Any non-wildcard tags, we put into #ontags - wildcarded tags need to be handled separately.
    -- (Note: only for 'on' tags;
    if @ontag1 is not null and @ontag1 not like '%[_%]%' insert into #ontags exec usp_gettagids @tag=@ontag1;
    if @ontag2 is not null and @ontag2 not like '%[_%]%' insert into #ontags exec usp_gettagids @tag=@ontag2;
    if @ontag3 is not null and @ontag3 not like '%[_%]%' insert into #ontags exec usp_gettagids @tag=@ontag3;
    if @ontag4 is not null and @ontag4 not like '%[_%]%' insert into #ontags exec usp_gettagids @tag=@ontag4;
    if @ontag5 is not null and @ontag5 not like '%[_%]%' insert into #ontags exec usp_gettagids @tag=@ontag5;
    if @ontag6 is not null and @ontag6 not like '%[_%]%' insert into #ontags exec usp_gettagids @tag=@ontag6;
    if @ontag7 is not null and @ontag7 not like '%[_%]%' insert into #ontags exec usp_gettagids @tag=@ontag7;
    if @ontag8 is not null and @ontag8 not like '%[_%]%' insert into #ontags exec usp_gettagids @tag=@ontag8;
    if @ontag9 is not null and @ontag9 not like '%[_%]%' insert into #ontags exec usp_gettagids @tag=@ontag9;

    -- If there are any nulls in #ontags - that implies the tag doesn't exist;
    -- if the tag doesn't exist, no work case can have it applied, ergo there are
    -- no cases with that tag, might as well exit now with nothing
    if exists (select 1 from #ontags where id is null) return;

    -- The off tags all go into #offtags. Note that we don't need to separate 'static' and
    -- 'wildcarded' tags here due to the nature of the test we're doing (ie. if there are *any*
    -- tag matches we discard the item)
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

    -- Note that this disables locks for the purposes of this operation - that disabling is
    -- only in place while the dynamic sql is running and it reverts to the database default
    -- after the SQL has been completed.
    -- This is safe since we are using a 'soft lock' - ie. a lock implemented by our
    -- database schema to ensure the safety of the case lock, and we want to remove
    -- contention with other aspects of the queue management.
    set @sql = '
    set transaction isolation level read uncommitted;
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
        ';
    if @keyfilter is not null begin
        SET @sql = @sql + '
        and i.keyvalue = @keyfilter';
    end
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

    declare @onwildcardsql nvarchar(max)
    set @onwildcardsql = '
        and exists (
            select 1
            from BPAWorkQueueItemTag it
              join BPATag t on it.tagid = t.id
            where it.queueitemident = i.ident
              and t.tag like @ontag';
    if @ontag1 like '%[_%]%' set @sql = @sql + @onwildcardsql + '1)';
    if @ontag2 like '%[_%]%' set @sql = @sql + @onwildcardsql + '2)';
    if @ontag3 like '%[_%]%' set @sql = @sql + @onwildcardsql + '3)';
    if @ontag4 like '%[_%]%' set @sql = @sql + @onwildcardsql + '4)';
    if @ontag5 like '%[_%]%' set @sql = @sql + @onwildcardsql + '5)';
    if @ontag6 like '%[_%]%' set @sql = @sql + @onwildcardsql + '6)';
    if @ontag7 like '%[_%]%' set @sql = @sql + @onwildcardsql + '7)';
    if @ontag8 like '%[_%]%' set @sql = @sql + @onwildcardsql + '8)';
    if @ontag9 like '%[_%]%' set @sql = @sql + @onwildcardsql + '9)';

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
        @lockid uniqueidentifier,
        @ontag1 nvarchar(255),
        @ontag2 nvarchar(255),
        @ontag3 nvarchar(255),
        @ontag4 nvarchar(255),
        @ontag5 nvarchar(255),
        @ontag6 nvarchar(255),
        @ontag7 nvarchar(255),
        @ontag8 nvarchar(255),
        @ontag9 nvarchar(255)
    ';

    -- Attempt to lock the next item a maximum of 100 times before bailing
    declare @attempt int, @maxattempts int;
    set @maxattempts = 100;
    set @attempt = 1;
    while @attempt <= @maxattempts
    begin
        begin try
            -- Actually call the SQL to create the lock record
            exec sp_executesql @sql,@params,@sess=@sess,@queuename=@queuename,@keyfilter=@keyfilter,@lockid=@lockid,@ontag1=@ontag1,@ontag2=@ontag2,@ontag3=@ontag3,@ontag4=@ontag4,@ontag5=@ontag5,@ontag6=@ontag6,@ontag7=@ontag7,@ontag8=@ontag8,@ontag9=@ontag9;
            break;
        end try
        begin catch
            if error_number() = 2627 begin -- ie. primary constraint error
                -- Increment the attempt counter and try again
                set @attempt = @attempt + 1;
                if @attempt <= @maxattempts
                    continue;
                -- If we've overshot the max attempts, just rethrow the last error
            end;
            -- Otherwise "rethrow" (which doesn't become a keyword until SQL Server 2012)
            exec usp_rethrow;
            return;
        end catch
    end;
    -- Join the lock record and get the queue / case details to return
    select i.encryptid, i.id, i.ident, i.keyvalue, i.data, i.status, i.attempt
      from BPAWorkQueueItem i
        join BPAWorkQueue q on i.queueident = q.ident
        join BPACaseLock l on i.ident = l.id
      where l.lockid = @lockid;

GO



-- set DB version
INSERT INTO BPADBVersion VALUES (
  '232',    
  GETUTCDATE(),
  'db_upgradeR232.sql UTC',
  'usp_getnextcase stored procedure optimisation',
   0 -- UTC
);
