Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore

Public Class ctlChooseWebServiceName

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Timer1.Start()
    End Sub

    Public Sub Setup(ByVal serviceDetails As clsWebserviceDetails)
        If serviceDetails.Complete Then
            txtName.Text = serviceDetails.FriendlyName
        Else
            txtName.Text = serviceDetails.ServiceToUse
        End If
        mServiceDetails = serviceDetails
    End Sub

    Private mChanged As Boolean
    Private mServiceDetails As clsWebServiceDetails

    Private Sub txtName_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtName.TextChanged
        mChanged = True
        NavigateNext = False
        UpdateNavigate()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If mChanged Then
            NavigateNext = CheckName()
            UpdateNavigate()
            mChanged = False
        End If
    End Sub

    Private Function CheckName() As Boolean
        Dim name As String = txtName.Text
        If Not String.IsNullOrEmpty(name) Then
            If mServiceDetails.Complete AndAlso name = mServiceDetails.FriendlyName Then
                txtName.BackColor = Color.Empty
                lblNameInUse.Visible = False
                btnCorrect.Enabled = False
                Return True
            End If

            If Not AlreadyExists(name) Then
                txtName.BackColor = Color.Empty
                lblNameInUse.Visible = False
                btnCorrect.Enabled = False
                Return True
            Else
                txtName.BackColor = Color.Red
                lblNameInUse.Visible = True
                btnCorrect.Enabled = True
            End If
        End If
        Return False
    End Function

    Public Function AlreadyExists(ByVal name As String) As Boolean
        If gSv.GetWebServiceId(name) = Guid.Empty Then
            Return False
        End If
        Return True
    End Function

    Public Property ServiceName() As String
        Get
            Return txtName.Text
        End Get
        Set(ByVal value As String)
            txtName.Text = value
        End Set
    End Property


    Private Sub btnCorrect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCorrect.Click
        Dim newName As String = txtName.Text
        If AlreadyExists(newName) Then
            Dim name As String = newName
            Dim index As Integer = 0
            Do
                index += 1
                name = newName & index
            Loop Until Not AlreadyExists(name)
            newName = name
        End If
        txtName.Text = newName
    End Sub
End Class
