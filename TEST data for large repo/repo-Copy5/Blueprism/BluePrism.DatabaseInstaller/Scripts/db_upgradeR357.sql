/*
SCRIPT         : 357
AUTHOR         : William Forster
PURPOSE        : Add Canada Public Holidays
*/

if not exists ( select 1
                from INFORMATION_SCHEMA.columns
                where table_name = 'BPAPublicHoliday' and column_name = 'relativedayofweek')
begin
    alter table BPAPublicHoliday add relativedayofweek smallint null;
end
go

declare @GroupId int, @publicHolidayId int

insert into BPAPublicHolidayGroup ([name])
values ('Canada')
set @GroupId = SCOPE_IDENTITY()

select @publicHolidayId = max(id) from BPAPublicHoliday 

insert into BPAPublicHoliday (id, [name], dd, mm, [dayofweek], nthofmonth, relativetoholiday, relativedaydiff, eastersunday, excludesaturday, shiftdaytypeid, relativedayofweek)
values (@publicHolidayId + 1, 'Canada Day', 1, 7, null, null, null, null, null, 1, 1, null),
	   (@publicHolidayId + 2, 'Thanksgiving Day', null, 10, 1, 2, null, null, null, 0, 1, null),
	   (@publicHolidayId + 3, 'Remembrance Day', 11, 11, null, null, null, null, null, 1, 1, null),
	   (@publicHolidayId + 4, 'Boxing Day', 26, 12, null, null, null, null, null, 0, 0, null),
	   (@publicHolidayId + 5, 'Current UK Monarch''s Birthday', 25, 5, null, null, null, null, null, 0, 0, null),
	   (@publicHolidayId + 6, 'Victoria Day', null, null, null, null, @publicHolidayId + 5, null, null, 0, 0, -1)


insert into BPAPublicHolidayGroupMember (publicholidaygroupid, publicholidayid)
values (@GroupId, 3),
	   (@GroupId, 6),
	   (@GroupId, 7), 
	   (@GroupId, @publicHolidayId + 1),
	   (@Groupid, 25),
	   (@GroupId, @publicHolidayId + 2),
	   (@GroupId, @publicHolidayId + 3),
	   (@Groupid, 29),
	   (@GroupId, @publicHolidayId + 4),
	   (@GroupId, @publicHolidayId + 6)

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('357', 
 GETUTCDATE(), 
 'db_upgradeR356.sql', 
 'Add Canada Public Holidays', 
 0
);