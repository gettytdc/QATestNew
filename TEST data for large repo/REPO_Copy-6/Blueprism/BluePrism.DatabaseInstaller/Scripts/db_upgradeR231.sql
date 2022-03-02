/*
SCRIPT         : 231
AUTHOR         : AMB
PURPOSE        : Adds additional process validation check for exception stage types
*/


INSERT INTO BPAValCheck
           ([checkid]
           ,[catid]
           ,[typeid]
           ,[description]
           ,[enabled])
     VALUES
           (142
           ,0 --stage validation
           ,2 --advice
           ,'Exception Type ''{0}'' not previously defined within the environment'
           ,0)
GO
    

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '231',
  GETUTCDATE(),
  'db_upgradeR231.sql UTC',
  'Adds additional process validation check for exception stage types',
   0 -- UTC
);
