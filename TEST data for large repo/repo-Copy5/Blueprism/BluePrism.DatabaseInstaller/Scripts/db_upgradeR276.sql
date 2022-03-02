------------------------------- WARNING 09/11/2018 -------------------------------------
-- This upgrade script has been moved into clsServerSchedule.GetScheduleFromDatabase()
-- It has been left in the upgrade process to avoid confusion.
----------------------------------------------------------------------------------------

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('276',
       getutcdate(),
       'db_upgradeR276.sql',
       'Add usp_GetSchedules stored procedure.',
       0);