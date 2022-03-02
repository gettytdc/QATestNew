/*
SCRIPT         : 42
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : addwebservices
CREATION DATE  : 04 Apr 2006
AUTHOR         : GMB
PURPOSE        : Adds the new BPAWebservices Table to the database
NOTES          : Documentation can be found in DFD_BPA2_001 Web Services.doc
*/

CREATE TABLE [BPAWebService] (
    [serviceid] [uniqueidentifier] NOT NULL
       constraint PK_BPAWebService primary key,
    [enabled] [bit] NOT NULL ,
    [servicename] [varchar] (128) NULL ,
    [url] [varchar] (2083) NULL ,
    [wsdl] [text] NULL ,
    [settingsXML] [text] NULL  
)
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '42',
  GETUTCDATE(),
  'db_upgradeR42.sql UTC',
  'Addition of Web Services Table'
)
GO

