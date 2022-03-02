/*
SCRIPT         : 10
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Create base version of Blue Prism Database.
*/

/************************************************************************************
/
/   Drop all the database objects we are going to create. This allows the database
/   to be rebuilt from scratch without dropping it first. The order of operations
/   is important here - for example, tables with FK constraints referencing other
/   tables must be dropped before the tables which are the target of the reference.
/   In the case of a circular reference, the constraints themselves must be dropped
/   before the tables.
/
/   Everything here checks for the existence of the object before attempting to
/   drop it, since this script must also work on a completely empty database.
/
************************************************************************************/

-- Roles

if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_System') is not null
    DROP ROLE bpa_ExecuteSP_System;

if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_DataSource_bpSystem') is not null
    DROP ROLE bpa_ExecuteSP_DataSource_bpSystem;

if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_DataSource_custom') is not null
    DROP ROLE bpa_ExecuteSP_DataSource_custom;

-- Foreign Keys

IF object_id('FK_BPAProcess_DefaultRealTimeStatsView') IS NOT NULL
    ALTER TABLE BPAProcess DROP CONSTRAINT FK_BPAProcess_DefaultRealTimeStatsView
IF object_id('FK_BPAProcess') IS NOT NULL
    ALTER TABLE [BPARealTimeStatsView] DROP CONSTRAINT FK_BPAProcess
IF object_id('fk_bpaoldpassword_bpauser') IS NOT NULL
    ALTER TABLE BPAOldPassword DROP CONSTRAINT fk_bpaoldpassword_bpauser
IF object_id('FK_BPAPassword_BPAUser') IS NOT NULL
    ALTER TABLE BPAPassword DROP CONSTRAINT FK_BPAPassword_BPAUser

IF object_id('FK_BPMIConfiguredSnapshot_BPAWorkQueue') IS NOT NULL
    ALTER TABLE BPMIConfiguredSnapshot DROP CONSTRAINT FK_BPMIConfiguredSnapshot_BPAWorkQueue
IF object_id('FK_BPMIQueueSnapshot_BPAWorkQueue') IS NOT NULL
    ALTER TABLE BPMIQueueSnapshot DROP CONSTRAINT FK_BPMIQueueSnapshot_BPAWorkQueue
IF object_id('FK_BPMIQueueTrend_BPAWorkQueue') IS NOT NULL
    ALTER TABLE BPMIQueueTrend DROP CONSTRAINT FK_BPMIQueueTrend_BPAWorkQueue   
IF object_id('FK_BPMIQueueInterimSnapshot_BPAWorkQueue') IS NOT NULL
    ALTER TABLE BPMIQueueInterimSnapshot DROP CONSTRAINT FK_BPMIQueueInterimSnapshot_BPAWorkQueue
IF object_id('FK_BPMIConfiguredSnapshot_BPMISnapshotTrigger') IS NOT NULL
    ALTER TABLE BPMISnapshotTrigger DROP CONSTRAINT FK_BPMIConfiguredSnapshot_BPMISnapshotTrigger

IF object_id('FK_BPASessionLog_Unicode_BPASession_pre65') IS NOT NULL
    ALTER TABLE BPASessionLog_Unicode_pre65 DROP CONSTRAINT FK_BPASessionLog_Unicode_BPASession_pre65

IF object_id('FK_BPASessionLog_NonUnicode_BPASession_pre65') IS NOT NULL
    ALTER TABLE BPASessionLog_NonUnicode_pre65 DROP CONSTRAINT FK_BPASessionLog_NonUnicode_BPASession_pre65

IF object_id('FK_BPAUserExternalIdentity_BPAUser') IS NOT NULL
    ALTER TABLE BPAUserExternalIdentity DROP CONSTRAINT FK_BPAUserExternalIdentity_BPAUser

IF object_id('FK_BPAExternalProvider_BPAExternalProviderType') IS NOT NULL
    ALTER TABLE BPAExternalProvider DROP CONSTRAINT FK_BPAExternalProvider_BPAExternalProviderType

IF object_id('FK_BPAUserExternalIdentity_BPAExternalProvider') IS NOT NULL
    ALTER TABLE BPAUserExternalIdentity DROP CONSTRAINT FK_BPAUserExternalIdentity_BPAExternalProvider

IF object_id('fk_BPAEnvironment_BPAEnvironmentType') IS NOT NULL
    ALTER TABLE BPAEnvironment DROP CONSTRAINT fk_BPAEnvironment_BPAEnvironmentType

IF object_id('FK_BPAMappedActiveDirectoryUser_BPAUser') IS NOT NULL
    ALTER TABLE BPAMappedActiveDirectoryUser DROP CONSTRAINT FK_BPAMappedActiveDirectoryUser_BPAUser

IF object_id('FK_BPAWorkQueueItemAggregate_BPAWorkQueue') IS NOT NULL
    ALTER TABLE BPAWorkQueueItemAggregate DROP CONSTRAINT FK_BPAWorkQueueItemAggregate_BPAWorkQueue
        
IF object_id('FK_BPAUserExternalReloginToken_BPAUser') IS NOT NULL
    ALTER TABLE BPAUserExternalReloginToken DROP CONSTRAINT FK_BPAUserExternalReloginToken_BPAUser

-- Views

IF  EXISTS (SELECT * FROM sysobjects where type='V' and id = OBJECT_ID(N'[vw_Audit]'))
DROP VIEW [vw_Audit]

IF  EXISTS (SELECT * FROM sysobjects where type='V' and id = OBJECT_ID(N'[vw_Audit_improved]'))
DROP VIEW [vw_Audit_improved]

IF  EXISTS (SELECT * FROM sysobjects where type='V' and id = OBJECT_ID(N'[vwBPACalendar]'))
DROP VIEW [vwBPACalendar]

IF  EXISTS (SELECT * FROM sysobjects where type='V' and id = OBJECT_ID(N'[vwBPAUptime]'))
DROP VIEW [vwBPAUptime]

if exists (select * from sysobjects where type='V' and id = object_id('BPVWorkQueueItem'))
    drop view BPVWorkQueueItem;

If object_id('BPViewWorkQueueItemTag') is not null
    drop view BPViewWorkQueueItemTag

If object_id('BPViewWorkQueueItemTagBare') is not null
    drop VIEW BPViewWorkQueueItemTagBare

if object_id('BPVSessionInfo') is not null
    drop view BPVSessionInfo;

if object_id('BPVSession') is not null
    drop view BPVSession;

if object_id('BPVAnnotatedScheduleLog') is not null
    drop view BPVAnnotatedScheduleLog;

if object_id('BPVScriptEnvironment') is not null
    drop view BPVScriptEnvironment;

-- Functions
if OBJECT_ID('ufn_GetReportDays') is not null
    drop function ufn_GetReportDays;

if OBJECT_ID('ufn_GetReportMonths') is not null
    drop function ufn_GetReportMonths;

if OBJECT_ID('ufn_GetQueueEvents') is not null
    drop function ufn_GetQueueEvents;

-- Stored Procedures
IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[spBPAUpdateCalendar]') AND type in (N'P', N'PC'))
DROP PROCEDURE [spBPAUpdateCalendar]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[spBPAUptimeMonth]') AND type in (N'P', N'PC'))
DROP PROCEDURE [spBPAUptimeMonth]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_RefreshProductivityData]') AND type = N'P')
DROP PROCEDURE [usp_RefreshProductivityData]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_RefreshUtilisationData]') AND type = N'P')
DROP PROCEDURE [usp_RefreshUtilisationData]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_RefreshMI]') AND type = N'P')
DROP PROCEDURE [usp_RefreshMI]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_CreateInterimQueueSnapshot]') AND type = N'P')
DROP PROCEDURE [usp_CreateInterimQueueSnapshot]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_CreateFirstQueueSnapshot]') AND type = N'P')
DROP PROCEDURE [usp_CreateFirstQueueSnapshot]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_CreateNextQueueSnapshot]') AND type = N'P')
DROP PROCEDURE [usp_CreateNextQueueSnapshot]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_CalculateQueueTrends]') AND type = N'P')
DROP PROCEDURE [usp_CalculateQueueTrends]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_TriggerQueueSnapshot]') AND type = N'P')
DROP PROCEDURE [usp_TriggerQueueSnapshot]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_WorkforceAvailability]') AND type = N'P')
DROP PROCEDURE [BPDS_WorkforceAvailability]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_TotalAutomations]') AND type = N'P')
DROP PROCEDURE [BPDS_TotalAutomations]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_LargestTables]') AND type = N'P')
DROP PROCEDURE [BPDS_LargestTables]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_DailyUtilisation]') AND type = N'P')
DROP PROCEDURE [BPDS_DailyUtilisation]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_ProcessUtilisationByHour]') AND type = N'P')
DROP PROCEDURE [BPDS_ProcessUtilisationByHour]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_ResourceUtilisationByHour]') AND type = N'P')
DROP PROCEDURE [BPDS_ResourceUtilisationByHour]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_AverageHandlingTime]') AND type = N'P')
DROP PROCEDURE [BPDS_AverageHandlingTime]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_DailyProductivity]') AND type = N'P')
DROP PROCEDURE [BPDS_DailyProductivity]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_FTEProductivityComparison]') AND type = N'P')
DROP PROCEDURE [BPDS_FTEProductivityComparison]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_AverageRetries]') AND type = N'P')
DROP PROCEDURE [BPDS_AverageRetries]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_Exceptions]') AND type = N'P')
DROP PROCEDURE [BPDS_Exceptions]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_QueueVolumesNow]') AND type = N'P')
DROP PROCEDURE [BPDS_QueueVolumesNow]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[BPDS_QueueSnapshotComparison]') AND type = N'P')
DROP PROCEDURE [BPDS_QueueSnapshotComparison]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_CreateUpdateEnvironmentData]') AND type = N'P')
DROP PROCEDURE [usp_CreateUpdateEnvironmentData]

-- Tables
IF object_id('BPAMIControl') IS NOT NULL
    DROP TABLE BPAMIControl
IF object_id('BPMIUtilisationShadow') IS NOT NULL
    DROP TABLE BPMIUtilisationShadow
IF object_id('BPMIUtilisationDaily') IS NOT NULL
    DROP TABLE BPMIUtilisationDaily
IF object_id('BPMIUtilisationMonthly') IS NOT NULL
    DROP TABLE BPMIUtilisationMonthly
IF object_id('BPMIProductivityShadow') IS NOT NULL
    DROP TABLE BPMIProductivityShadow
IF object_id('BPMIProductivityDaily') IS NOT NULL
    DROP TABLE BPMIProductivityDaily
IF object_id('BPMIProductivityMonthly') IS NOT NULL
    DROP TABLE BPMIProductivityMonthly

IF object_id('BPAGroupUserRolePerm') IS NOT NULL
    DROP TABLE BPAGroupUserRolePerm
IF object_id('BPATreeDefaultGroup') IS NOT NULL
    DROP TABLE BPATreeDefaultGroup
IF object_id('BPATreePerm') IS NOT NULL
    DROP TABLE BPATreePerm
IF object_id('BPACredentialRole') IS NOT NULL
    DROP TABLE BPACredentialRole
IF object_id('BPAResourceRole') IS NOT NULL
    DROP TABLE BPAResourceRole
IF object_id('BPAUserRoleAssignment') IS NOT NULL
    DROP TABLE BPAUserRoleAssignment
IF object_id('BPAUserRolePerm') IS NOT NULL
    DROP TABLE BPAUserRolePerm
IF object_id('BPAUserRole') IS NOT NULL
    DROP TABLE BPAUserRole
IF object_id('BPAUserExternalIdentity') IS NOT NULL
    DROP TABLE BPAUserExternalIdentity
IF object_id('BPAExternalProviderType') IS NOT NULL
    DROP TABLE BPAExternalProviderType
IF object_id('BPAExternalProvider') IS NOT NULL
    DROP TABLE BPAExternalProvider
IF object_id('BPAPermGroupMember') IS NOT NULL
    DROP TABLE BPAPermGroupMember
IF object_id('BPAPermGroup') IS NOT NULL
    DROP TABLE BPAPermGroup
IF object_id('BPAMappedActiveDirectoryUser') IS NOT NULL
    DROP TABLE BPAMappedActiveDirectoryUser
IF object_id('BPAUserExternalReloginToken') IS NOT NULL
	DROP TABLE BPAUserExternalReloginToken
    
    
IF object_id('BPAPermScope') IS NOT NULL
    DROP TABLE BPAPermScope
IF object_id('BPAScope') IS NOT NULL
    DROP TABLE BPAScope
IF object_id('BPAPerm') IS NOT NULL
    DROP TABLE BPAPerm

IF object_id('BPADataTracker') IS NOT NULL
    DROP TABLE BPADataTracker

IF object_id('BPAExceptionType') IS NOT NULL
    DROP TABLE BPAExceptionType

IF object_id('BPAInternalAuth') IS NOT NULL
    DROP TABLE BPAInternalAuth

if object_id('BPAReleaseEntry') is not null
    drop table BPAReleaseEntry;
if object_id('BPARelease') is not null
    drop table BPARelease;

if object_id('BPAPackageProcessGroupMember') is not null
    drop table BPAPackageProcessGroupMember;
if object_id('BPAPackageTile') is not null
    drop table BPAPackageTile;
if object_id('BPAPackageDashboard') is not null
    drop table BPAPackageDashboard;
if object_id('BPAPackageProcess') is not null
    drop table BPAPackageProcess;
if object_id('BPAPackageWorkQueue') is not null
    drop table BPAPackageWorkQueue;
if object_id('BPAPackageCredential') is not null
    drop table BPAPackageCredential;
if object_id('BPAPackageSchedule') is not null
    drop table BPAPackageSchedule;
if object_id('BPAPackageCalendar') is not null
    drop table BPAPackageCalendar;
if object_id('BPAPackageScheduleList') is not null
    drop table BPAPackageScheduleList;
if object_id('BPAPackageWebService') is not null
    drop table BPAPackageWebService;
if object_id('BPAPackageEnvironmentVar') is not null
    drop table BPAPackageEnvironmentVar;
if object_id('BPAPackageFont') is not null
    drop table BPAPackageFont;
if object_id('BPAPackageWebApi') is not null
    drop table BPAPackageWebApi;
if object_id('BPAPackage') is not null
    drop table BPAPackage;

IF object_id('BPAGroupUserPref') IS NOT NULL
    DROP TABLE BPAGroupUserPref
IF object_id('BPAGroupGroup') IS NOT NULL
    DROP TABLE BPAGroupGroup
IF object_id('BPAGroupProcess') IS NOT NULL
    DROP TABLE BPAGroupProcess
IF object_id('BPAGroupResource') IS NOT NULL
    DROP TABLE BPAGroupResource
IF object_id('BPAGroupQueue') IS NOT NULL
    DROP TABLE BPAGroupQueue
IF object_id('BPAGroupTile') IS NOT NULL
    DROP TABLE BPAGroupTile
IF object_id('BPAGroupUser') IS NOT NULL
     DROP TABLE BPAGroupUser

-- We slip work queues between the group detail tables (BPAGroupQueue has FK to BPAWorkQueue)
-- and groups themselves (BPAWorkQueue has FK to BPAGroup)
if object_id('BPACaseLock') is not null
    drop table BPACaseLock;
IF object_id('BPAWorkQueueItemTag') IS NOT NULL
    DROP TABLE BPAWorkQueueItemTag
IF object_id('BPATag') IS NOT NULL
    DROP TABLE BPATag
IF object_id('BPAWorkQueueItem') IS NOT NULL
    DROP TABLE BPAWorkQueueItem
IF object_id('BPAWorkQueueItemAggregate') IS NOT NULL
    DROP TABLE BPAWorkQueueItemAggregate

-- We have a circular dependency here - queue has an FK on session;
-- session has an FK on queue, so we drop the latter here before
-- dropping the queue table
if exists (select *
  from sys.foreign_keys
   where object_id = object_id(N'FK_BPASession_BPAWorkQueue')
   and parent_object_id = object_id(N'BPASession')
)
  alter table BPASession drop constraint FK_BPASession_BPAWorkQueue;
  
IF object_id('BPAWorkQueue') IS NOT NULL
    DROP TABLE BPAWorkQueue
IF object_id('BPAWorkQueueFilter') IS NOT NULL
    DROP TABLE BPAWorkQueueFilter
IF object_id('BPAWorkQueueLog') IS NOT NULL
    DROP TABLE BPAWorkQueueLog

IF object_id('BPASnapshotConfiguration') IS NOT NULL
    DROP TABLE BPASnapshotConfiguration
IF object_id('BPMIConfiguredSnapshot') IS NOT NULL
    DROP TABLE BPMIConfiguredSnapshot
IF object_id('BPMIQueueSnapshot') IS NOT NULL
    DROP TABLE BPMIQueueSnapshot
IF object_id('BPMIQueueInterimSnapshot') IS NOT NULL
    DROP TABLE BPMIQueueInterimSnapshot
IF object_id('BPMIQueueTrend') IS NOT NULL
    DROP TABLE BPMIQueueTrend
IF object_id('BPMISnapshotTrigger') IS NOT NULL
    DROP TABLE BPMISnapshotTrigger

IF object_id('BPAGroup') IS NOT NULL
    DROP TABLE BPAGroup
IF object_id('BPATree') IS NOT NULL
    DROP TABLE BPATree

IF object_id('BPADataPipelineInput') IS NOT NULL
    DROP TABLE BPADataPipelineInput

IF object_id('BPADataPipelineProcess') IS NOT NULL
    DROP TABLE BPADataPipelineProcess

IF object_id('BPADataPipelineProcessConfig') IS NOT NULL
    DROP TABLE BPADataPipelineProcessConfig

IF object_id('BPADataPipelineSettings') IS NOT NULL
    DROP TABLE BPADataPipelineSettings

IF object_id('BPADataPipelineOutputConfig') IS NOT NULL
    DROP TABLE BPADataPipelineOutputConfig

IF object_id('BPADashboardTile') IS NOT NULL
    DROP TABLE BPADashboardTile
IF object_id('BPADashboard') IS NOT NULL
    DROP TABLE BPADashboard
IF object_id('BPATile') IS NOT NULL
    DROP TABLE BPATile
IF object_id('BPATileDataSources') IS NOT NULL
    DROP TABLE BPATileDataSources

IF object_id('BPAProcessMITemplate') IS NOT NULL
    DROP TABLE BPAProcessMITemplate

if object_id('BPANonWorkingDay') is not null
    drop table BPANonWorkingDay;
if object_id('BPAPublicHolidayWorkingDay') is not null
    drop table BPAPublicHolidayWorkingDay;
if object_id('BPAScheduleTrigger') is not null
    drop table BPAScheduleTrigger;
if object_id('BPACalendar') is not null
    drop table BPACalendar;

if object_id('BPAValActionMap') is not null
    drop table BPAValActionMap;
if object_id('BPAValCheck') is not null
    drop table BPAValCheck;
if object_id('BPAValCategory') is not null
    drop table BPAValCategory;
if object_id('BPAValType') is not null
    drop table BPAValType;
if object_id('BPAValAction') is not null
    drop table BPAValAction;

if object_id('BPAAlertEvent') is not null
    drop table BPAAlertEvent;
if object_id('BPAScheduleAlert') is not null
    drop table BPAScheduleAlert;
if object_id('BPAProcessAlert') is not null
    drop table BPAProcessAlert;

        
if object_id('BPAEnvLock') is not null
    drop table BPAEnvLock;

if object_id('BPAFont') is not null
    drop table BPAFont;

IF object_id('BPACredentialsProcesses') IS NOT NULL
    DROP TABLE BPACredentialsProcesses
IF object_id('BPACredentialsResources') IS NOT NULL
    DROP TABLE BPACredentialsResources
IF object_id('BPACredentialsProperties') IS NOT NULL
    DROP TABLE BPACredentialsProperties
IF object_id('BPACredentials') IS NOT NULL
    DROP TABLE BPACredentials

if object_id('BPATaskProcess') is not null
    drop table BPATaskProcess;
if object_id('BPATaskSession') is not null
    drop table BPATaskSession;
if object_id('BPAScheduleLogEntry') is not null
    drop table BPAScheduleLogEntry;
if object_id('BPAScheduleLog') is not null
    drop table BPAScheduleLog;
if object_id('BPAScheduleListSchedule') is not null
    drop table BPAScheduleListSchedule;
if object_id('BPAScheduleList') is not null
    drop table BPAScheduleList;
if object_id('BPATask') is not null
    drop table BPATask;
if object_id('BPASchedule') is not null
    drop table BPASchedule;
    
if object_id('BPAPublicHolidayGroupMember') is not null
    drop table BPAPublicHolidayGroupMember;
if object_id('BPAPublicHolidayGroup') is not null
    drop table BPAPublicHolidayGroup;
if object_id('BPAPublicHoliday') is not null
    drop table BPAPublicHoliday;

if object_id('BPAIntegerPref') is not null
    drop table BPAIntegerPref;
if object_id('BPAStringPref') is not null
    drop table BPAStringPref;
if object_id('BPAPref') is not null
    drop table BPAPref;

IF object_id('BPAProcessEnvVar') IS NOT NULL
    DROP TABLE BPAProcessEnvVar

IF object_id('BPAEnvironmentVar') IS NOT NULL
    DROP TABLE BPAEnvironmentVar

IF object_id('BPAProcessGroupMembership') IS NOT NULL
    DROP TABLE BPAProcessGroupMembership
IF object_id('BPAProcessGroup') IS NOT NULL
    DROP TABLE BPAProcessGroup

if object_id('BPAScreenshot') is not null
    drop table BPAScreenshot;

IF OBJECT_ID('BPADocumentProcessingQueueOverride') IS NOT NULL
    DROP TABLE [BPADocumentProcessingQueueOverride];

IF OBJECT_ID('BPADocumentTypeQueues') IS NOT NULL
    DROP TABLE [BPADocumentTypeQueues];

IF OBJECT_ID('BPADocumentTypeDefaultQueue') IS NOT NULL
    DROP TABLE [BPADocumentTypeDefaultQueue];


if exists (select * from sysobjects where id = object_id(N'[BPAAlert]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAAlert]

if exists (select * from sysobjects where id = object_id(N'[BPAAlertEvent]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAAlertEvent]

if exists (select * from sysobjects where id = object_id(N'[BPAAlertsMachines]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAAlertsMachines]

if exists (select * from sysobjects where id = object_id(N'[BPAAliveResources]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAAliveResources]

if exists (select * from sysobjects where id = object_id(N'[BPAAuditEvents]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAAuditEvents]

if exists (select * from sysobjects where id = object_id(N'[BPACalendar]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPACalendar]

if exists (select * from sysobjects where id = object_id(N'[BPAClock]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAClock]

if exists (select * from sysobjects where id = object_id(N'[BPAPermission]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAPermission]

if exists (select * from sysobjects where id = object_id(N'[BPAProcessAttribute]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessAttribute]

if exists (select * from sysobjects where id = object_id(N'[BPAUserViewPreferencePerProcess]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAUserViewPreferencePerProcess]

if exists (select * from sysobjects where id = object_id(N'[BPARealTimeStatsView]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPARealTimeStatsView]

if exists (select * from sysobjects where id = object_id(N'[BPAResourceAttribute]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAResourceAttribute]

if exists (select * from sysobjects where id = object_id(N'[BPAResourceConfig]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAResourceConfig]

if exists (select * from sysobjects where id = object_id(N'[BPARole]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPARole]

if exists (select * from sysobjects where id = object_id(N'[BPAStatistics]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAStatistics]

if exists (select * from sysobjects where id = object_id(N'[BPAToolPosition]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAToolPosition]

if exists (select * from sysobjects where id = object_id(N'[BPAWebServiceAsset]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAWebServiceAsset]

if exists (select * from sysobjects where id = object_id(N'[BPAWebService]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAWebService]

if exists (select * from sysobjects where id = object_id(N'[BPASession_OLD]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASession_OLD]

if exists (select * from sysobjects where id = object_id(N'[BPASessionLog_OLD]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASessionLog_OLD]

if exists (select * from sysobjects where id = object_id(N'[BPAProcessBackup]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessBackup]

if exists (select * from sysobjects where id = object_id(N'[BPADBVersion]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPADBVersion]

if exists (select * from sysobjects where id = object_id(N'[BPAProcessLock]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessLock]

if exists (select * from sysobjects where id = object_id(N'[BPARecent]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPARecent]

if exists (select * from sysobjects where id = object_id(N'[BPAReport]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAReport]

if exists (select * from sysobjects where id = object_id(N'[BPAResourceUnit]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAResourceUnit]

if exists (select * from sysobjects where id = object_id(N'[BPAScenarioLink]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAScenarioLink]

if exists (select * from sysobjects where id = object_id(N'[BPASessionLog]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASessionLog]

if exists (select * from sysobjects where id = object_id(N'[BPASessionLog_v4]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASessionLog_v4]

if exists (select * from sysobjects where id = object_id(N'[BPASessionLog_NonUnicode]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASessionLog_NonUnicode]

if exists (select * from sysobjects where id = object_id(N'[BPASessionLog_NonUnicode_pre65]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASessionLog_NonUnicode_pre65]

if exists (select * from sysobjects where id = object_id(N'[BPASessionLog_Unicode]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASessionLog_Unicode]

if exists (select * from sysobjects where id = object_id(N'[BPASessionLog_Unicode_pre65]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASessionLog_Unicode_pre65]

if exists (select * from sysobjects where id = object_id(N'[BPASession]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASession]

if exists (select * from sysobjects where id = object_id(N'[BPASysConfig]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASysConfig]

if exists (select * from sysobjects where id = object_id(N'[BPAUserPreference]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAUserPreference]

if exists (select * from sysobjects where id = object_id(N'[BPAUserPrefNar]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAUserPrefNar]

if exists (select * from sysobjects where id = object_id(N'[BPAUserRole]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAUserRole]

if exists (select * from sysobjects where id = object_id(N'[BPAUserRoleNar]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAUserRoleNar]

if exists (select * from sysobjects where id = object_id(N'[BPAScenarioDetail]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAScenarioDetail]

if exists (select * from sysobjects where id = object_id(N'[BPAScenario]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAScenario]

if exists (select * from sysobjects where id = object_id(N'[BPAResource]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAResource]

if exists (select * from sysobjects where id = object_id(N'[BPAWebSkillVersion]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAWebSkillVersion]

if exists (select * from sysobjects where id = object_id(N'[BPASkillVersion]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASkillVersion]

if exists (select * from sysobjects where id = object_id(N'[BPAProcessSkillDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessSkillDependency]

if exists (select * from sysobjects where id = object_id(N'[BPASkill]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPASkill]

if exists (select * from sysobjects where id = object_id(N'[BPAProcessIDDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessIDDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessNameDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessNameDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessParentDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessParentDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessActionDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessActionDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessElementDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessElementDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessWebServiceDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessWebServiceDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessWebApiDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessWebApiDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessQueueDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessQueueDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessCredentialsDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessCredentialsDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessEnvironmentVarDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessEnvironmentVarDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessCalendarDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessCalendarDependency]
if exists (select * from sysobjects where id = object_id(N'[BPAProcessFontDependency]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcessFontDependency]

if exists (select * from sysobjects where id = object_id(N'[BPAProcess]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAProcess]

if exists (select * from sysobjects where id = object_id(N'[BPALicenseActivationRequest]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPALicenseActivationRequest]
if exists (select * from sysobjects where id = object_id(N'[BPALicense]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPALicense]
if exists (select * from sysobjects where id = object_id(N'[BPAUser]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAUser]

if exists (select * from sysobjects where id = object_id(N'[BPAPasswordRules]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAPasswordRules]

if exists (select * from sysobjects where id = object_id(N'[BPAOldPassword]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAOldPassword]

if exists (select * from sysobjects where id = object_id(N'[BPAPassword]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAPassword]

if exists (select * from sysobjects where id = object_id(N'[BPAStatus]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAStatus]

if exists (select * from sysobjects where id = object_id(N'[BPACacheETags]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPACacheETags]

if exists (select * from sysobjects where id = object_id(N'[BPAKeyStore]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAKeyStore]


IF object_id('BPAEnvironment') IS NOT NULL
    DROP TABLE [BPAEnvironment]
IF object_id('BPAEnvironmentType') IS NOT NULL
     DROP TABLE [BPAEnvironmentType]


IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_SetCacheETag]') AND type in (N'P', N'PC'))
DROP PROCEDURE [usp_SetCacheETag]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_GetCacheETag]') AND type in (N'P', N'PC'))
DROP PROCEDURE [usp_GetCacheETag]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_GetSchedule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [usp_GetSchedule]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_getitembyid]') AND type in (N'P', N'PC'))
DROP PROCEDURE [usp_getitembyid]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_getmappedadusers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [usp_getmappedadusers]

IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[usp_SetTargetSessionsForMultipleWorkQueues]') AND type in (N'P', N'PC'))
DROP PROCEDURE [usp_SetTargetSessionsForMultipleWorkQueues]

IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND NAME = N'ActiveDirectoryUserTableType')
    DROP TYPE ActiveDirectoryUserTableType;

IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND NAME = N'TargetSessionDetails')
    DROP TYPE TargetSessionDetails;

IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND NAME = N'GroupIdParameterTable')
    DROP TYPE GroupIdParameterTable;
if exists (select * from sysobjects where id = object_id(N'BPAWebApiCustomOutputParameter') and objectproperty(id, N'IsUserTable') = 1)
  drop table BPAWebApiCustomOutputParameter;
if exists (select * from sysobjects where id = object_id(N'BPAWebApiParameter') and objectproperty(id, N'IsUserTable') = 1)
  drop table BPAWebApiParameter;
if exists (select * from sysobjects where id = object_id(N'BPAWebApiHeader') and objectproperty(id, N'IsUserTable') = 1)
  drop table BPAWebApiHeader;
if exists (select * from sysobjects where id = object_id(N'BPAWebApiAction') and objectproperty(id, N'IsUserTable') = 1)
  drop table BPAWebApiAction;
if exists (select * from sysobjects where id = object_id(N'BPAWebApiService') and objectproperty(id, N'IsUserTable') = 1)
  drop table BPAWebApiService;
if exists (select * from sysobjects where id = object_id(N'BPASysWebUrlSettings') and objectproperty(id, N'IsUserTable') = 1)
  drop table BPASysWebUrlSettings;
if exists (select * from sysobjects where id = object_id(N'BPASysWebConnectionSettings') and objectproperty(id, N'IsUserTable') = 1)
  drop table BPASysWebConnectionSettings;



CREATE TABLE [BPADBVersion] (
    [dbversion] [varchar] (50) COLLATE DATABASE_DEFAULT NOT NULL
       constraint PK_BPADBVersion primary key,
    [scriptrundate] [datetime] NULL ,
    [scriptname] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [description] [varchar] (200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)



CREATE TABLE [BPAProcess] (
    [processid] [uniqueidentifier] NOT NULL
       constraint PK_BPAProcess primary key,
    [name] [varchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [description] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [version] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [statusid] [int] NULL ,
    [createdate] [datetime] NULL ,
    [createdby] [uniqueidentifier] NULL ,
    [lastmodifieddate] [datetime] NULL ,
    [lastmodifiedby] [uniqueidentifier] NULL ,
    [processxml] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)



CREATE TABLE [BPAProcessLock] (
    [processid] [uniqueidentifier] NOT NULL
       constraint PK_BPAProcessLock primary key,
    [lockdatetime] [datetime] NULL ,
    [userid] [uniqueidentifier] NULL ,
    [resourceid] [uniqueidentifier] NULL 
)



CREATE TABLE [BPARecent] (
    [id] [uniqueidentifier] NOT NULL
       constraint PK_BPARecent primary key,
    [type] [int] NOT NULL ,
    [datelastopened] [datetime] NOT NULL ,
    [userid] [uniqueidentifier] NOT NULL ,
    [name] [varchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)



CREATE TABLE [BPAReport] (
    [reportid] [uniqueidentifier] NOT NULL
       constraint PK_BPAReport primary key,
    [name] [varchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [description] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [reportdata] [image] NULL 
)



CREATE TABLE [BPAResource] (
    [resourceid] [uniqueidentifier] NOT NULL
       constraint PK_BPAResource primary key,
    [name] [varchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [status] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [processesrunning] [int] NULL ,
    [actionsrunning] [int] NULL ,
    [unitsallocated] [int] NULL ,
    [lastupdated] [datetime] NULL 
)



CREATE TABLE [BPAResourceUnit] (
    [resourceunitid] [uniqueidentifier] NOT NULL
       constraint PK_BPAResourceUnit primary key,
    [name] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [resourceid] [uniqueidentifier] NULL ,
    [capabilities] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)



CREATE TABLE [BPAScenario] (
    [scenarioid] [uniqueidentifier] NOT NULL ,
    [testnum] [numeric](18, 0) NOT NULL ,
    [passed] [smallint] NULL ,
    [scenariotext] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [scenarionotes] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    CONSTRAINT [PK_BPAScenario] PRIMARY KEY  CLUSTERED 
    (
        [scenarioid],
        [testnum]
    )  


)



CREATE TABLE [BPAScenarioLink] (
    [scenarioid] [uniqueidentifier] NOT NULL ,
    [processid] [uniqueidentifier] NOT NULL ,
    [scenarioname] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [createdate] [datetime] NULL ,
    [userid] [uniqueidentifier] NULL ,
    CONSTRAINT [PK_BPAScenarioLink] PRIMARY KEY  CLUSTERED 
    (
        [scenarioid],
        [processid]
    )  
)



CREATE TABLE [BPASession] (
    [sessionid] [uniqueidentifier] NOT NULL
       constraint PK_BPASession primary key,
    [startdatetime] [datetime] NULL ,
    [enddatetime] [datetime] NULL ,
    [processid] [uniqueidentifier] NULL ,
    [starterresourceid] [uniqueidentifier] NULL ,
    [starteruserid] [uniqueidentifier] NULL ,
    [runningresourceid] [uniqueidentifier] NULL ,
    [runningosusername] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [statusid] [int] NULL ,
    [sessionstatexml] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)



CREATE TABLE [BPASessionLog] (
    [logid] [uniqueidentifier] NOT NULL
       constraint PK_BPASessionLog primary key,
    [sessionid] [uniqueidentifier] NULL ,
    [seqnum] [int] NULL ,
    [logdatetime] [datetime] NULL ,
    [message] [varchar] (2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)



CREATE TABLE [BPAStatus] (
    [statusid] [int] NOT NULL
       constraint PK_BPAStatus primary key,
    [type] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
    [description] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)



CREATE TABLE [BPASysConfig] (
    [id] [int] NOT NULL
       constraint PK_BPASysConfig primary key,
    [maxnumconcproc] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [populateusernameusing] [int] NOT NULL 
)



CREATE TABLE [BPAUser] (
    [userid] [uniqueidentifier] NOT NULL
       constraint PK_BPAUser primary key,
    [username] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [password] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [validfromdate] [datetime] NULL ,
    [validtodate] [datetime] NULL ,
    [passwordexpirydate] [datetime] NULL ,
    [useremail] [varchar] (60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)



CREATE TABLE [BPAUserPreference] (
    [userid] [uniqueidentifier] NOT NULL ,
    [userprefid] [int] NOT NULL ,
    CONSTRAINT [PK_BPAUserPreference] PRIMARY KEY  CLUSTERED 
    (
        [userid],
        [userprefid]
    )  
)



CREATE TABLE [BPAUserPrefNar] (
    [userprefid] [int] NOT NULL
       constraint PK_BPAUserPrefNar primary key,
    [preference1] [bit] NOT NULL ,
    [preference2] [bit] NOT NULL 
)



CREATE TABLE [BPAUserRole] (
    [userid] [uniqueidentifier] NOT NULL ,
    [roleid] [int] NOT NULL ,
    CONSTRAINT [PK_BPAUserRole] PRIMARY KEY  CLUSTERED 
    (
        [userid],
        [roleid]
    )


)



CREATE TABLE [BPAUserRoleNar] (
    [roleid] [int] NOT NULL
       constraint PK_BPAUserRoleNar primary key,
    [description] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)



ALTER TABLE [BPASysConfig] WITH NOCHECK ADD 
    CONSTRAINT [DF_BPASysConfig_populateusernameusing] DEFAULT (0) FOR [populateusernameusing]


ALTER TABLE [BPAProcess] ADD 
    CONSTRAINT [FK_BPAProcess_BPAStatus] FOREIGN KEY 
    (
        [statusid]
    ) REFERENCES [BPAStatus] (
        [statusid]
    ),
    CONSTRAINT [FK_BPAProcess_BPAUser] FOREIGN KEY 
    (
        [createdby]
    ) REFERENCES [BPAUser] (
        [userid]
    ),
    CONSTRAINT [FK_BPAProcess_BPAUser1] FOREIGN KEY 
    (
        [lastmodifiedby]
    ) REFERENCES [BPAUser] (
        [userid]
    )



ALTER TABLE [BPAProcessLock] ADD 
    CONSTRAINT [FK_BPAProcessLock_BPAProcess] FOREIGN KEY 
    (
        [processid]
    ) REFERENCES [BPAProcess] (
        [processid]
    ),
    CONSTRAINT [FK_BPAProcessLock_BPAResource] FOREIGN KEY 
    (
        [resourceid]
    ) REFERENCES [BPAResource] (
        [resourceid]
    ),
    CONSTRAINT [FK_BPAProcessLock_BPAUser] FOREIGN KEY 
    (
        [userid]
    ) REFERENCES [BPAUser] (
        [userid]
    )



alter table [BPAProcessLock] nocheck constraint [FK_BPAProcessLock_BPAProcess]


ALTER TABLE [BPARecent] ADD 
    CONSTRAINT [FK_BPARecent_BPAUser] FOREIGN KEY 
    (
        [userid]
    ) REFERENCES [BPAUser] (
        [userid]
    )





ALTER TABLE [BPAResourceUnit] ADD 
    CONSTRAINT [FK_BPAResourceUnit_BPAResource] FOREIGN KEY 
    (
        [resourceid]
    ) REFERENCES [BPAResource] (
        [resourceid]
    )




ALTER TABLE [BPAScenarioLink] ADD 
    CONSTRAINT [FK_BPAScenarioLink_BPAProcess] FOREIGN KEY 
    (
        [processid]
    ) REFERENCES [BPAProcess] (
        [processid]
    )



ALTER TABLE [BPASession] ADD 
    CONSTRAINT [FK_BPASession_BPAProcess] FOREIGN KEY 
    (
        [processid]
    ) REFERENCES [BPAProcess] (
        [processid]
    ),
    CONSTRAINT [FK_BPASession_BPAResource] FOREIGN KEY 
    (
        [starterresourceid]
    ) REFERENCES [BPAResource] (
        [resourceid]
    ),
    CONSTRAINT [FK_BPASession_BPAResource1] FOREIGN KEY 
    (
        [runningresourceid]
    ) REFERENCES [BPAResource] (
        [resourceid]
    ),
    CONSTRAINT [FK_BPASession_BPAStatus] FOREIGN KEY 
    (
        [statusid]
    ) REFERENCES [BPAStatus] (
        [statusid]
    ),
    CONSTRAINT [FK_BPASession_BPAUser] FOREIGN KEY 
    (
        [starteruserid]
    ) REFERENCES [BPAUser] (
        [userid]
    )



alter table [BPASession] nocheck constraint [FK_BPASession_BPAStatus]


ALTER TABLE [BPASessionLog] ADD 
    CONSTRAINT [FK_BPASessionLog_BPASession] FOREIGN KEY 
    (
        [sessionid]
    ) REFERENCES [BPASession] (
        [sessionid]
    )



alter table [BPASessionLog] nocheck constraint [FK_BPASessionLog_BPASession]





ALTER TABLE [BPAUserPreference] ADD 
    CONSTRAINT [FK_BPAUserPreference_BPAUser] FOREIGN KEY 
    (
        [userid]
    ) REFERENCES [BPAUser] (
        [userid]
    ),
    CONSTRAINT [FK_BPAUserPreference_BPAUserPrefNar] FOREIGN KEY 
    (
        [userprefid]
    ) REFERENCES [BPAUserPrefNar] (
        [userprefid]
    ) 



alter table [BPAUserPreference] nocheck constraint [FK_BPAUserPreference_BPAUser]


alter table [BPAUserPreference] nocheck constraint [FK_BPAUserPreference_BPAUserPrefNar]



ALTER TABLE [BPAUserRole] ADD 
    CONSTRAINT [FK_BPAUserRole_BPAUser] FOREIGN KEY 
    (
        [userid]
    ) REFERENCES [BPAUser] (
        [userid]
    ),
    CONSTRAINT [FK_BPAUserRole_BPAUserRoleNar] FOREIGN KEY 
    (
        [roleid]
    ) REFERENCES [BPAUserRoleNar] (
        [roleid]
    )



alter table [BPAUserRole] nocheck constraint [FK_BPAUserRole_BPAUser]


alter table [BPAUserRole] nocheck constraint [FK_BPAUserRole_BPAUserRoleNar]

GO

/* Insert into the database data which needs to be initialised */

/* Initialise BPAStatus - available status codes */

insert into BPAStatus values (0,'RUN','Pending');
insert into BPAStatus values (1,'RUN','Running');
insert into BPAStatus values (2,'RUN','Failed');
insert into BPAStatus values (3,'RUN','Stopped');
insert into BPAStatus values (4,'RUN','Completed');
insert into BPAStatus values (6,'ENV','Test');
insert into BPAStatus values (7,'ENV','Live');
insert into BPAStatus values (8,'ENV','Archived');

/* Initialise BPASysConfig  - system wide configuration - NB maxnumconcproc should actually be encrypted */

insert into BPASysConfig values (1,'1',0);

/* Initialise BPAUserRoleNar  - predefined user roles  - again only indicative*/

insert into BPAUserRoleNar values (1, 'System Manager');
insert into BPAUserRoleNar values (2, 'Process Studio');
insert into BPAUserRoleNar values (3, 'Test Lab');
insert into BPAUserRoleNar values (4, 'Control Room');
insert into BPAUserRoleNar values (5, 'Report Console');


/* Initialise BPAUserPrefNar  - predefined  available user preferences */

insert into BPAUserPrefNar values (1, 1, 1);

/* Add the machine the script is run on to the available resources */
insert into BPAResource values (NewID(),HOST_NAME(),'','','','','');

insert into BPADBVersion values ('10',GETUTCDATE(),'db_createR10.sql UTC','Create base version of Blue Prism Database.');

/* Add temporary column to indicate install is in pprogress */
alter table BPASysConfig add InstallInProgress bit null;
