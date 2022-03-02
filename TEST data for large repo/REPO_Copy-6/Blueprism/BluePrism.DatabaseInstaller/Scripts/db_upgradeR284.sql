
if (select InstallInProgress from BPVScriptEnvironment) = 1
    update BPAMIControl set
        mienabled=1,
        autorefresh=1,
        refreshat=DATEADD(HOUR, 1, CAST(FLOOR(CAST(GETDATE() as float)) as datetime));

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('284',
       getutcdate(),
       'db_upgradeR284.sql',
       'Enable MI reporting by default for new installations.',
       0);
