Imports System.Windows.Forms
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Core.Utility

Namespace EnvironmentFunctions

    Public Class GetClipboardFunction : Inherits EnvironmentFunction
        
        Private ReadOnly mClipboard As IClipboard

        Public Sub New(clipboard As IClipboard)
            mClipboard = clipboard
        End Sub

        ''' <returns>A clsProcessValue containing the text of the clipboard or "" if there
        ''' is no text in the clipboard.</returns>
        Protected Overrides Function InnerEvaluate(parameters As IList(Of clsProcessValue), process As clsProcess) As clsProcessValue

            Dim iData = mClipboard.GetDataObject()

            If iData.GetDataPresent(DataFormats.Text) OrElse iData.GetDataPresent(DataFormats.UnicodeText) Then
                Return New clsProcessValue(DataType.text, CType(iData.GetData(DataFormats.UnicodeText), String), False)
            Else
                Return New clsProcessValue(DataType.text, String.Empty, False)
            End If

        End Function

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.GetClipboardFunction_GetsTextFromTheClipboard
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "GetClipboard"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.GetClipboardFunction_GetTextFromClipboard
            End Get
        End Property
    End Class
End NameSpace