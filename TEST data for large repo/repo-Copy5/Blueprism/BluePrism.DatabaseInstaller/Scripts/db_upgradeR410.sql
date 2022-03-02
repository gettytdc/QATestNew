/*
STORY:      BP-2577
PURPOSE:    Adds new column to the BPAUser table to store which Authentication Server user is mapped to this user
            and a new column to the BPASysConfig table that indicates which credential to use when making calls
            to the Authentication Server API as part of the user mapping
*/

alter table BPAUser
	add authenticationserveruserid int null

alter table BPASysConfig
    add authenticationserverapicredential uniqueidentifier null

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '410',
  GETUTCDATE(),
  'db_upgradeR410.sql UTC',
  'Adds new columns for Authentication Server user mapping',
  0
);
