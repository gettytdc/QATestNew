Imports System.Xml

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Groups
Imports System.Runtime.Serialization
Imports BluePrism.Core.Xml
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Component representing a tile.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class TileComponent : Inherits PackageComponent

#Region " Conflict definitions "

    ''' <summary>
    ''' The conflict definitions which are handled by the tile component.
    ''' </summary>
    Public Class MyConflicts
        Public Shared ReadOnly Property IDClash As ConflictDefinition
            Get
                If mIDClash Is Nothing Then
                    mIDClash = GetIdClashConflictDefinition()
                Else
                    Dim conflict = GetIdClashConflictDefinition()
                    mIDClash.Hint = conflict.Hint
                    mIDClash.Text = conflict.Text
                    mIDClash.Options = conflict.Options
                End If
                Return mIDClash
            End Get
        End Property

        Private Shared Function GetIdClashConflictDefinition() As ConflictDefinition

            Return New ConflictDefinition("TileIDClash",
                                          My.Resources.MyConflicts_ATileWithTheSameIDAlreadyExistsInTheDatabase,
                                          My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                          New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                             My.Resources.
                                                                MyConflicts_OverwriteTheExistingTileWithTheIncomingTile),
                                          New ConflictOption(ConflictOption.UserChoice.NewId,
                                                             My.Resources.MyConflicts_ChooseANewIDForTheIncomingTile,
                                                             New ConflictDataHandler("NewName",
                                                                                     My.Resources.
                                                                                        MyConflicts_TheTileWithTheSameIDAlsoHasTheSameNamePleaseEnterANewName,
                                                                                     New ConflictArgument("Tile Name", "",
                                                                                                          My.Resources.
                                                                                                             MyConflicts_TileName))),
                                          New ConflictOption(ConflictOption.UserChoice.Skip,
                                                             My.Resources.
                                                                MyConflicts_DonTImportThisTileLeaveTheExistingOneInPlace)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private Shared mIDClash As ConflictDefinition

        Public Shared readonly Property NameClash As ConflictDefinition 
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

            Return New ConflictDefinition("TileNameClash",
                                          My.Resources.MyConflicts_ATileWithTheSameNameAlreadyExistsInTheDatabase,
                                          My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                          New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                             My.Resources.
                                                                MyConflicts_OverwriteTheExistingTileWithTheIncomingTile),
                                          New ConflictOption(ConflictOption.UserChoice.Rename,
                                                             My.Resources.MyConflicts_ChooseANewNameForTheIncomingTile,
                                                             New ConflictDataHandler("NewName", Nothing,
                                                                                     New ConflictArgument("Tile Name", "",
                                                                                                          My.Resources.
                                                                                                             MyConflicts_TileName))),
                                          New ConflictOption(ConflictOption.UserChoice.RenameExisting,
                                                             My.Resources.MyConflicts_ChooseANewNameForTheExistingTile,
                                                             New ConflictDataHandler("NewName", Nothing,
                                                                                     New ConflictArgument("Tile Name", "",
                                                                                                          My.Resources.
                                                                                                             MyConflicts_TileName))),
                                          New ConflictOption(ConflictOption.UserChoice.Skip,
                                                             My.Resources.
                                                                MyConflicts_DonTImportThisTileLeaveTheExistingOneInPlace)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private shared mNameClash As ConflictDefinition
    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new tile component using data from the given provider.
    ''' </summary>
    ''' <param name="prov">The data provider</param>
    Public Sub New(owner As OwnerComponent, prov As IDataProvider)
        Me.New(owner, prov.GetValue("id", Guid.Empty), prov.GetString("name"))
    End Sub

    ''' <summary>
    ''' Creates a new tile component using the given id and name.
    ''' </summary>
    ''' <param name="id">The tile ID</param>
    ''' <param name="name">The tile name</param>
    Public Sub New(owner As OwnerComponent, id As Guid, name As String)
        MyBase.New(owner, id, name)
    End Sub

    ''' <summary>
    ''' Creates a new tile component drawing data from the given XML reader.
    ''' </summary>
    ''' <param name="reader">The XML reader</param>
    ''' <param name="ctx">The loading context</param>
    Public Sub New(owner As OwnerComponent, reader As XmlReader, ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Member variables "

    'The ID of the existing tile in the event of a clash
    <DataMember>
    Private mExistingTileID As Guid

#End Region

#Region " Properties "

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type As PackageComponentType
        Get
            Return PackageComponentType.Tile
        End Get
    End Property

    ''' <summary>
    ''' The tile object represented by this component.
    ''' </summary>
    Public ReadOnly Property AssociatedTile As Tile
        Get
            Return CType(AssociatedData, Tile)
        End Get
    End Property

    ''' <summary>
    ''' The permission required by a user to import a component of this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return "Import Tile"
        End Get
    End Property

    ''' <summary>
    ''' The ID of the database tile that clashes with the incoming tile.
    ''' </summary>
    Public ReadOnly Property ExistingTileID As Guid
        Get
            Return mExistingTileID
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return gSv.GetTileDefinition(Me.IdAsGuid)
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

        Dim thisTile As Tile = Me.AssociatedTile
        If thisTile Is Nothing Then Throw New NoSuchElementException(
         My.Resources.TileComponent_NoTileWithTheID0WasFound, Id)

        Dim compTile As Tile = DirectCast(comp, TileComponent).AssociatedTile

        If thisTile.Description <> compTile.Description OrElse
            thisTile.Type <> compTile.Type OrElse
            thisTile.RefreshInterval <> compTile.RefreshInterval OrElse
            thisTile.XMLProperties <> compTile.XMLProperties Then Return True

        Return False
    End Function

    ''' <summary>
    ''' Gets the dependents of this component - for tiles this is the data source.
    ''' </summary>
    ''' <returns>The collection of dependent components</returns>
    Public Overrides Function GetDependents() As ICollection(Of PackageComponent)
        Dim deps As New List(Of PackageComponent)
        Dim ds As String = ExtractDataSource()
        If ds <> String.Empty AndAlso Not ds.StartsWith("BPDS_") Then
            'Only export custom data sources
            deps.Add(New DataSourceComponent(Me.Owner, ds))
        End If
        Return deps
    End Function

    ''' <summary>
    ''' Returns any group assignments for the tile represented by this component.
    ''' </summary>
    Public Overrides Function GetGroupInfo() As IDictionary(Of Guid, String)
        Dim mem As New TileGroupMember()
        mem.Id = Id
        Return gSv.GetPathsToMember(mem)
    End Function

    ''' <summary>
    ''' Returns the data source associated with this tile. Currently there are only
    ''' chart tiles, so this will need to be done differently as more tile types get
    ''' added.
    ''' </summary>
    Private Function ExtractDataSource() As String
        Try
            Dim props As New ReadableXmlDocument(AssociatedTile.XMLProperties)
            Return props.GetElementsByTagName("Procedure")(0).Attributes("name").Value
        Catch ex As Exception
            Return String.Empty
        End Try
    End Function

#End Region

#Region " XML Handling "

    ''' <summary>
    ''' Appends XML defining this tile to the given XML Writer.
    ''' </summary>
    ''' <param name="writer">The XML writer</param>
    Protected Overrides Sub WriteXmlBody(writer As XmlWriter)
        If Me.AssociatedTile Is Nothing Then Throw New NoSuchElementException(
         My.Resources.TileComponent_NoTileWithTheName0Exists, Me.Name)

        writer.WriteAttributeString("type", CStr(Me.AssociatedTile.Type))
        writer.WriteAttributeString("description", Me.AssociatedTile.Description)
        writer.WriteAttributeString("autorefresh", CStr(Me.AssociatedTile.RefreshInterval))
        writer.WriteCData(Me.AssociatedTile.XMLProperties)
    End Sub

    ''' <summary>
    ''' Reads this component from the given XML reader.
    ''' </summary>
    ''' <param name="r">The XML reader</param>
    ''' <param name="ctx">The loading context</param>
    Protected Overrides Sub ReadXmlBody(r As XmlReader, ctx As IComponentLoadingContext)
        Dim t As New Tile()
        t.ID = Me.IdAsGuid
        t.Name = Me.Name
        t.Type = CType(r("type"), TileTypes)
        t.Description = r("description")
        t.RefreshInterval = CType(r("autorefresh"), TileRefreshIntervals)
        t.XMLProperties = r.ReadElementContentAsString()
        Me.AssociatedData = t
    End Sub

#End Region

#Region " Conflict Handling "

    ''' <summary>
    ''' Gets clashes between this component and objects currently in the database.
    ''' </summary>
    ''' <returns>Collection of clash definitions</returns>
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)
        Dim conflicts As New List(Of ConflictDefinition)
        mExistingTileID = Nothing

        'Look for tile with matching ID
        Dim existingName As String = gSv.GetTileNameByID(Me.IdAsGuid)
        If existingName = String.Empty Then
            'Not found so look for match on name
            Dim existingID As Guid = gSv.GetTileIDByName(Me.Name)
            If existingID <> Guid.Empty Then
                'Found so add name clash & record id of the tile we've clashed with
                conflicts.Add(MyConflicts.NameClash)
                mExistingTileID = existingID
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
    ''' Applies the conflict resolutions to all tiles in the given release.
    ''' This cannot be an instance method since it must perform cross-component
    ''' checking to ensure there are no internal clashes when renaming tiles.
    ''' </summary>
    ''' <param name="rel">The release being imported</param>
    ''' <param name="resolutions">The resolutions to apply</param>
    ''' <param name="errors">The error log to append errors to.</param>
    Private Shared Sub ApplyConflictResolutions(rel As clsRelease,
                                                resolutions As ICollection(Of ConflictResolution),
                                                errors As clsErrorLog)

        'Single-component validation
        For Each comp As PackageComponent In rel
            Dim tc As TileComponent = TryCast(comp, TileComponent)
            If tc IsNot Nothing Then tc.ApplyResolutions(resolutions, errors)
        Next

        'Cross component validation:
        ' 1) Check renames don't clash with non-conflicting incoming names
        ' 2) Check renames don't clash with other renames
        Dim matrix As New clsRenamingMatrix()
        Dim componentsByName As New Dictionary(Of String, TileComponent)
        For Each res As ConflictResolution In resolutions
            Dim tc As TileComponent = TryCast(res.Conflict.Component, TileComponent)
            If tc IsNot Nothing Then
                matrix(tc.Name) = res.GetArgumentString("Tile Name")
                componentsByName(tc.Name) = tc
            End If
        Next

        For Each c As clsRenamingMatrix.Clash In matrix.Validate()
            Dim comp As TileComponent = componentsByName(c.Source)

            ' Each clash is in the form of :
            ' c.Source : The source (original) name of the component which clashed
            ' c.OriginalName : The original name of the component it clashed with
            ' c.NewName : The new name of the component it clashed with - may be null
            If c.NewName Is Nothing Then
                errors.Add(comp, My.Resources.TileComponent_TheTile0CannotBeRenamedTo1ThereIsATileInThisReleaseWithThatName, c.Source, c.OriginalName)
            Else
                errors.Add(comp, My.Resources.TileComponent_TheIncomingTiles0And1CannotBothBeRenamedTo2,
                           c.Source, c.OriginalName, c.NewName)
            End If
        Next

    End Sub

    ''' <summary>
    ''' Applies the given resolutions to this tile component.
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
                    errors.Add(Me, My.Resources.TileComponent_YouMustChooseHowToHandleTheConflictingIDForTile0, Name)
                ElseIf defn Is MyConflicts.NameClash Then
                    errors.Add(Me, My.Resources.TileComponent_YouMustChooseHowToHandleTheConflictingNameForTile0, Name)
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
        Dim proposedName As String = res.GetArgumentString("Tile Name")

        'Validate and apply the resolutions for the chosen options
        If opt.Choice = ConflictOption.UserChoice.Skip Then
            'Flag to skip tile
            mods(ModificationType.Skip) = True

        ElseIf opt.Choice = ConflictOption.UserChoice.Overwrite Then
            'Flag to overwrite tile, recording old ID if there was a name clash
            mods(ModificationType.OverwritingExisting) = True
            If defn Is MyConflicts.NameClash Then
                mods(ModificationType.IncomingId) = mExistingTileID
            End If

        Else
            'If mExistingTile is set then we're renaming (either the incoming or
            'existing item) following a name clash
            If mExistingTileID <> Nothing OrElse gSv.GetTileIDByName(Me.Name) <> Guid.Empty Then
                Dim thing As String = "incoming tile"
                If opt.Choice = ConflictOption.UserChoice.RenameExisting Then thing = "existing tile"

                'If the incoming name matches the existing tile name with the
                'same ID as this tile, then we must capture a new name.
                If proposedName = "" Then Return New clsError(
                    Me, My.Resources.TileComponent_YouMustProvideAnAlternativeNameForThe01, thing, Name)

                'Check that the new name isn't also in use
                If gSv.GetTileIDByName(proposedName) <> Guid.Empty Then Return New clsError(
                    Me, My.Resources.TileComponent_YouCannotRenameThe01To2ThatNameIsAlreadyInUseInThisEnvironment, thing, Name, proposedName)

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
