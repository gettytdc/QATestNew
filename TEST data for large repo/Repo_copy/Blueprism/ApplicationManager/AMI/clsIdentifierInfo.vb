Imports System.Collections.Generic
Imports BluePrism.BPCoreLib.Collections
Imports ComparisonTypes = BluePrism.AMI.clsAMI.ComparisonTypes

Namespace BluePrism.ApplicationManager.AMI
    ''' <summary>
    ''' The type of the identifier
    ''' </summary>
    Public Enum IdentifierType
        Normal
        Parent
    End Enum

    ''' Project  : AMI
    ''' Class    : clsIdentifierInfo
    ''' 
    ''' <summary>
    ''' This class is used to return information to the client about a particular
    ''' Identifier for an Element. Where relevant, a value can also be present.
    ''' </summary>
    Public Class clsIdentifierInfo

#Region " Member Variables "

        ' The ID of this identifier
        Private mId As String

        ' The name of this identifier
        Private mName As String

        ' The type ('Normal' or 'Parent') of this identifier
        Private mType As IdentifierType

        ' The value of this identifier
        Private mValues As ICollection(Of String)

        ' Whether this identifier is enabled by default on a spy operation
        Private mEnableByDefault As Boolean

        ' The datatype associated with this identifier
        Private mDataType As String

        ' Flag indicating whether this identifier can be populated with multiple values
        Private mMultiple As Boolean

        ' The comparison type used for this identifier
        Private mComparisonType As ComparisonTypes

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new identifier info.
        ''' </summary>
        ''' <param name="id">The unique ID of the identifier</param>
        ''' <param name="type">Type of the identifier - ie. whether it represents the
        ''' associated element or its parent.</param>
        ''' <param name="name">A friendly name for the identifier</param>
        ''' <param name="dtype">The associated Automate datatype ID</param>
        Friend Sub New(ByVal id As String,
     ByVal type As IdentifierType, ByVal name As String, ByVal dtype As String)
            Me.New(id, type, name, dtype, False)
        End Sub

        ''' <summary>
        ''' Creates a new identifier info.
        ''' </summary>
        ''' <param name="id">The unique ID of the identifier</param>
        ''' <param name="type">Type of the identifier - ie. whether it represents the
        ''' associated element or its parent.</param>
        ''' <param name="name">A friendly name for the identifier</param>
        ''' <param name="dtype">The associated Automate datatype ID</param>
        ''' <param name="multiple">Whether the identifier can accept multiple values or
        ''' not.</param>
        Friend Sub New(ByVal id As String, ByVal type As IdentifierType,
     ByVal name As String, ByVal dtype As String, ByVal multiple As Boolean)
            mId = id
            mType = type
            mName = name
            mDataType = dtype
            mMultiple = multiple
        End Sub

#End Region

#Region " Methods "

        ''' <summary>
        ''' Deep clones this identifier.
        ''' </summary>
        ''' <returns>A full clone of this identifier - ie. a copy of this object, with
        ''' exactly the same values as this object.</returns>
        Friend Function Clone() As clsIdentifierInfo
            Return Clone(Nothing)
        End Function

        ''' <summary>
        ''' Deep clones this identifier.
        ''' </summary>
        ''' <param name="customId">Custom value to assign to the ID property</param>
        ''' <returns>A full clone of this identifier - ie. a copy of this object, with
        ''' exactly the same values as this object.</returns>
        Friend Function Clone(customId As String) As clsIdentifierInfo
            Dim id As clsIdentifierInfo = MemberwiseClone()
            If mValues IsNot Nothing Then
                Dim vals As New List(Of String)
                vals.AddRange(mValues)
                id.mValues = vals
            End If
            If Not customId Is Nothing Then
                id.mId = customId
            End If
            Return id
        End Function

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets whether the identifier represented by this object can be populated with
        ''' multiple values.
        ''' </summary>
        Public ReadOnly Property SupportsMultiple() As Boolean
            Get
                Return mMultiple
            End Get
        End Property

        ''' <summary>
        ''' The unique ID string of this identifier.
        ''' </summary>
        Public ReadOnly Property ID() As String
            Get
                Return mId
            End Get
        End Property

        ''' <summary>
        ''' The name of this identifier. This is a friendly name to display to the user.
        ''' </summary>
        Public ReadOnly Property Name() As String
            Get
                Return mName
            End Get
        End Property

        ''' <summary>
        ''' The fully qualified name of this identifier, prefixed with "Parent" if it
        ''' represents a parent identifier.
        ''' </summary>
        Public ReadOnly Property FullyQualifiedName() As String
            Get
                If mType = IdentifierType.Parent _
             Then Return String.Format(LocaleTools.Properties.GlobalResources.Parent0, mName) _
             Else Return mName
            End Get
        End Property

        ''' <summary>
        ''' The Type of the identifier which is currently one of Normal or Parent.
        ''' </summary>
        Public ReadOnly Property Type() As IdentifierType
            Get
                Return mType
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the value associated with this identifier. Note that, after
        ''' setting this value, the given value will become a single value associated
        ''' with this object. If, previously, this identifier had a number of
        ''' <see cref="Values"/> associated with it, they will be cleared before setting
        ''' this value.
        ''' This will return null if no value is set within this identifier.
        ''' </summary>
        Public Property Value() As String
            Get
                Return CollectionUtil.First(Values)
            End Get
            Set(ByVal value As String)
                Dim coll As ICollection(Of String) = Values
                If coll.Count > 0 Then coll.Clear()
                coll.Add(value)
            End Set
        End Property

        ''' <summary>
        ''' The collection of values associated with this identifier. This is typically
        ''' going to contain a single value (if value is et at all)
        ''' </summary>
        Public ReadOnly Property Values() As ICollection(Of String)
            Get
                If mValues Is Nothing Then mValues = New List(Of String)
                Return mValues
            End Get
        End Property

        ''' <summary>
        ''' True if the identifier should be selected by default in Integration Assistant
        ''' after spying, otherwise False.
        ''' </summary>
        Public Property EnableByDefault() As Boolean
            Get
                Return mEnableByDefault
            End Get
            Set(ByVal value As Boolean)
                mEnableByDefault = value
            End Set
        End Property

        ''' <summary>
        ''' The Automate DataType to associate with this identifier. The value is the
        ''' datatype's ID, for example "text", as per the API Documentation.
        ''' </summary>
        Public Property DataType() As String
            Get
                Return mDataType
            End Get
            Set(ByVal value As String)
                mDataType = value
            End Set
        End Property

        ''' <summary>
        ''' The comparison type to be used for this identifier.
        ''' </summary>
        Public Property ComparisonType() As BluePrism.AMI.clsAMI.ComparisonTypes
            Get
                Return mComparisonType
            End Get
            Set(ByVal value As BluePrism.AMI.clsAMI.ComparisonTypes)
                mComparisonType = value
            End Set
        End Property

#End Region

    End Class

End Namespace