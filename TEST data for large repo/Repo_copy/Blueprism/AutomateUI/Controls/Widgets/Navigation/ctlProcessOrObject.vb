Public Class ctlProcessOrObject

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Friend Property ChosenType() As frmWizard.WizardType
        Get
            If rdoObject.Checked Then Return frmWizard.WizardType.BusinessObject
            If rdoProcess.Checked Then Return frmWizard.WizardType.Process
            Return frmWizard.WizardType.Selection
        End Get
        Set(ByVal value As frmWizard.WizardType)
            Select Case value
                Case frmWizard.WizardType.BusinessObject
                    rdoObject.Checked = True
                    rdoProcess.Checked = False
                Case Else
                    rdoObject.Checked = False
                    rdoProcess.Checked = True
            End Select

        End Set
    End Property

End Class
