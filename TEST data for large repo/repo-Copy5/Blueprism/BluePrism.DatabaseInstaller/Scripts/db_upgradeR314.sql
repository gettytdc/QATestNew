
IF NOT EXISTS(SELECT *
              FROM sys.objects
              WHERE type = 'P' AND object_id = OBJECT_ID('usp_CreateInterimQueueSnapshot'))
BEGIN
    EXEC(N'create procedure usp_CreateInterimQueueSnapshot as begin set nocount on; end;')
END
GO


IF NOT EXISTS(SELECT *
              FROM sys.objects
              WHERE type = 'P' AND object_id = OBJECT_ID('usp_CreateFirstQueueSnapshot'))
BEGIN
    EXEC(N'create procedure usp_CreateFirstQueueSnapshot as begin set nocount on; end;')
END
GO

IF NOT EXISTS(SELECT *
              FROM sys.objects
              WHERE type = 'P' AND object_id = OBJECT_ID('usp_CreateNextQueueSnapshot'))
BEGIN
    EXEC(N'create procedure usp_CreateNextQueueSnapshot as begin set nocount on; end;')
END
GO

IF NOT EXISTS(SELECT *
              FROM sys.objects
              WHERE type = 'P' AND object_id = OBJECT_ID('usp_CalculateQueueTrends'))
BEGIN
    EXEC(N'create procedure usp_CalculateQueueTrends as begin set nocount on; end;')
END
GO

IF NOT EXISTS(SELECT *
              FROM sys.objects
              WHERE type = 'TF' AND object_id = OBJECT_ID('ufn_GetQueueEvents'))
BEGIN
    EXEC(N'create function ufn_GetQueueEvents () returns @summary table (queueident int) as begin return; end;')
END
GO

GRANT EXECUTE ON OBJECT::usp_CreateInterimQueueSnapshot TO bpa_ExecuteSP_System;
GO
GRANT EXECUTE ON OBJECT::usp_CreateFirstQueueSnapshot TO bpa_ExecuteSP_System;
GO
GRANT EXECUTE ON OBJECT::usp_CreateNextQueueSnapshot TO bpa_ExecuteSP_System;
GO
GRANT EXECUTE ON OBJECT::usp_CalculateQueueTrends TO bpa_ExecuteSP_System;
GO

alter function ufn_GetQueueEvents (
    @queueIdent int,
    @fromDatetime datetime,
    @toDatetime datetime)
returns @summary table(
    queueIdent int,
    totalItems int,
    itemsPending int,
    itemsCompleted int,
    itemsReferred int,
    createdSinceLast int,
    completedSinceLast int,
    referredSinceLast int,
    totalWorktimeCompleted bigint,
    totalWorktimeReferred bigint,
    totalIdletime bigint)
as
begin
    declare @itemEvents table (
        queueIdent int,
        created int,
        completed int,
        referred int,
        deletedPending int,
        deletedCompleted int,
        deletedReferred int,
        completedWorktime bigint,
        referredWorktime bigint,
        finishedElapsedTime bigint)

    -- Collect the work item events for this queue in the passed period
    insert into @itemEvents
        (queueIdent, created, completed, referred, deletedPending, deletedCompleted,
         deletedReferred, completedWorktime, referredWorktime, finishedElapsedTime)
    select
        @queueIdent,
        ISNULL(SUM(case when eventid = 1 then 1 else 0 end), 0),
        ISNULL(SUM(case when eventid = 5 then 1 else 0 end), 0),
        ISNULL(SUM(case when eventid = 6 then 1 else 0 end), 0),
        ISNULL(SUM(case when (eventid = 7 and statewhendeleted = 1) then 1 else 0 end), 0),
        ISNULL(SUM(case when (eventid = 7 and statewhendeleted = 4) then 1 else 0 end), 0),
        ISNULL(SUM(case when (eventid = 7 and statewhendeleted = 5) then 1 else 0 end), 0),
        ISNULL(SUM(case when eventid = 5 then worktime else 0 end), 0),
        ISNULL(SUM(case when eventid = 6 then worktime else 0 end), 0),
        ISNULL(SUM(case when (eventid = 5 or eventid = 6) then CAST(elapsedtime as bigint) else 0 end), 0)
    from BPMIProductivityShadow
    where queueident = @queueIdent and
        eventid in (1, 5, 6, 7) and
        eventdatetime > @fromDatetime and
        eventdatetime <= @toDatetime;
    
    -- Return the aggregated information
    insert into @summary
        (queueIdent, totalItems, itemsPending, itemsCompleted,
         itemsReferred, createdSinceLast, completedSinceLast, referredSinceLast,
         totalWorktimeCompleted, totalWorktimeReferred, totalIdletime)
    select
        @queueIdent,
        created - (deletedPending + deletedCompleted + deletedReferred),
        created - (completed + referred + deletedPending),
        completed - deletedCompleted,
        referred - deletedReferred,
        created,
        completed,
        referred,
        completedWorktime,
        referredWorktime,
        finishedElapsedTime - completedWorktime - referredWorktime
    from @itemEvents;
    return;
end
GO

alter procedure usp_TriggerQueueSnapshot
as

declare @triggers table (
    queueIdent int,
    snapshotId bigint,
    eventType int,
    snapshotDate datetimeoffset,
    snapshotDateUtc datetime,
    midnightUtc datetime)

declare @regularSnapshot int = 1;
declare @interimSnapshot int = 2;
declare @trendCalculation int = 4;

-- Load current snapshot triggers into memory
insert into @triggers select
    queueIdent,
    snapshotId,
    eventType,
    snapshotdate,
    snapshotdateutc,
    midnightutc
from BPMISnapshotTrigger
where snapshotdateutc <= GETUTCDATE()
order by snapshotdate asc;

declare @queueIdent int, @snapshotId bigint, @eventType int;
declare @snapshotDate datetimeoffset, @midnightUtc datetime, @snapshotDateUtc datetime;
declare @result int, @lockName as varchar(255);

set transaction isolation level snapshot;

while exists (select 1 from @triggers)
begin
    -- Get next trigger
    select top 1
        @queueIdent = queueident,
        @snapshotId = snapshotId,
        @eventType = eventType,
        @snapshotDate = snapshotDate,
        @snapshotDateUtc = snapshotDateUtc,
        @midnightUtc = midnightUtc
    from @triggers
    order by snapshotDate asc;

    begin try
        begin transaction;

        -- Acquire lock for this queue (skip if can't get it)
        set @lockName = 'QueueSnapshot:' + CAST(@queueIdent as varchar);
        exec @result = sp_getapplock @Resource=@lockName, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=100;
        if @result < 0
            begin
                rollback transaction;
                delete from @triggers where queueIdent = @queueIdent and snapshotId = @snapshotId;
                continue;
            end
            
        -- If required create first/next snapshot
        if (@eventType & @regularSnapshot) = @regularSnapshot
            begin
                declare @lastSnapshotId bigint;
                select @lastSnapshotId = lastsnapshotid from BPAWorkQueue where ident = @queueIdent;
                if @lastSnapshotId is null
                    exec usp_CreateFirstQueueSnapshot @queueIdent, @snapshotId, @snapshotDate, @snapshotDateUtc, @midnightUtc;
                else
                    exec usp_CreateNextQueueSnapshot @queueIdent, @snapshotId, @snapshotDate, @snapshotDateUtc, @midnightUtc;
            end

        -- If required create an interim snapshot       
        if (@eventType & @interimSnapshot) = @interimSnapshot
            begin
                exec usp_CreateInterimQueueSnapshot @queueIdent, @snapshotDate, @snapshotId;
            end

        -- If required calculate trend data
        if (@eventType & @trendCalculation) = @trendCalculation
            begin
                exec usp_CalculateQueueTrends @queueIdent, @snapshotDate, @snapshotId;
            end
            
        -- Delete processed trigger and release lock
        delete from BPMISnapshotTrigger where queueident = @queueIdent and snapshotId = @snapshotId;
        exec sp_releaseapplock @Resource=@lockName;
        commit transaction;
    end try
    begin catch
        rollback transaction;
    end catch

    delete from @triggers where queueIdent = @queueIdent and snapshotId = @snapshotId;
end
GO

alter procedure usp_CreateFirstQueueSnapshot
    @queueIdent int,
    @snapshotId bigint,
    @snapshotDate datetimeoffset,
    @snapshotDateUtc datetime,
    @midnightUtc datetime
as
    
declare @itemSummary table (
    queueIdent int,
    pending int,
    completed int,
    referred int)

declare @addedSince table (
    queueIdent int,
    newSinceMidnight int,
    newLast24Hours int)

-- Get queue item summary
insert into @itemSummary
    (queueIdent, pending, completed, referred)
select
    @queueIdent,
    ISNULL(SUM(case when finished is null then 1 else 0 end), 0),
    ISNULL(SUM(case when completed is not null then 1 else 0 end), 0),
    ISNULL(SUM(case when exception is not null then 1 else 0 end), 0)
from BPAWorkQueueItem
where queueident = @queueIdent;

-- Calculate UTC dates
declare @24HoursAgoUtc datetime = DATEADD(hour, -24, @snapshotDateUtc);

-- Get items added since midnight/in last 24 hours
insert into @addedSince
select
    @queueIdent,
    ISNULL(SUM(case when (eventid = 1 and eventdatetime > @midnightUtc) then 1 else 0 end), 0),
    ISNULL(SUM(case when eventid = 1 then 1 else 0 end), 0)
from BPMIProductivityShadow
where queueident = @queueIdent and
    eventid = 1 and
    eventdatetime > @24HoursAgoUtc and
    eventdatetime <= @snapshotDateUtc;

-- Insert data into snapshot table
insert into BPMIQueueSnapshot
    (snapshotid, queueident, snapshotdate, totalitems, itemspending, itemscompleted, itemsreferred, newitemsdelta,
     completeditemsdelta, referreditemsdelta, totalworktimecompleted, totalworktimereferred, totalidletime,
     totalnewsincemidnight, totalnewlast24hours, averagecompletedworktime, averagereferredworktime, averageidletime)
select
    @snapshotId,
    @queueIdent,
    @snapshotDate,
    summary.pending + summary.completed + summary.referred,
    summary.pending,
    summary.completed,
    summary.referred,
    0,
    0,
    0,
    0,
    0,
    0,
    ISNULL(added.newSinceMidnight, 0),
    ISNULL(added.newLast24Hours, 0),
    0,
    0,
    0
from @itemSummary summary
    inner join @addedSince added on added.queueident = summary.queueident;

update BPAWorkQueue set lastsnapshotid = @snapshotId where ident = @queueIdent;
GO

alter procedure usp_CreateInterimQueueSnapshot
    @queueIdent int,
    @snapshotDate datetimeoffset,
    @snapshotId BIGINT
as

-- Calculate UTC datetime of this interim snapshot
declare @snapshotDateUtc datetime = CONVERT(datetime, DATEADD(minute, -DATEPART(TzOffset, @snapshotDate), @snapshotDate))

-- Create or update the interim snapshot table for this queue with events in last 48 hours
if not exists (select queueident from BPMIQueueInterimSnapshot where queueident = @queueIdent)
    begin
        insert into BPMIQueueInterimSnapshot
            (queueident, snapshotdate, totalitems, itemspending, itemscompleted,
             itemsreferred, newitemsdelta, completeditemsdelta, referreditemsdelta,
             totalworktimecompleted, totalworktimereferred, totalidletime)
        select
            @queueIdent,
            @snapshotDate,
            totalItems,
            itemsPending,
            itemsCompleted,
            itemsReferred,
            createdSinceLast,
            completedSinceLast,
            referredSinceLast,
            totalWorktimeCompleted,
            totalWorktimeReferred,
            totalIdletime
        from ufn_GetQueueEvents(@queueIdent, DATEADD(hour, -24, @snapshotDateUtc), @snapshotDateUtc);
    end
else
    begin
        update interim set
            interim.snapshotdate = @snapshotDate,
            interim.totalitems = interim.totalitems + deltas.totalItems,
            interim.itemspending = interim.itemspending + deltas.itemsPending,
            interim.itemscompleted = interim.itemscompleted + deltas.itemsCompleted,
            interim.itemsreferred = interim.itemsreferred + deltas.itemsReferred,
            interim.newitemsdelta = interim.newitemsdelta + deltas.createdSinceLast,
            interim.completeditemsdelta = interim.completeditemsdelta + deltas.completedSinceLast,
            interim.referreditemsdelta = interim.referreditemsdelta + deltas.referredSinceLast,
            interim.totalworktimecompleted = interim.totalworktimecompleted + deltas.totalWorktimeCompleted,
            interim.totalworktimereferred = interim.totalworktimereferred + deltas.totalWorktimeReferred,
            interim.totalidletime = interim.totalidletime + deltas.totalIdletime
        from BPMIQueueInterimSnapshot interim
            inner join ufn_GetQueueEvents(@queueIdent, DATEADD(hour, -24, @snapshotDateUtc), @snapshotDateUtc) deltas on deltas.queueIdent = interim.queueident
        where interim.queueident = @queueIdent;
    end

-- Set the last snapshot id of the interim snapshot we have just taken so the server doesn't repeat the process unnessecarily.
UPDATE BPAWorkQueue
SET lastsnapshotid = @snapshotId
WHERE ident = @queueIdent;
GO

alter procedure usp_CreateNextQueueSnapshot
    @queueIdent int,
    @snapshotId bigint,
    @snapshotDate datetimeoffset,
    @snapshotDateUtc datetime,
    @midnightUtc datetime
as

declare @previousSnapshot table (
    queueIdent int,
    snapshotDate datetimeoffset,
    allItems int,
    pendingItems int,
    completedItems int,
    referredItems int)

declare @InterimSnapshot table (
    queueIdent int,
    snapshotDate datetimeoffset,
    allItems int,
    pendingItems int,
    completedItems int,
    referredItems int,
    createdSinceLast int,
    completedSinceLast int,
    referredSinceLast int,
    completedWorktime bigint,
    referredWorktime bigint,
    idleTime bigint)

declare @addedSince table (
    queueIdent int,
    createdSinceMidnight int,
    createdInLast24Hours int)

-- Get values from previous snapshot
insert into @previousSnapshot
    (queueIdent, snapshotDate, allItems, pendingItems, completedItems, referredItems)
select top 1
    @queueIdent,
    snapshotdate,
    totalitems,
    itemspending,
    itemscompleted,
    itemsreferred
from BPMIQueueSnapshot
where queueident = @queueIdent
order by id desc;

-- Get values from any interim snapshot
insert into @InterimSnapshot
    (queueIdent, snapshotDate, allItems, pendingItems, completedItems,
     referredItems, createdSinceLast, completedSinceLast, referredSinceLast,
     completedWorktime, referredWorktime, idleTime)
select top 1
    @queueIdent,
    snapshotdate,
    totalitems,
    itemspending,
    itemscompleted,
    itemsreferred,
    newitemsdelta,
    completeditemsdelta,
    referreditemsdelta,
    totalworktimecompleted,
    totalworktimereferred,
    totalidletime
from BPMIQueueInterimSnapshot
where queueident = @queueIdent
order by queueident;

-- Calculate UTC dates
declare @24HoursAgoUtc datetime = DATEADD(hour, -24, @snapshotDateUtc);
declare @lastSnapshotDate datetimeoffset;
select @lastSnapshotDate = ISNULL(interim.snapshotDate, previous.snapshotDate) from @previousSnapshot previous
    left join @InterimSnapshot interim on interim.queueIdent = previous.queueIdent;
declare @lastSnapshotDateUtc datetime;
set @lastSnapshotDateUtc = CONVERT(datetime, DATEADD(minute, -DATEPART(TzOffset, @lastSnapshotDate), @lastSnapshotDate));

-- Get items added since midnight/in last 24 hours
insert into @addedSince
    (queueIdent, createdSinceMidnight, createdInLast24Hours)
select
    @queueIdent,
    ISNULL(SUM(case when (eventid = 1 and eventdatetime > @midnightUtc) then 1 else 0 end), 0),
    ISNULL(SUM(case when (eventid = 1 and eventdatetime > @24HoursAgoUtc) then 1 else 0 end), 0)
from BPMIProductivityShadow
where queueident = @queueIdent and
    eventid = 1 and
    eventdatetime > @24HoursAgoUtc and
    eventdatetime <= @snapshotDateUtc;

-- Insert data into snapshot table
insert into BPMIQueueSnapshot
    (snapshotid, queueident, snapshotdate, totalitems, itemspending, itemscompleted, itemsreferred, newitemsdelta,
     completeditemsdelta, referreditemsdelta, totalworktimecompleted, totalworktimereferred, totalidletime,
     totalnewsincemidnight, totalnewlast24hours, averagecompletedworktime, averagereferredworktime, averageidletime)
select
    @snapshotId,
    @queueIdent,
    @snapshotDate,
    previous.allItems + ISNULL(interim.allItems, 0) + deltas.totalItems,
    previous.pendingItems + ISNULL(interim.pendingItems, 0) + deltas.itemsPending,
    previous.completedItems + ISNULL(interim.completedItems, 0) + deltas.itemsCompleted,
    previous.referredItems + ISNULL(interim.referredItems, 0) + deltas.itemsReferred,
    ISNULL(interim.createdSinceLast, 0) + deltas.createdSinceLast,
    ISNULL(interim.completedSinceLast, 0) + deltas.completedSinceLast,
    ISNULL(interim.referredSinceLast, 0) + deltas.referredSinceLast,
    ISNULL(interim.completedWorktime, 0) + deltas.totalWorktimeCompleted,
    ISNULL(interim.referredWorktime, 0) + deltas.totalWorktimeReferred,
    ISNULL(interim.idleTime, 0) + deltas.totalIdletime,
    added.createdSinceMidnight,
    added.createdInLast24Hours,
    (case when ISNULL(interim.completedSinceLast, 0) + deltas.completedSinceLast > 0 then (ISNULL(interim.completedWorktime, 0) + deltas.totalWorktimeCompleted)/(ISNULL(interim.completedSinceLast, 0) + deltas.completedSinceLast) else 0 end),
    (case when ISNULL(interim.referredSinceLast, 0) + deltas.referredSinceLast > 0 then (ISNULL(interim.referredWorktime, 0) + deltas.totalWorktimeReferred)/(ISNULL(interim.referredSinceLast, 0) + deltas.referredSinceLast) else 0 end),
    (case when ISNULL(interim.completedSinceLast, 0) + ISNULL(interim.referredSinceLast, 0) + deltas.completedSinceLast + deltas.referredSinceLast > 0 then (ISNULL(interim.idleTime, 0) + deltas.totalIdletime)/(ISNULL(interim.completedSinceLast, 0) + ISNULL(interim.referredSinceLast, 0) + deltas.completedSinceLast + deltas.referredSinceLast) else 0 end)
from @previousSnapshot previous
    left join @InterimSnapshot interim on interim.queueIdent = previous.queueIdent
    inner join @addedSince added on added.queueident = previous.queueident
    inner join ufn_GetQueueEvents(@queueIdent, @lastSnapshotDateUtc, @snapshotDateUtc) deltas on deltas.queueIdent = previous.queueIdent;

-- Update queue and tidy up any interim snapshot data
update BPAWorkQueue set lastsnapshotid = @snapshotId where ident = @queueIdent;
delete from BPMIQueueInterimSnapshot where queueident = @queueIdent;
GO

alter procedure usp_CalculateQueueTrends
    @queueIdent int,
    @snapshotDate date,
    @snapshotId BIGINT
as

declare @trendDate date = CAST(DATEADD(day, -1, @snapshotDate) as date);

-- Delete existing trend data for this queue
delete from BPMIQueueTrend where queueident = @queueIdent;

-- Calculate trend over last 7 days
insert into BPMIQueueTrend
    (snapshottimeofdaysecs, queueident, trendid,
     averagetotalitems, averageitemspending, averageitemscompleted, averageitemsreferred,
     averagenewitemsdelta, averagecompleteditemsdelta, averagereferreditemsdelta,
     averagetotalworktimecompleted, averagetotalworktimereferred, averagetotalidletime,
     averagetotalnewsincemidnight, averagetotalnewlast24hours,
     averageaveragecompletedworktime, averageaveragereferredworktime, averageaverageidletime)
select
    configuration.timeofdaysecs,
    snapshots.queueident,
    1,
    AVG(snapshots.totalitems),
    AVG(snapshots.itemspending),
    AVG(snapshots.itemscompleted),
    AVG(snapshots.itemsreferred),
    AVG(snapshots.newitemsdelta),
    AVG(snapshots.completeditemsdelta),
    AVG(snapshots.referreditemsdelta),
    AVG(snapshots.totalworktimecompleted),
    AVG(snapshots.totalworktimereferred),
    AVG(snapshots.totalidletime),
    AVG(snapshots.totalnewsincemidnight),
    AVG(snapshots.totalnewlast24hours),
    AVG(snapshots.averagecompletedworktime),
    AVG(snapshots.averagereferredworktime),
    AVG(snapshots.averageidletime)
from BPMIConfiguredSnapshot configuration
    inner join BPMIQueueSnapshot snapshots on snapshots.snapshotid = configuration.snapshotid and snapshots.queueident = configuration.queueident
where configuration.queueident = @queueIdent
    and CAST(snapshots.snapshotdate as date) > CAST(DATEADD(day, -7, @trendDate) as date)
    and CAST(snapshots.snapshotdate as date) <= @trendDate
group by configuration.timeofdaysecs, snapshots.queueident

-- Calculate trend over last 28 days
insert into BPMIQueueTrend
    (snapshottimeofdaysecs, queueident, trendid,
     averagetotalitems, averageitemspending, averageitemscompleted, averageitemsreferred,
     averagenewitemsdelta, averagecompleteditemsdelta, averagereferreditemsdelta,
     averagetotalworktimecompleted, averagetotalworktimereferred, averagetotalidletime,
     averagetotalnewsincemidnight, averagetotalnewlast24hours,
     averageaveragecompletedworktime, averageaveragereferredworktime, averageaverageidletime)
select
    configuration.timeofdaysecs,
    snapshots.queueident,
    2,
    AVG(snapshots.totalitems),
    AVG(snapshots.itemspending),
    AVG(snapshots.itemscompleted),
    AVG(snapshots.itemsreferred),
    AVG(snapshots.newitemsdelta),
    AVG(snapshots.completeditemsdelta),
    AVG(snapshots.referreditemsdelta),
    AVG(snapshots.totalworktimecompleted),
    AVG(snapshots.totalworktimereferred),
    AVG(snapshots.totalidletime),
    AVG(snapshots.totalnewsincemidnight),
    AVG(snapshots.totalnewlast24hours),
    AVG(snapshots.averagecompletedworktime),
    AVG(snapshots.averagereferredworktime),
    AVG(snapshots.averageidletime)
from BPMIConfiguredSnapshot configuration
    inner join BPMIQueueSnapshot snapshots on snapshots.snapshotid = configuration.snapshotid and snapshots.queueident = configuration.queueident
where configuration.queueident = @queueIdent
    and CAST(snapshots.snapshotdate as date) > CAST(DATEADD(day, -28, @trendDate) as date)
    and CAST(snapshots.snapshotdate as date) <= @trendDate
group by configuration.timeofdaysecs, snapshots.queueident

-- Calculate trend over last 4 instances of the current snapshot weekday
insert into BPMIQueueTrend
    (snapshottimeofdaysecs, queueident, trendid,
     averagetotalitems, averageitemspending, averageitemscompleted, averageitemsreferred,
     averagenewitemsdelta, averagecompleteditemsdelta, averagereferreditemsdelta,
     averagetotalworktimecompleted, averagetotalworktimereferred, averagetotalidletime,
     averagetotalnewsincemidnight, averagetotalnewlast24hours,
     averageaveragecompletedworktime, averageaveragereferredworktime, averageaverageidletime)
select
    configuration.timeofdaysecs,
    snapshots.queueident,
    3,
    AVG(snapshots.totalitems),
    AVG(snapshots.itemspending),
    AVG(snapshots.itemscompleted),
    AVG(snapshots.itemsreferred),
    AVG(snapshots.newitemsdelta),
    AVG(snapshots.completeditemsdelta),
    AVG(snapshots.referreditemsdelta),
    AVG(snapshots.totalworktimecompleted),
    AVG(snapshots.totalworktimereferred),
    AVG(snapshots.totalidletime),
    AVG(snapshots.totalnewsincemidnight),
    AVG(snapshots.totalnewlast24hours),
    AVG(snapshots.averagecompletedworktime),
    AVG(snapshots.averagereferredworktime),
    AVG(snapshots.averageidletime)
from BPMIConfiguredSnapshot configuration
    inner join BPMIQueueSnapshot snapshots on snapshots.snapshotid = configuration.snapshotid and snapshots.queueident = configuration.queueident
where configuration.queueident = @queueIdent
    and CAST(snapshots.snapshotdate as date) > CAST(DATEADD(day, -29, @snapshotDate) as date)
    and CAST(snapshots.snapshotdate as date) < @snapshotDate
    and DATEPART(weekday, snapshots.snapshotdate) = DATEPART(weekday, @snapshotDate)
group by configuration.timeofdaysecs, snapshots.queueident

-- Purge any old snapshot rows
delete from BPMIQueueSnapshot
where queueident = @queueIdent and
    snapshotdate < DATEADD(day, -28, @trendDate);

-- Set the last snapshot id of the queue trend snapshot calculation we have just taken so the server doesn't repeat the process unnessecarily.
UPDATE BPAWorkQueue
SET lastsnapshotid = @snapshotId
WHERE ident = @queueIdent;
GO

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('314',
       getutcdate(),
       'db_upgradeR314.sql',
       'Create stored procedures for work queue analysis',
       0);
