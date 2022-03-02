
Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore

''' Project  : Automate
''' Class    : ctlProcessExpressionBuilder
''' 
''' <summary>
''' The control used for building expressions by dragging/dropping data items
''' function names etc.
''' </summary>
Friend Class ctlProcessExpressionBuilder
    Inherits UserControl

#Region " Delegate Definitions "

    ''' <summary>
    ''' Signature definition for validation callback.
    ''' </summary>
    Public Delegate Function ExpressionValidator(ByVal ShowConfirm As Boolean) As Boolean

    ''' <summary>
    ''' Signature definition for testing callback.
    ''' </summary>
    Public Delegate Sub ExpressionTester()

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' Stage object used for building expression. Determines scope etc.
    ''' </summary>
    Private mStage As clsProcessStage

    ''' <summary>
    ''' The last stage added as the result of an autocreate request
    ''' </summary>
    Private mLastStageAdded As clsProcessStage

    ''' <summary>
    ''' The last relative position used in an autocreate request.
    ''' </summary>
    Private mLastRelativePosition As clsProcessStagePositioner.RelativePositions

    ''' <summary>
    ''' The method delegate which should be used to validate expressions in this
    ''' control.
    ''' </summary>
    Private mValidator As ExpressionValidator = AddressOf IsValidExpression

    ''' <summary>
    ''' The method delegate which should be used to evaluate expressions in this
    ''' control.
    ''' </summary>
    Private mTester As ExpressionTester = AddressOf TestExpression
    Private WithEvents mExpressionSplitPane As System.Windows.Forms.SplitContainer

    ' The process viewer used to launch the stage properties
    Private mProcessViewer As ctlProcessViewer


#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new process expression builder control.
    ''' </summary>
    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.SetStyle(ControlStyles.ResizeRedraw, True)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The validation routine to be called when validate button is clicked.
    ''' Defaults to IsValidExpression. You may want to change this to
    ''' IsValidDecision().
    ''' </summary>
    ''' <value>Value.</value>
    Public WriteOnly Property Validator() As ExpressionValidator
        Set(ByVal value As ExpressionValidator)
            mValidator = value
        End Set
    End Property

    ''' <summary>
    ''' The testing routine to be called when test button is clicked.
    ''' Defaults to TestExpression. You may want to change this to
    ''' TestDecision().
    ''' </summary>
    ''' <value>Value.</value>
    Public WriteOnly Property Tester() As ExpressionTester
        Set(ByVal value As ExpressionTester)
            mTester = value
        End Set
    End Property

    ''' <summary>
    ''' The text set in the Store In result text field within this builder. Note that
    ''' any leading/trailing "[" or "]" chars respectively are trimmed from the
    ''' value before returning it from this property.
    ''' </summary>
    Public Property StoreInText() As String
        Get
            Return txtResult.Text.TrimStart(" "c, "["c).TrimEnd(" "c, "]"c)
        End Get
        Set(ByVal value As String)
            txtResult.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the expression text in the expression field within this builder
    ''' </summary>
    Public Property ExpressionText() As String
        Get
            Return rtbExpression.Text
        End Get
        Set(ByVal value As String)
            rtbExpression.Text = value
            rtbExpression.ColourText()
        End Set
    End Property

    ''' <summary>
    ''' Gets the expression text in this builder after trimming and compiling into a
    ''' single line.
    ''' </summary>
    Public ReadOnly Property ExpressionTrimmedText() As String
        Get
            Return ExpressionText.Trim().Replace(vbCrLf, " ")
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets whether the 'Store In' label/textbox is visible in this control
    ''' </summary>
    Public Property StoreInVisible() As Boolean
        Get
            Return txtResult.Visible
        End Get
        Set(ByVal value As Boolean)
            txtResult.Visible = value
            lblResult.Visible = value
        End Set
    End Property

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
            functionCtl.ProcessViewer = value
            treeDataItems.ProcessViewer = value
        End Set
    End Property

#End Region

#Region " Windows Form Designer generated code "

    'Form overrides dispose to clean up the component list.
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
    Friend WithEvents lblFunction As System.Windows.Forms.Label
    Friend WithEvents treeDataItems As AutomateUI.ctlDataItemTreeView
    Friend WithEvents btnTest As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblFuncDetail As System.Windows.Forms.Label
    Private WithEvents rtbExpression As AutomateUI.ctlExpressionRichTextBox
    Friend WithEvents functionCtl As AutomateUI.ctlFunction
    Friend WithEvents lblResult As System.Windows.Forms.Label
    Private WithEvents txtResult As ctlStoreInEdit
    Friend WithEvents btnValidate As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents treeExpression As AutomateUI.clsExpressionTreeView
    Friend WithEvents lblDataItems As System.Windows.Forms.Label
    Friend WithEvents mMainSplitPane As System.Windows.Forms.SplitContainer
    Friend WithEvents mFunctionSplit As System.Windows.Forms.SplitContainer
    Friend WithEvents lblExpression As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessExpressionBuilder))
        Me.mExpressionSplitPane = New System.Windows.Forms.SplitContainer()
        Me.rtbExpression = New AutomateUI.ctlExpressionRichTextBox()
        Me.lblExpression = New System.Windows.Forms.Label()
        Me.btnValidate = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnTest = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.txtResult = New AutomateUI.ctlStoreInEdit()
        Me.lblResult = New System.Windows.Forms.Label()
        Me.mFunctionSplit = New System.Windows.Forms.SplitContainer()
        Me.treeExpression = New AutomateUI.clsExpressionTreeView()
        Me.lblFunction = New System.Windows.Forms.Label()
        Me.functionCtl = New AutomateUI.ctlFunction()
        Me.lblFuncDetail = New System.Windows.Forms.Label()
        Me.lblDataItems = New System.Windows.Forms.Label()
        Me.mMainSplitPane = New System.Windows.Forms.SplitContainer()
        Me.treeDataItems = New AutomateUI.ctlDataItemTreeView()
        CType(Me.mExpressionSplitPane, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mExpressionSplitPane.Panel1.SuspendLayout()
        Me.mExpressionSplitPane.Panel2.SuspendLayout()
        Me.mExpressionSplitPane.SuspendLayout()
        CType(Me.mFunctionSplit, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mFunctionSplit.Panel1.SuspendLayout()
        Me.mFunctionSplit.Panel2.SuspendLayout()
        Me.mFunctionSplit.SuspendLayout()
        CType(Me.mMainSplitPane, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mMainSplitPane.Panel1.SuspendLayout()
        Me.mMainSplitPane.Panel2.SuspendLayout()
        Me.mMainSplitPane.SuspendLayout()
        Me.SuspendLayout()
        '
        'mExpressionSplitPane
        '
        resources.ApplyResources(Me.mExpressionSplitPane, "mExpressionSplitPane")
        Me.mExpressionSplitPane.Name = "mExpressionSplitPane"
        '
        'mExpressionSplitPane.Panel1
        '
        Me.mExpressionSplitPane.Panel1.Controls.Add(Me.rtbExpression)
        Me.mExpressionSplitPane.Panel1.Controls.Add(Me.lblExpression)
        Me.mExpressionSplitPane.Panel1.Controls.Add(Me.btnValidate)
        Me.mExpressionSplitPane.Panel1.Controls.Add(Me.btnTest)
        Me.mExpressionSplitPane.Panel1.Controls.Add(Me.txtResult)
        Me.mExpressionSplitPane.Panel1.Controls.Add(Me.lblResult)
        '
        'mExpressionSplitPane.Panel2
        '
        Me.mExpressionSplitPane.Panel2.Controls.Add(Me.mFunctionSplit)
        '
        'rtbExpression
        '
        Me.rtbExpression.AllowDrop = True
        resources.ApplyResources(Me.rtbExpression, "rtbExpression")
        Me.rtbExpression.DetectUrls = False
        Me.rtbExpression.HideSelection = False
        Me.rtbExpression.HighlightingEnabled = True
        Me.rtbExpression.Name = "rtbExpression"
        Me.rtbExpression.PasswordChar = ChrW(0)
        '
        'lblExpression
        '
        resources.ApplyResources(Me.lblExpression, "lblExpression")
        Me.lblExpression.Name = "lblExpression"
        '
        'btnValidate
        '
        resources.ApplyResources(Me.btnValidate, "btnValidate")
        Me.btnValidate.Name = "btnValidate"
        Me.btnValidate.UseVisualStyleBackColor = False
        '
        'btnTest
        '
        resources.ApplyResources(Me.btnTest, "btnTest")
        Me.btnTest.Name = "btnTest"
        Me.btnTest.UseVisualStyleBackColor = False
        '
        'txtResult
        '
        Me.txtResult.AllowDrop = True
        resources.ApplyResources(Me.txtResult, "txtResult")
        Me.txtResult.AutoCreateDefault = Nothing
        Me.txtResult.BackColor = System.Drawing.Color.White
        Me.txtResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtResult.Name = "txtResult"
        Me.txtResult.PasswordChar = ChrW(0)
        '
        'lblResult
        '
        resources.ApplyResources(Me.lblResult, "lblResult")
        Me.lblResult.Name = "lblResult"
        '
        'mFunctionSplit
        '
        resources.ApplyResources(Me.mFunctionSplit, "mFunctionSplit")
        Me.mFunctionSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.mFunctionSplit.Name = "mFunctionSplit"
        '
        'mFunctionSplit.Panel1
        '
        Me.mFunctionSplit.Panel1.Controls.Add(Me.treeExpression)
        Me.mFunctionSplit.Panel1.Controls.Add(Me.lblFunction)
        '
        'mFunctionSplit.Panel2
        '
        Me.mFunctionSplit.Panel2.Controls.Add(Me.functionCtl)
        Me.mFunctionSplit.Panel2.Controls.Add(Me.lblFuncDetail)
        '
        'treeExpression
        '
        resources.ApplyResources(Me.treeExpression, "treeExpression")
        Me.treeExpression.ItemHeight = 20
        Me.treeExpression.Name = "treeExpression"
        Me.treeExpression.Process = Nothing
        Me.treeExpression.Sorted = True
        '
        'lblFunction
        '
        resources.ApplyResources(Me.lblFunction, "lblFunction")
        Me.lblFunction.Name = "lblFunction"
        '
        'functionCtl
        '
        resources.ApplyResources(Me.functionCtl, "functionCtl")
        Me.functionCtl.BackColor = System.Drawing.Color.White
        Me.functionCtl.Name = "functionCtl"
        '
        'lblFuncDetail
        '
        resources.ApplyResources(Me.lblFuncDetail, "lblFuncDetail")
        Me.lblFuncDetail.Name = "lblFuncDetail"
        '
        'lblDataItems
        '
        resources.ApplyResources(Me.lblDataItems, "lblDataItems")
        Me.lblDataItems.Name = "lblDataItems"
        '
        'mMainSplitPane
        '
        resources.ApplyResources(Me.mMainSplitPane, "mMainSplitPane")
        Me.mMainSplitPane.Name = "mMainSplitPane"
        '
        'mMainSplitPane.Panel1
        '
        Me.mMainSplitPane.Panel1.Controls.Add(Me.mExpressionSplitPane)
        '
        'mMainSplitPane.Panel2
        '
        Me.mMainSplitPane.Panel2.Controls.Add(Me.treeDataItems)
        Me.mMainSplitPane.Panel2.Controls.Add(Me.lblDataItems)
        '
        'treeDataItems
        '
        resources.ApplyResources(Me.treeDataItems, "treeDataItems")
        Me.treeDataItems.CheckBoxes = False
        Me.treeDataItems.IgnoreScope = False
        Me.treeDataItems.Name = "treeDataItems"
        Me.treeDataItems.Stage = Nothing
        Me.treeDataItems.StatisticsMode = False
        '
        'ctlProcessExpressionBuilder
        '
        Me.Controls.Add(Me.mMainSplitPane)
        Me.Name = "ctlProcessExpressionBuilder"
        resources.ApplyResources(Me, "$this")
        Me.mExpressionSplitPane.Panel1.ResumeLayout(False)
        Me.mExpressionSplitPane.Panel1.PerformLayout()
        Me.mExpressionSplitPane.Panel2.ResumeLayout(False)
        CType(Me.mExpressionSplitPane, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mExpressionSplitPane.ResumeLayout(False)
        Me.mFunctionSplit.Panel1.ResumeLayout(False)
        Me.mFunctionSplit.Panel1.PerformLayout()
        Me.mFunctionSplit.Panel2.ResumeLayout(False)
        Me.mFunctionSplit.Panel2.PerformLayout()
        CType(Me.mFunctionSplit, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mFunctionSplit.ResumeLayout(False)
        Me.mMainSplitPane.Panel1.ResumeLayout(False)
        Me.mMainSplitPane.Panel2.ResumeLayout(False)
        Me.mMainSplitPane.Panel2.PerformLayout()
        CType(Me.mMainSplitPane, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mMainSplitPane.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Sets the stage being worked with and populates control with relevant info.
    ''' </summary>
    ''' <param name="stg">The stage to populate this control with. A null stage is
    ''' ignored.</param>
    Public Sub SetStage(ByVal stg As clsProcessStage)

        If stg Is Nothing Then Return

        Me.mStage = stg
        If mStage.StageType = StageTypes.Calculation Then
            With CType(stg, clsCalculationStage)
                rtbExpression.Text = .Expression.LocalForm
                txtResult.Text = .StoreIn
            End With
        End If

        'The expression tree view needs to know what process we
        'are working with...
        treeExpression.Process = mStage.Process

        'populate expression textbox
        rtbExpression.ColourText()
        rtbExpression.Select(rtbExpression.Text.Length, 0)
        rtbExpression.ClearUndoBuffer()
        rtbExpression.AddCurentStateToUndoBuffer()

        'paint the form as early as possible to make it appear to be more responsive
        Me.Update()
        'then finish off other stuff...

        'populate data items treeview
        functionCtl.SetStage(mStage)
        treeDataItems.Populate(mStage)
        treeExpression.Populate()

    End Sub

#End Region

#Region " Test / Evaluate methods "

    ''' <summary>
    ''' Validates the current expression displaying result in user interface message.
    ''' </summary>
    ''' <param name="showConfirm">True to show a confirmation to the user if the
    ''' expression is valid.</param>
    ''' <returns>True if the expression is valid, False otherwise.</returns>
    Public Function IsValidExpression(ByVal showConfirm As Boolean) As Boolean
        Try
            Dim sErr As String = Nothing

            'Check the expression is valid...
            Dim proc As clsProcess = mStage.Process
            Dim expr As BPExpression = BPExpression.FromLocalised(rtbExpression.Text)

            If Not clsExpression.EvaluateExpression(expr, Nothing, mStage, True, Nothing, sErr) Then
                Throw New InvalidExpressionException(
                 My.Resources.ctlProcessExpressionBuilder_YourExpressionDoesNotAppearToBeValid001, vbCrLf, sErr)
            End If

            'Check casting of result type to target type, scope, and related issues
            Dim calcStg As clsCalculationStage = TryCast(mStage, clsCalculationStage)
            If calcStg IsNot Nothing Then
                calcStg.Expression = expr
                calcStg.StoreIn = txtResult.Text

                Dim errors As List(Of ValidateProcessResult) = calcStg.CheckCalculation()
                If errors.Count > 0 Then
                    Dim rules = gSv.GetValidationInfo()
                    Dim validationInfo = rules.ToDictionary(Of Integer, clsValidationInfo)(Function(y) y.CheckID, Function(z) z)
                    Dim sb As New StringBuilder(My.Resources.ctlProcessExpressionBuilder_TheFollowingErrorsExist)
                    sb.AppendLine().AppendLine()
                    For Each err As ValidateProcessResult In errors
                        sb.AppendLine(err.FormatMessage(validationInfo(err.CheckID).Message))
                    Next
                    Throw New InvalidExpressionException(sb.ToString())
                End If
            End If

            If showConfirm Then UserMessage.OK(My.Resources.ctlProcessExpressionBuilder_TheExpressionIsValid)

            Return True

        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
            rtbExpression.Focus()
            Return False

        End Try
    End Function


    ''' <summary>
    ''' Checks if the current expression is valid and that its resultant
    ''' data type matches that specified.
    ''' </summary>
    ''' <param name="dtype">The desired data type of the result of the
    ''' expression.</param>
    ''' <returns>True if the expression is valid and returns a value with the
    ''' desired data type; False if it is invalid, or returns a value of a type
    ''' different to that specified.
    ''' <strong>Note: </strong> If the expression is not valid, the user will be
    ''' told using a UI dialog where the problem lies.</returns>
    Private Function IsValid(ByVal dtype As DataType) As Boolean
        Try
            Dim proc As clsProcess = mStage.Process
            Dim res As clsProcessValue = Nothing
            Dim sErr As String = Nothing

            If Not clsExpression.EvaluateExpression(
             BPExpression.FromLocalised(rtbExpression.Text), res, mStage,
             True, Nothing, sErr) Then Throw New InvalidExpressionException(sErr)

            If res.DataType <> dtype Then
                If dtype = DataType.text _
                 Then Throw New InvalidExpressionException(My.Resources.ctlProcessExpressionBuilder_TheResultOfTheExpressionMustBeText) _
                 Else Throw New InvalidExpressionException(My.Resources.ctlProcessExpressionBuilder_TheResultOfTheExpressionMustBeA0, dtype)
            End If
            Return True

        Catch ex As Exception
            UserMessage.Err(ex, ex.Message)
            rtbExpression.Focus()
            Return False

        End Try
    End Function

    ''' <summary>
    ''' Validates the current expression as a decision displaying result in
    ''' user interface message.
    ''' </summary>
    Public Function IsValidDecision(ByVal showConfirm As Boolean) As Boolean

        If IsValid(DataType.flag) Then
            If showConfirm Then
                UserMessage.OK(My.Resources.ctlProcessExpressionBuilder_TheExpressionIsValidDecision)
            End If
            Return True
        Else
            Return False
        End If

    End Function

    ''' <summary>
    ''' Checks if the current expression is valid for an alert - basically that
    ''' it resolves to a text data type.
    ''' </summary>
    ''' <param name="showConfirm">True to tell the user that the expression is
    ''' valid; False to keep silent if it is valid, and only indicate to the
    ''' user if it is invalid.</param>
    ''' <returns>True if the current expression resolves to a text data item,
    ''' False otherwise.</returns>
    Public Function IsValidAlert(ByVal showConfirm As Boolean) As Boolean

        If IsValid(DataType.text) Then
            If showConfirm Then
                UserMessage.OK(My.Resources.ctlProcessExpressionBuilder_TheExpressionIsValidDecision)
            End If
            Return True
        Else
            Return False
        End If

    End Function


    ''' <summary>
    ''' Tests the given expression and displays the results through a UI message.
    ''' </summary>
    ''' <param name="localExpr">The expression to test - this may be a full
    ''' expression or the selected portion of an expression.</param>
    Public Sub TestExpression(ByVal localExpr As String)
        Try
            Dim proc As clsProcess = mStage.Process
            Dim res As clsProcessValue = Nothing
            Dim info As clsExpressionInfo = Nothing
            Dim err As String = Nothing
            Dim expr As BPExpression = BPExpression.FromLocalised(localExpr)

            If Not clsExpression.EvaluateExpression(
             expr, res, mStage, True, info, err) Then _
             Throw New InvalidExpressionException(err)

            'Check if the expression references any data items:
            If info.DataItems.Count = 0 Then

                'If no data items were referenced, we can simply
                'evaluate the expression and display the result to
                'the user....
                If clsExpression.EvaluateExpression(
                 expr, res, mStage, False, Nothing, err) Then
                    UserMessage.OK(My.Resources.ctlProcessExpressionBuilder_ExpressionResult01,
                     res.FormattedValue, clsDataTypeInfo.GetLocalizedFriendlyName(res.DataType))

                Else
                    Throw New InvalidExpressionException(err)

                End If

            Else
                'The expression references data items, so before we can can evaluate it, the user needs to supply some values for them
                Using f As New frmExpressionTest(Me, proc, localExpr, mStage, txtResult.Text)
                    AddHandler f.NewExpressionApplied, AddressOf NewExpressionAppliedHandler
                    f.StartPosition = FormStartPosition.CenterParent
                    f.ShowDialog(Me)
                End Using
            End If

        Catch ex As Exception
            UserMessage.Show(ex.Message, ex)
        End Try
        rtbExpression.Focus()

    End Sub

    Public Sub NewExpressionAppliedHandler(sender As Object, expression As String)
        If Not String.IsNullOrEmpty(rtbExpression.SelectedText) Then
            Dim startIndex = rtbExpression.SelectionStart
            rtbExpression.SelectedText = expression
            rtbExpression.Select(startIndex, expression.Length)
        Else
            rtbExpression.Text = expression
        End If
    End Sub

    ''' <summary>
    ''' Tests the current expression as a decision displaying result in
    ''' user interface message.
    ''' </summary>
    Public Sub TestDecision()
        TestExpression(rtbExpression.Text)
    End Sub

    ''' <summary>
    ''' Tests the current expression displaying result in
    ''' user interface message.
    ''' </summary>
    Public Sub TestExpression()
        If rtbExpression.SelectedText <> "" Then
            TestExpression(rtbExpression.SelectedText)
        Else
            TestExpression(rtbExpression.Text)
        End If
    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Pastes the given expression into the expression text box.
    ''' </summary>
    ''' <param name="e">The expression to paste in.</param>
    Private Sub HandleExpressionPasted(ByVal e As String) Handles functionCtl.PasteExpression
        rtbExpression.SelectedText = e
    End Sub


    ''' <summary>
    ''' Handler for an expression being selected in the express treeview.
    ''' </summary>
    ''' <param name="e">The source of the event.</param>
    Private Sub HandleExpressionSelected(ByVal e As Object) Handles treeExpression.SelectExpression
        functionCtl.Populate(e)
    End Sub

    ''' <summary>
    ''' Handles data being dropped into the results text box.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The arguments defining the event.</param>
    Private Sub HandleDragDropped(ByVal sender As Object, ByVal e As DragEventArgs) Handles txtResult.DragDrop

        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim NodeTag As Object = CType(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag
            If TypeOf NodeTag Is IDataField Then
                txtResult.Text = DirectCast(NodeTag, IDataField).FullyQualifiedName
            End If
        End If

    End Sub


    ''' <summary>
    ''' Handles data being dragged into the results text box.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">The arguments detailing the event.</param>
    Private Sub HandleDragEntered(ByVal sender As Object, ByVal e As DragEventArgs) Handles txtResult.DragEnter
        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim NodeTag As Object = CType(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag
            If TypeOf NodeTag Is IDataField Then
                e.Effect = DragDropEffects.Move
            Else
                e.Effect = DragDropEffects.None
            End If
        End If
    End Sub


    ''' <summary>
    ''' Handles the "Check for errors" button being clicked.
    ''' </summary>
    Private Sub HandleCheckForErrors(ByVal sender As Object, ByVal e As EventArgs) Handles btnValidate.Click
        If mValidator IsNot Nothing Then
            mValidator.Invoke(True)
        Else
            UserMessage.Show(My.Resources.ctlProcessExpressionBuilder_InternalConfigurationErrorNoValidatorPresent)
        End If
        rtbExpression.Invalidate()
    End Sub


    ''' <summary>
    ''' Handles the 'Evaluate [Expression/Selection]' button being clicked.
    ''' </summary>
    Protected Sub HandleEvaluateClicked(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTest.Click
        If mTester IsNot Nothing Then
            Try
                mTester.Invoke()
            Catch ' This should already have been displayed to the user... just ignore it.
            End Try
        Else
            UserMessage.Show(My.Resources.ctlProcessExpressionBuilder_InternalConfigurationErrorNoTesterPresent)
        End If
        rtbExpression.Invalidate()
    End Sub

    ''' <summary>
    ''' Handles the text being changed in the expression text box.
    ''' </summary>
    Private Sub HandleExpressionChanged(ByVal sender As Object, ByVal e As EventArgs) Handles rtbExpression.TextChanged
        Me.btnTest.Enabled = (rtbExpression.Text.Length > 0)
        Me.btnValidate.Enabled = Me.btnTest.Enabled
    End Sub

    ''' <summary>
    ''' Handles the selection changing in the expression text box. (ie. text being
    ''' selected or unselected)
    ''' </summary>
    Private Sub HandleExpressionSelectedTextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles rtbExpression.SelectionChanged
        If rtbExpression.SelectedText <> "" Then
            Me.btnTest.Text = My.Resources.ctlProcessExpressionBuilder_EvaluateSelection
        Else
            Me.btnTest.Text = My.Resources.ctlProcessExpressionBuilder_EvaluateExpression
        End If
    End Sub

    ''' <summary>
    ''' Handles the 'autocreate' function of the 'store in' edit box being requested
    ''' </summary>
    ''' <param name="name">The name of the data item that was requested.</param>
    Private Sub HandleAutoCreateRequested(ByVal name As String) Handles txtResult.AutoCreateRequested

        Dim dt As DataType = _
         clsProcessStagePositioner.DataTypeFromExpression(mStage, rtbExpression.Text)

        Dim stg As clsDataStage = _
         clsProcessStagePositioner.CreateDataItem(name, mStage, dt, mLastStageAdded, mLastRelativePosition)

        If stg IsNot Nothing Then treeDataItems.Repopulate(stg)

    End Sub

    ''' <summary>
    ''' Handles a splitter containing the expression rich text box being moved.
    ''' The rich text box draws its own border, but it appears to use its bounds
    ''' from before the splitter was moved - this means that the bits of the border
    ''' that weren't drawn get rendered in the bog standard Win2K stylee 3d border.
    ''' This ensures that the panel containing the rich text box is invalidated so
    ''' that the border is correctly drawn.
    ''' </summary>
    Private Sub HandleSplitterMoved(ByVal sender As Object, ByVal e As SplitterEventArgs) _
     Handles mMainSplitPane.SplitterMoved, mExpressionSplitPane.SplitterMoved
        If rtbExpression.Parent IsNot Nothing Then rtbExpression.Parent.Invalidate(True)
    End Sub

#End Region

End Class
