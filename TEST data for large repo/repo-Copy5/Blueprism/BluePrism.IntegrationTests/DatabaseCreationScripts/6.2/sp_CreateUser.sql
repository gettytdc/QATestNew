create procedure [sp_CreateUser_6.2] 
    @id uniqueidentifier,
    @name nvarchar(128),
    @validfrom datetime,
    @validto datetime,
    @passwordDurationweeks int,
    @passwordexpiry datetime,
    @salt varchar(max),
    @hash varchar(max)
as
begin

    insert into [BPAUser] (userid, username, validfromdate, validtodate, PasswordDurationWeeks, passwordexpirydate)
    values (@id,@name,@validfrom,@validto,@passwordDurationWeeks,@passwordexpiry)

    update BPAPassword set lastuseddate = getutcdate() where lastuseddate is null and userid = @id
    update BPAPassword set active = 0 where userid = @id
    insert into BPAPassword (active, type, userid, salt, hash, lastuseddate) values (1, 1, @id, @salt, @hash, null)

end