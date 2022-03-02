/*
SCRIPT         : 102
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Change the default report & timetable to show more useful data.
*/


-- Recent Activity report     - "Show [all schedules] for 2 days ending Today" 
-- Today & Tomorrow timetable - "Show [all schedules] for 2 days starting Today" 
update BPAScheduleList 
    set relativedate = 1, daysdistance = 2, allschedules = 1, absolutedate = NULL
    where  (listtype = 1 and name = 'Recent Activity') 
        or (listtype = 2 and name = 'Today & Tomorrow');
        
-- set DB version
INSERT INTO BPADBVersion VALUES (
  '102',
  GETUTCDATE(),
  'db_upgradeR102.sql UTC',
  'Change the default report & timetable to show more useful data'
);
