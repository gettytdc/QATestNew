Imports System.Runtime.Serialization
Imports System.Xml.Linq

Namespace WebApis
    <Serializable, DataContract([Namespace]:="bp")>
    Public MustInherit Class ResponseOutputParameter : Implements IXElement, IParameter

        <DataMember>
        Private ReadOnly mName As String

        <DataMember>
        Private ReadOnly mDescription As String

        <DataMember>
        Private ReadOnly mDataType As DataType

        Public ReadOnly Property Id As Integer

        Public MustOverride ReadOnly Property Type As OutputMethodType

        Public MustOverride ReadOnly Property Path As String

        Protected Sub New(name As String, description As String, dataType As DataType)
            Me.New(0, name, description, dataType)
        End Sub

        Protected Sub New(id As Integer, name As String, description As String, dataType As DataType)
            If name Is Nothing Then Throw New ArgumentNullException(NameOf(name))
            Me.Id = id
            mName = name
            mDataType = dataType
            mDescription = description
        End Sub

        Public ReadOnly Property Name As String Implements IParameter.Name
            Get
                Return mName
            End Get
        End Property

        Public ReadOnly Property Description As String
            Get
                Return mDescription
            End Get
        End Property

        Public ReadOnly Property DataType As DataType Implements IParameter.DataType
            Get
                Return mDataType
            End Get
        End Property

        Public ReadOnly Property Direction As ParameterDirection Implements IParameter.Direction
            Get
                Return ParameterDirection.Out
            End Get
        End Property

        Public MustOverride Function ToXElement() As XElement Implements IXElement.ToXElement

    End Class

End Namespace
