Imports System.Xml
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.Server.Domain.Models

<Serializable, DataContract([Namespace]:="bp")>
Public Class FontComponent : Inherits NameEqualsIdComponent

#Region " Class Scope Definitions "

    ''' <summary>
    ''' The conflict definitions which are handled by the font component.
    ''' </summary>
    Public Class MyConflicts

        ''' <summary>
        ''' 'Conflict' which occurs when a font being imported already exists in
        ''' the target environment.
        ''' </summary>
        Public Shared Readonly Property ExistingFont As ConflictDefinition
        Get
            If mExistingFont Is Nothing Then
                mExistingFont = GetExistingFontConflictDefinition()
            Else
                mExistingFont.UpdateConflictDefinitionStrings(GetExistingFontConflictDefinition())
            End If
            Return mExistingFont
        End Get
        End Property
        Public Shared Function GetExistingFontConflictDefinition() As ConflictDefinition 
            Return  New ConflictDefinition("ExistingFont", My.Resources.MyConflicts_AFontAlreadyExistsWithThisName,
                                           My.Resources.MyConflicts_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                           New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                                              My.Resources.
                                                                 MyConflicts_OverwriteTheExistingFontWithTheIncomingFont),
                                           New ConflictOption(ConflictOption.UserChoice.Skip,
                                                              My.Resources.MyConflicts_DonTImportThisFont)) _
                With {.DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                    .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite}
        End Function

        Private Shared mExistingFont As ConflictDefinition

    End Class

#End Region

#Region " Members "

    ' A version.
    <DataMember>
    Private mVersion As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new environment variable component from the given provider.
    ''' </summary>
    ''' <param name="prov">The provider of data for this component. This expects a
    ''' single property :- name : String</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        Me.New(owner, prov.GetString("name"), prov.GetString("version"))
    End Sub

    ''' <summary>
    ''' Creates a new component representing an environment variable with the given
    ''' name.
    ''' </summary>
    ''' <param name="name">The name of the environment variable that this component
    ''' should represent.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal name As String, ByVal version As String)
        MyBase.New(owner, name)
        mVersion = version
    End Sub

    ''' <summary>
    ''' Creates a new component from data in the given reader, using the specified
    ''' loading context.
    ''' </summary>
    ''' <param name="reader">The reader from which to draw the XML with which this
    ''' component should be populated.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' component.</param>
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.Font
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return "System - Fonts"
        End Get
    End Property

    ''' <summary>
    ''' The font associated with this component.
    ''' </summary>
    Public ReadOnly Property AssociatedFontData() As String
        Get
            Return DirectCast(AssociatedData, String)
        End Get
    End Property

    ''' <summary>
    ''' The version number held for this font.
    ''' </summary>
    Public Property Version() As String
        Get
            If mVersion Is Nothing Then Return ""
            Return mVersion
        End Get
        Set(ByVal value As String)
            mVersion = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the delegate responsible for applying the conflict resolutions.
    ''' </summary>
    Public Overrides ReadOnly Property ResolutionApplier() As Conflict.Resolver
        Get
            Return AddressOf ApplyConflictResolutions
        End Get
    End Property

    ''' <summary>
    ''' Override to indicate that this component updates font data
    ''' </summary>
    Public Overrides ReadOnly Property VersionDataName() As String
        Get
            Return DataNames.Font
        End Get
    End Property

#End Region

#Region " Xml Handling "

    ''' <summary>
    ''' Writes the XML head for a font component.
    ''' </summary>
    ''' <param name="writer">The writer to which this font component's header data
    ''' should be written.</param>
    Protected Overrides Sub WriteXmlHead(ByVal writer As XmlWriter)
        MyBase.WriteXmlHead(writer)
        ' When writing out, just test the version - if we don't have it, default to 1.0
        writer.WriteAttributeString("version", CStr(IIf(mVersion = "", "1.0", mVersion)))
    End Sub

    ''' <summary>
    ''' Appends XML defining this environment variable to the given XML Writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this environment variable should be
    ''' written.</param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)
        writer.WriteString(AssociatedFontData)
    End Sub

    ''' <summary>
    ''' Writes the XML head for a font component.
    ''' </summary>
    ''' <param name="r">The reader to which this font component's header data
    ''' should be read.</param>
    ''' <param name="ctx">The context </param>
    Protected Overrides Sub ReadXmlHead(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.ReadXmlHead(r, ctx)
        ' When reading in, just leave the version blank if we don't have it.
        mVersion = r("version")
    End Sub
    ''' <summary>
    ''' Reads this component from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader to draw the XML from.</param>
    ''' <param name="ctx">The loading context.</param>
    Protected Overrides Sub ReadXmlBody(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)
        ' We're on the opening XML Element - the contents *is* the font data.
        If Not r.IsEmptyElement Then
            r.Read()
            AssociatedData = r.Value
        End If
    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return gSv.GetFont(Me.Name, mVersion)
    End Function

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the data in the
    ''' given component differs from the data in this component.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean
        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True

        Dim xml As String = AssociatedFontData
        Dim compxml As String = DirectCast(comp, FontComponent).AssociatedFontData

        Return (Normalise(xml) <> Normalise(compxml))
    End Function

    ''' <summary>
    ''' Replaces any CRLF char pairs in the given XML with a single LF char so that
    ''' the XML ends up 'normalised'. MS XML Writer does this automatically, so that
    ''' a written WSDL may end up different to a stored WSDL, even though it should
    ''' be identical.
    ''' </summary>
    ''' <param name="xml">The XML to normalise</param>
    ''' <returns>The normalised XML, or null if null was given.</returns>
    Private Function Normalise(ByVal xml As String) As String
        If xml Is Nothing Then Return Nothing
        Return xml.Replace(vbCrLf, vbLf)
    End Function

    ''' <summary>
    ''' Gets collisions between this component and the current database.
    ''' </summary>
    ''' <returns>A collection of collision types which will occur when importing this
    ''' component into the database.</returns>
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)
        If gSv.GetFont(Name, Nothing) Is Nothing Then
            Return GetEmpty.ICollection(Of ConflictDefinition)()
        End If
        Return GetSingleton.ICollection(MyConflicts.ExistingFont)
    End Function

    ''' <summary>
    ''' Applies all the conflict resolutions to this component - in reality, it only
    ''' needs one and there is no cross-component validation required for a
    ''' credential, so we can use an instance method rather than a static one.
    ''' </summary>
    ''' <param name="rel">The release in which the conflict occurred.</param>
    ''' <param name="resolutions">All the conflict resolutions for the release.
    ''' </param>
    ''' <param name="errors">The error log to which any validation errors should be
    ''' reported.</param>
    Private Sub ApplyConflictResolutions(ByVal rel As clsRelease,
     ByVal resolutions As ICollection(Of ConflictResolution), ByVal errors As clsErrorLog)
        Dim mods As IDictionary(Of ModificationType, Object) = Modifications

        ' Clear down the modifications so we're starting from scratch
        mods.Clear()

        For Each res As ConflictResolution In resolutions
            If res.Conflict.Component IsNot Me Then Continue For

            ' Check that the user has selected an option
            Dim opt As ConflictOption = res.ConflictOption
            If opt Is Nothing Then
                errors.Add(Me, My.Resources.FontComponent_YouMustChooseHowToHandleTheNewFont0, Me.Name)
            Else
                Select Case opt.Choice
                    Case ConflictOption.UserChoice.Skip
                        mods.Add(ModificationType.Skip, True)
                    Case ConflictOption.UserChoice.Overwrite
                        mods.Add(ModificationType.OverwritingExisting, True)
                    Case Else
                        Throw New BluePrismException(My.Resources.FontComponent_UnrecognisedOption0, opt.Choice)
                End Select
                res.Passed = True
            End If
            ' There's only one conflict resolution for fonts, since there's only one conflict
            ' No point in going through the rest of the loop.
            Return
        Next

    End Sub


#End Region

End Class
