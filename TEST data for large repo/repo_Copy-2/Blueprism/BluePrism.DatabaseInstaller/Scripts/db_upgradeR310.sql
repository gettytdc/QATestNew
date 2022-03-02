CREATE TABLE BPMISnapshotTrigger(
    queueident INT NOT NULL,
    snapshotid BIGINT NOT NULL,
    lastsnapshotid BIGINT NULL,
    eventtype INT NOT NULL,
    snapshotdate DATETIMEOFFSET NOT NULL,
    snapshotdateutc  AS (CONVERT(DATETIME, DATEADD(MINUTE, -DATEPART(TZOFFSET, snapshotdate), snapshotdate))),
    midnightutc  AS (CONVERT(DATETIME, DATEADD(MINUTE, -DATEPART(TZOFFSET, snapshotdate), TODATETIMEOFFSET(CONVERT(DATE, snapshotdate), DATEPART(TZOFFSET, snapshotdate))))),
CONSTRAINT PK_BPMISnapshotTrigger PRIMARY KEY CLUSTERED (queueident, snapshotid));

IF NOT EXISTS(SELECT *
              FROM sys.objects
              WHERE type = 'P' AND object_id = OBJECT_ID('usp_TriggerQueueSnapshot'))
BEGIN
    EXEC(N'create procedure usp_TriggerQueueSnapshot as begin set nocount on; end')
END
GO

ALTER PROCEDURE usp_TriggerQueueSnapshot
AS
BEGIN
declare @triggers table (
    queueIdent int,
    snapshotId bigint,
    snapshotDate datetimeoffset)

-- Load snapshot triggers into memory
insert into @triggers
select queueIdent, snapshotId, snapshotdate
from BPMISnapshotTrigger where snapshotdateutc <= GETUTCDATE();

declare @queueIdent int, @snapshotId bigint, @snapshotDate datetimeoffset

while exists (select 1 from @triggers)
begin
    -- Get next trigger
    select top 1
        @queueIdent = queueident,
        @snapshotId = snapshotId,
        @snapshotDate = snapshotDate
    from @triggers;

    insert into BPMIQueueSnapshot
    (snapshotid, queueident, snapshotdate, totalitems, itemspending, itemscompleted, itemsreferred, newitemsdelta, completeditemsdelta,
    referreditemsdelta, totalworktimecompleted, totalworktimereferred, totalidletime, totalnewsincemidnight, totalnewlast24hours,
    averagecompletedworktime, averagereferredworktime, averageidletime)
    values (@snapshotId, @queueIdent, @snapshotDate, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    delete from BPMISnapshotTrigger where queueident = @queueIdent and snapshotId = @snapshotId;
    delete from @triggers where queueIdent = @queueIdent and snapshotId = @snapshotId;
end

END
GO

GRANT EXECUTE ON OBJECT::usp_TriggerQueueSnapshot TO bpa_ExecuteSP_System;

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('310',
       GETUTCDATE(),
       'db_upgradeR310.sql',
       'Add BPMISnapshotTrigger table and usp_TriggerQueueSnapshot stored procedure.',
       0);
