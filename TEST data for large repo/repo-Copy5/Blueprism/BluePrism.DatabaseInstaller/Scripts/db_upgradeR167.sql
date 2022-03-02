/*
SCRIPT         : 167
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 05 Mar 2014
AUTHOR         : GB
PURPOSE        : Add BPAWebServiceAsset table.
NOTES          : 
*/

create table BPAWebServiceAsset (
    serviceid uniqueidentifier not null
        constraint FK_BPAWebServiceAsset_BPAWebService
            foreign key references BPAWebService(serviceid)
            on delete cascade,
    assettype tinyint not null,
    assetxml ntext null
)
GO

create clustered index Index_BPAWebServiceAsset_serviceid_assettype
  on BPAWebServiceAsset (serviceid, assettype)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '167',
  GETUTCDATE(),
  'db_upgradeR167.sql UTC',
  'Add BPAWebServiceAsset table'
)

