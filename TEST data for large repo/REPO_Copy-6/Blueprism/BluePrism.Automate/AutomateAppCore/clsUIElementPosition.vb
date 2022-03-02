Imports BluePrism.BPCoreLib.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Runtime.Serialization

''' <summary>
''' Class representing a UI element position - typically a toolbar.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class clsUIElementPosition

#Region " Class Scope Declarations "

    ''' <summary>
    ''' Converts the given char to a dock style used in the <see cref="Dock"/>
    ''' property.
    ''' </summary>
    ''' <param name="c">The character - should be 't', 'b', 'l' or 'r' to represent
    ''' 'top', 'bottom', 'left' and 'right' respectively. If it is anything other
    ''' than one of these values, the representation of 'top' is assumed.</param>
    ''' <returns>The DockStyle corresponding to the character given</returns>
    Public Shared Function CharToDock(ByVal c As Char) As DockStyle
        Select Case c
            Case "b"c : Return DockStyle.Bottom
            Case "l"c : Return DockStyle.Left
            Case "r"c : Return DockStyle.Right
            Case Else : Return DockStyle.Top
        End Select
    End Function

    ''' <summary>
    ''' Converts the given dock style into a representative character.
    ''' </summary>
    ''' <param name="d">The dock style to convert</param>
    ''' <returns>The character which can represent the supplied dock style. This will
    ''' be one of 't', 'b', 'l' or 'r' to represent 'top', 'bottom', 'left' and
    ''' 'right' respectively.</returns>
    Public Shared Function DockToChar(ByVal d As DockStyle) As Char
        Select Case d
            Case DockStyle.Bottom : Return "b"c
            Case DockStyle.Left : Return "l"c
            Case DockStyle.Right : Return "r"c
            Case Else : Return "t"c
        End Select
    End Function

#End Region

#Region " Member Variables "

    ' The name of the UI element
    <DataMember>
    Private mName As String

    ' The area of the screen the element should be docked to
    <DataMember>
    Private mDock As DockStyle

    ' The location of the element
    <DataMember>
    Private mLocation As Point

    ' Whether the element is visible or not
    <DataMember>
    Private mVisible As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty UI element position object.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, DockStyle.Top, Point.Empty, True)
    End Sub

    ''' <summary>
    ''' Creates a new UI element with the given name.
    ''' </summary>
    ''' <param name="name">The identifying name of the UI element</param>
    Public Sub New(ByVal name As String)
        Me.New(name, DockStyle.Top, Point.Empty, True)
    End Sub

    ''' <summary>
    ''' Creates a new UI element with the given properties.
    ''' </summary>
    ''' <param name="name">The identifying name of the UI element.</param>
    ''' <param name="dock">The dock position of the element.</param>
    ''' <param name="x">The x co-ord at which the element is located.</param>
    ''' <param name="y">The y co-ord at which the element is located.</param>
    ''' <param name="visible">Flag indicating if the element is visible.</param>
    Public Sub New(ByVal name As String, _
     ByVal dock As Char, ByVal x As Integer, ByVal y As Integer, ByVal visible As Boolean)
        Me.New(name, CharToDock(dock), New Point(x, y), visible)
    End Sub

    ''' <summary>
    ''' Creates a new UI element with the given properties.
    ''' </summary>
    ''' <param name="name">The identifying name of the UI element.</param>
    ''' <param name="dock">The dock position of the element.</param>
    ''' <param name="locn">The point at which the element is located.</param>
    ''' <param name="visible">Flag indicating if the element is visible.</param>
    Public Sub New( _
     ByVal name As String, ByVal dock As Char, ByVal locn As Point, ByVal visible As Boolean)
        Me.New(name, CharToDock(dock), locn, visible)
    End Sub

    ''' <summary>
    ''' Creates a new UI element with properties gleaned from the given data provider
    ''' </summary>
    ''' <param name="prov">The data provider. This is expected to provide the
    ''' following data :- <list>
    ''' <item>name : string</item>
    ''' <item>position : char (one of 't', 'b', 'l', 'r' to represent top, bottom,
    ''' left, right, respectively)</item>
    ''' <item>x : integer</item>
    ''' <item>y : integer</item>
    ''' <item>visible : boolean</item>
    ''' </list></param>
    Public Sub New(ByVal prov As IDataProvider)
        Me.New( _
         prov.GetString("name"), _
         CharToDock(prov.GetValue("position", "t"c)), _
         New Point(prov.GetValue("x", 0), prov.GetValue("y", 0)), _
         prov.GetValue("visible", True) _
        )
    End Sub

    ''' <summary>
    ''' Creates a new UI element with the given properties.
    ''' </summary>
    ''' <param name="name">The identifying name of the UI element.</param>
    ''' <param name="dock">The dock position of the element.</param>
    ''' <param name="locn">The point at which the element is located.</param>
    ''' <param name="visible">Flag indicating if the element is visible.</param>
    Public Sub New( _
     ByVal name As String, ByVal dock As DockStyle, ByVal locn As Point, ByVal visible As Boolean)
        mName = name
        mDock = dock
        mLocation = locn
        mVisible = visible
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The name of the UI element
    ''' </summary>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    ''' <summary>
    ''' The area of the screen to which the UI element should be docked
    ''' </summary>
    Public Property Dock() As DockStyle
        Get
            Return mDock
        End Get
        Set(ByVal value As DockStyle)
            mDock = value
        End Set
    End Property

    ''' <summary>
    ''' The area of the screen that the element should be docked into, as a
    ''' character
    ''' </summary>
    Public Property DockChar() As Char
        Get
            Return DockToChar(mDock)
        End Get
        Set(ByVal value As Char)
            mDock = CharToDock(value)
        End Set
    End Property

    ''' <summary>
    ''' The location of the UI element.
    ''' </summary>
    Public Property Location() As Point
        Get
            Return mLocation
        End Get
        Set(ByVal value As Point)
            mLocation = value
        End Set
    End Property

    ''' <summary>
    ''' The X co-ordinate of the UI element
    ''' </summary>
    Public Property X() As Integer
        Get
            Return mLocation.X
        End Get
        Set(ByVal value As Integer)
            mLocation.X = value
        End Set
    End Property

    ''' <summary>
    ''' The Y co-ordinate of the UI element.
    ''' </summary>
    Public Property Y() As Integer
        Get
            Return mLocation.Y
        End Get
        Set(ByVal value As Integer)
            mLocation.Y = value
        End Set
    End Property

    ''' <summary>
    ''' Whether this ui element is visible or not.
    ''' </summary>
    Public Property Visible() As Boolean
        Get
            Return mVisible
        End Get
        Set(ByVal value As Boolean)
            mVisible = True
        End Set
    End Property

#End Region

End Class
