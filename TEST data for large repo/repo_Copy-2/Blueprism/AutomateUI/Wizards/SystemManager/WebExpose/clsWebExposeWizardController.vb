Imports AutomateControls.Wizard
Imports BluePrism.AutomateAppCore.Groups

Public Class clsWebExposeWizardController
    Inherits WizardController

    Public Class ExposeDetails
        Public Member As ProcessBackedGroupMember
    End Class

    Public Sub New()
        m_ExposeDetails = New ExposeDetails
    End Sub

    Public ReadOnly Property Dialog() As frmWebExpose
        Get
            Return CType(m_WizardDialog, frmWebExpose)
        End Get
    End Property

    Public ReadOnly Property Details() As ExposeDetails
        Get
            Return m_ExposeDetails
        End Get
    End Property
    Private m_ExposeDetails As ExposeDetails

    Protected Overrides Sub OnNavigateBegin(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
        Dim proc As ctlChooseWebExposeProcess = TryCast(Me.CurrentPanel, ctlChooseWebExposeProcess)
        If proc IsNot Nothing Then
            m_ExposeDetails.Member = TryCast(proc.ctlProcesses.FirstSelectedItem, ProcessBackedGroupMember)
        End If
        Dim name As ctlChooseWebExposeName = TryCast(Me.CurrentPanel, ctlChooseWebExposeName)
        If name IsNot Nothing Then
            m_ExposeDetails.Member.WebServiceName = name.txtExposeName.Text
            m_ExposeDetails.Member.WebServiceDocLit = name.forceSoapDocumentCheckbox.Checked
            m_ExposeDetails.Member.WebServiceLegacyNamespace = name.UseLegacyNamespaceCheckbox.Checked
        End If
        MyBase.OnNavigateBegin(sender, e)
    End Sub

    Protected Overrides Sub OnNavigateEnd(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim name As ctlChooseWebExposeName = TryCast(Me.CurrentPanel, ctlChooseWebExposeName)
        If name IsNot Nothing Then
            name.Setup(m_ExposeDetails.Member)
        End If
        MyBase.OnNavigateEnd(sender, e)
    End Sub

    Protected Overrides Sub OnNavigateFinish(ByVal sender As Object, ByVal e As System.EventArgs)
        Dialog.DialogResult = DialogResult.OK
        MyBase.OnNavigateFinish(sender, e)
    End Sub
End Class
