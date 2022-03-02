Imports System.Runtime.Serialization

Namespace WebApis

    ''' <summary>
    ''' Contains a set of actions that are executed by making HTTP requests to an
    ''' HTTP API
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class WebApi : Implements IObjectDetails

        <DataMember>
        Private mId As Guid

        <DataMember>
        Private mName As String

        <DataMember>
        Private mEnabled As Boolean

        <DataMember>
        Private mConfiguration As WebApiConfiguration

        ''' <summary>
        ''' Creates a new WebApi instance
        ''' </summary>
        ''' <param name="id">ID used to identify this WebApi</param>
        ''' <param name="name">The name of this WebApi</param>
        ''' <param name="enabled">Controls whether this WebApi is active and available 
        ''' to be used</param>
        ''' <param name="configuration">An object containing the core actions, parameters 
        ''' and headers configured for this WebApi</param>
        Public Sub New(id As Guid, name As String, enabled As Boolean, configuration As WebApiConfiguration)
            If String.IsNullOrWhiteSpace(name) Then
                Throw New ArgumentException(My.Resources.Resources.WebApi_InvalidName, NameOf(name))
            End If
            If configuration Is Nothing Then
                Throw New ArgumentNullException(NameOf(configuration))
            End If

            mId = id
            mName = name
            mEnabled = enabled
            mConfiguration = configuration

        End Sub

        ''' <summary>
        ''' ID used to identify this WebApi
        ''' </summary>
        Public ReadOnly Property Id As Guid
            Get
                Return mId
            End Get
        End Property

        ''' <summary>
        ''' The name of this WebApi
        ''' </summary>
        Public Property Name As String
            Get
                Return mName
            End Get
            Set
                If String.IsNullOrWhiteSpace(Value) Then
                    Throw New ArgumentException(My.Resources.Resources.WebApi_InvalidName, NameOf(Value))
                End If
                mName = Value
            End Set
        End Property

        ''' <summary>
        ''' Controls whether this WebApi is active and available to be used
        ''' </summary>
        Public Property Enabled As Boolean
            Get
                Return mEnabled
            End Get
            Set
                mEnabled = Value
            End Set
        End Property

        ''' <summary>
        ''' Contains the core actions, parameters and headers configured for this
        ''' WebApi
        ''' </summary>
        Public Property Configuration As WebApiConfiguration
            Get
                Return mConfiguration
            End Get
            Set
                If Value Is Nothing Then
                    Throw New ArgumentNullException(NameOf(Value))
                End If
                mConfiguration = Value
            End Set
        End Property

        ''' <summary>
        ''' Required to be implemented to be apart of clsGroupObjectDetails children.
        ''' </summary>
        Public Property FriendlyName As String Implements IObjectDetails.FriendlyName
            Get
                Return Name
            End Get
            Set(value As String)
                Name = value
            End Set
        End Property

    End Class

End Namespace