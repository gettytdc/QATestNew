Public Enum SysConfEventCode
    <EventCode("S001","EventCodeAttribute_TheUser0ModifiedTheArchivingSettingsOnTheResource1")>
    ModifyArchive
    <EventCode("S002","EventCodeAttribute_TheUser0ModifiedTheSystemLicenseKey")>
    ModifySystemLicenceKey
    <EventCode("S003","EventCodeAttribute_TheUser0ModifiedBluePrismSUserRolesConfiguration")>
    ModifyRoles
    <EventCode("S004","EventCodeAttribute_TheUser0ModifiedBluePrismSSingleSignOnConfigurationInSystemManager")>
    ModifySingleSignOn
    <EventCode("S005","EventCodeAttribute_TheUser0ModifiedBluePrismSDesignControlConfiguration")>
    ModifyDesignControl
    <EventCode("S006","EventCodeAttribute_TheUser0ModifiedBluePrismSSignOnSettings")>
    ModifySignonSettings
    <EventCode("S007","EventCodeAttribute_TheUser0RanThe1Report")>
    RunReport
    <EventCode("S008","EventCodeAttribute_TheUser0ModifiedTheMIReportingConfiguration")>
    ModifyReporting
    <EventCode("S009","EventCodeAttribute_TheUser0ModifiedTheUnicodeSessionLoggingOption")>
    ModifyLogging
    <EventCode("S010","EventCodeAttribute_TheUser0ModifiedTheDefaultEncryptionScheme")>
    DefaultEncrypter
    <EventCode("S011","EventCodeAttribute_TheUser0ChangedTheResourceRegistrationMode")>
    ModifyResourceRegistrationMode
    <EventCode("S012","EventCodeAttribute_TheUser0ChangedThePreventResourceRegistrationSetting")>
    ModifyPreventResourceRegistration
    <EventCode("S013","EventCodeAttribute_TheUser0ChangedTheRequireSecuredResourceConnectionSetting")>
    ModifyRequireSecuredResourceConnections
    <EventCode("S014","EventCodeAttribute_TheUser0ChangedTheAllowAnonymousResourcesSetting")>
    ModifyAllowAnonResources
    <EventCode("S015","EventCodeAttribute_TheUser0ChangedASystemSetting")>
    ModifySystemSetting
    <EventCode("S016","EventCodeAttribute_TheUser0ExportedSessionLogsForSession1")>
    ExportSessionLogs
    <EventCode("S017","EventCodeAttribute_TheUser0ExportedTheAuditLog")>
    ExportAuditLog
    <EventCode("S018","EventCodeAttribute_TheUser0ModifiedGroupLevelAccessRights")>
    ModifyGroupPermissions
    <EventCode("S019","EventCodeAttribute_TheUser0ManuallyReleasedTheArchiveLock")>
    ReleaseArchiveLock
    <EventCode("S020","EventCodeAttribute_TheUser0ChangedTheSchedulerSettings")>
    ModifySchedulerSettings
    <EventCode("S021","EventCodeAttribute_TheUser0ChangedTheSessionManagementEnforcesPermissionsOfControllingUserSetting")>
    ModifyControllingUserPermissionSetting
    <EventCode("S022","EventCodeAttribute_TheUser0ToggledTheSettingThatDeterminesWhetherAPersonalRuntimeResourceWillStartWhenLoggingIntoTheClient")>
    ModifyStartProcEngine
    <EventCode("S023","EventCodeAttribute_TheUser0ChangedTheAutoSaveIntervalValueTo1Minutes")>
    ModifyAutoSaveInterval
    <EventCode("S024","EventCodeAttribute_TheUser0ToggledTheSettingThatForcesUsersToSummariseTheirChangesWhenSavingAProcessOrBusinessObjectTo1")>
    ModifyForceSummaryOnSave
    <EventCode("S025","EventCodeAttribute_TheUser0ToggledTheSettingThatEnablesOfflineHelpTo1")>
    ModifyOfflinehelp
    <EventCode("S026","EventCodeAttribute_TheUser0ToggledTheSettingThatEnablesOfflineHelpTo1AndUpdatedTheUrlTo2")>
    ModifyOfflineHelpData
    <EventCode("S027","EventCodeAttribute_TheUser0UpdatedTheOfflineHelpUrlTo1")>
    ModifyOfflineHelpUrl
    <EventCode("S028","EventCodeAttribute_TheUser0ToggledTheSettingThatHidesTheDigitalExchangeTabTo1")>
    ModifyHideDigitalExchangeTab
    <EventCode("S029","EventCodeAttribute_TheUser0ToggledTheSettingThatSavesEnvironmentDataToTheDatabaseTo1")>
    ModifyEnableBpaEnvironmentData
    <EventCode("S030","EventCodeAttribute_User0InitiatedTheConversionOfTheDatabaseFromSingleAuthenticationActiveDirectoryToMultiAuthentication")>
    ConvertDatabaseToMultiAuth
    <EventCode("S031","EventCodeAttribute_TheUser0ManuallyReleasedTheMIRefreshLock")>
    ReleaseMIRefreshLock
    <EventCode("S032", "EventCodeAttribute_TheUser0CompletedAConversionFromSingleAuthenticationSsoToMultiAuthAdSso")>
    CompletedConvertingToMultiAuthDatabase
    <EventCode("S033", "EventCodeAttribute_TheConversionFromSingleAuthSsoToMultiAuthAdSsoFailedTheFollowingErrorOccurred1")>
    AbortedConvertingToMultiAuthDatabase
    <EventCode("S034", "EventCodeAttribute_TheUser0ChangedTheUseFixedBrowserExtensionPortOnlySetting")>
    ModifyBrowserPluginLegacyPortOverride
    <EventCode("S035", "EventCodeAttribute_StartCreationOfBluePrismUserTemplate")>
    StartCreationOfBluePrismUserTemplate
    <EventCode("S036", "EventCodeAttribute_CompletedCreationOfBluePrismUserTemplate")>
    CompletedCreationOfBluePrismUserTemplate
    <EventCode("S037", "EventCodeAttribute_CreationOfBluePrismUserTemplateFailed")>
    CreationOfBluePrismUserTemplateFailed
End Enum
