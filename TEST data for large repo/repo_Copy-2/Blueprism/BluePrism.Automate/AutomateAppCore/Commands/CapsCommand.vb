Imports BluePrism.AutomateProcessCore

Namespace Commands

    ''' <summary>
    ''' The 'caps' command
    ''' </summary>
    Public Class CapsCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "caps"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.CapsCommand_GetResourceCapabilities
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Authed
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String

            Return GetMyCapabilitiesFriendly() & "<EOF>"

        End Function

        Private Function GetMyCapabilitiesFriendly() As String

            Dim externalObjectInfo = Options.Instance.GetExternalObjectsInfo()

            Using objs As New clsGroupBusinessObject(externalObjectInfo)
                Dim sb As New StringBuilder()
                For Each obj As clsBusinessObject In objs.Children
                    DescendChildren(obj, sb, 0)
                Next
                Return sb.ToString()
            End Using

        End Function

        Private Sub DescendChildren(obj As clsBusinessObject, sb As StringBuilder, indent As Integer)
            ' Remove any newline characters from the Object Friendly Name
            Dim boName As String = RegularExpressions.Regex.Replace(obj.FriendlyName, "[\r\n]+", "")

            'Ignore objects with no name
            If String.IsNullOrEmpty(boName) Then Return

            'Ignore invalid objects
            If Not obj.Valid Then Return

            Dim group = TryCast(obj, clsGroupBusinessObject)
            'Ignore the internal group
            If group IsNot Nothing AndAlso group.InternalGroup Then Return

            sb.Append(" "c, indent)
            sb.AppendLine(boName)

            If group IsNot Nothing Then
                For Each childObj As clsBusinessObject In group.Children
                    DescendChildren(childObj, sb, indent + 3)
                Next
            End If
        End Sub

    End Class
End Namespace