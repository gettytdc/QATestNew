/*
SCRIPT         : 354
AUTHOR         : William Forster
PURPOSE        : Add French Public Holidays
*/

if not exists (select 1 from [BPAPublicHolidayShiftDayTypes] where [Name] = 'NoShift')
begin
    insert into [BPAPublicHolidayShiftDayTypes] ([id], [name])
    values (0, 'NoShift');
end

declare @GroupId int, @publicHolidayId int

insert into BPAPublicHolidayGroup ([name])
values ('France')
set @GroupId = SCOPE_IDENTITY()

select @publicHolidayId = max(id) from BPAPublicHoliday 

insert into BPAPublicHoliday (id, [name], dd, mm, [dayofweek], nthofmonth, relativetoholiday, relativedaydiff, eastersunday, excludesaturday, shiftdaytypeid)
values (@publicHolidayId + 1, 'New Years Day', 1, 1, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 2, 'May Day/Labour Day', 1, 5, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 3, 'Victory in Europe Day', 8, 5, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 4, 'Ascension Day', null, null, null, null, 1, 39, null, 0, 1),
	   (@publicHolidayId + 5, 'Whit Monday', null, null, null, null, 1, 50, null, 0, 1),
	   (@publicHolidayId + 6, 'Bastille Day', 14, 7, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 7, 'Assumption of Mary to Heaven', 15, 8, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 8, 'All Saints'' Day', 1, 11, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 9, 'Armistance Day', 11, 11, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 10, 'Christmas Day', 25, 12, null, null, null, null, null, 0, 0),
	   (@publicHolidayId + 11, 'St Stephen''s Day', 26, 12, null, null, null, null, null, 0, 0)

insert into BPAPublicHolidayGroupMember (publicholidaygroupid, publicholidayid)
values (@GroupId, @publicHolidayId + 1),
	   (@GroupId, 6),
	   (@GroupId, 7), 
	   (@GroupId, @publicHolidayId + 2), 
	   (@GroupId, @publicHolidayId + 3), 
	   (@GroupId, @publicHolidayId + 4), 
	   (@GroupId, @publicHolidayId + 5), 
	   (@GroupId, @publicHolidayId + 6), 
	   (@GroupId, @publicHolidayId + 7), 
	   (@GroupId, @publicHolidayId + 8), 
	   (@GroupId, @publicHolidayId + 9), 
	   (@GroupId, @publicHolidayId + 10),
	   (@GroupId, @publicHolidayId + 11)

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('354', 
 GETUTCDATE(), 
 'db_upgradeR354.sql', 
 'Add French Public Holidays', 
 0
);