Imports BluePrism.BPCoreLib
Imports AutomateControls

''' <summary>
''' A class used to apply the default font to forms and controls.
''' </summary>
''' <remarks></remarks>
Public Class clsFont

    ''' <summary>
    ''' The default font family name.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared msFamilyName As String

    Shared Sub New()
        msFamilyName = BPUtil.AvailableFont()
    End Sub

    ''' <summary>
    ''' The font family name.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Property FamilyName() As String
        Get
            Return msFamilyName
        End Get
        Set(ByVal value As String)
            msFamilyName = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the font to use.
    ''' </summary>
    ''' <param name="oCurrentFont"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function GetFont(ByVal oCurrentFont As Font) As Font

        Dim iStyle As FontStyle

        If oCurrentFont.Name = "Webdings" _
        OrElse oCurrentFont.Name = "Wingdings" Then

            'Exceptions to the rule - Webdings and Wingdings are used in 
            'places for symbols such as arrows
            Return oCurrentFont

        Else
            'Disregard any itallic settings,
            If (oCurrentFont.Style And FontStyle.Italic) > 0 Then
                iStyle = oCurrentFont.Style Xor FontStyle.Italic
            Else
                iStyle = oCurrentFont.Style
            End If

            Dim family As String = msFamilyName
            If oCurrentFont.Name.EndsWith("Light") Then
                family &= " Light"
            End If

            Return New System.Drawing.Font(family, oCurrentFont.Size, iStyle)

        End If

    End Function

    ''' <summary>
    ''' Applies the current font to a form.
    ''' </summary>
    ''' <param name="oForm"></param>
    ''' <remarks></remarks>
    Public Shared Sub SetFont(ByRef oForm As Form)

        oForm.Font = GetFont(oForm.Font)

        For Each child As Control In oForm.Controls
            If TypeOf child Is TitleBar Then SetFont(child)
        Next

    End Sub


    ''' <summary>
    ''' Applies the current font to a control
    ''' </summary>
    ''' <param name="oControl"></param>
    ''' <remarks></remarks>
    Public Shared Sub SetFont(ByRef oControl As Control)

        oControl.Font = GetFont(oControl.Font)
        For Each child As Control In oControl.Controls
            SetFont(child)
        Next

        If TypeOf oControl Is TabControl Then
            Dim oTabControl As TabControl = CType(oControl, TabControl)
            oTabControl.Font = GetFont(oTabControl.Font)
            For Each oTabPage As TabPage In oTabControl.TabPages

                oTabPage.Font = GetFont(oTabPage.Font)

                For Each child As Control In oTabPage.Controls
                    SetFont(child)
                Next
            Next
        ElseIf TypeOf oControl Is ToolStrip Then
            Dim oToolstrip As ToolStrip = CType(oControl, ToolStrip)
            For Each oToolStripItem As ToolStripItem In oToolstrip.Items
                oToolStripItem.Font = GetFont(oToolStripItem.Font)
            Next
        ElseIf TypeOf oControl Is TitleBar Then
            Dim oTitleBar As TitleBar = CType(oControl, TitleBar)
            oTitleBar.TitleFont = GetFont(oTitleBar.TitleFont)
        End If

    End Sub

End Class
