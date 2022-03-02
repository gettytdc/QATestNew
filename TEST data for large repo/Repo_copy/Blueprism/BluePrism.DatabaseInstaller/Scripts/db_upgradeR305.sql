/*
SCRIPT         : 305
PROJECT NAME   : Automate
AUTHOR         : Craig M
PURPOSE        : public holidays for USA, Japan, Hongkong and China
*/
DECLARE @newGroupId int
DECLARE @publicHolidayId int =0

SELECT @publicHolidayId = max(id) FROM BPAPublicHoliday  

-- USA Holidays
    
    INSERT INTO BPAPublicHolidayGroup (name) VALUES ('USA')
    SET @newGroupId = SCOPE_IDENTITY()  
    INSERT INTO BPAPublicHoliday (id, name,dd,mm,dayofweek,nthofmonth,relativetoholiday ,relativedaydiff,eastersunday)  
        SELECT @publicHolidayId + 1, 'New Year''s Day',1,1,null,null,null,null,null
            UNION ALL
        SELECT @publicHolidayId + 2, 'Martin Luther King Jr. Day',null,1,1,3,null,null,null
            UNION ALL
        SELECT @publicHolidayId + 3, 'Presidents'' Day',null,2,1,3,null,null,null
            UNION ALL
        SELECT @publicHolidayId + 4, 'Memorial Day',null,5,1,-1,null,null,null
            UNION ALL
        SELECT @publicHolidayId + 5, 'Independence Day',4,7,null,null,null,null,null
                UNION ALL
        SELECT @publicHolidayId + 6, 'Labor Day',null,9,1,1,null,null,null
                UNION ALL
        SELECT @publicHolidayId + 7, 'Columbus Day',14,10,null,null,null,null,null
                UNION ALL
        SELECT @publicHolidayId + 8, 'Veterans Day',11,11,null,null,null,null,null
                UNION ALL
        SELECT @publicHolidayId + 9, 'Thanksgiving Day',null,11,4,4,null,null,null
                UNION ALL
        SELECT @publicHolidayId + 10, 'Christmas Day',25,12,null,null,null,null,null        
    
    --Associate the holidays with the BPAPublicHolidayGroup record  
    INSERT INTO BPAPublicHolidayGroupMember(publicholidaygroupid, publicholidayid) 
    
    SELECT @newGroupId  , @publicHolidayId + 1
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 2
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 3
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 4
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 5
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 6
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 7
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 8
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 9
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 10
    
    --Japan
    INSERT INTO BPAPublicHolidayGroup (name) VALUES ('Japan')
    SET @newGroupId = SCOPE_IDENTITY()  

    INSERT INTO BPAPublicHoliday (id, name,dd,mm,dayofweek,nthofmonth,relativetoholiday ,relativedaydiff,eastersunday) 
    SELECT @publicHolidayId + 11, 'New Year''s Day',1,1,null,null,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 12,'January 2 Bank Holiday',null,null,null,null,@publicHolidayId + 11,1,null
        UNION ALL
    SELECT @publicHolidayId + 13,'January 3 Bank Holiday',null,null,null,null,@publicHolidayId + 12,1,null
        UNION ALL
    SELECT @publicHolidayId + 14,'Coming of Age Day',null,1,1,2,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 15,'National Foundation Day',11,2,null,null,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 16,'Shōwa',29,4,null,null,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 17,'Constitution Memorial Day',3,5,null,null,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 18,'Greenery Day',null,null,null,null,@publicHolidayId + 17,1,null
        UNION ALL
    SELECT @publicHolidayId + 19,'Children''s Day',null,null,null,null,@publicHolidayId + 18,1,null
        UNION ALL
    SELECT @publicHolidayId + 20,'Marine Day',null,7,1,3,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 21,'Mountain Day',11,8,null,null,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 22,'Respect for the Aged Day',null,9,1,3,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 23,'Health and Sports Day',null,10,1,2,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 24,'Culture Day',3,11,null,null,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 25,'Labor Thanksgiving Day',23,11,null,null,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 26,'December 31 Bank Holiday',31,12,null,null,null,null,null
    

    INSERT INTO BPAPublicHolidayGroupMember(publicholidaygroupid, publicholidayid) 
    SELECT @newGroupId  , @publicHolidayId + 11
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 12
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 13
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 14
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 15
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 16
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 17
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 18
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 19
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 20
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 21
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 22
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 23
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 24
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 25
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 26

    --Hong Kong
    INSERT INTO BPAPublicHolidayGroup (name) VALUES ('Hong Kong')
    SET @newGroupId = SCOPE_IDENTITY()  

    INSERT INTO BPAPublicHoliday (id, name,dd,mm,dayofweek,nthofmonth,relativetoholiday ,relativedaydiff,eastersunday) 
    SELECT @publicHolidayId + 27, 'New Year''s Day',1,1,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 28, 'Hong Kong Special Administrative Region Establishment Day',1,7,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 29, 'National Day',1,10,null,null,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 30, 'Christmas Day',25,12,null,null,null,null,null
        UNION ALL
    SELECT @publicHolidayId + 31, 'Boxing Day',null,null,null,null,@publicHolidayId + 30,1,null

    INSERT INTO BPAPublicHolidayGroupMember(publicholidaygroupid, publicholidayid) 
    SELECT @newGroupId  , @publicHolidayId + 27
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 28
        UNION ALL   
    SELECT @newGroupId  , @publicHolidayId + 29
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 30
        UNION ALL   
    SELECT @newGroupId  , @publicHolidayId + 31

    --China
    INSERT INTO BPAPublicHolidayGroup (name) VALUES ('China')
    SET @newGroupId = SCOPE_IDENTITY()  

    INSERT INTO BPAPublicHoliday (id, name,dd,mm,dayofweek,nthofmonth,relativetoholiday ,relativedaydiff,eastersunday) 
    SELECT @publicHolidayId + 32, 'New Year''s Day',1,1,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 33,'Labor Day',1,5,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 34,'National Day',1,10,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 35,'National Day Golden Week holiday',2,10,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 36,'National Day Golden Week holiday',3,10,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 37,'National Day Golden Week holiday',4,10,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 38,'National Day Golden Week holiday',5,10,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 39,'National Day Golden Week holiday',6,10,null,null,null,null,null
        UNION ALL
    SELECT  @publicHolidayId + 40,'National Day Golden Week holiday',7,10,null,null,null,null,null

    INSERT INTO BPAPublicHolidayGroupMember(publicholidaygroupid, publicholidayid) 
    SELECT @newGroupId  , @publicHolidayId + 32
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 33
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 34
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 35
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 36
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 37
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 38
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 39
        UNION ALL
    SELECT @newGroupId  , @publicHolidayId + 40


    -- Set DB version.
insert into BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('305',
        getutcdate(),
        'db_upgradeR305.sql',
        'Added public holidays for USA, China, Hong Kong and Japan.',
        0);


