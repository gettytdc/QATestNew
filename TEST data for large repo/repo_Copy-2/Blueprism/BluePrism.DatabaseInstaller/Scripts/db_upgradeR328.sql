/*
SCRIPT         : 328
AUTHOR         : DPoole
PURPOSE        : Rename any duplicate published dashboards. Add a new index to BPAAuditEvents to improve process history query performance 
*/

-- Row over each published dashboard and append the uniqueno to the name
update BPADashboard 
 set name = name + 
 ( 
 select case when s.uniqueno = 1 then '' else ' (' + cast(uniqueno-1 as varchar) + ')' end from 
 ( 
 select id, dashtype, name, row_number() over(partition by upper(rtrim(ltrim(name))) order by lastsent) as uniqueno 
 from BPADashboard 
 ) s 
 where BPADashboard.id = s.id 
 ) 
 where dashtype = 2

-- Add index
create index IX_BPAAuditEvents_gTgtProcID on BPAAuditEvents (gTgtProcID)

-- Set DB version
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('328',
       getutcdate(),
       'db_upgradeR328.sql',
       'Rename published dashboards. Add index to BPAAuditEvents',
       0);
 