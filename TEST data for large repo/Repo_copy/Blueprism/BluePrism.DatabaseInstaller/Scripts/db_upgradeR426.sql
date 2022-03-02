/*
SCRIPT         : 426
AUTHOR         : Nathan Parkinson
PURPOSE        : Add MTE to BP API session methods
*/


IF NOT EXISTS(SELECT * FROM sys.[types] AS [t2] WHERE [t2].[name]='IntIdTableType')
	CREATE type IntIdTableType as table (id int PRIMARY KEY NOT NULL);

go



if object_id(N'ufn_UserRolePermExists', N'FN') IS NOT NULL
    drop function ufn_UserRolePermExists
go
create function [ufn_UserRolePermExists]
(
    @permissions as IntIdTableType readonly,
    @userroles as IntIdTableType readonly
)
returns bit
-- Indicates whether any permission in set is related to any user role
as
begin
    return case
        when exists
        (
            select 1
            from BPAUserRolePerm urp
            inner join BPAUserRole ur on ur.id = urp.userroleid
            where exists (select 1 from @userroles r where r.id = ur.id) 
                and exists (select 1 from @permissions p where p.id = urp.permid)                
        )
        then 1
        else 0
    end
    
end
go




if object_id(N'ufn_GetProcessesWithPermissionOnRole', N'TF') IS NOT NULL
    drop function [ufn_GetProcessesWithPermissionOnRole]
go
create function [ufn_GetProcessesWithPermissionOnRole]
(
    @permissions as IntIdTableType readonly,
    @userroles as IntIdTableType readonly
)
returns @result table(
    id uniqueidentifier not null primary key
)
as
begin
    -- Check if permission assigned to role (used if not restricted at group level)
    declare @hasPermissionIfUnrestricted bit
    exec @hasPermissionIfUnrestricted = ufn_UserRolePermExists @permissions, @userroles;

    with restrictedprocessgroup as 
    (
        select 
            g.id as groupid,
            case g.isrestricted when 1 then g.id else null end as restrictedgroupid
        from BPAGroup g
        where treeid=2
            and not exists(select 1 from BPAGroupGroup gg where gg.memberid = g.id)
        union all
        select 
            g.id as groupid,
            case g.isrestricted when 1 then g.id else parent.restrictedgroupid end as restrictedgroupid
        from BPAGroup g
        inner join BPAGroupGroup gg on gg.memberid = g.id
        inner join restrictedprocessgroup parent on gg.groupid = parent.groupid
    )
    insert @result(id)
    select distinct gp.processid from BPAGroupProcess gp
    inner join restrictedprocessgroup rpg on rpg.groupid = gp.groupid
    where 
    (
        rpg.restrictedgroupid is null and @hasPermissionIfUnrestricted = 1
    )
    or
    (
        rpg.restrictedgroupid is not null
        and exists 
        (
            select 1 
            from BPAGroupUserRolePerm gurp 
            where gurp.groupid = rpg.restrictedgroupid
   				and exists(select 1 from @permissions p where p.[id]=[gurp].[permid])
				and exists(select 1 from @userroles p where p.[id]=[gurp].userroleid)
        )
    )    
    return;
end
go

if object_id(N'ufn_GetResourcesWithPermissionOnRole', N'TF') IS NOT NULL
    drop function [ufn_GetResourcesWithPermissionOnRole]
go

create function [ufn_GetResourcesWithPermissionOnRole]
(
    @permissions as IntIdTableType readonly,
    @userroles as IntIdTableType readonly
)
returns @result table(
    id uniqueidentifier not null primary key
)
as
begin
    declare @hasPermissionIfUnrestricted bit
    exec @hasPermissionIfUnrestricted = ufn_UserRolePermExists @permissions, @userroles;

    with restrictedresourcegroup as 
    (
        select 
            g.id as groupid,
            case g.isrestricted when 1 then g.id else null end as restrictedgroupid
        from BPAGroup g
        where treeid=5 
            and not exists(select 1 from BPAGroupGroup gg where gg.memberid = g.id)
        union all
        select 
            g.id as groupid,
            case g.isrestricted when 1 then g.id else parent.restrictedgroupid end as restrictedgroupid
        from BPAGroup g
        inner join BPAGroupGroup gg on gg.memberid = g.id
        inner join restrictedresourcegroup parent on gg.groupid = parent.groupid
    )
    insert @result(id)
    select distinct gr.memberid from BPAGroupResource gr
    inner join restrictedresourcegroup rrg on rrg.groupid = gr.groupid
    where 
    (
        rrg.restrictedgroupid is null and @hasPermissionIfUnrestricted = 1
    )
    or
    (
        rrg.restrictedgroupid is not null        
        and exists 
        (
            select 1 
            from BPAGroupUserRolePerm gurp 
            where gurp.groupid = rrg.restrictedgroupid
				and exists(select 1 from @permissions p where p.[id]=[gurp].[permid])
				and exists(select 1 from @userroles p where p.[id]=[gurp].userroleid)
        )
    )    
    return;
end
go


insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('426', 
 GETUTCDATE(), 
 'db_upgradeR426.sql', 
 'Add MTE to BP API session methods', 
 0
);
