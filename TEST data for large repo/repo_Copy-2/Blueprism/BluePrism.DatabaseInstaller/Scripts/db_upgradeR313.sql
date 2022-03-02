IF NOT EXISTS ( SELECT 1
                FROM INFORMATION_SCHEMA.columns
                WHERE table_name = 'BPAPublicHoliday' and column_name = 'excludesaturday')
BEGIN
    ALTER TABLE BPAPublicHoliday ADD excludesaturday BIT NULL;
END
GO

UPDATE h
SET h.excludesaturday = 1
FROM BPAPublicHoliday h 
left join BPAPublicHolidayGroupMember m on m.publicholidayid = h.id 
left join BPAPublicHolidayGroup g on m.publicholidaygroupid = g.id
WHERE g.[Name] = 'Hong Kong';

UPDATE h
SET h.excludesaturday = 0
FROM BPAPublicHoliday h 
left join BPAPublicHolidayGroupMember m on m.publicholidayid = h.id 
left join BPAPublicHolidayGroup g on m.publicholidaygroupid = g.id
WHERE g.[Name] IS NULL OR g.[Name] <> 'Hong Kong';

ALTER TABLE BPAPublicHoliday ALTER COLUMN excludesaturday bit NOT NULL; 

UPDATE h
SET h.excludesaturday = 1
FROM BPAPublicHoliday h 
left join BPAPublicHolidayGroupMember m on m.publicholidayid = h.id 
left join BPAPublicHolidayGroup g on m.publicholidaygroupid = g.id
WHERE g.[Name] = 'Japan';


IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'BPAPublicHolidayShiftDayTypes')
BEGIN
    CREATE TABLE BPAPublicHolidayShiftDayTypes (
        [id] int not null,
        [name] varchar(256) not null,
        CONSTRAINT PK_BPAPublicHolidayShiftDayTypes PRIMARY KEY ([id]),
        CONSTRAINT IX_BPAPublicHolidayShiftDayTypes_Name UNIQUE ([name])
    )
END

IF NOT EXISTS (SELECT 1 FROM [BPAPublicHolidayShiftDayTypes] WHERE [Name] = 'ShiftForward')
BEGIN
    INSERT INTO [BPAPublicHolidayShiftDayTypes] ([id], [name])
    VALUES (1, 'ShiftForward');
END
GO

IF NOT EXISTS (SELECT 1 FROM [BPAPublicHolidayShiftDayTypes] WHERE [Name] = 'ShiftBackwardOrForward')
BEGIN
    INSERT INTO [BPAPublicHolidayShiftDayTypes] ([id], [name])
    VALUES (2, 'ShiftBackwardOrForward');
END
GO

IF NOT EXISTS ( select 1
                from INFORMATION_SCHEMA.columns
                where table_name = 'BPAPublicHoliday' and column_name = 'shiftdaytypeid')
BEGIN
    ALTER TABLE BPAPublicHoliday add shiftdaytypeid int NULL;
END
GO

IF(OBJECT_ID('FK_BPAPublicHoliday_BPAPublicHolidayShiftDayTypes', 'F') IS NULL)
ALTER TABLE [BPAPublicHoliday] WITH CHECK ADD CONSTRAINT [FK_BPAPublicHoliday_BPAPublicHolidayShiftDayTypes] 
FOREIGN KEY(shiftdaytypeid) 
REFERENCES [BPAPublicHolidayShiftDayTypes]([id]);

UPDATE h 
SET h.shiftdaytypeid = 1 
FROM BPAPublicHoliday h
left join BPAPublicHolidayGroupMember m on m.publicholidayid = h.id 
left join BPAPublicHolidayGroup g on m.publicholidaygroupid = g.id
WHERE g.[name] IS NULL OR g.[name] <> 'USA';

UPDATE h 
SET h.shiftdaytypeid = 2 
FROM BPAPublicHoliday h
left join BPAPublicHolidayGroupMember m on m.publicholidayid = h.id 
left join BPAPublicHolidayGroup g on m.publicholidaygroupid = g.id
WHERE g.[name] = 'USA';

ALTER TABLE BPAPublicHoliday ALTER COLUMN shiftdaytypeid int NOT NULL; 

UPDATE BPAPublicHoliday
SET dd = null, dayofweek = 1, nthofmonth = 2
WHERE [name] = 'Columbus Day';

DELETE hgm
FROM BPAPublicHolidayGroupMember hgm
INNER JOIN BPAPublicHolidayGroup hg on hgm.publicholidaygroupid = hg.id
INNER JOIN BPAPublicHoliday h on hgm.publicholidayid = h.id
WHERE hg.[name] = 'China' and h.[name] = 'National Day Golden Week holiday';

DELETE FROM BPAPublicHoliday
WHERE [name] = 'National Day Golden Week holiday';

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('313',
       getutcdate(),
       'db_upgradeR313.sql',
       'Add additional holiday related columns to handle new countries',
       0);
