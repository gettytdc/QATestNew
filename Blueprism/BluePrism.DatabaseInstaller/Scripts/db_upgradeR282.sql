IF NOT EXISTS(SELECT *
              FROM sys.objects
              WHERE type = 'P' AND object_id = OBJECT_ID('BPDS_HoursSpentWorkingQueuesByMonth'))
BEGIN
    EXEC(N'create procedure BPDS_HoursSpentWorkingQueuesByMonth as begin set nocount on; end')
END
GO

ALTER procedure BPDS_HoursSpentWorkingQueuesByMonth
    @NumberOfMonths int = 6,
    @QueueName nvarchar(max) = null
as
    IF @NumberOfMonths < 1 RAISERROR('@NumberOfMonths must be 1 or greater', 11, 1);
    SET @QueueName = ISNULL(LTRIM(RTRIM(@QueueName)), '');
    IF @QueueName = ''
    BEGIN
        -- Display results against all queues.
        SELECT Results.[Month],
               CAST(ROUND(SUM(Results.Seconds/3600), 0) AS FLOAT) AS HoursWorked
        FROM (
            -- First get all months with a default value of 0 seconds
            SELECT 
                -- Returns months working backwords from the current month, for @NumberOfMonths months, formatted as Oct 2018 -> 2018-10
                CAST(TheYear as char(4)) + '-' + RIGHT('00' + CAST(TheMonth AS VARCHAR(2)), 2) As [Month],
                0.00 as Seconds
              FROM ufn_GetReportMonths(@NumberOfMonths)
              UNION ALL
              -- Get all productive months, with total working time in seconds
              SELECT CAST(reportyear as char(4)) + '-' + RIGHT('00' + CAST(reportmonth AS VARCHAR(2)), 2) AS [Month],
                     (completed + exceptioned) * avgworktime AS Seconds
              FROM BPMIProductivityMonthly pm
              INNER JOIN ufn_GetReportMonths(@NumberOfMonths) 
                ON TheYear = reportyear AND TheMonth = reportmonth) Results
        GROUP BY Results.[Month]
        ORDER BY Results.[Month] ASC
    END
    ELSE BEGIN
        -- Display results against specific queue.
        -- All as above, with additional filter on queue name
        SELECT Results.[Month],
               CAST(ROUND(SUM(Results.Seconds/3600), 0) AS FLOAT) AS HoursWorked
        FROM (SELECT CAST(TheYear as char(4)) + '-' + RIGHT('00' + CAST(TheMonth AS VARCHAR(2)), 2) As [Month],
                     0.00 as Seconds
              FROM ufn_GetReportMonths(@NumberOfMonths)
              UNION ALL
              SELECT CAST(reportyear as char(4)) + '-' + RIGHT('00' + CAST(reportmonth AS VARCHAR(2)), 2) AS [Month],
                     (completed + exceptioned) * avgworktime AS Seconds
              FROM BPMIProductivityMonthly pm
              INNER JOIN ufn_GetReportMonths(@NumberOfMonths) 
                ON TheYear = reportyear AND TheMonth = reportmonth
              INNER JOIN BPAWorkQueue wq
                ON pm.queueident = wq.ident
              WHERE wq.[name] = @QueueName) Results
        GROUP BY Results.[Month]
        ORDER BY Results.[Month] ASC
    END
return;
GO

INSERT INTO BPATile(id, [name], tiletype, [description], autorefresh, xmlproperties)
VALUES(NEWID(),
       'Hours Spent Working Queues',
       1,
       'Hours spent by Digital Workers on one or more Work Queues on a monthly basis.',
       0,
       '<Chart type="3" plotByRow="false"><Procedure name="BPDS_HoursSpentWorkingQueuesByMonth" /></Chart>');

GRANT EXECUTE ON OBJECT::BPDS_HoursSpentWorkingQueuesByMonth TO bpa_ExecuteSP_DataSource_bpSystem;

INSERT INTO BPATileDataSources(spname, tiletype, helppage)
VALUES('BPDS_HoursSpentWorkingQueuesByMonth', 1, 'HoursSpentWorkingQueuesByMonth.htm');

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('282',
       getutcdate(),
       'db_upgradeR282.sql',
       'Add dashboard tile to show hours spent working on work queues.',
       0);
