/*
SCRIPT         : 163
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Corrects names of public holidays
*/

-- A couple of simple name changes
update BPAPublicHoliday set name='New Year''s Day' where id=3;
update BPAPublicHoliday set name='Early May Bank Holiday' where id=8;

-- A bit more akward - ROI doesn't have 'Boxing Day' - it has St Stephen's Day
-- so we need to create a new one
insert into BPAPublicHoliday
    (id, name, relativetoholiday, relativedaydiff)
    values (19, 'St Stephen''s Day', 2, 1);

-- Then we need to drop Boxing Day (id:18) from ROI's public holiday group
-- and add St Stephens Day (id:19)
declare @eire int
set @eire = (select id from BPAPublicHolidayGroup where name = 'Republic of Ireland')
delete from BPAPublicHolidayGroupMember where publicholidaygroupid = @eire and publicholidayid = 18;
insert into BPAPublicHolidayGroupMember (publicholidaygroupid, publicholidayid) values (@eire, 19);

-- Finally we need to transfer any overrides set for ROI on Boxing Day to the new bank holiday
update wd
  set wd.publicholidayid = 19
  from BPAPublicHolidayWorkingDay wd
    join BPACalendar c on wd.calendarid = c.id
  where c.publicholidaygroupid = @eire
    and wd.publicholidayid = 18;


--set DB version
insert into BPADBVersion values (
  '163',
  GETUTCDATE(),
  'db_upgradeR163.sql UTC',
  'Corrects names of public holidays'
);
