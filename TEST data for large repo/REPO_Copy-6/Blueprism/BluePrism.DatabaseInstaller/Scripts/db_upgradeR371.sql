/*
SCRIPT         : 371
AUTHOR         : Gary Crosbie
PURPOSE        : Add German Public Holidays
*/

declare @GroupId int, @publicHolidayId int

insert into BPAPublicHolidayGroup ([name])
values ('Germany')
set @GroupId = SCOPE_IDENTITY()

select @publicHolidayId = max(id) from BPAPublicHoliday 

insert into BPAPublicHoliday (id, [name], dd, mm, [dayofweek], nthofmonth, relativetoholiday, relativedaydiff, eastersunday, excludesaturday, shiftdaytypeid)
values (@publicHolidayId + 1, 'May Day', 1, 5, null, null, null, null, null, 0, 0),   
	   (@publicHolidayId + 2, 'Day of German Unity', 3, 10, null, null, null, null, null, 0, 0)

insert into BPAPublicHolidayGroupMember (publicholidaygroupid, publicholidayid)
values (@GroupId, 54),
	   (@GroupId, 6),
	   (@GroupId, 7), 
	   (@GroupId, @publicHolidayId + 1), 
	   (@GroupId, 57), 
	   (@GroupId, 58), 
	   (@GroupId, @publicHolidayId + 2), 
	   (@GroupId, 63), 
	   (@GroupId, 68)

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('371', 
 GETUTCDATE(), 
 'db_upgradeR371.sql', 
 'Add German Public Holidays', 
 0
);