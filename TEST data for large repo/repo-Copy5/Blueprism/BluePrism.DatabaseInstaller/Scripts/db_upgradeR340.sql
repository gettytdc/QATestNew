alter view BPVGroupedUsers as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    u.userid as id,
    isnull(u.username, '[' + u.systemusername + ']') as name, --this is upn for ad
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
go



-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('340',
       getutcdate(),
       'db_upgradeR340.sql',
       'Add auth type to grouped users view instead of issystemuser.',
       0);