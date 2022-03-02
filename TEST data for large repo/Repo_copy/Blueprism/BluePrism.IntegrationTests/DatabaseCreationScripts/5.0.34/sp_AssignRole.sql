create procedure [sp_AssignRole_5.0.34]
    @userId uniqueidentifier,
    @roleId int
as
begin

    delete from [BPAUserRoleAssignment] where userroleid = @roleId and userid = @userId

    insert into [BPAUserRoleAssignment] (userid, userroleid) values (@userId, @roleId)
end
