/*
SCRIPT         : 233
AUTHOR         : Ciaran / DJM
PURPOSE        : Add an internal tile datasource for license info
*/

-- Register as system data source
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_I_LicenseInformation', 1, 'LicenseInformation.htm');

-- Add new tile
insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'License Information',1,'Information about limits and usage based on the active license',0,'<Chart type="3" plotByRow="false"><Procedure name="BPDS_I_LicenseInformation" /></Chart>');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '233',
  GETUTCDATE(),
  'db_upgradeR233.sql UTC',
  'Add an internal tile datasource for license info',
  0 -- UTC
);
