
Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore

Public Class ctlProcessExposure
    Implements IStubbornChild, IPermission, IMode, IHelp, IRefreshable

    Private mMode As ProcessType

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub

    Public Property Mode() As ProcessType Implements IMode.Mode
        Get
            Return mMode
        End Get
        Set(ByVal value As ProcessType)
            mMode = value
            ctlExposedProcesses.TreeType = mMode.TreeType
            If Mode.IsProcess Then
                SetProcessResources()
            Else
                SetObjectResources()
            End If
        End Set
    End Property

    Private Sub ctlProcessExposure_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If DesignMode Then Return

        mMode.ChangeText(Me)
        ctlExposedProcesses.TreeType = mMode.TreeType
        ctlExposedProcesses.Filter = ProcessBackedGroupMember.NotRetiredAndExposed
    End Sub

    Private Sub llExpose_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llExpose.LinkClicked
        Try
            Dim f As New frmWebExpose
            f.SetEnvironmentColours(mParent)
            f.Setup(mMode)
            If f.ShowDialog() = DialogResult.OK Then
                Dim d As clsWebExposeWizardController.ExposeDetails = f.Details
                Dim mem = d.Member
                Dim sErr As String = Nothing

                Dim wsDetails = New WebServiceDetails(mem.WebServiceName, mem.WebServiceDocLit, mem.WebServiceLegacyNamespace)
                If Mode = ProcessType.BusinessObject Then
                    gSv.ExposeObjectAsWebService(mem.IdAsGuid, wsDetails)
                Else
                    gSv.ExposeProcessAsWebService(mem.IdAsGuid, wsDetails)
                End If
                mem.AddAttribute(ProcessAttributes.PublishedWS)

                ctlExposedProcesses.UpdateView()
            End If
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlProcessExposure_FailedToExpose01, mMode.ModeString(), ex.Message.ToLower())
        End Try
    End Sub

    Private Sub llConceal_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llConceal.LinkClicked
        Try
            For Each mem As IGroupMember In ctlExposedProcesses.SelectedMembers
                Dim pgm = TryCast(mem, ProcessBackedGroupMember)
                If pgm Is Nothing Then Continue For

                If Mode = ProcessType.BusinessObject Then
                    gSv.ConcealObjectWebService(mem.IdAsGuid)
                Else
                    gSv.ConcealProcessWebService(mem.IdAsGuid)
                End If
                pgm.RemoveAttribute(ProcessAttributes.PublishedWS)

            Next
            ctlExposedProcesses.UpdateView()
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlProcessExposure_FailedToConceal01, mMode.ModeString(), ex.Message.ToLower())
        End Try
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            If Mode.IsProcess Then
                Return Permission.ByName("Processes - Exposure")
            Else
                Return Permission.ByName("Business Objects - Exposure")
            End If
        End Get
    End Property

    Private Sub SetProcessResources()
        Label2.Text = My.Resources.ctlProcessExposure_PTitle
        llConceal.Text = My.Resources.ctlProcessExposure_PConceal
        llExpose.Text = My.Resources.ctlProcessExposure_PExpose
    End Sub

    Private Sub SetObjectResources()
        Label2.Text = My.Resources.ctlProcessExposure_BOTitle
        llConceal.Text = My.Resources.ctlProcessExposure_BOConceal
        llExpose.Text = My.Resources.ctlProcessExposure_BOExpose
    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmWebServiceExpose.htm"
    End Function

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        return True
    End Function

    Public Sub RefreshView() Implements IRefreshable.RefreshView
        ctlExposedProcesses.UpdateView(True)
    End Sub
End Class
