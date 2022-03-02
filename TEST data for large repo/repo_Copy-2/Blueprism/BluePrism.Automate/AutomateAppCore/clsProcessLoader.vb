Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.CharMatching
Imports BluePrism.Core.Compression
Imports BluePrism.AutomateProcessCore.ProcessLoading
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : clsAutomateProcessLoader
''' 
''' <summary>
''' A class to load data from an external source, typically the database. This is
''' responsible for loading process XML and attributes, environment variables and
''' fonts.
''' Note that modifications cannot be made via this class.
''' </summary>
Public Class clsAutomateProcessLoader
    Implements IProcessLoader

    Private mGlobalInfo As clsGlobalInfo
    Private mProcessCache As New Dictionary(Of Guid, clsProcessCacheRecord)
    Private mFontStore As New clsAutomateFontStore()
    Private mCachedEnvVars As Dictionary(Of String, clsArgument) = Nothing
    Private mCachedGlobalInfo As clsGlobalInfo = Nothing
    Private mCheckedProcesses As New HashSet(Of Guid)
    Private mProcessCacheBehaviour As CacheRefreshBehaviour = CacheRefreshBehaviour.CheckForUpdatesEveryTime

    Public Property CacheBehaviour As CacheRefreshBehaviour Implements IProcessLoader.CacheBehaviour
        Get
            Return mProcessCacheBehaviour
        End Get
        Set(value As CacheRefreshBehaviour)
            mProcessCacheBehaviour = value
            If value = CacheRefreshBehaviour.CheckForUpdatesOnce Then _
                mCheckedProcesses = New HashSet(Of Guid)
        End Set
    End Property

    Public Sub SaveFont(font As BPFont) Implements IFontStore.SaveFont
        Throw New NotImplementedException()
    End Sub
    Public Sub SaveFontOcrPlus(name As String, data As String) Implements IFontStore.SaveFontOcrPlus
        Throw New NotImplementedException()
    End Sub

    Public Function DeleteFont(name As String) As Boolean Implements IFontStore.DeleteFont
        Throw New NotImplementedException()
    End Function

    ''' <summary>
    ''' A record of a locally cached process
    ''' </summary>
    Private Class clsProcessCacheRecord
        Public Sub New()
        End Sub

        Public XML As String
        Public LastModified As DateTime
        Public Attributes As ProcessAttributes
        Public Function GetTuple() As (Xml As String, lastmod As DateTime, attributes As ProcessAttributes)
            Return (XML, LastModified, Attributes)
        End Function
    End Class


    Private Function GetCachedProcessDetails(gProcessID As Guid) As (Xml As String, LastModification As DateTime, Attributes As ProcessAttributes)
        SyncLock mProcessCache

            Dim cacheRecord As clsProcessCacheRecord = Nothing
            Dim found = mProcessCache.TryGetValue(gProcessID, cacheRecord)

            If found Then
                If CacheBehaviour = CacheRefreshBehaviour.NeverCheckForUpdates _
                    OrElse (CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesOnce _
                    AndAlso mCheckedProcesses.Contains(gProcessID)) Then
                    Return cacheRecord.GetTuple()
                Else
                    Dim lastModified = gSv.GetProcessLastModified(gProcessID)

                    If cacheRecord.LastModified = lastModified Then
                        If CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesOnce Then _
                            mCheckedProcesses.Add(gProcessID)

                        Return cacheRecord.GetTuple()
                    Else
                        mProcessCache.Remove(gProcessID)
                    End If
                End If
            End If

            Try
                Dim processDetails = gSv.GetProcessXMLAndAssociatedData(gProcessID)
                Dim processXML As String
                If processDetails.Zipped Then
                    processXML = GZipCompression.Decompress(processDetails.Xml)
                Else
                    processXML = Encoding.Unicode.GetString(processDetails.Xml)
                End If
                cacheRecord = New clsProcessCacheRecord() _
                        With {.XML = processXML,
                          .LastModified = processDetails.LastModified,
                          .Attributes = processDetails.Attributes
                          }
                mProcessCache.Add(gProcessID, cacheRecord)

                If CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesOnce Then _
                    mCheckedProcesses.Add(gProcessID)

                Return cacheRecord.GetTuple()
            Catch ex As PermissionException
                Return ("", Nothing, Nothing)
            End Try
        End SyncLock
    End Function

    ''' <summary>
    ''' Reads the process XML into a string.
    ''' </summary>
    ''' <param name="gProcessID">The process id</param>
    ''' <param name="sXML">The XML string</param>
    ''' <param name="lastmod">The date/time the process was modified.</param>
    ''' <param name="sErr">The error message</param>
    ''' <returns>True if successful</returns>
    Public Function GetProcessXML(ByVal gProcessID As Guid, ByRef sXML As String, ByRef lastmod As DateTime, ByRef sErr As String) As Boolean Implements IProcessLoader.GetProcessXML

        Dim processDetails = GetCachedProcessDetails(gProcessID)

        If processDetails.Xml = "" Then
            sErr = My.Resources.clsAutomateProcessLoader_FailedToGetProcessXML
            Return False
        Else
            sXML = processDetails.Xml
            lastmod = processDetails.LastModification
            Return True
        End If

    End Function

    Public Function GetEffectiveRunMode(ByVal processID As Guid) As BusinessObjectRunMode _
        Implements IProcessLoader.GetEffectiveRunMode

        Dim extRunModes As New Dictionary(Of String, BusinessObjectRunMode)
        Dim externalObjectDetails = CType(Options.Instance.GetExternalObjectsInfo(), clsGroupObjectDetails)
        Dim comGroup As New clsGroupObjectDetails(externalObjectDetails.Permissions)
        For Each comObj In externalObjectDetails.Children
            If TypeOf (comObj) Is clsCOMObjectDetails Then _
                comGroup.Children.Add(comObj)
        Next
        Using objRefs As New clsGroupBusinessObject(comGroup, Nothing, Nothing)
            extRunModes = objRefs.GetNonVBORunModes()
        End Using
        Return gSv.GetEffectiveRunMode(processID, extRunModes)

    End Function


    Public Function GetProcessAtrributes(ByVal gProcessID As Guid, ByRef attributes As ProcessAttributes, ByRef sErr As String) As Boolean Implements IProcessLoader.GetProcessAtrributes
        Dim processDetails = GetCachedProcessDetails(gProcessID)
        attributes = processDetails.Attributes
        Return True
    End Function

    ''' <summary>
    ''' Get the full set environment variables.
    ''' </summary>
    ''' <returns>A Dictionary(Of String,clsArgument) containing all the known
    ''' variables and their values.</returns>
    ''' <remarks>This should always return a new Dictionary, which the caller then
    ''' owns.</remarks>
    Public Function GetEnvVars(freshFromDatabase As Boolean) As Dictionary(Of String, clsArgument) Implements IProcessLoader.GetEnvVars
        If Not ServerFactory.ServerAvailable Then _
            Return New Dictionary(Of String, clsArgument)

        If freshFromDatabase OrElse mCachedEnvVars Is Nothing OrElse CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesEveryTime Then
            Dim tmpCachedEnvVars = GetEnvironmentVariablesFromServer()
            If mCachedEnvVars IsNot Nothing AndAlso tmpCachedEnvVars IsNot Nothing Then
                For Each item In mCachedEnvVars
                    If tmpCachedEnvVars.ContainsKey(item.Key) AndAlso mCachedEnvVars(item.Key)?.Value IsNot Nothing Then
                        Dim arg = clsAPC.ProcessLoader.GetEnvVarSingle(item.Key, False)
                        If arg IsNot Nothing Then tmpCachedEnvVars(item.Key) = arg
                    End If
                Next
            End If

            If mCachedEnvVars Is Nothing Then
                mCachedEnvVars = New Dictionary(Of String, clsArgument)
            End If
            mCachedEnvVars.Clear()
            For Each item In tmpCachedEnvVars
                mCachedEnvVars.Add(item.Key, item.Value)
            Next
            Return mCachedEnvVars
        Else
            Return mCachedEnvVars
        End If
    End Function


    Public Function GetEnvVars() As Dictionary(Of String, clsArgument) Implements IProcessLoader.GetEnvVars
        Return GetEnvVars(False)
    End Function

    Private Function GetEnvironmentVariablesFromServer() As Dictionary(Of String, clsArgument)
        Dim dict = New Dictionary(Of String, clsArgument)
        Dim list = gSv.GetEnvironmentVariablesNames()
        If list IsNot Nothing AndAlso list.Count > 0 Then
            For Each item In list
                dict.Add(item, Nothing)
            Next
        End If

        Return dict
    End Function

    Public Function GetEnvVarSingle(name As String, updateCache As Boolean) As clsArgument Implements IProcessLoader.GetEnvVarSingle
        Dim arg As clsArgument = Nothing
        Try
            If mCachedEnvVars IsNot Nothing AndAlso mCachedEnvVars.ContainsKey(name) AndAlso mCachedEnvVars(name)?.Value IsNot Nothing Then _
                arg = mCachedEnvVars(name)

            If arg Is Nothing OrElse updateCache = False Then
                Dim envar = gSv.GetEnvironmentVariable(name)
                arg = envar.CreateArgument(envar.Name)
                If updateCache AndAlso arg IsNot Nothing AndAlso mCachedEnvVars.ContainsKey(envar.Name) Then mCachedEnvVars(envar.Name) = arg
            End If
        Catch
            Return Nothing
        End Try

        Return arg
    End Function

    ''' <summary>
    ''' Get the definition of the specified font.
    ''' </summary>
    ''' <param name="name">The name of the font.</param>
    ''' <returns>The font's XML definition, or Nothing if the font doesn't exist.
    ''' </returns>
    ''' <exception cref="NoSuchFontException">If no font with the given name was
    ''' found in this loader</exception>
    Public Function GetFont(ByVal name As String) As BPFont Implements IProcessLoader.GetFont
        Return mFontStore.GetFont(name)
    End Function
    Public Function GetFontOcrPlus(name As String) As String Implements IFontStore.GetFontOcrPlus
        Return mFontStore.GetFontOcrPlus(name)
    End Function

    ''' <summary>
    ''' Gets the font names available in this loader
    ''' </summary>
    Public ReadOnly Property AvailableFontNames() As ICollection(Of String) Implements IProcessLoader.AvailableFontNames
        Get
            Return mFontStore.AvailableFontNames
        End Get
    End Property


    Public Function GetAMIInfo() As clsGlobalInfo Implements IProcessLoader.GetAMIInfo
        If mCachedGlobalInfo Is Nothing OrElse CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesEveryTime Then
            mCachedGlobalInfo = New clsGlobalInfo() With
             {.TesseractEngine = gSv.GetTesseractEngine()}
        End If
        Return mCachedGlobalInfo
    End Function
End Class
