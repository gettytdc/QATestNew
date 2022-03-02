Imports BluePrism.AutomateProcessCore

''' <summary>
''' Class to represent the details of the group
''' </summary>
Public Class clsGroupObjectDetails : Implements IGroupObjectDetails

    ''' <summary>
    ''' Provides an empty group object details.
    ''' </summary>
    Public Shared ReadOnly Property Empty As New clsGroupObjectDetails(New MemberPermissions(Nothing))

    ''' <summary>
    ''' Construct group object details with permissions
    ''' </summary>
    ''' <param name="permissions"></param>
    Public Sub New(permissions As IMemberPermissions)
        Me.Permissions = permissions
    End Sub

    ''' <summary>
    ''' The friendly name of the group
    ''' </summary>
    ''' <returns></returns>
    Public Property FriendlyName As String Implements IObjectDetails.FriendlyName

    ''' <summary>
    ''' The permissions of the group
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Permissions As IMemberPermissions

    ''' <summary>
    ''' The children of the group
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property Children As IList(Of IObjectDetails) = New List(Of IObjectDetails) _
        Implements IGroupObjectDetails.Children
End Class
