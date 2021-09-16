Imports System.Runtime.Serialization
Imports System.Xml

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.Server.Domain.Models


''' <summary>
''' Component representing a tile.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class DashboardComponent : Inherits PackageComponent

#Region " Conflict definitions "

    ''' <summary>
    ''' The conflict definitions which are handled by the dashboard component.
    ''' </summary>
    Public Class MyConflicts

        Public Shared ReadOnly Property IDClash As ConflictDefinition
            Get
                If mIDClash Is Nothing Then
                    mIDClash = GetIDClashConflictDefinition()
                Else
                    mIDClash.UpdateConflictDefinitionStrings(GetIDClashConflictDefinition())
                End If
                Return mIDClash
            End Get
        End Property

        Private Shared Function GetIDClashConflictDefinition() As ConflictDefinition

            Return New ConflictDefinition("DashboardIDClash",
                                          My.Resources.MyConflicts_ADashboardWithTheSameIDAlreadyExistsInTheDatabase,
                                          My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                          New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                             My.Resources.
                                                                MyConflicts_OverwriteTheExistingTileWithTheIncomingDashboard),
                                          New ConflictOption(ConflictOption.UserChoice.NewId,
                                                             My.Resources.MyConflicts_ChooseANewIDForTheIncomingDashboard,
                                                             New ConflictDataHandler("NewName",
                                                                                     My.Resources.
                                                                                        MyConflicts_TheDashboardWithTheSameIDAlsoHasTheSameNamePleaseEnterANewName,
                                                                                     New ConflictArgument("Dashboard Name",
                                                                                                          "",
                                                                                                          My.Resources.
                                                                                                             MyConflicts_DashboardName))),
                                          New ConflictOption(ConflictOption.UserChoice.Skip,
                                                             My.Resources.
                                                                MyConflicts_DonTImportThisDashboardLeaveTheExistingOneInPlace)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private Shared mIDClash As ConflictDefinition

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
            Return New ConflictDefinition("DashboardNameClash",
                                          My.Resources.MyConflicts_ADashboardWithTheSameNameAlreadyExistsInTheDatabase,
                                          My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                          New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                             My.Resources.
                                                                MyConflicts_OverwriteTheExistingTileWithTheIncomingDashboard),
                                          New ConflictOption(ConflictOption.UserChoice.Rename,
                                                             My.Resources.MyConflicts_ChooseANewNameForTheIncomingDashboard,
                                                             New ConflictDataHandler("NewName", Nothing,
                                                                                     New ConflictArgument("Dashboard Name",
                                                                                                          "",
                                                                                                          My.Resources.
                                                                                                             MyConflicts_DashboardName))),
                                          New ConflictOption(ConflictOption.UserChoice.RenameExisting,
                                                             My.Resources.MyConflicts_ChooseANewNameForTheExistingDashboard,
                                                             New ConflictDataHandler("NewName", Nothing,
                                                                                     New ConflictArgument("Dashboard Name",
                                                                                                          "",
                                                                                                          My.Resources.
                                                                                                             MyConflicts_DashboardName))),
                                          New ConflictOption(ConflictOption.UserChoice.Skip,
                                                             My.Resources.
                                                                MyConflicts_DonTImportThisDashboardLeaveTheExistingOneInPlace)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private Shared mNameClash As ConflictDefinition
    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new dashboard component using data from the given provider.
    ''' </summary>
    ''' <param name="prov">The data provider</param>
    Public Sub New(owner As OwnerComponent, prov As IDataProvider)
        Me.New(owner, prov.GetValue("id", Guid.Empty), prov.GetString("name"))
    End Sub


    ''' <summary>
    ''' Creates a new dashboard component using the given id and name.
    ''' </summary>
    ''' <param name="id">The dashboard ID</param>
    ''' <param name="name">The dashboard name</param>
    Public Sub New(owner As OwnerComponent, id As Guid, name As String)
        MyBase.New(owner, id, name)
    End Sub

    ''' <summary>
    ''' Creates a new dashboard component drawing data from the given XML reader.
    ''' </summary>
    ''' <param name="reader">The XML reader</param>
    ''' <param name="ctx">The loading context</param>
    Public Sub New(owner As OwnerComponent, reader As XmlReader, ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Member variables "

    'The ID of the existing dashboard in the event of a clash
    <DataMember>
    Private mExistingDashboardID As Guid

#End Region

#Region " Properties "

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type As PackageComponentType
        Get
            Return PackageComponentType.Dashboard
        End Get
    End Property

    ''' <summary>
    ''' The dashboard object represented by this component.
    ''' </summary>
    Public ReadOnly Property AssociatedDashboard As Dashboard
        Get
            Return CType(AssociatedData, Dashboard)
        End Get
    End Property

    ''' <summary>
    ''' The permission required by a user to import a component of this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            If AssociatedDashboard Is Nothing Then
                Return Nothing
            End If

            Select Case AssociatedDashboard.Type
                Case DashboardTypes.Global
                    Return Permission.Analytics.ImportGlobalDashboard
                Case DashboardTypes.Published
                    Return Permission.Analytics.ImportPublishedDashboard
                Case Else
                    Return Nothing
            End Select
        End Get
    End Property

    ''' <summary>
    ''' The ID of the database dashboard that clashes with the incoming dashboard.
    ''' </summary>
    Public ReadOnly Property ExistingDashboardID As Guid
        Get
            Return mExistingDashboardID
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Dim dash = gSv.GetDashboardById(IdAsGuid)
        dash.Tiles = gSv.GetDashboardTiles(Me.IdAsGuid)
        Return dash
    End Function

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the data in the
    ''' given component differs from the data in this component.
    ''' </summary>
    ''' <param name="comp">The component to check against</param>
    ''' <returns>True if the component differs from this, otherwise false</returns>
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean
        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True

        Dim thisDash As Dashboard = Me.AssociatedDashboard
        If thisDash Is Nothing Then Throw New NoSuchElementException(
         My.Resources.DashboardComponent_NoDashboardWithTheID0WasFound, Id)

        Dim compDash As Dashboard = DirectCast(comp, DashboardComponent).AssociatedDashboard

        If thisDash.Type <> compDash.Type OrElse
            thisDash.Tiles.Count <> compDash.Tiles.Count Then Return True

        For i As Integer = 0 To thisDash.Tiles.Count - 1
            If thisDash.Tiles(i).Size <> compDash.Tiles(i).Size Then Return True
        Next

        Return False
    End Function

    ''' <summary>
    ''' Gets the dependents of this component - for tiles this is the data source.
    ''' </summary>
    ''' <returns>The collection of dependent components</returns>
    Public Overrides Function GetDependents() As ICollection(Of PackageComponent)
        Dim deps As New List(Of PackageComponent)
        For Each dt As DashboardTile In AssociatedDashboard.Tiles
            'Need to add tile and associated data source as dependents
            Dim tComp As New TileComponent(Me.Owner, dt.Tile.ID, dt.Tile.Name)
            deps.AddRange(tComp.GetDependents())
            deps.Add(tComp)
        Next
        Return deps
    End Function

#End Region

#Region " XML Handling "

    ''' <summary>
    ''' Appends XML defining this data source to the given XML Writer.
    ''' </summary>
    ''' <param name="writer">The XML writer</param>

    Protected Overrides Sub WriteXmlHead(ByVal writer As XmlWriter)
        MyBase.WriteXmlHead(writer)
        If AssociatedDashboard IsNot Nothing Then
            writer.WriteAttributeString("type", AssociatedDashboard.Type.ToString())
        End If
    End Sub

    ''' <summary>
    ''' Appends XML defining this data source to the given XML Writer.
    ''' </summary>
    ''' <param name="writer">The XML writer</param>
    Protected Overrides Sub WriteXmlBody(writer As XmlWriter)
        If Me.AssociatedDashboard Is Nothing Then Throw New NoSuchElementException(
         My.Resources.DashboardComponent_NoDashboardWithTheName0Exists, Me.Name)

        writer.WriteStartElement("tile-list")
        For Each dt As DashboardTile In Me.AssociatedDashboard.Tiles
            writer.WriteStartElement("tile")
            writer.WriteAttributeString("id", dt.Tile.ID.ToString())
            writer.WriteAttributeString("name", dt.Tile.Name)
            writer.WriteAttributeString("width", dt.Size.Width.ToString())
            writer.WriteAttributeString("height", dt.Size.Height.ToString())
            writer.WriteEndElement()
        Next
        writer.WriteEndElement()
    End Sub

    ''' <summary>
    ''' Reads this data source from the given XML reader
    ''' </summary>
    ''' <param name="reader">The XML reader</param>
    ''' <param name="ctx">The loading context</param>
    Protected Overrides Sub ReadXmlHead(reader As XmlReader, ctx As IComponentLoadingContext)

        MyBase.ReadXmlHead(reader, ctx)
        Dim type = reader.GetAttribute("type")
        Dim dashboardType = DashboardTypes.Global
        If type IsNot Nothing Then
            dashboardType = DirectCast([Enum].Parse(GetType(DashboardTypes), type), DashboardTypes)
        End If
        Dim dash As New Dashboard(dashboardType, Me.IdAsGuid, Me.Name)
        AssociatedData = dash

    End Sub

    ''' <summary>
    ''' Reads this data source from the given XML reader.
    ''' </summary>
    ''' <param name="r">The XML reader</param>
    ''' <param name="ctx">The loading context</param>
    Protected Overrides Sub ReadXmlBody(r As XmlReader, ctx As IComponentLoadingContext)
        While r.Read()
            If r.LocalName = "tile" Then
                Dim dt As New DashboardTile()
                dt.Tile = New Tile()
                dt.Tile.ID = New Guid(r.GetAttribute("id"))
                dt.Tile.Name = r.GetAttribute("name")
                dt.Size = New Drawing.Size(CInt(r.GetAttribute("width")),
                                           CInt(r.GetAttribute("height")))
                AssociatedDashboard.Tiles.Add(dt)
            End If
        End While
    End Sub

#End Region

#Region " Conflict Handling "

    ''' <summary>
    ''' Gets clashes between this component and objects currently in the database.
    ''' </summary>
    ''' <returns>Collection of clash definitions</returns>
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)
        Dim conflicts As New List(Of ConflictDefinition)
        mExistingDashboardID = Nothing

        'Look for dashboard with matching ID
        Dim existingName As String = gSv.GetDashboardNameByID(Me.IdAsGuid)
        If existingName = String.Empty Then
            'Not found so look for match on name
            Dim existingID As Guid = gSv.GetDashboardIDByName(Me.Name)
            If existingID <> Guid.Empty Then
                'Found so add name clash & record id of the dashboard we've clashed with
                conflicts.Add(MyConflicts.NameClash)
                mExistingDashboardID = existingID
            End If
        Else
            'Found so add ID conflict
            conflicts.Add(MyConflicts.IDClash)
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
    ''' Applies the conflict resolutions to all dashboards in the given release.
    ''' This cannot be an instance method since it must perform cross-component
    ''' checking to ensure there are no internal clashes when renaming dashboards.
    ''' </summary>
    ''' <param name="rel">The release being imported</param>
    ''' <param name="resolutions">The resolutions to apply</param>
    ''' <param name="errors">The error log to append errors to.</param>
    Private Shared Sub ApplyConflictResolutions(rel As clsRelease,
                                                resolutions As ICollection(Of ConflictResolution),
                                                errors As clsErrorLog)

        'Single-component validation
        For Each comp As PackageComponent In rel
            Dim dc As DashboardComponent = TryCast(comp, DashboardComponent)
            If dc IsNot Nothing Then dc.ApplyResolutions(resolutions, errors)
        Next

        'Cross component validation:
        ' 1) Check renames don't clash with non-conflicting incoming names
        ' 2) Check renames don't clash with other renames
        Dim matrix As New clsRenamingMatrix()
        Dim componentsByName As New Dictionary(Of String, DashboardComponent)
        For Each res As ConflictResolution In resolutions
            Dim dc As DashboardComponent = TryCast(res.Conflict.Component, DashboardComponent)
            If dc IsNot Nothing Then
                matrix(dc.Name) = res.GetArgumentString("Dashbaord Name")
                componentsByName(dc.Name) = dc
            End If
        Next

        For Each c As clsRenamingMatrix.Clash In matrix.Validate()
            Dim comp As DashboardComponent = componentsByName(c.Source)

            ' Each clash is in the form of :
            ' c.Source : The source (original) name of the component which clashed
            ' c.OriginalName : The original name of the component it clashed with
            ' c.NewName : The new name of the component it clashed with - may be null
            If c.NewName Is Nothing Then
                errors.Add(comp, My.Resources.DashboardComponent_TheDashboard0CannotBeRenamedTo1ThereIsADashboardInThisReleaseWithThatName, c.Source, c.OriginalName)
            Else
                errors.Add(comp, My.Resources.DashboardComponent_TheIncomingDashboards0And1CannotBothBeRenamedTo2,
                           c.Source, c.OriginalName, c.NewName)
            End If
        Next

    End Sub

    ''' <summary>
    ''' Applies the given resolutions to this dashboard component.
    ''' </summary>
    ''' <param name="resolutions">The resolutions to check</param>
    ''' <param name="errors">The error log to append errors to</param>
    Private Sub ApplyResolutions(resolutions As ICollection(Of ConflictResolution),
                                 errors As clsErrorLog)

        ' Clear down the modifications so we're starting from scratch
        Dim mods As IDictionary(Of ModificationType, Object) = Modifications
        mods.Clear()

        For Each res As ConflictResolution In resolutions
            ' The conflict that the resolution is resolving
            Dim con As Conflict = res.Conflict

            ' Skip those that aren't me
            If con.Component IsNot Me Then Continue For

            ' The definition of that conflict
            Dim defn As ConflictDefinition = con.Definition

            ' The resolution option chosen (may be null if no option is chosen)
            Dim opt As ConflictOption = res.ConflictOption

            ' Deal with the user not choosing an option first - this is an
            ' error for all (current) conflict types.
            If res.ConflictOption Is Nothing Then
                If defn Is MyConflicts.IDClash Then
                    errors.Add(Me, My.Resources.DashboardComponent_YouMustChooseHowToHandleTheConflictingIDForDashboard0, Name)
                ElseIf defn Is MyConflicts.NameClash Then
                    errors.Add(Me, My.Resources.DashboardComponent_YouMustChooseHowToHandleTheConflictingNameForDashboard0, Name)
                End If
                ' Move onto the next resolution - without an option there's little we can
                ' do for this one.
                Continue For
            Else
                Dim err As clsError = ApplyResolution(res, mods)
                If err Is Nothing Then res.Passed = True Else errors.Add(err)
            End If
        Next

    End Sub

    ''' <summary>
    ''' Applies the given resolution to this component, adding any modifications to
    ''' the given map and returning an error as appopriate.
    ''' </summary>
    ''' <param name="res">The conflict resolution to apply</param>
    ''' <param name="mods">The modifications to add</param>
    ''' <returns>The error which this resolution has evoked, or null</returns>
    Private Function ApplyResolution(res As ConflictResolution,
                    mods As IDictionary(Of ModificationType, Object)) As clsError

        'The definition of that conflict
        Dim defn As ConflictDefinition = res.Conflict.Definition

        'The resolution option chosen (may be null if no option is chosen)
        Dim opt As ConflictOption = res.ConflictOption
        If opt Is Nothing Then Throw New ArgumentNullException("res.ConflictOption")

        'The proposed name - null if not applicable
        Dim proposedName As String = res.GetArgumentString("Dashboard Name")

        'Validate and apply the resolutions for the chosen options
        If opt.Choice = ConflictOption.UserChoice.Skip Then
            'Flag to skip tile
            mods(ModificationType.Skip) = True

        ElseIf opt.Choice = ConflictOption.UserChoice.Overwrite Then
            'Flag to overwrite dashboard, recording old ID if there was a name clash
            mods(ModificationType.OverwritingExisting) = True
            If defn Is MyConflicts.NameClash Then
                mods(ModificationType.IncomingId) = mExistingDashboardID
            End If

        Else
            'If mExistingDashboardID is set then we're either renaming (either the incoming or
            'existing item) following a name clash
            If mExistingDashboardID <> Nothing OrElse gSv.GetDashboardIDByName(Me.Name) <> Guid.Empty Then
                Dim thing As String = "incoming dashboard"
                If opt.Choice = ConflictOption.UserChoice.RenameExisting Then thing = "existing dashboard"

                'If the incoming name matches the existing tile name with the
                'same ID as this tile, then we must capture a new name.
                If proposedName = "" Then Return New clsError(
                    Me, My.Resources.DashboardComponent_YouMustProvideAnAlternativeNameForThe01, thing, Name)

                'Check that the new name isn't also in use
                If gSv.GetDashboardIDByName(proposedName) <> Guid.Empty Then Return New clsError(
                    Me, My.Resources.DashboardComponent_YouCannotRenameThe01To2ThatNameIsAlreadyInUseInThisEnvironment, thing, Name, proposedName)

                'Proposed name is valid
                If opt.Choice = ConflictOption.UserChoice.RenameExisting Then
                    mods(ModificationType.ExistingName) = proposedName
                Else
                    mods(ModificationType.IncomingName) = proposedName
                End If
            End If
            If opt.Choice = ConflictOption.UserChoice.NewId Then
                'If it was an ID clash then get a new ID
                mods(ModificationType.IncomingId) = Guid.NewGuid()
            End If
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the data handler for the given conflict option.
    ''' </summary>
    ''' <param name="c">The conflict</param>
    ''' <param name="o">The conflict option chosen.</param>
    ''' <returns>The handler to be used for that particular option in the given
    ''' conflict, or null if no further data is required.</returns>
    Public Overrides Function GetHandlerForOption(c As Conflict,
                                                  o As ConflictOption) As ConflictDataHandler

        If (o.Choice = ConflictOption.UserChoice.NewId AndAlso gSv.GetTileIDByName(Me.Name) <> Guid.Empty) _
         OrElse o.Choice = ConflictOption.UserChoice.Rename OrElse o.Choice = ConflictOption.UserChoice.RenameExisting Then
            Dim h As ConflictDataHandler = o.CreateHandler("NewName")
            ' Default the name to the name of this component
            CollectionUtil.First(h.Arguments).Value = New clsProcessValue(Name)
            Return h
        End If

        ' Otherwise no handler required
        Return Nothing
    End Function

#End Region

End Class
