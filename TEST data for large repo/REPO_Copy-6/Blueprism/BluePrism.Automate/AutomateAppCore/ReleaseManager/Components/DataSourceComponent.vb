Imports System.Xml

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models

Imports System.IO
Imports System.Runtime.Serialization
Imports System.Data.SqlClient
Imports System.Security.Cryptography

''' <summary>
''' Component representing a tile data source.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class DataSourceComponent : Inherits NameEqualsIdComponent : Implements ISaveToFile

#Region " Conflicts "

    ''' <summary>
    ''' The conflict definitions which are handled by the dashboard component.
    ''' </summary>
    Public Class MyConflicts

        Public Shared Function GenerateNoCreatePermissionConflict(fileName As String) As ConflictDefinition
            Return _
                New ConflictDefinition("DataSourceNoCreatePermission",
                                       My.Resources.MyConflicts_YouDoNotHaveTheRequiredDatabasePermissionsToCreateThisDataSource,
                                       My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                       New ConflictOption(ConflictOption.UserChoice.SaveToFile,
                                                          String.Format(
                                                              My.Resources.MyConflicts_SaveDataSourceAsASQLScript0,
                                                              fileName)),
                                       New ConflictOption(ConflictOption.UserChoice.Skip,
                                                          My.Resources.MyConflicts_DonTImportThisDataSource)) _
                    With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Skip,
                        .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail}
        End Function

        Public Shared Function GenerateNoGrantPermissionConflict(fileName As String) As ConflictDefinition
            Return _
                New ConflictDefinition("DataSourceNoGrantPermission",
                                       My.Resources.MyConflicts_YouDoNotHaveTheRequiredDatabasePermissionsToExtendTheCustomDataSourcesSecurityR,
                                       My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                       New ConflictOption(ConflictOption.UserChoice.OmitPostProcess,
                                                          My.Resources.MyConflicts_ImportThisDataSourceWithoutExtendingTheSecurityRoleSomeUsersMayNotBeAbleToUseIt),
                                       New ConflictOption(ConflictOption.UserChoice.SaveToFile,
                                                          String.Format(
                                                              My.Resources.MyConflicts_SaveDataSourceAsASQLScript0,
                                                              fileName)),
                                       New ConflictOption(ConflictOption.UserChoice.Skip,
                                                          My.Resources.MyConflicts_DonTImportThisDataSource)) _
                    With {.DefaultInteractiveResolution = ConflictOption.UserChoice.OmitPostProcess,
                        .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail}
        End Function

        Public Shared Function GenerateNoAlterPermissionConflict(fileName As String) As ConflictDefinition
            Return New ConflictDefinition("DataSourceNoAlterPermission",
                                       My.Resources.MyConflicts_ADataSourceWithTheSameNameAlreadyExistsInTheDatabaseHoweverYouDoNotHavePermissi,
                                       My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                       New ConflictOption(ConflictOption.UserChoice.SaveToFile,
                                                          String.Format(
                                                              My.Resources.MyConflicts_SaveDataSourceAsASQLScript0,
                                                              fileName)),
                                       New ConflictOption(ConflictOption.UserChoice.Skip,
                                                          My.Resources.
                                                             MyConflicts_DonTImportThisDataSourceLeaveTheExistingOneInPlace)) _
                    With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Skip,
                        .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail}
        End Function

        Public Shared ReadOnly Property NameClash As ConflictDefinition
            Get
                If mNameClash Is Nothing Then
                    mNameClash = GetNameClashConflictDefinition()
                Else
                    mNameClash.UpdateConflictDefinitionStrings(GetNameClashConflictDefinition())
                End If
                Return mNameClash
            End Get
        End Property

        Private Shared Function GetNameClashConflictDefinition() As ConflictDefinition

            Return New ConflictDefinition("DataSourceNameClash",
                                          My.Resources.
                                             MyConflicts_ADataSourceWithTheSameNameAlreadyExistsInTheDatabasePleaseChooseOneOfTheFollowi,
                                          My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                          New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                             My.Resources.
                                                                MyConflicts_OverwriteTheExistingDataSourceWithTheIncomingDefinition),
                                          New ConflictOption(ConflictOption.UserChoice.Skip,
                                                             My.Resources.
                                                                MyConflicts_DonTImportThisDataSourceLeaveTheExistingOneInPlace)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private Shared mNameClash As ConflictDefinition 
    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new data source component using data from the given provider.
    ''' </summary>
    ''' <param name="prov">The data provider</param>
    Public Sub New(owner As OwnerComponent, prov As IDataProvider)
        Me.New(owner, prov.GetString("name"))
    End Sub

    ''' <summary>
    ''' Creates a new data source component using the given name.
    ''' </summary>
    ''' <param name="name">The data source name</param>
    Public Sub New(owner As OwnerComponent, name As String)
        MyBase.New(owner, name)
    End Sub

    ''' <summary>
    ''' Creates a new data source component drawing data from the given XML reader.
    ''' </summary>
    ''' <param name="reader">The XML reader</param>
    ''' <param name="ctx">The loading context</param>
    Public Sub New(owner As OwnerComponent, reader As XmlReader, ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type As PackageComponentType
        Get
            Return PackageComponentType.DataSource
        End Get
    End Property

    ''' <summary>
    ''' The SQL to create (or alter) the stored procedure on the database
    ''' </summary>
    Public ReadOnly Property SQL As SqlCommand()
        Get
            Return BuildSQLCommandList()
        End Get
    End Property

    ''' <summary>
    ''' The full path and name of the script for this data source, as used by the 
    ''' Save To File conflict resolution.
    ''' </summary>
    Public ReadOnly Property ScriptName As String
        Get
            Return Path.Combine(
                My.Computer.FileSystem.SpecialDirectories.MyDocuments,
                String.Format("create_procedure_{0}.sql", Me.Name))
        End Get
    End Property

    ''' <summary>
    ''' The permission required by a user to import a component of this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return Nothing
        End Get
    End Property

    ' By default grant execute to custom data source security role
    Private mGrantExecute As Boolean = True

    ' Checksum value read from xml header
    <NonSerialized>
    Private mChecksum As String

    ''' <summary>
    ''' Format of string to create sql checksum
    ''' </summary>
    Private Const ChecksumFormat As String = "SQL{0}SQL"

    ''' <summary>
    ''' Name of checksum attribute in xml
    ''' </summary>
    Private Const ChecksumAttribute As String = "checksum"

#End Region

#Region " Methods "

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return gSv.GetDataSourceDefinition(Me.Name)
    End Function

    ''' <summary>
    ''' Generates an SQL script to create/alter this data source.
    ''' Make sure you update the script generation in SaveToFile() if you change this
    ''' </summary>
    ''' <returns>The SQL script</returns>
    Private Function BuildSQLCommandList() As SqlCommand()

        If String.IsNullOrEmpty(mChecksum) Then
            Throw New BluePrismException(My.Resources.DataSourceComponent_ChecksumNotFoundOnlyDataSourcesExportedFromADatabaseCompatibleWithVersion6OrAbo)
        End If

        Dim incoming = GetChecksum(String.Format(ChecksumFormat, Me.AssociatedData.ToString()))
        If incoming <> mChecksum Then
            Throw New BluePrismException(String.Format(My.Resources.DataSourceComponent_ChecksumForDataSourceSQL0DoesNotMatchChecksumValueFoundInImportPackage, Me.Name))
        End If

        Dim commandList As New List(Of SqlCommand)

        ' Check name is query safe.
        Dim createSP As New SqlCommand
        With createSP
            .CommandType = CommandType.StoredProcedure
            .CommandText = "usp_setupDataSource"
            .Parameters.AddWithValue("@spName", Me.Name)
            .Parameters.AddWithValue("@grant", mGrantExecute)
        End With

        commandList.Add(createSP)

        'Add procedure definition, changing 'create' to 'alter'
        Dim createSQL As String = CStr(AssociatedData)
        Dim pos As Integer = createSQL.IndexOf("create procedure", StringComparison.CurrentCultureIgnoreCase) + 16

        Dim alterSP As New SqlCommand
        With alterSP
            .CommandType = CommandType.Text
            .CommandText = "alter procedure" & createSQL.Substring(pos)
        End With

        commandList.Add(alterSP)

        Return commandList.ToArray()
    End Function

    ''' <summary>
    ''' Saves the create script for this data source to a file.
    ''' </summary>
    Public Sub SaveToFile() Implements ISaveToFile.SaveToFile

        Dim s As New StringBuilder()
        s.AppendLine(String.Format("EXEC usp_setupDataSource {0}, {1} ", Me.Name, mGrantExecute.ToString))
        s.AppendLine("GO")
        s.AppendLine()

        Dim createSpSQL As String = CStr(AssociatedData)
        Dim pos As Integer = createSpSQL.IndexOf("create procedure", StringComparison.CurrentCultureIgnoreCase) + 16
        s.AppendLine("alter procedure" & createSpSQL.Substring(pos))
        s.AppendLine("GO")

        FileIO.FileSystem.WriteAllText(Me.ScriptName, s.ToString, False)
    End Sub

#End Region

#Region " XML Handling "

    ''' <summary>
    ''' Writes a checksum of the sql to the data source xml
    ''' </summary>
    ''' <param name="writer"></param>
    Protected Overrides Sub WriteXmlHead(ByVal writer As XmlWriter)
        MyBase.WriteXmlHead(writer)

        If String.IsNullOrWhiteSpace(Me.AssociatedData.ToString) Then Return

        writer.WriteAttributeString(ChecksumAttribute,
                                    GetChecksum(String.Format(ChecksumFormat,
                                                              Me.AssociatedData.ToString)))
    End Sub

    ''' <summary>
    ''' Calculates a checksum for the sql in the xml
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    Private Shared Function GetChecksum(source As String) As String
        Dim ue As New UnicodeEncoding()
        Dim bytes() = ue.GetBytes(source)
        Using sha As New SHA256CryptoServiceProvider()
            Dim bytehash() As Byte = sha.ComputeHash(bytes)
            Return Convert.ToBase64String(bytehash)
        End Using
    End Function

    ''' <summary>
    ''' Appends XML defining this data source to the given XML Writer.
    ''' </summary>
    ''' <param name="writer">The XML writer</param>
    Protected Overrides Sub WriteXmlBody(writer As XmlWriter)
        If Me.AssociatedData Is Nothing Then Throw New NoSuchElementException(
         My.Resources.DataSourceComponent_NoDataSourceWithTheName0Exists, Me.Name)

        writer.WriteCData(Me.AssociatedData.ToString())
    End Sub

    ''' <summary>
    ''' Reads the checksum from the xml header
    ''' </summary>
    ''' <param name="reader"></param>
    ''' <param name="ctx"></param>
    Protected Overrides Sub ReadXmlHead(
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.ReadXmlHead(reader, ctx)
        mChecksum = reader.GetAttribute(ChecksumAttribute)
    End Sub

    ''' <summary>
    ''' Reads this data source from the given XML reader.
    ''' </summary>
    ''' <param name="r">The XML reader</param>
    ''' <param name="ctx">The loading context</param>
    Protected Overrides Sub ReadXmlBody(r As XmlReader, ctx As IComponentLoadingContext)
        Me.AssociatedData = r.ReadElementContentAsString()
    End Sub

#End Region

#Region " Conflict Handling "

    ''' <summary>
    ''' Gets clashes between this component and objects currently in the database.
    ''' </summary>
    ''' <returns>Collection of clash definitions</returns>
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)
        Dim conflicts As New List(Of ConflictDefinition)

        If gSv.ListStoredProcedures().ContainsKey(Me.Name) Then
            'If data source already exists, check user has database perms to alter it
            If gSv.CanAlterDataSource(Me.Name) Then
                conflicts.Add(MyConflicts.NameClash)
            Else
                conflicts.Add(MyConflicts.GenerateNoAlterPermissionConflict(ScriptName))
            End If
        Else
            'Otherwise check user has database perms to create it
            If Not gSv.CanCreateDataSource() Then
                conflicts.Add(MyConflicts.GenerateNoCreatePermissionConflict(ScriptName))
            ElseIf Not gSv.CanGrantExecuteOnDataSource() Then
                conflicts.Add(MyConflicts.GenerateNoGrantPermissionConflict(ScriptName))
            End If
        End If

        Return conflicts
    End Function

    ''' <summary>
    ''' Gets the delegate responsible for applying the conflict resolutions.
    ''' </summary>
    Public Overrides ReadOnly Property ResolutionApplier() As Conflict.Resolver
        Get
            Return AddressOf ApplyConflictResolutions
        End Get
    End Property

    ''' <summary>
    ''' Applies the conflict resolutions to all data sources in the given release.
    ''' This can be an instance method since there is no cross-component checking.
    ''' </summary>
    ''' <param name="rel">The release being imported</param>
    ''' <param name="resolutions">The resolutions to apply</param>
    ''' <param name="errors">The error log to append errors to.</param>
    Private Sub ApplyConflictResolutions(rel As clsRelease,
                                         resolutions As ICollection(Of ConflictResolution),
                                         errors As clsErrorLog)

        ' Clear down the modifications so we're starting from scratch
        Dim mods As IDictionary(Of ModificationType, Object) = Modifications
        mods.Clear()

        For Each res As ConflictResolution In resolutions
            If res.Conflict.Component IsNot Me Then Continue For

            ' Check that the user has selected an option
            Dim opt As ConflictOption = res.ConflictOption
            If opt Is Nothing Then
                errors.Add(Me, My.Resources.DataSourceComponent_YouMustChooseHowToHandleTheDataSource0, Me.Name)
            Else
                Select Case opt.Choice
                    Case ConflictOption.UserChoice.Skip
                        mods.Add(ModificationType.Skip, True)
                    Case ConflictOption.UserChoice.Overwrite
                        mGrantExecute = False
                        mods.Add(ModificationType.OverwritingExisting, True)
                    Case ConflictOption.UserChoice.OmitPostProcess
                        mGrantExecute = False
                    Case ConflictOption.UserChoice.SaveToFile
                        mods.Add(ModificationType.Skip, True)
                    Case Else
                        Throw New BluePrismException(My.Resources.DataSourceComponent_UnrecognisedOption0, opt.Choice)
                End Select
                res.Passed = True
            End If
        Next

    End Sub
#End Region

End Class

''' <summary>
''' Interface for Package Components that support saving their contents to file. E.g.
''' Data Sources
''' </summary>
Public Interface ISaveToFile
    Sub SaveToFile()
End Interface
