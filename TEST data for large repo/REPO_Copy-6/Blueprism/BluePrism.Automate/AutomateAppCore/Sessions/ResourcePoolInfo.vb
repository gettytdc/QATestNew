Imports System.Runtime.Serialization
Namespace Sessions

    <Serializable, DataContract(Name:="rpi", [Namespace]:="bp")>
    Public Class ResourcePoolInfo

        <DataMember(Name:="r", EmitDefaultValue:=False)>
        Private ReadOnly mResourceId As Guid

        <DataMember(Name:="i", EmitDefaultValue:=False)>
        Private ReadOnly mResourceName As String

        <DataMember(Name:="p", EmitDefaultValue:=False)>
        Private ReadOnly mPoolName As String

        <DataMember(Name:="pc", EmitDefaultValue:=False)>
        Private ReadOnly mPoolControllerId As Guid

        <DataMember(Name:="pi", EmitDefaultValue:=False)>
        Private ReadOnly mPoolId As Guid


        Public ReadOnly Property ResourceId As Guid
            Get
                Return mResourceId
            End Get
        End Property

        Public ReadOnly Property ResourceName As String
            Get
                Return mResourceName
            End Get
        End Property

        Public ReadOnly Property PoolName As String
            Get
                Return mPoolName
            End Get
        End Property

        Public ReadOnly Property PoolControllerId As Guid
            Get
                Return mPoolControllerId
            End Get
        End Property

        Public ReadOnly Property PoolId As Guid
            Get
                Return mPoolId
            End Get
        End Property

        Public Sub New(resourceId As Guid, resourceName As String, poolName As String,
                       poolControllerId As Guid, poolId As Guid)
            mResourceId = resourceId
            mResourceName = resourceName
            mPoolName = poolName
            mPoolControllerId = poolControllerId
            mPoolId = poolId
        End Sub
    End Class
End Namespace
