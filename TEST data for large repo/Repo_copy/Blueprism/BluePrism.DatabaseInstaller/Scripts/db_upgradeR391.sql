/*
SCRIPT         : 391
AUTHOR         : Rowland Hill
PURPOSE        : Add Mexico Public Holidays
*/


declare @GroupId int, @publicHolidayId int

insert into BPAPublicHolidayGroup ([name])
values ('Mexico')
set @GroupId = SCOPE_IDENTITY()

select @publicHolidayId = max(id) from BPAPublicHoliday 

insert into BPAPublicHoliday (id, [name], dd, mm, [dayofweek], nthofmonth, relativetoholiday, relativedaydiff, eastersunday, excludesaturday, shiftdaytypeid)
values (@publicHolidayId + 1, 'Day off for Constitution Day', null, 2, 1, 1, null, null, null, 0, 0),
	   (@publicHolidayId + 2, 'Constitution Day', 5, 2, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 3, 'Day off for Benito Juarez''s birthday memorial', null, 3, 1, 3, null, null, null, 0, 0),
	   (@publicHolidayId + 4, 'Benito Juarez''s birthday memorial', 21, 3, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 5, 'Labor Day', 1, 5, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 6, 'Independence Day', 16, 9, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 7, 'Day off for Revolution Day', null, 11, 1, 3, null, null, null, 0, 0),
	   (@publicHolidayId + 8, 'Revolution Day', 20, 11, null, null, null, null, null, 0, 0)

insert into BPAPublicHolidayGroupMember (publicholidaygroupid, publicholidayid)
values 
	   (@GroupId, 63),
	   (@GroupId, 54), 
	   (@GroupId, @publicHolidayId + 1),
	   (@GroupId, @publicHolidayId + 2), 
	   (@GroupId, @publicHolidayId + 3), 
	   (@GroupId, @publicHolidayId + 4), 
	   (@GroupId, @publicHolidayId + 5), 
	   (@GroupId, @publicHolidayId + 6), 
	   (@GroupId, @publicHolidayId + 7), 
	   (@GroupId, @publicHolidayId + 8)


-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('391', 
 GETUTCDATE(), 
 'db_upgradeR391.sql', 
 'Add Mexico Public Holidays', 
 0
);