/*
SCRIPT         : 217
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : AMB
PURPOSE        : Adds BPAGroupUser and creates associated view;
*/

create table BPAGroupUser (
  groupid uniqueidentifier not null
    constraint FK_BPAGroupUser_BPAGroup
      foreign key references BPAGroup(id) on delete cascade,
  memberid uniqueidentifier not null
    constraint FK_BPAGroupUser_BPAUser
      foreign key references BPAUser(userid) on delete cascade,
   constraint PK_BPAGroupUser
        primary key clustered (groupid, memberid)
);
GO

if not exists(select * from sys.views where name = 'BPVGroupedUsers')
  exec (N'create view BPVGroupedUsers as select 1 as placeholder');
GO

alter view BPVGroupedUsers as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    u.userid as id,
    isnull(u.username, '[' + u.systemusername + ']') as name, --this is upn for ad
    case when u.systemusername is not null then 1 else 0 end as issystemuser,
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
GO


INSERT INTO BPATree
           ([id]
           ,[name])
     VALUES
           (6
           ,'users');
GO



-- set DB version
INSERT INTO BPADBVersion VALUES (
  '217',
  GETUTCDATE(),
  'db_upgrade217.sql UTC',
  'Adds BPAGroupUser and creates associated view;'
);