Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib.Extensions
Imports AutomateControls
Imports AutomateUI.Extensions
Imports System.Runtime.CompilerServices

''' Project  : Automate
''' Class    : frmExpressionTest
''' 
''' <summary>
''' A form to test expressions by substituting data item names for temporary values. 
''' </summary>
Friend Class frmExpressionTest : Inherits Forms.TitledHelpButtonForm

#Region " SandboxProcess class "

    ''' <summary>
    ''' Limited scope process used to contain the fields used within the expression
    ''' being tested. This process isn't suitable for anything other than as a
    ''' sandbox in which values can be set and collections can be modified without
    ''' any danger of affecting the underlying process.
    ''' </summary>
    Private Class SandboxProcess : Inherits clsProcess

        ' The base process from which this process is derived.
        Private mBaseProcess As clsProcess

        ' The calc / decision stage where the expression lies. This is a clone of the
        ' actual stage in the original process. It has the same ID (but a different
        ' subsheet ID)
        Private mStage As clsProcessStage

        ' The expression which is being tested
        Private mExpression As BPExpression

        ' The expression information regarding the expression that this process is
        ' dealing with (and thus the fields it copies from the base process)
        Private mExprInfo As clsExpressionInfo

        ''' <summary>
        ''' Creates a new sandbox process using the given process, stage and
        ''' expression. This process will not be functional as a process, but
        ''' provides a place where the relevant data items extracted from the
        ''' expression can be modified without danger of altering the data in the
        ''' parent process if any unexpected errors should occur.
        ''' </summary>
        ''' <param name="proc">The base process from which to draw the data stages.
        ''' </param>
        ''' <param name="procStage">The calc or decision stage which contains the
        ''' expression.</param>
        ''' <param name="expr">The expression being tested.</param>
        ''' <exception cref="OutOfScopeException">If any of the referenced data items
        ''' are out of scope of the stage containing the expression</exception>
        ''' <exception cref="InvalidExpressionException">If the expression could not
        ''' be evaluated correctly</exception>
        Public Sub New(
         ByVal proc As clsProcess, ByVal procStage As clsProcessStage, ByVal expr As String)

            ' The 'type' (ie. process/object) is largely arbitrary - this is only here as a
            ' coathanger for a few data stages - not for a full on process.
            ' Likewise - make it 'readonly' - we have no need for business objects at
            ' all, so optimise as much as possible away.
            MyBase.New(Nothing, DiagramType.Process, False)

            mBaseProcess = proc
            mExpression = BPExpression.FromLocalised(expr)

            ' Clone the given process stage, so that it exists within this process
            ' to allow for use as a scope stage.
            mStage = procStage.Clone()
            mStage.SetSubSheetID(GetActiveSubSheet())
            AddStage(mStage)

            ' Validate to get information about the expression, which
            ' we will store and use throughout the class...
            Dim val As clsProcessValue = Nothing, sErr As String = Nothing
            If Not clsExpression.EvaluateExpression(
             mExpression, val, procStage, True, mExprInfo, sErr) Then
                'This should not happen, since we validate the expression
                'before we came in here...
                Throw New InvalidExpressionException(My.Resources.ErrorInExpression0, sErr)
            End If

            ' Copy each of the data items referred to in the expression info into this
            ' sandbox process - these can be modified to our hearts content without
            ' any danger of affecting the base process.
            For Each stgId As Guid In mExprInfo.DataItems
                Dim stg As clsDataStage
                Dim dataStg As clsDataStage = DirectCast(proc.GetStage(stgId), clsDataStage)
                If Not dataStg.IsInScope(procStage) Then
                    Throw New OutOfScopeException("Stage '{0}' is out of scope", dataStg.Name)
                End If

                ' If it's a collection, copy the structure from the stage
                If dataStg.DataType = DataType.collection Then
                    Dim baseCollStg As clsCollectionStage =
                     DirectCast(dataStg, clsCollectionStage)

                    Dim collstg As clsCollectionStage = New clsCollectionStage(Me)
                    ' Force creation of the definition, and populate it from the
                    ' *current value* in the base stage.
                    collstg.SingleRow = False
                    Dim defn As clsCollectionInfo = Nothing
                    If baseCollStg.Value IsNot Nothing _
                     AndAlso baseCollStg.Value.Collection IsNot Nothing _
                     Then defn = baseCollStg.Value.Collection.Definition
                    ' If there is no current value, just use the collection stage's
                    ' definition
                    If defn Is Nothing Then defn = baseCollStg.Definition
                    collstg.Definition.SetFrom(defn)

                    ' Set the value
                    collstg.Value = baseCollStg.Value.Clone()

                    stg = collstg
                Else
                    ' Otherwise, just create a data stage.
                    stg = New clsDataStage(Me)
                    stg.SetDataType(dataStg.GetDataType())
                    stg.Value = dataStg.Value.Clone()
                End If

                ' Set its basic stuff from the base process so that the expression
                ' can still be evaluated correctly
                stg.Id = stgId
                stg.Name = dataStg.Name

                AddStage(stg)
            Next
        End Sub

        ''' <summary>
        ''' The base process from where this process's data stages were drawn
        ''' </summary>
        Public ReadOnly Property BaseProcess() As clsProcess
            Get
                Return mBaseProcess
            End Get
        End Property

        ''' <summary>
        ''' The stage to use for scoping when evaluating the expression.
        ''' </summary>
        Public ReadOnly Property ScopeStage() As clsProcessStage
            Get
                Return mStage
            End Get
        End Property

        ''' <summary>
        ''' The expression being tested.
        ''' </summary>
        Public ReadOnly Property Expression() As BPExpression
            Get
                Return mExpression
            End Get
        End Property

        ''' <summary>
        ''' The expression info from the expression held in this process.
        ''' </summary>
        Public ReadOnly Property ExpressionInfo() As clsExpressionInfo
            Get
                Return mExprInfo
            End Get
        End Property

    End Class

#End Region

#Region " Windows Form Designer generated code "

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
            ' Ensure that the process is disposed of (though it shouldn't really
            ' be using much in the way of resources, bearing in mind it's only
            ' a little sandbox process).
            mProcess.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents rtbExpression As AutomateUI.ctlExpressionRichTextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtResult As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnTest As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents pnlBody As System.Windows.Forms.Panel
    Friend WithEvents lblNameTemplate As System.Windows.Forms.Label
    Protected WithEvents btnCancel As Buttons.StandardStyledButton
    Protected WithEvents btnOK As Buttons.StandardStyledButton
    Friend WithEvents txtValueTemplate As AutomateControls.Textboxes.StyledTextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmExpressionTest))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.rtbExpression = New AutomateUI.ctlExpressionRichTextBox()
        Me.pnlBody = New System.Windows.Forms.Panel()
        Me.lblNameTemplate = New System.Windows.Forms.Label()
        Me.txtValueTemplate = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtResult = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnTest = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Panel1.SuspendLayout()
        Me.pnlBody.SuspendLayout()
        Me.SuspendLayout()
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Controls.Add(Me.rtbExpression)
        Me.Panel1.Controls.Add(Me.pnlBody)
        Me.Panel1.Controls.Add(Me.Label4)
        Me.Panel1.Controls.Add(Me.Label3)
        Me.Panel1.Controls.Add(Me.txtResult)
        Me.Panel1.Controls.Add(Me.btnTest)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Name = "Panel1"
        '
        'rtbExpression
        '
        Me.rtbExpression.AllowDrop = True
        resources.ApplyResources(Me.rtbExpression, "rtbExpression")
        Me.rtbExpression.BackColor = System.Drawing.SystemColors.Window
        Me.rtbExpression.DetectUrls = False
        Me.rtbExpression.ForeColor = System.Drawing.SystemColors.WindowText
        Me.rtbExpression.HideSelection = False
        Me.rtbExpression.HighlightingEnabled = True
        Me.rtbExpression.Name = "rtbExpression"
        Me.rtbExpression.PasswordChar = ChrW(0)
        Me.rtbExpression.TabStop = False
        '
        'pnlBody
        '
        resources.ApplyResources(Me.pnlBody, "pnlBody")
        Me.pnlBody.BackColor = System.Drawing.SystemColors.Control
        Me.pnlBody.Controls.Add(Me.lblNameTemplate)
        Me.pnlBody.Controls.Add(Me.txtValueTemplate)
        Me.pnlBody.Name = "pnlBody"
        '
        'lblNameTemplate
        '
        resources.ApplyResources(Me.lblNameTemplate, "lblNameTemplate")
        Me.lblNameTemplate.Name = "lblNameTemplate"
        '
        'txtValueTemplate
        '
        Me.txtValueTemplate.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtValueTemplate, "txtValueTemplate")
        Me.txtValueTemplate.Name = "txtValueTemplate"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'txtResult
        '
        Me.txtResult.BackColor = System.Drawing.SystemColors.Window
        Me.txtResult.BorderColor = System.Drawing.Color.Empty
        Me.txtResult.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtResult.HideSelection = False
        resources.ApplyResources(Me.txtResult, "txtResult")
        Me.txtResult.Name = "txtResult"
        Me.txtResult.ReadOnly = True
        Me.txtResult.TabStop = False
        '
        'btnTest
        '
        Me.btnTest.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.btnTest, "btnTest")
        Me.btnTest.Name = "btnTest"
        Me.btnTest.UseVisualStyleBackColor = False
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.BackColor = System.Drawing.Color.White
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = False
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.BackColor = System.Drawing.Color.White
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = False
        '
        'frmExpressionTest
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "frmExpressionTest"
        Me.Title = "Give each data item a temporary value to obtain a test result"
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.Panel1, 0)
        Me.Controls.SetChildIndex(Me.btnOK, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.pnlBody.ResumeLayout(False)
        Me.pnlBody.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Static 'Show()' utility methods "

    ''' <summary>
    ''' Shows the expression test form with the given arguments.
    ''' </summary>
    ''' <param name="owner">The window which should take 'ownership' of the
    ''' dialog. Null indicates no parent window.</param>
    ''' <param name="proc">The process which is being shown from.</param>
    ''' <param name="expr">The expression being tested.</param>
    ''' <param name="stage">The calc / decision stage to which the expression
    ''' applies.</param>
    ''' <param name="name">The result data item name.</param>
    Public Overloads Shared Sub Show(
     ByVal owner As Control,
     ByVal proc As clsProcess,
     ByVal expr As String,
     ByVal stage As clsProcessStage,
     ByVal name As String)
        Using f As New frmExpressionTest(owner, proc, expr, stage, name)
            f.ShowDialog(owner)
        End Using
    End Sub

#End Region

#Region " Member Variables "

    ' The process.
    Private mProcess As SandboxProcess

    ' The controls used to hold the test values mapped against their label.
    Private mControls As IDictionary(Of Label, Control)

    ' The control with focus.
    Private ctlFocus As Control

    ' Information about the expression, as derived from clsProcess.EvaluateExpression
    Private mExpressionInfo As clsExpressionInfo

    ' A flag to indicate that the result field has been made bigger to accommodate
    ' potentially large text results
    Private mbHeightIsExpanded As Boolean

#End Region

    Public Delegate Sub NewExpressionAppliedEventHandler(sender As Object, expression As String)

    Public Event NewExpressionApplied As NewExpressionAppliedEventHandler

    ''' <summary>
    ''' Creates a new, empty expression text form
    ''' </summary>
    Public Sub New()
        InitializeComponent()
        Me.Title = My.Resources.frmExpressionTest_GiveEachDataItemATemporaryValueToObtainATestResult
        Me.objBluebar.Title = My.Resources.frmExpressionTest_GiveEachDataItemATemporaryValueToObtainATestResult
    End Sub

    ''' <summary>
    ''' Creates a new, expression text form populated with an expression from a
    ''' process.
    ''' </summary>
    ''' <param name="proc">The process we are working with</param>
    ''' <param name="expr">The expression text</param>
    ''' <param name="stage">The calculation or decision stage</param>
    ''' <param name="name">The result data item name</param>
    Public Sub New(
                  ByVal owner As Control,
                  ByVal proc As clsProcess,
                  ByVal expr As String,
                  ByVal stage As clsProcessStage,
                  ByVal name As String)
        Me.New()

        'Store the expression...
        mProcess = New SandboxProcess(proc, stage, expr)
        mExpressionInfo = mProcess.ExpressionInfo

        'Populate the text box...
        rtbExpression.Text = mProcess.Expression.LocalForm
        rtbExpression.ColourText()

        'Populate the controls...
        Populate()

        SetEnvironmentColoursFromAncestor(owner)
        ShowInTaskbar = False
        HelpButton = True
        MinimizeBox = False
        MaximizeBox = False
    End Sub

    ''' <summary>
    ''' Makes a new row of input controls.
    ''' </summary>
    ''' <param name="item">The item the controls are related to. This will
    ''' either be a data item, or a collection field info object.</param>
    ''' <param name="top">The top position of the row</param>
    ''' <param name="tabIndex">The tab index of the input control</param>
    ''' <param name="scrollbarAdj">The horizontal adjustment
    ''' required for a vertical scroll bar</param>
    Private Sub MakeRow(ByVal item As IDataField,
     ByVal top As Integer, ByVal tabIndex As Integer, ByVal scrollbarAdj As Integer)

        Dim name As String = item.FullyQualifiedName
        If name.Length > 30 Then
            name = "[..." & name.Right(30) & "]"
        Else
            name = "[" & name & "]"
        End If

        'Make a label based on the template label
        Dim lblName As New Label()
        lblName.Top = top
        lblName.Left = lblNameTemplate.Left
        lblName.Height = lblNameTemplate.Height
        lblName.Width = lblNameTemplate.Width
        lblName.Font = lblNameTemplate.Font
        lblName.Text = name
        pnlBody.Controls.Add(lblName)

        'Make an input control based on the template. The Tag
        'for the control is the Stage Guid of the data item it
        'represents.

        Dim ctlValue As Control = clsProcessValueControl.GetControl(item.DataType)
        ctlValue.Tag = item
        ctlValue.Top = top
        ctlValue.Left = txtValueTemplate.Left
        ctlValue.Height = txtValueTemplate.Height
        ctlValue.Width = txtValueTemplate.Width - scrollbarAdj
        ctlValue.Font = txtValueTemplate.Font


        Dim currValue As clsProcessValue = item.Value
        If currValue IsNot Nothing Then
            DirectCast(ctlValue, IProcessValue).Value = currValue.Clone()
        End If

        If TypeOf ctlValue Is TextBox Then
            DirectCast(ctlValue, TextBox).BorderStyle = txtValueTemplate.BorderStyle
        End If

        pnlBody.Controls.Add(ctlValue)

        'Apply a tab stop position
        ctlValue.TabIndex = tabIndex

        'Add an event handler
        AddHandler ctlValue.LostFocus, AddressOf Control_LostFocus

        mControls(lblName) = ctlValue

        If ctlFocus Is Nothing Then ctlFocus = ctlValue

    End Sub

    ''' <summary>
    ''' Creates a label and input control for each data item found in
    ''' the expression and adds them to the body panel.
    ''' </summary>
    Private Sub Populate()

        Dim fields As New List(Of IDataField)

        'Get the data items and collection fields used in the expression.
        For Each stgId As Guid In mExpressionInfo.DataItems
            Dim stg As clsDataStage = CType(mProcess.GetStage(stgId), clsDataStage)
            Dim collStg As clsCollectionStage = TryCast(stg, clsCollectionStage)
            ' If it's a collection stage, get all of definitions of the used fields
            ' and add them to our list.
            If collStg IsNot Nothing Then
                ' Note that even if the fields are not defined in the collection
                ' stage in the main process - they will be in the sandbox process
                ' that this test is operating within, since the definition is copied
                ' from the stage's value not from the stage itself.
                For Each fldName As String In mExpressionInfo.GetFieldNames(stgId)
                    fields.Add(collStg.FindFieldDefinition("$." & fldName))
                Next
            Else
                fields.Add(stg)
            End If
        Next

        'Expand the form to suit the number of elements.
        Const maxRowCount As Integer = 5
        Const rowMargin As Integer = 5

        Dim scrollAdj As Integer = 0
        Dim heightInc As Integer
        Dim top, tabInd As Integer

        If fields.Count > maxRowCount Then
            'Adjust width to accomodate scroll bar
            scrollAdj = 25
            'Line scroll bar up with scroll bar of result
            pnlBody.Width -= pnlBody.Right - txtResult.Right + 1
            heightInc = (maxRowCount - 1) * (txtValueTemplate.Height + rowMargin)
        Else
            heightInc = (fields.Count - 1) * (txtValueTemplate.Height + rowMargin)
        End If

        Me.Height += heightInc
        pnlBody.Height += heightInc
        txtResult.Top += heightInc
        btnTest.Top += heightInc

        'Create the controls for each field (data items / collection fields).
        pnlBody.Controls.Clear()
        mControls = New Dictionary(Of Label, Control)
        top = txtValueTemplate.Top
        tabInd = 1
        For Each fld As IDataField In fields
            MakeRow(fld, top, tabInd, scrollAdj)
            top += txtValueTemplate.Height + rowMargin
            tabInd += 1
        Next

        'Hide the template controls
        lblNameTemplate.Visible = False
        txtValueTemplate.Visible = False
        txtValueTemplate.TabStop = False
        txtResult.Text = ""
        btnTest.TabIndex = tabInd + 2

    End Sub

    ''' <summary>
    ''' Ensures that a row exists in the given collection such that a value for the
    ''' field defined by the specified field definition exists. This also ensures
    ''' that any rows leading up to that row are set as the current row within
    ''' their parent collection.
    ''' </summary>
    ''' <param name="fld">The field definition for which a value must exist within
    ''' <paramref name="coll"/></param>
    ''' <param name="coll">The collection in which the required rows should be 
    ''' created.</param>
    Private Sub EnsureRowExists(ByVal fld As clsCollectionFieldInfo, ByVal coll As clsCollection)

        ' Find each field within the stage using the partially qualified name
        For Each fieldName As String In fld.PartiallyQualifiedName.Split("."c)

            ' Ensure that the collection has a current row...
            If coll.CurrentRowIndex = -1 Then coll.CurrentRowIndex = coll.Add()
            Dim row As clsCollectionRow = coll.GetCurrentRow()
            Dim val As clsProcessValue = row(fieldName)
            If val IsNot Nothing AndAlso val.DataType = DataType.collection AndAlso val.IsNull Then
                val.Collection = New clsCollection()
                val.Collection.CopyDefinition(coll.Definition.GetField(fieldName).Children)
            End If
            ' Now ready ourselves for the iteration... if we're at the end, that's
            ' okay - we're not returning anything, just ensuring that it's there.
            coll = val.Collection
        Next
    End Sub

    ''' <summary>
    ''' Updates the collection fields based on the values the user has
    ''' entered into the controls.
    ''' </summary>
    ''' <returns>True if successful, False otherwise.</returns>
    Private Function UpdateCollectionFieldsFromControls() As Boolean

        Dim sErr As String = ""

        For Each ctl As Control In mControls.Values

            Dim fld As clsCollectionFieldInfo = TryCast(ctl.Tag, clsCollectionFieldInfo)

            If fld IsNot Nothing Then

                If fld Is Nothing Then
                    UserMessage.Show(My.Resources.frmExpressionTest_InternalErrorUnexpectedNullReferenceToStage)
                    Return False
                End If

                Dim inputVal As clsProcessValue = DirectCast(ctl, IProcessValue).Value
                If inputVal.IsNull Then
                    UserMessage.Show(String.Format(My.Resources.frmExpressionTest_InvalidOrBlankValueSuppliedFor0, fld.FullyQualifiedName))
                    Return False
                End If

                If fld.DataType <> inputVal.DataType Then
                    UserMessage.Show(My.Resources.frmExpressionTest_InputDataTypeDoesNotMatchTheTypeExpectedForThisDataItem)
                    Return False
                End If

                Try

                    Dim stg As clsCollectionStage = fld.Parent.RootStage
                    Dim val As clsProcessValue = stg.GetValue()
                    If val.IsNull Then
                        ' A null collection - we need to copy the definition from the stage.
                        val.Collection = New clsCollection()
                        val.Collection.CopyDefinition(stg.Definition)
                    End If

                    ' Ensure that any rows which need to exist in order to reach this
                    ' field exist in the stage's collection.
                    EnsureRowExists(fld, val.Collection)

                    ' Set the field to the given input value.
                    fld.Value = inputVal


                Catch e As Exception
                    UserMessage.Show(String.Format(My.Resources.frmExpressionTest_ValueFor0IsNotValid, fld.FullyQualifiedName))
                    Return False

                End Try

            End If

        Next

        Return True

    End Function

    ''' <summary>
    ''' Updates the data items based on the values the user has
    ''' entered into the controls.
    ''' </summary>
    ''' <returns>True if successful, False otherwise.</returns>
    Private Function UpdateDataItemsFromControls() As Boolean

        For Each ctl As Control In mControls.Values

            Dim stg As clsDataStage = TryCast(ctl.Tag, clsDataStage)
            If stg IsNot Nothing Then

                Dim val As clsProcessValue = DirectCast(ctl, IProcessValue).Value
                If Not val.IsValid Then
                    UserMessage.Show(String.Format(My.Resources.frmExpressionTest_InvalidValueSuppliedForDataItem0, stg.GetName()))
                    Return False
                End If

                If stg.GetDataType() <> val.DataType Then
                    UserMessage.Show(
                     My.Resources.frmExpressionTest_InputDataTypeDoesNotMatchTheTypeExpectedForThisDataItem)
                    Return False
                End If

                Try
                    ' seems pointless to create new value object, but it 
                    ' causes validation to be performed.
                    stg.SetValue(val.Clone())

                Catch e As Exception
                    UserMessage.Show(String.Format(My.Resources.frmExpressionTest_ValueFor0IsNotValid, stg.GetName()))
                    Return False

                End Try

            End If

        Next

        Return True

    End Function

    ''' <summary>
    ''' Test button handler
    ''' </summary>
    Private Sub btnTest_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnTest.Click
        'Update the data items...
        If Not UpdateDataItemsFromControls() Then Exit Sub

        'Update the collections...
        If Not UpdateCollectionFieldsFromControls() Then Exit Sub

        'Evaluate the expression...
        Try
            Dim err As String = Nothing
            Dim res As clsProcessValue = Nothing

            If clsExpression.EvaluateExpression(BPExpression.FromLocalised(rtbExpression.Text), res, mProcess.ScopeStage, False, Nothing, err) Then

                txtResult.Text = String.Format(My.Resources.frmExpressionTest_01, res.FormattedValue, clsProcessDataTypes.GetFriendlyName(res.DataType))

                'Expand the result field if necessary.
                If res.DataType = DataType.text And Not mbHeightIsExpanded Then
                    'Make the result box scrollable
                    Me.Height += 47
                    txtResult.Height += 47
                    txtResult.ScrollBars = ScrollBars.Vertical
                    mbHeightIsExpanded = True
                End If

            Else
                UserMessage.Show(err)

            End If

        Catch ex As Exception
            UserMessage.Show(My.Resources.frmExpressionTest_InternalErrorYourExpressionCannotBeEvaluatedTheSystemReturnedTheFollowingError & ex.Message)

        End Try

    End Sub

    Private Sub Control_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOK.LostFocus, btnCancel.LostFocus, btnTest.LostFocus
        ctlFocus = CType(sender, Control)
    End Sub

    Public Overrides Function GetHelpFile() As String
        Return "frmExpressionTest.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Try
            Dim err As String = Nothing
            Dim res As clsProcessValue = Nothing
            If clsExpression.EvaluateExpression(BPExpression.FromLocalised(rtbExpression.Text), res, mProcess.ScopeStage, True, Nothing, err) Then
                RaiseEvent NewExpressionApplied(sender, rtbExpression.Text)
                Close()
            Else
                UserMessage.Show(err)
            End If
        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
            rtbExpression.Focus()
        End Try
    End Sub
End Class

