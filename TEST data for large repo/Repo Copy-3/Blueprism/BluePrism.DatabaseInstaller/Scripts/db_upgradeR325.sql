/*
STORY      : us-4743
PURPOSE    : Stop archiving failing when attributeXML field is huge
*/




--sessionlog AttributeXml length, only read upto length else discard
if not exists (select * from BPAPref where [name] = 'SessionLogMaxAttributeXmlLength' AND userid IS NULL)
begin
    insert into BPAPref ([name]) values ('SessionLogMaxAttributeXmlLength')
end

DECLARE @prefId INT = (select TOP 1 id from BPAPref where [name] = 'SessionLogMaxAttributeXmlLength' AND userid IS NULL);

if not exists (select * from BPAIntegerPref where prefid = @prefId)
begin
    insert into BPAIntegerPref (prefid, [value])
    values (@prefId, 2000000000)
end


if not exists (select * from BPADataTracker where dataname = 'Preferences')
begin
    insert into BPADataTracker (dataname, versionno)
    values ('Preferences', 1)
end




--====================================================================================================================


-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('325',
       getutcdate(),
       'db_upgradeR325sql',
       'Stop archiving failing when attributeXML huge and when huge numbers of sessionlogs',
       0);
go
