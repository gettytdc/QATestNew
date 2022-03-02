Imports System.Collections.Generic
Imports System.Runtime.Serialization

Namespace BluePrism.ApplicationManager.AMI
    ''' Project  : AMI
    ''' Class    : clsElementTypeInfo
    ''' 
    ''' <summary>
    ''' This class is used to return information to the client about a particular
    ''' Element Type.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsElementTypeInfo

        ''' <summary>
        ''' Used to informally identify the different application types described by an
        ''' element, for the purposes of deciding the correct query to use, etc.
        ''' </summary>
        Public Enum AppTypes
            Application
            Win32
            HTML
            AA
            UIAutomation
            Web
            Java
            Mainframe
            SAP
            NET
            DDE
        End Enum

        ''' <summary>
        ''' Get the type of application this element belongs to - one of AppTypes. This
        ''' is an informal indentification for the purposes of deciding on the correct
        ''' query, etc.
        ''' </summary>
        Public ReadOnly Property AppType() As AppTypes
            Get
                Return mAppType
            End Get
        End Property
        <DataMember>
        Private mAppType As AppTypes

        ''' <summary>
        ''' The identifier for this Element Type. For Active Accessibility, HTML and
        ''' Java element types, the ID is prefixed with AA, HTML or Java respectively.
        ''' </summary>
        Public ReadOnly Property ID() As String
            Get
                Return mID
            End Get
        End Property
        <DataMember>
        Private mID As String

        ''' <summary>
        ''' The name of this element type, which is in 'friendly' form for display
        ''' to the user.
        ''' </summary>
        Public ReadOnly Property Name() As String
            Get
                Return mName
            End Get
        End Property
        <DataMember>
        Private mName As String

        ''' <summary>
        ''' The name of this element type, which is in 'friendly' form for display
        ''' to the user.
        ''' </summary>
        Public ReadOnly Property HelpText() As String
            Get
                Return mHelpText
            End Get
        End Property
        <DataMember>
        Private mHelpText As String

        ''' <summary>
        ''' The default data type for this element type. This is a standard Automate
        ''' data type identifier, e.g. "text", or an empty string if there is no default
        ''' type defined.
        ''' </summary>
        Public ReadOnly Property DefaultDataType() As String
            Get
                Return mDefaultDataType
            End Get
        End Property
        <DataMember>
        Private mDefaultDataType As String

        ''' <summary>
        ''' Indicates whether the element can be written to, using some form
        ''' of query (ultimately from a write stage). 
        ''' </summary>
        ''' <remarks>E.g. there is no java API support for a scrollbar, so java
        ''' scrollbars are readonly.</remarks>
        Public ReadOnly Property [Readonly]() As Boolean
            Get
                Return mReadonly
            End Get
        End Property
        <DataMember>
        Private mReadonly As Boolean

        ''' <summary>
        ''' A list of alternate element types which may be selected in place of this
        ''' one. This only allows higher level types to be converted to lower - e.g.
        ''' "CheckBox" is an alternate type for "Button", and "Button" an alternate
        ''' type for "Window", but not the other way round.
        ''' </summary>
        Public ReadOnly Property AlternateTypes() As List(Of clsElementTypeInfo)
            Get
                Return mAlternateTypes
            End Get
        End Property
        <DataMember>
        Private mAlternateTypes As New List(Of clsElementTypeInfo)

        ''' <summary>
        ''' Details used for identifying this element type as a SAP element, or Nothing
        ''' if that's not possible. The content can either be "*" indicating that it
        ''' should match if nothing else does (i.e. it's a generic component), or
        ''' otherwise it's a comma-separated list of SAP Component Types that match to
        ''' this element type.
        ''' </summary>
        Public Property SAPIdentification() As String
            Get
                Return mSAPIdentification
            End Get
            Set(ByVal value As String)
                mSAPIdentification = value
            End Set
        End Property
        <DataMember>
        Private mSAPIdentification As String


        ''' <summary>
        ''' If the element is not ReadOnly, this can contain a query to be used for
        ''' writing to the element. It may be Nothing, in which case the code should
        ''' determine the correct query. Arguments can be substituted into the query by
        ''' surrounding their name with '$'.
        ''' </summary>
        Public Property WriteQuery() As String
            Get
                Return mWriteQuery
            End Get
            Set(ByVal value As String)
                mWriteQuery = value
            End Set
        End Property
        <DataMember>
        Private mWriteQuery As String


        ''' <summary>
        ''' A dictionary of read action IDs and their associated read queries, or Nothing
        ''' if the code should determine what's allowed and how to implement it.
        ''' </summary>
        Public Property ReadQueries() As Dictionary(Of String, String)
            Get
                Return mReadQueries
            End Get
            Set(ByVal value As Dictionary(Of String, String))
                mReadQueries = value
            End Set
        End Property
        <DataMember>
        Private mReadQueries As Dictionary(Of String, String)


        ''' <summary>
        ''' A dictionary of action IDs and their associated queries, or Nothing
        ''' if the code should determine what's allowed and how to implement it.
        ''' </summary>
        Public Property ActionQueries() As Dictionary(Of String, String)
            Get
                Return mActionQueries
            End Get
            Set(ByVal value As Dictionary(Of String, String))
                mActionQueries = value
            End Set
        End Property
        <DataMember>
        Private mActionQueries As Dictionary(Of String, String)


        ''' <summary>
        ''' Constructor, to be used only within AMI itself. Clients of AMI receive
        ''' instances of this class only by calling the relevant clsAMI methods.
        ''' </summary>
        ''' <param name="elementType">The type of the element</param>
        ''' <param name="ID">Identifier for the element type</param>
        ''' <param name="name">Name of the element type</param>
        ''' <param name="defaultDataType">Default data type - see DefaultDataType
        ''' property for full details</param>
        ''' <param name="helpText">Help text for this element type</param>
        Friend Sub New(ByVal elementType As AppTypes, ByVal ID As String, ByVal name As String, ByVal defaultDataType As String, ByVal helpText As String, ByVal isReadonly As Boolean)
            mAppType = elementType
            mID = ID
            mName = name
            mHelpText = helpText
            mDefaultDataType = defaultDataType
            mReadonly = isReadonly
        End Sub

    End Class
End Namespace
