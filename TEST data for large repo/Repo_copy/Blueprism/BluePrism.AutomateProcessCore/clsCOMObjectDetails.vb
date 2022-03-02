Public Class clsCOMObjectDetails : Implements IObjectDetails

    ''' <summary>
    ''' The progID of the COM object 
    ''' </summary>
    Public Property ProgID As String

    ''' <summary>
    ''' The Business Object Configration XML
    ''' </summary>
    Public Property Config As String

    Public Property FriendlyName As String Implements IObjectDetails.FriendlyName
        Get
            Return BusinessObject.FriendlyName
        End Get
        Set(value As String)
            'Do Nothing
        End Set
    End Property

    Private ReadOnly Property BusinessObject As clsBusinessObject
        Get
            If mBusinessObject Is Nothing Then
                mBusinessObject = New clsCOMBusinessObject(ProgID, Config)
            End If
            Return mBusinessObject
        End Get
    End Property
    Private mBusinessObject As clsBusinessObject

    Public Sub New(progID As String, configXML As String)
        Me.ProgID = progID
        Me.Config = configXML
    End Sub
End Class
