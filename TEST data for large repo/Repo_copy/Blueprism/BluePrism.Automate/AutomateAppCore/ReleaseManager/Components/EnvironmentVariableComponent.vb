Imports System.Xml

Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.AutomateProcessCore

Imports BluePrism.Server.Domain.Models
Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization

''' <summary>
''' Component representing an environment variable.
''' Slightly odd in that for this, the name <em>is</em> the ID.
''' </summary>
<Serializable, DataContract([Namespace]:="bp"), KnownType(GetType(clsEnvironmentVariable))> _
Public Class EnvironmentVariableComponent : Inherits NameEqualsIdComponent

#Region " Constructors "

    ''' <summary>
    ''' Creates a new environment variable component from the given provider.
    ''' </summary>
    ''' <param name="prov">The provider of data for this component. This expects a
    ''' single property :- name : String</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        MyBase.New(owner, prov.GetString("name"))
    End Sub

    ''' <summary>
    ''' Creates a new component representing the given environment variable
    ''' </summary>
    ''' <param name="ev">The variable to create a component from.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal ev As clsEnvironmentVariable)
        MyBase.New(owner, ev.Name)
        Me.AssociatedData = ev
    End Sub

    ''' <summary>
    ''' Creates a new component representing an environment variable with the given
    ''' name.
    ''' </summary>
    ''' <param name="name">The name of the environment variable that this component
    ''' should represent.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal name As String)
        MyBase.New(owner, name)
    End Sub

    ''' <summary>
    ''' Creates a new component from data in the given reader, using the specified
    ''' loading context.
    ''' </summary>
    ''' <param name="reader">The reader from which to draw the XML with which this
    ''' component should be populated.</param>
    ''' <param name="ctx">The object providing context for the loading of this
    ''' component.</param>
    Public Sub New(ByVal owner As OwnerComponent, _
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
            Return PackageComponentType.EnvironmentVariable
        End Get
    End Property

    ''' <summary>
    ''' Gets the environment variable associated with this component or null if it
    ''' currently has no associated environment variable.
    ''' </summary>
    Public ReadOnly Property AssociatedEnvVar() As clsEnvironmentVariable
        Get
            Return DirectCast(AssociatedData, clsEnvironmentVariable)
        End Get
    End Property

    ''' <summary>
    ''' Adds the permissions which are required by this component that the currently
    ''' logged in user is missing.
    ''' Generally, this collection should contain the names of the required
    ''' permissions, but if the rules are more complex (eg. either of 2 permissions
    ''' will satisfy the requirement), the output can be tailored to match the rule,
    ''' but each type of component should produce the same output for the same user.
    ''' </summary>
    ''' <param name="perms">The collection of permissions to add to</param>
    Public Overrides Sub AddMissingImportPermissions(ByVal perms As ICollection(Of String))

        ' If the user has either of these permissions then it's fine.
        If User.Current.HasPermission( _
         "Processes - Configure Environment Variables", _
         "Business Objects - Configure Environment Variables") Then Return

        ' The perm name is just too long to put them both out with an -or- between them.
        perms.Add("Processes/Business Objects - Configure Environment Variables")

    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Loads the database data for this component.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Return gSv.GetEnvironmentVariable(Me.Name)
    End Function

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the exportable data
    ''' in the given component differs from the data in this component.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean
        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True
        Return Not AssociatedEnvVar.SameVariable( _
         DirectCast(comp, EnvironmentVariableComponent).AssociatedEnvVar)
    End Function

    ''' <summary>
    ''' Appends XML defining this environment variable to the given XML Writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this environment variable should be
    ''' written.</param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)

        Dim ev As clsEnvironmentVariable = AssociatedEnvVar
        If ev Is Nothing Then Throw New NoSuchElementException( _
         "No environment variable with the name '{0}' exists", Me.Name)

        writer.WriteAttributeString("type", ev.Value.EncodedType)
        writer.WriteAttributeString("value", ev.Value.EncodedValue)
        writer.WriteElementString("description", ev.Description)

    End Sub

    ''' <summary>
    ''' Reads this component from the given XML reader.
    ''' </summary>
    ''' <param name="r">The reader to draw the XML from.</param>
    ''' <param name="ctx">The loading context.</param>
    Protected Overrides Sub ReadXmlBody(ByVal r As XmlReader, ByVal ctx As IComponentLoadingContext)
        Dim ev As New clsEnvironmentVariable()
        ev.Name = Me.Name
        ev.Value = clsProcessValue.Decode(r("type"), r("value"))
        While r.Read()
            If r.NodeType = XmlNodeType.Element AndAlso r.LocalName = "description" Then
                r.Read() ' Get to the text element
                ev.Description = r.Value
                Exit While
            End If
        End While
        Me.AssociatedData = ev
    End Sub

#End Region

End Class
