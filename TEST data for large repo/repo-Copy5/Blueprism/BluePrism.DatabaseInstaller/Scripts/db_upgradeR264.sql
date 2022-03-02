-- Upgrade script.
INSERT INTO BPAValCheck(checkid, 
                        catid, 
                        typeid, 
                        [description], 
                        [enabled])
VALUES(146, 
       1, 
       0, 
       'The Skill ''{0}'' is not installed or is unavailable.', 
       1);

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('264',
       getutcdate(),
       'db_upgradeR264.sql',
       'Add validation message for clsSkillStage.',
       0);