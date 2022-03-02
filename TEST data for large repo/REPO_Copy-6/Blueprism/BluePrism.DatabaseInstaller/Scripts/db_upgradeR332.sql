/*
SCRIPT         : 332
AUTHOR         : John Brown
PURPOSE        : Removal of BPAPermScope and BPAScope for cleanup purposes
*/

IF OBJECT_ID('BPAPermScope', 'U') IS NOT NULL 
  DROP TABLE BPAPermScope; 

IF OBJECT_ID('BPAScope', 'U') IS NOT NULL 
  DROP TABLE BPAScope; 

-- Set DB version.
INSERT INTO BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
VALUES
('332', 
 GETUTCDATE(), 
 'db_upgradeR332.sql', 
 'Removal of BPAPermScope and BPAScope for cleanup purposes.', 
 0
);