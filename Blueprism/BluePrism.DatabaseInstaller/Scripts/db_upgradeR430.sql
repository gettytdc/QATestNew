/*
SCRIPT         : 430
PURPOSE        : Add hasBluePrismApiScope column to BPAUser table. Update BPVGroupedUsers view to filter out service accounts without the Blue Prism API scope.
*/

alter table BPAUser
add hasBluePrismApiScope bit not null default 0;
GO

alter view BPVGroupedUsers as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    u.userid as id,
    isnull(u.username, '[' + u.systemusername + ']') as name,
    u.authtype as authtype,
    u.validfromdate as validfrom,
    u.validtodate as validto,
    u.passwordexpirydate as passwordexpiry,
    u.lastsignedin as lastsignedin,
    u.isdeleted as isdeleted,
    u.loginattempts as loginattempts,
    c.maxloginattempts as maxloginattempts,
    ura.userroleid as roleid
  from BPAUser u
    cross join BPASysConfig c
    left join (
       BPAGroupUser gu
            inner join BPAGroup g on gu.groupid = g.id
      ) on gu.memberid = u.userid
    left join BPAUserRoleAssignment ura
      on ura.userid = u.userid
  where u.authtype <> 8 or u.hasBluePrismApiScope = 1
go

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('430', 
 GETUTCDATE(), 
 'db_upgradeR430.sql', 
 'Add hasBluePrismApiScope column to BPAUser table.  Update BPVGroupedUsers view to filter out service accounts without the Blue Prism API scope.', 
 0
);
