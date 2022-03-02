if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_System') is not null
begin
    grant execute on object::usp_GetCacheETag to bpa_ExecuteSP_System;
    grant execute on object::usp_SetCacheETag to bpa_ExecuteSP_System;
end


-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('266',
       getutcdate(),
       'db_upgradeR266.sql',
       'Give usp_cacheETag procedures execute permissions.',
       0);
