Imports AutomateUI.Controls.Widgets.SystemManager.WebApi.Request
Imports BluePrism.AutomateProcessCore.Compilation

Namespace Controls.Widgets.SystemManager.WebApi

    Public Class CustomCodeControl

        Public Event CodeChanged As CustomCodeChangedEventHandler
        Private mCommonCode As CodePropertiesDetails
        Private mMethodDefinition As MethodDefinition

        Friend Sub New(commonCode As CodePropertiesDetails,
                       methodDefinition As MethodDefinition)

            InitializeComponent()

            mCommonCode = commonCode

            UpdateMethodDefinition(methodDefinition)

            ctlCodeEditor.Populate(methodDefinition.Body, mCommonCode.Language.Name)
        End Sub

        Private Function FormattedParameters() As IList(Of String)
            Return mMethodDefinition.
                    Parameters.
                    OrderBy(Function(p) p.IsOutput).
                    ThenBy(Function(p) p.Name).
                    Select(Function(p) $"{CodeCompiler.GetIdentifier(p.Name)} ({p.DataType.ToString()}), {If(p.IsOutput, "Out", "In")}").
                    ToList()
        End Function

        Public Sub UpdateMethodDefinition(methodDefinition As MethodDefinition)
            mMethodDefinition = methodDefinition
            lstParameters.DataSource = FormattedParameters()
        End Sub

        Private Sub EditorValidating(sender As Object, e As CancelEventArgs) Handles ctlCodeEditor.Validating
            OnCodeChanged(New CustomCodeControlEventArgs(ctlCodeEditor.Code))
        End Sub

        Public Sub OnCodeChanged(e As CustomCodeControlEventArgs)
            RaiseEvent CodeChanged(Me, e)
        End Sub

        Private Sub ValidateCode(sender As Object, e As EventArgs) Handles btnCheckCode.Click
            mMethodDefinition.Body = ctlCodeEditor.Code
            CodeValidationHelper.Validate(mCommonCode, {mMethodDefinition},
                                          Sub(m) UserMessage.Show(m), Sub(m) UserMessage.Err(m))

        End Sub

    End Class
End Namespace