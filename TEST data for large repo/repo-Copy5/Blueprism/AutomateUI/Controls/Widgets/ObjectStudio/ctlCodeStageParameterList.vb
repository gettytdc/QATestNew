Imports BluePrism.BPCoreLib.Collections.CollectionUtil

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

''' <summary>
''' Read-only list of parameters on a code stage
''' </summary>
Public Class ctlCodeStageParameterList

    ' The stage whose parameters are displayed in this list
    Private mStage As clsCodeStage

    ''' <summary>
    ''' Gets or sets the stage associated with this list, updating the list to match
    ''' the parameters in the stage. Setting it to null will effectively clear the
    ''' list
    ''' </summary>
    <Browsable(False)> _
    Friend Property Stage() As clsCodeStage
        Get
            Return mStage
        End Get
        Set(ByVal value As clsCodeStage)
            mStage = value

            lstList.Items.Clear()
            If mStage Is Nothing Then Return

            For Each param As clsProcessParameter In _
             MergeList(mStage.GetInputs(), mStage.GetOutputs())
                Dim tp As Type = param.NativeType
                If tp Is Nothing Then Continue For

                lstList.Items.Add(String.Format( _
                 "{0}:{1} - {2}", _
                 IIf(param.Direction = ParamDirection.In, "In", "Out"), _
                 mStage.GetParameterName(param), tp))
            Next

        End Set
    End Property

    ''' <summary>
    ''' Handles the 'loading' of the control, ensuring that the control is fully
    ''' initialized.
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        ' We do this here so that we're not hardcoding the font into the designer -
        ' at this stage the font will have been inherited from the parent, so we
        ' can just 'boldify' that font to get what we want
        lblParams.Font = New Font(lblParams.Font, FontStyle.Bold)
    End Sub

End Class
