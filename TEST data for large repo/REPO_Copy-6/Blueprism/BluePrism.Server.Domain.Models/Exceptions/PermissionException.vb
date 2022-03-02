Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a permission check fails
''' </summary>
<Serializable()> _
Public Class PermissionException : Inherits BluePrismException

    ''' <summary>
    ''' Converts the given collection of permissions into a semi-colon separated
    ''' string.
    ''' </summary>
    ''' <param name="perms">The permissions to stringify</param>
    ''' <returns>A string containing all the given permissions, separated by
    ''' semi colons.</returns>
    Private Shared Function StringifyPermissions(ByVal perms As ICollection(Of String)) As String
        If Not If(perms?.Any(), False) Then Return My.Resources.PermissionException_None
        Return String.Join("; ", perms)
    End Function

    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message.
    ''' </summary>
    ''' <param name="msg">The message.</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="formattedMsg">The message, with format placeholders, as
    ''' defined in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    Public Sub New(ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(formattedMsg, args)
    End Sub

    ''' <summary>
    ''' Creates a new permission exception, which failed due to the given
    ''' permissions
    ''' </summary>
    ''' <param name="prefixMsg">The prefix message to display before the
    ''' permissions are enumerated. The exception message will be made up of this
    ''' message, followed by a line feed and then the collection of permissions.
    ''' </param>
    ''' <param name="permissions">The permissions required which the user does
    ''' not have.</param>
    Public Sub New(ByVal prefixMsg As String, ByVal permissions As ICollection(Of String))
        Me.New(prefixMsg & vbCrLf & StringifyPermissions(permissions))
    End Sub

    ''' <summary>
    ''' Creates a new permission exception, which failed due to the given
    ''' permissions
    ''' </summary>
    ''' <param name="permissions">The permissions required which the user does
    ''' not have.</param>
    Public Sub New(ByVal permissions As ICollection(Of String))
        Me.New(My.Resources.PermissionException_YouMustHaveTheFollowingPermissionsToPerformThisAction, permissions)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
