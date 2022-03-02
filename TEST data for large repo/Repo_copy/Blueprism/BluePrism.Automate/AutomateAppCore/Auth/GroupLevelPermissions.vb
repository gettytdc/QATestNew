Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.clsServerPartialClasses

Namespace Auth

    ''' <summary>
    ''' Class to represent the group level permissions for a specific group
    ''' and user role
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp", Name:="glp")>
    Public Class GroupLevelPermissions : Inherits PermHolder : Implements IEquatable(Of GroupLevelPermissions)

        Public Sub New(roleid As Integer)
            MyBase.New(roleid, String.Empty, Feature.None)
        End Sub

        Public Sub New(roleid As Integer, name As String)
            MyBase.New(roleid, name, Feature.None)
        End Sub

        Public Overloads Function Equals(other As GroupLevelPermissions) As Boolean Implements IEquatable(Of GroupLevelPermissions).Equals
            Return MyBase.Equals(other)
        End Function
    End Class

End Namespace
