Imports System.Text.RegularExpressions

Imports BluePrism.AutomateProcessCore

Imports clsProcessOperator = _
 BluePrism.AutomateProcessCore.clsProcessOperators.clsProcessOperator
Imports LiteralValue = AutomateUI.clsExpressionTreeView.LiteralValue
Imports System.Globalization

''' Project  : Automate
''' Class    : ctlFunction
''' 
''' <summary>
''' A control to display the details of a function or operator used in the
''' expression of a decision or calculation stage.
''' </summary>
Public Class ctlFunction : Inherits UserControl

    ''' <summary>
    ''' Regular expression to recognise a data item in a piece of text.
    ''' </summary>
    Private Shared ReadOnly RegexDataItem As New Regex("^\[.+\]$")

#Region " Windows Form Designer generated code "

    'UserControl overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents pnlHeader As System.Windows.Forms.Panel
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents pnlFooter As System.Windows.Forms.Panel
    Friend WithEvents pnlBody As System.Windows.Forms.Panel
    Friend WithEvents lblNameTemplate As System.Windows.Forms.Label
    Friend WithEvents txtValueTemplate As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnPaste As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblFooter As System.Windows.Forms.Label
    Friend WithEvents lblDescriptionTemplate As System.Windows.Forms.Label
    Friend WithEvents pnlAll As AutomateUI.clsPanel
    Friend WithEvents pnlHelp As System.Windows.Forms.Panel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents pnlHelpFlowLayoutPanel As FlowLayoutPanel
    Friend WithEvents rtbSubtitle As System.Windows.Forms.RichTextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlFunction))
        Me.pnlAll = New AutomateUI.clsPanel()
        Me.pnlHeader = New System.Windows.Forms.Panel()
        Me.rtbSubtitle = New System.Windows.Forms.RichTextBox()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.pnlBody = New System.Windows.Forms.Panel()
        Me.pnlHelp = New System.Windows.Forms.Panel()
        Me.pnlHelpFlowLayoutPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblDescriptionTemplate = New System.Windows.Forms.Label()
        Me.txtValueTemplate = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblNameTemplate = New System.Windows.Forms.Label()
        Me.pnlFooter = New System.Windows.Forms.Panel()
        Me.btnPaste = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblFooter = New System.Windows.Forms.Label()
        Me.pnlAll.SuspendLayout
        Me.pnlHeader.SuspendLayout
        Me.pnlBody.SuspendLayout
        Me.pnlHelp.SuspendLayout
        Me.pnlHelpFlowLayoutPanel.SuspendLayout
        Me.pnlFooter.SuspendLayout
        Me.SuspendLayout
        '
        'pnlAll
        '
        resources.ApplyResources(Me.pnlAll, "pnlAll")
        Me.pnlAll.BorderColor = System.Drawing.SystemColors.InactiveCaption
        Me.pnlAll.BorderStyle = AutomateUI.clsPanel.BorderMode.[On]
        Me.pnlAll.BorderWidth = 1
        Me.pnlAll.Controls.Add(Me.pnlHeader)
        Me.pnlAll.Controls.Add(Me.pnlBody)
        Me.pnlAll.Controls.Add(Me.pnlFooter)
        Me.pnlAll.Name = "pnlAll"
        '
        'pnlHeader
        '
        resources.ApplyResources(Me.pnlHeader, "pnlHeader")
        Me.pnlHeader.Controls.Add(Me.rtbSubtitle)
        Me.pnlHeader.Controls.Add(Me.lblTitle)
        Me.pnlHeader.Name = "pnlHeader"
        '
        'rtbSubtitle
        '
        resources.ApplyResources(Me.rtbSubtitle, "rtbSubtitle")
        Me.rtbSubtitle.BackColor = System.Drawing.Color.White
        Me.rtbSubtitle.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.rtbSubtitle.Name = "rtbSubtitle"
        Me.rtbSubtitle.ReadOnly = true
        '
        'lblTitle
        '
        resources.ApplyResources(Me.lblTitle, "lblTitle")
        Me.lblTitle.Name = "lblTitle"
        '
        'pnlBody
        '
        resources.ApplyResources(Me.pnlBody, "pnlBody")
        Me.pnlBody.BackColor = System.Drawing.Color.White
        Me.pnlBody.Controls.Add(Me.pnlHelp)
        Me.pnlBody.Controls.Add(Me.lblDescriptionTemplate)
        Me.pnlBody.Controls.Add(Me.txtValueTemplate)
        Me.pnlBody.Controls.Add(Me.lblNameTemplate)
        Me.pnlBody.Name = "pnlBody"
        '
        'pnlHelp
        '
        resources.ApplyResources(Me.pnlHelp, "pnlHelp")
        Me.pnlHelp.Controls.Add(Me.pnlHelpFlowLayoutPanel)
        Me.pnlHelp.Name = "pnlHelp"
        '
        'pnlHelpFlowLayoutPanel
        '
        resources.ApplyResources(Me.pnlHelpFlowLayoutPanel, "pnlHelpFlowLayoutPanel")
        Me.pnlHelpFlowLayoutPanel.Controls.Add(Me.Label1)
        Me.pnlHelpFlowLayoutPanel.Controls.Add(Me.Label2)
        Me.pnlHelpFlowLayoutPanel.Controls.Add(Me.Label3)
        Me.pnlHelpFlowLayoutPanel.Name = "pnlHelpFlowLayoutPanel"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'lblDescriptionTemplate
        '
        resources.ApplyResources(Me.lblDescriptionTemplate, "lblDescriptionTemplate")
        Me.lblDescriptionTemplate.Name = "lblDescriptionTemplate"
        '
        'txtValueTemplate
        '
        Me.txtValueTemplate.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtValueTemplate, "txtValueTemplate")
        Me.txtValueTemplate.Name = "txtValueTemplate"
        '
        'lblNameTemplate
        '
        resources.ApplyResources(Me.lblNameTemplate, "lblNameTemplate")
        Me.lblNameTemplate.Name = "lblNameTemplate"
        '
        'pnlFooter
        '
        resources.ApplyResources(Me.pnlFooter, "pnlFooter")
        Me.pnlFooter.BackColor = System.Drawing.Color.White
        Me.pnlFooter.Controls.Add(Me.btnPaste)
        Me.pnlFooter.Controls.Add(Me.lblFooter)
        Me.pnlFooter.Name = "pnlFooter"
        '
        'btnPaste
        '
        resources.ApplyResources(Me.btnPaste, "btnPaste")
        Me.btnPaste.BackColor = System.Drawing.SystemColors.Control
        Me.btnPaste.Name = "btnPaste"
        Me.btnPaste.UseVisualStyleBackColor = false
        '
        'lblFooter
        '
        resources.ApplyResources(Me.lblFooter, "lblFooter")
        Me.lblFooter.Name = "lblFooter"
        '
        'ctlFunction
        '
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.pnlAll)
        Me.Name = "ctlFunction"
        resources.ApplyResources(Me, "$this")
        Me.pnlAll.ResumeLayout(false)
        Me.pnlHeader.ResumeLayout(false)
        Me.pnlBody.ResumeLayout(false)
        Me.pnlBody.PerformLayout
        Me.pnlHelp.ResumeLayout(false)
        Me.pnlHelp.PerformLayout
        Me.pnlHelpFlowLayoutPanel.ResumeLayout(false)
        Me.pnlHelpFlowLayoutPanel.PerformLayout
        Me.pnlFooter.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub

#End Region


    Public Event PasteExpression(ByVal e As String)

    Private mobjStage As clsProcessStage
    Private Const ciMargin As Integer = 10
    Private mParamControls As IList(Of Control)

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        mParamControls = New List(Of Control)
    End Sub

    ''' <summary>
    ''' Makes a reference to the stage object.
    ''' </summary>
    ''' <param name="value">The stage</param>
    Public Sub SetStage(ByVal value As clsProcessStage)
        mobjStage = value
    End Sub


    ''' <summary>
    ''' A process viewer used to launch stage properties.
    ''' </summary>
    ''' <remarks>May be null, but if null then no stage properties can be viewed.</remarks>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Friend Property ProcessViewer() As ctlProcessViewer
        Get
            Return mProcessViewer
        End Get
        Set(ByVal value As ctlProcessViewer)
            mProcessViewer = value
        End Set
    End Property
    Private mProcessViewer As ctlProcessViewer

    Private Sub GenerateControls(ByRef itop As Integer, ByVal name As String, ByVal helpText As String)
        Dim lblName As Label
        Dim lblDescription As Label
        Dim ctlValue As ctlExpressionEdit

        lblName = New Label
        pnlBody.Controls.Add(lblName)
        lblName.Text = name
        lblName.Font = lblNameTemplate.Font
        lblName.Width = lblNameTemplate.Width
        lblName.Height = lblNameTemplate.Height
        lblName.Left = lblNameTemplate.Left
        lblName.Top = itop

        ctlValue = New ctlExpressionEdit
        ctlValue.Stage = Me.mobjStage
        ctlValue.ProcessViewer = Me.ProcessViewer
        ctlValue.Border = True
        pnlBody.Controls.Add(ctlValue)
        mParamControls.Add(ctlValue)
        ctlValue.Width = txtValueTemplate.Width
        ctlValue.Height = txtValueTemplate.Height
        ctlValue.Left = txtValueTemplate.Left
        ctlValue.Top = itop

        ctlValue.AllowDrop = True
        AddHandler ctlValue.DragDrop, AddressOf Control_DragDrop
        AddHandler ctlValue.DragEnter, AddressOf Control_DragEnter

        lblDescription = New Label
        pnlBody.Controls.Add(lblDescription)
        lblDescription.Text = helpText
        lblDescription.Font = lblDescriptionTemplate.Font
        lblDescription.Width = lblDescriptionTemplate.Width
        lblDescription.Height = lblDescriptionTemplate.Height
        lblDescription.Left = lblDescriptionTemplate.Left
        lblDescription.Top = itop + lblName.Height

        'Allow wordwrap and set to wrap at the end of the requested width.
        lblDescription.AutoSize = True
        lblDescription.MaximumSize = New Size(lblDescriptionTemplate.Width, 0)

        SetAutoScrollMargins(lblDescription)

        itop += lblName.Height + lblDescription.Height + ciMargin
    End Sub

    ''' <summary>
    ''' Populates the control using the operator or function object.
    ''' </summary>
    ''' <param name="e">A clsProcessOperator or a clsFunction object.</param>
    Public Sub Populate(ByRef e As Object)

        Dim iTop, iIndex As Integer
        Dim lblName As Label
        Dim lblDescription As Label
        Dim ctlValue As ctlExpressionEdit
        Dim objFunction As clsFunction
        Dim objParameter As clsFunctionParm
        Dim objOperator As clsProcessOperators.clsProcessOperator
        Dim bShowFooter As Boolean

        iTop = lblNameTemplate.Top

        If TypeOf e Is clsFunction Then
            'make a reference to the function in the tag
            Me.Tag = e
            pnlBody.Controls.Clear()
            mParamControls.Clear()
            objFunction = CType(e, clsFunction)
            Dim sTitle As String = objFunction.Name & "("
            For Each objParameter In objFunction.DefaultSignature
                sTitle &= objParameter.Name & ", "
                bShowFooter = True
            Next
            If objFunction.DefaultSignature.Length > 0 Then
                sTitle = sTitle.Substring(0, sTitle.Length - 2)
            End If
            sTitle &= ")"

            pnlHeader.SuspendLayout()
            lblTitle.Height = 100 'First give ourselves enough room for the text.

            Dim g As Graphics = lblTitle.CreateGraphics
            Dim textsize As SizeF = g.MeasureString(sTitle, lblTitle.Font, lblTitle.Size, New StringFormat)

            lblTitle.Height = CInt(textsize.Height) 'Now resize the height to exactly what is needed.

            lblTitle.Text = sTitle

            'Budge the description down so that it doesn't overlap
            rtbSubtitle.Top = lblTitle.Bottom
            rtbSubtitle.Height = pnlHeader.Height - rtbSubtitle.Top

            rtbSubtitle.Text = objFunction.HelpText
            pnlHeader.ResumeLayout()

            If objFunction.TextAppendFunction Then
                GenerateControls(iTop, "Text", My.Resources.ctlFunction_TheExistingTextToAppendTo)
            End If
            For Each objParameter In objFunction.DefaultSignature

                GenerateControls(iTop, objParameter.Name, objParameter.HelpText)
            Next

        ElseIf TypeOf e Is clsProcessOperators.clsProcessOperator Then
            'make a reference to the operator in the tag
            Me.Tag = e
            pnlBody.Controls.Clear()
            mParamControls.Clear()
            objOperator = CType(e, clsProcessOperators.clsProcessOperator)
            lblTitle.Text = objOperator.Name & " (" & objOperator.Symbol & ")"
            rtbSubtitle.Text = objOperator.HelpText

            For iIndex = 0 To 1
                lblName = New Label
                pnlBody.Controls.Add(lblName)
                If iIndex = 0 Then
                    lblName.Text = My.Resources.ctlFunction_OperandA
                Else
                    lblName.Text = My.Resources.ctlFunction_OperandB
                End If
                lblName.Font = lblNameTemplate.Font
                lblName.Width = lblNameTemplate.Width
                lblName.Height = lblNameTemplate.Height
                lblName.Left = lblNameTemplate.Left
                lblName.Top = iTop

                ctlValue = New ctlExpressionEdit
                ctlValue.Stage = mobjStage
                ctlValue.ProcessViewer = Me.ProcessViewer
                ctlValue.Border = True
                pnlBody.Controls.Add(ctlValue)
                mParamControls.Add(ctlValue)
                ctlValue.Width = txtValueTemplate.Width
                ctlValue.Height = txtValueTemplate.Height
                ctlValue.Left = txtValueTemplate.Left
                ctlValue.Top = iTop

                ctlValue.AllowDrop = True
                AddHandler ctlValue.DragDrop, AddressOf Control_DragDrop
                AddHandler ctlValue.DragEnter, AddressOf Control_DragEnter

                lblDescription = New Label
                pnlBody.Controls.Add(lblDescription)
                If iIndex = 0 Then
                    lblDescription.Text = My.Resources.ctlFunction_TheLeftHandOperand
                Else
                    lblDescription.Text = My.Resources.ctlFunction_TheRightHandOperand
                End If
                lblDescription.Font = lblDescriptionTemplate.Font
                lblDescription.Width = lblDescriptionTemplate.Width
                lblDescription.Height = lblDescriptionTemplate.Height
                lblDescription.Left = lblDescriptionTemplate.Left
                lblDescription.Top = iTop + lblName.Height
                SetAutoScrollMargins(lblDescription)

                iTop += lblName.Height + lblDescription.Height + ciMargin
            Next
            bShowFooter = True

        ElseIf TypeOf e Is clsExpressionTreeView.LiteralValue Then
            'put literal value object in tag
            Me.Tag = e
            pnlBody.Controls.Clear()
            mParamControls.Clear()

            Dim objLiteralValue As clsExpressionTreeView.LiteralValue = CType(e, clsExpressionTreeView.LiteralValue)
            lblTitle.Text = objLiteralValue.Name
            rtbSubtitle.Text = objLiteralValue.HelpText

        Else
            Exit Sub
        End If

        'hide the default controls
        lblNameTemplate.Visible = False
        txtValueTemplate.Visible = False
        lblDescriptionTemplate.Visible = False
        pnlHelp.Visible = False

        'show the relevant other controls
        Me.btnPaste.Visible = True
        Me.lblFooter.Visible = bShowFooter

    End Sub





    ''' <summary>
    ''' Interprets the control details as an expression string.
    ''' </summary>
    ''' <param name="e">An object for which an expression string is required.</param>
    ''' <returns>An expression</returns>
    Public Function GetExpression(ByVal e As Object) As String
        ' "e" may be a function, an operator or a literal value (or something else
        ' completely, which, fittingly, induces a blank expression)
        Dim fn As clsFunction = TryCast(e, clsFunction)
        Dim op As clsProcessOperator = TryCast(e, clsProcessOperator)
        Dim lv As LiteralValue = TryCast(e, LiteralValue)

        Dim cult As CultureInfo = CultureInfo.CurrentCulture
        Dim argSep As String = cult.TextInfo.ListSeparator

        If fn IsNot Nothing Then ' It's a function!
            Dim expr As String

            ' Handle TextAppendFunction by doing whatever it is that this is doing
            If fn.TextAppendFunction Then
                Dim txt As String = GetControlText(mParamControls(0), DataType.text)
                If txt = "" Then expr = "{value}" Else expr = txt
                expr &= " & "
            Else
                expr = ""
            End If

            ' Append the parameters found in the signature
            expr &= fn.Name & "("
            For i As Integer = 0 To fn.DefaultSignature.Length - 1
                Dim param As clsFunctionParm = fn.DefaultSignature(i)
                ' Prepend a comma to the expression for all but the first param
                If i > 0 Then expr &= argSep & " "

                Dim txt As String = GetControlText(mParamControls(i), param.DataType)
                If txt = "" Then expr &= "{" & param.Name & "}" Else expr &= txt
            Next

            expr &= ")"

            Return expr

        ElseIf op IsNot Nothing Then ' It's an operator!
            Dim dt As DataType = DataType.unknown
            If op.GroupName = "Text" Then dt = DataType.text
            Return String.Format("{0} {1} {2}", _
             GetControlText(mParamControls(0), dt), _
             op.Symbol, _
             GetControlText(mParamControls(1), dt))

        ElseIf lv IsNot Nothing Then ' It's a literal value
            If lv.DataType = DataType.text _
             Then Return """" & lv.Name & """" _
             Else Return lv.Name

        Else ' So what is it?
            Return ""

        End If


    End Function

    ''' <summary>
    ''' Interprets a child control's value as a string.
    ''' </summary>
    ''' <param name="control">The control</param>
    ''' <returns>The string value</returns>
    Private Function GetControlText(ByVal control As Control) As String
        Return GetControlText(control, DataType.unknown)
    End Function

    ''' <summary>
    ''' Interprets a child control's value as a string.
    ''' </summary>
    ''' <param name="ctl">The control</param>
    ''' <param name="dtype">The datatype</param>
    ''' <returns>The string value</returns>
    Private Function GetControlText(ByVal ctl As Control, ByVal dtype As DataType) As String

        ' Default to just using the text from the control
        Dim txt As String = ctl.Text.Trim()

        If TypeOf ctl Is ComboBox AndAlso ctl.Tag IsNot Nothing Then
            Dim arr() As String = CType(ctl.Tag, String())
            Dim ind As Integer = Array.IndexOf(arr, ctl.Text.Trim())
            If ind >= 0 Then txt = ind.ToString()

        ElseIf TypeOf ctl Is ctlProcessDateTime Then
            Dim cult As CultureInfo = CultureInfo.CurrentCulture
            Dim argSep As String = cult.TextInfo.ListSeparator

            If Not RegexDataItem.IsMatch(txt) Then
                Dim val As clsProcessValue = CType(ctl, IProcessValue).Value
                Try
                    If val.IsNull _
                     Then txt = "" _
                     Else txt = val.FormatDate("'MakeDate('d'" & argSep & "'M'" & argSep & "'yyyy')'")

                Catch
                    txt = ""
                End Try
            End If

        End If

        ' For text types, autowrap any entries with quotes if they are not there
        ' already. Don't autowrap data items ("[]") or function calls ("()").
        If dtype = DataType.text AndAlso txt <> "" _
         AndAlso txt.IndexOfAny("""[]()".ToCharArray()) < 0 Then
            txt = """" & txt & """"
        End If

        Return txt

    End Function

    ''' <summary>
    ''' Sets the body panel's scroll margin according to a child control's position.
    ''' </summary>
    ''' <param name="child">The child control</param>
    Private Sub SetAutoScrollMargins(ByVal child As Control)
        ' If the text box is outside the panel's bounds, turn on auto-scrolling and set the margin. 
        If (child.Location.X > pnlBody.Location.X) Or (child.Location.Y > pnlBody.Location.Y) Then
            pnlBody.AutoScroll = True
            ' If the AutoScrollMargin is set to less than (5,5), set it to 5,5. 
            If (pnlBody.AutoScrollMargin.Width < 5) Or (pnlBody.AutoScrollMargin.Height < 5) Then
                pnlBody.SetAutoScrollMargin(5, 5)
            End If
        End If
    End Sub

    Private Sub btnPaste_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPaste.Click
        If Not Me.Tag Is Nothing Then
            RaiseEvent PasteExpression(GetExpression(Me.Tag))
        End If
    End Sub

    Private Sub MyBase_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles MyBase.DragEnter
        If e.Data.GetDataPresent(GetType(clsFunction)) Or e.Data.GetDataPresent(GetType(clsProcessOperators.clsProcessOperator)) Then
            e.Effect = DragDropEffects.Move
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub MyBase_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles MyBase.DragDrop
        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim NodeTag As Object = CType(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag

            Select Case True
                Case (TypeOf NodeTag Is clsFunction)
                    Populate(CType(NodeTag, clsFunction))
                Case (TypeOf NodeTag Is clsProcessOperators.clsProcessOperator)
                    Populate(CType(NodeTag, clsProcessOperators.clsProcessOperator))
            End Select
        End If
    End Sub

    Private Sub Control_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs)

        Dim ctl As Control = TryCast(sender, Control)
        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim NodeTag As Object = CType(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag

            Select Case True

                Case (TypeOf NodeTag Is IDataField)
                    ctl.Text = "[" & DirectCast(NodeTag, IDataField).FullyQualifiedName & "]"

                Case (TypeOf NodeTag Is Boolean)
                    ctl.Text = CType(NodeTag, clsExpressionTreeView.LiteralValue).Name

            End Select
        End If

    End Sub

    Private Sub Control_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs)
        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim NodeTag As Object = CType(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag
            Select Case True
                Case (TypeOf NodeTag Is IDataField), (TypeOf NodeTag Is clsProcessParameter), (TypeOf NodeTag Is Boolean)
                    e.Effect = DragDropEffects.Move
                Case Else
                    e.Effect = DragDropEffects.None
            End Select
        End If
    End Sub

    Private Sub ctlFunction_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        pnlHelp.Visible = True
        pnlHelp.BringToFront()
        lblFooter.Visible = False
        Me.btnPaste.Visible = False
    End Sub
End Class


