
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : Automate
''' Class    : clsExpressionRichTextBox
''' 
''' <summary>
''' A RichTextBox subclass with added text colouring and border drawing capabilities.
''' </summary>
Public Class ctlExpressionRichTextBox
    Inherits ctlRichTextBoxWithPasswords

#Region "Member variables"

    Private mcTextColor As Color = AutomateControls.ColourScheme.Expression.Text
    Private mcNumberColor As Color = AutomateControls.ColourScheme.Expression.Number
    Private mcDataItemColor As Color = AutomateControls.ColourScheme.Expression.DataItem
    Private mcOperatorColor As Color = AutomateControls.ColourScheme.Expression.Operator
    Private mcParameterColor As Color = AutomateControls.ColourScheme.Expression.Parameter
    Private maBracketColors As Color() = AutomateControls.ColourScheme.Expression.Brackets

    Private mbMeChanging As Boolean

    Private mbColourChange As Boolean = False
    Private colortable As String

    ''' <summary>
    ''' Here we store every text change in a collection for later retrieval.
    ''' </summary>
    Private mUndoBuffer As New Collection

    ''' <summary>
    ''' Keeps track of where we are in the undo/redo collection.
    ''' </summary>
    Private miUndoRedoIndex As Integer = 0  'Starts at zero, but immediately set to one via AddCurrentStateToUndoBuffer in constructor

    ''' <summary>
    ''' Private member to store public property HighlightingEnabled.
    ''' </summary>
    Private mbHighlightingEnabled As Boolean = True


    ''' <summary>
    ''' Used to Record the value of the Startselection() property  at the time the
    ''' mouse enters the textbox whilst dragging something.
    ''' 
    ''' This is necessary because as you drop something into the textbox, the 
    ''' selection is lost, meaning that you cannot access this data inside the 
    ''' drop event handler.
    ''' 
    ''' Therefore we observe it on the DragEnter handler and remember it here.
    ''' </summary>
    Private miExpressionBoxSelectionStart As Integer

    ''' <summary>
    ''' As for miExpressionBoxSelectionStart, but records the value of the selection
    ''' length.
    ''' </summary>
    Private miExpressionBoxSelectionLength As Integer


#End Region

#Region "Properties"

    ''' <summary>
    ''' Whenever we set the text of this control we need to syntax hilight it.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Property Text() As String
        Get
            Return MyBase.Text
        End Get
        Set(ByVal value As String)
            MyBase.Text = value
            Me.ColourText()
        End Set
    End Property

    ''' <summary>
    ''' Determines whether syntax highlighting is performed on text entered into 
    ''' this text field.
    ''' </summary>
    ''' <value></value>
    Public Property HighlightingEnabled() As Boolean
        Get
            Return Me.mbHighlightingEnabled
        End Get
        Set(ByVal Value As Boolean)
            Me.mbHighlightingEnabled = Value
            If Not Value Then
                Dim lText As String = Me.Text
                Dim iSelectionStart As Integer = Me.SelectionStart
                Me.Rtf = String.Empty
                Me.Text = lText
                Me.SelectionStart = iSelectionStart
            Else
                Me.mbColourChange = True
                Me.ColourText()
            End If
        End Set
    End Property

#End Region

#Region "Constructor"


    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        MyBase.New()
        MyBase.AllowDrop = True
        MyBase.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        mbColourChange = True
        Me.HideSelection = False
        AddHandler MyBase.TextChanged, AddressOf MyBase_TextChanged

        Me.DetectUrls = False
        Me.AddCurentStateToUndoBuffer()
    End Sub

#End Region

    Protected Overrides Sub OnHandleCreated(ByVal e As EventArgs)
        MyBase.OnHandleCreated(e)
        MyBase.AutoWordSelection = False
    End Sub

#Region "HandleKeyPress"

    Private Sub HandleKeyPress(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
        If e.Control Then
            Select Case e.KeyCode
                Case Keys.A
                    MyBase.SelectAll()
                Case Keys.Z
                    UndoText()
                Case Keys.Y
                    RedoText()
            End Select
        End If
    End Sub

#End Region

#Region "MyBase_TextChanged"

    Private mbAlreadyHandlingTextChange As Boolean


    Private Sub MyBase_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        If Not Me.mbAlreadyHandlingTextChange Then
            Me.mbAlreadyHandlingTextChange = True
            Try
                'first update the textbox so that the response is faster
                If Not mbMeChanging Then
                    RemoveHandler MyBase.TextChanged, AddressOf MyBase_TextChanged
                    RemoveHandler Me.TextChanged, AddressOf MyBase_TextChanged
                    ColourText()
                    AddHandler MyBase.TextChanged, AddressOf MyBase_TextChanged
                    AddHandler Me.TextChanged, AddressOf MyBase_TextChanged
                End If

                'then remember the text for undo/redo
                Me.AddCurentStateToUndoBuffer()
            Finally
                Me.mbAlreadyHandlingTextChange = False
            End Try
        End If
    End Sub

#End Region

#Region "rtbExpression_DragDrop"

    Private Sub rtbExpression_DragDrop(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles MyBase.DragDrop

        Dim objFunction As clsFunction
        Dim objProcessOperator As clsProcessOperators.clsProcessOperator
        Dim sExpression As String = ""
        Dim i As Integer

        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim NodeTag As Object = CType(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag

            Select Case True
                Case (TypeOf NodeTag Is IDataField)
                    'The dragged object is a data field (data stage or collection field).
                    sExpression = "[" & DirectCast(NodeTag, IDataField).FullyQualifiedName & "]"

                Case (TypeOf NodeTag Is clsExpressionTreeView.LiteralValue)
                    Dim l As clsExpressionTreeView.LiteralValue = CType(NodeTag, clsExpressionTreeView.LiteralValue)
                    If l.DataType = DataType.text Then
                        sExpression = """" & l.Name & """"
                    Else
                        'The dragged data is a literal value (eg True or eg False)
                        sExpression = l.Name
                    End If
                Case (TypeOf NodeTag Is clsFunction)

                    objFunction = CType(NodeTag, clsFunction)
                    sExpression = objFunction.Name & "("
                    If objFunction.DefaultSignature.Length > 0 Then
                        For i = 0 To objFunction.DefaultSignature.Length - 1
                            sExpression &= "{" & objFunction.DefaultSignature(i).Name & "}, "
                        Next
                        sExpression = sExpression.Substring(0, sExpression.Length - 2)
                    End If
                    sExpression = sExpression & ")"
                Case (TypeOf NodeTag Is clsProcessOperators.clsProcessOperator)
                    objProcessOperator = CType(NodeTag, clsProcessOperators.clsProcessOperator)
                    sExpression = "{Operand A}" & objProcessOperator.Symbol & "{Operand B}"
                Case Else
                    Throw New InvalidOperationException("Unknown type dragged onto expression field")
            End Select

        End If

        Me.Text = Me.Text.Remove(Me.miExpressionBoxSelectionStart, Me.miExpressionBoxSelectionLength).Insert(Me.miExpressionBoxSelectionStart, sExpression)

    End Sub


#End Region

#Region "rtbExpression_DragEnter"
    Private Sub rtbExpression_DragEnter(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles MyBase.DragEnter

        Dim objStage As clsDataStage

        If e.Data.GetDataPresent(GetType(TreeNode)) Then
            Dim NodeTag As Object = CType(e.Data.GetData(GetType(TreeNode)), TreeNode).Tag

            Select Case True
                Case (TypeOf NodeTag Is clsDataStage)

                    'The stage object should be reachable with this line (as in txtResult_DragEnter).
                    '   objStage = CType(e.Data.GetData(GetType(clsProcessStage), clsProcessStage)
                    'But it just doesn't work, so have referred to the SelectedNode of the tree view.
                    objStage = CType(NodeTag, clsDataStage)
                    If Not objStage Is Nothing Then
                        e.Effect = DragDropEffects.Move
                    Else
                        e.Effect = DragDropEffects.None
                    End If
                Case (TypeOf NodeTag Is clsCollectionFieldInfo)
                    e.Effect = DragDropEffects.Move
                Case (TypeOf NodeTag Is clsExpressionTreeView.LiteralValue)
                    e.Effect = DragDropEffects.Move
                Case (TypeOf NodeTag Is clsFunction) OrElse (TypeOf NodeTag Is clsProcessOperators.clsProcessOperator)
                    'The dragged object must be either a child of clsFunction or a clsProcessOperator
                    e.Effect = DragDropEffects.Move
            End Select
        End If

        Me.miExpressionBoxSelectionStart = Me.SelectionStart
        Me.miExpressionBoxSelectionLength = Me.SelectionLength

    End Sub

#End Region

#Region "ColourText"


    ''' <summary>
    ''' Applies syntax highlighting to the text currently in the box. Use the 
    ''' control's properties to dictate colour schemes etc.
    ''' </summary>
    Public Sub ColourText()

        If Me.IsDisposed OrElse Me.Disposing Then Exit Sub

        Dim OriginalText As String = Me.Text
        Dim aChars As Char() = Me.Text.ToCharArray
        Dim c As Char
        Dim mbString, mbDataItem, mbParameter As Boolean
        Dim sColor As String = ""
        Dim iSelectionStart As Integer
        Dim iIndex As Integer
        Dim iBracketIndex As Integer = 1

        Dim sDatacolor As String = "\cf3 "
        Dim sTextcolor As String = "\cf4 "
        Dim sNumbercolor As String = "\cf2 "
        Dim sOperatorColor As String = "\cf1 "
        Dim sParameterColor As String = "\cf11 "
        Dim sBlack As String = "\cf0 "
        Dim sBracketColors() As String = {"\cf5 ", "\cf6 ", "\cf7 ", "\cf8 ", "\cf9 ", "\cf10 ", "\cf11 "}

        Dim sBold As String = "\b "
        Dim sEndBold As String = "\b0 "

        Dim bBold As Boolean = False
        Dim bOldBold As Boolean = False
        Dim sOldColor As String = ""

        If mbColourChange Then CreateColourTable()
        Dim sRTFText As String = "{\rtf1\ansi\ansicpg1252\deff0{\fonttbl{\f0\fnil\fcharset0 Consolas;}}" &
         colortable & "\viewkind4\uc1\pard\lang1033\f0\fs" & CInt((2 * Me.Font.Size)).ToString & " "

        Try
            RemoveHandler MyBase.TextChanged, AddressOf MyBase_TextChanged

            iSelectionStart = Me.SelectionStart
            If iSelectionStart < 0 Then iSelectionStart = 0

            If Me.HighlightingEnabled Then
                For iIndex = 0 To aChars.Length - 1
                    c = aChars(iIndex)
                    Select Case c
                        Case """"c
                            sColor = sTextcolor
                            mbString = Not mbString
                            bBold = False
                        Case "["c
                            If Not mbString And Not mbDataItem And Not mbParameter Then
                                sColor = sDatacolor
                                bBold = True
                                mbDataItem = True
                            End If
                        Case "]"c
                            If Not mbString And Not mbParameter Then
                                mbDataItem = False
                                bBold = True
                            End If
                        Case "("c
                            If Not mbString And Not mbDataItem And Not mbParameter Then
                                bBold = True
                                sColor = sBracketColors(iBracketIndex)
                                iBracketIndex += 1
                                If iBracketIndex = maBracketColors.Length Then
                                    iBracketIndex = 0
                                End If
                            End If
                        Case ")"c
                            If Not mbString And Not mbDataItem And Not mbParameter Then
                                bBold = True
                                iBracketIndex -= 1
                                If iBracketIndex = -1 Then
                                    iBracketIndex = maBracketColors.Length - 1
                                End If
                                sColor = sBracketColors(iBracketIndex)
                            End If
                        Case "{"c
                            If Not mbString And Not mbDataItem Then
                                mbParameter = True
                                sColor = sParameterColor
                                bBold = False
                            End If
                        Case "}"c
                            If Not mbString And Not mbDataItem Then
                                mbParameter = False
                            End If
                            bBold = False
                        Case "+"c, "-"c, "/"c, "*"c, "&"c, "<"c, ">"c, "="c, "^"c
                            If Not mbString And Not mbDataItem And Not mbParameter Then
                                sColor = sOperatorColor
                                bBold = True
                            End If
                        Case "0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c, "."c
                            If Not mbString And Not mbDataItem And Not mbParameter Then
                                sColor = sNumbercolor
                                bBold = False
                            End If
                        Case ","c
                            If Not mbString And Not mbDataItem And Not mbParameter Then
                                bBold = True
                                sColor = sBlack
                            End If
                        Case Else
                            If Not (mbString Or mbDataItem Or mbParameter) Then
                                sColor = sBlack
                            End If
                            bBold = False
                    End Select

                    If bBold And Not bOldBold Then sRTFText &= sBold
                    If Not bBold And bOldBold Then sRTFText &= sEndBold
                    If Not sColor.Equals(sOldColor) Then
                        sRTFText &= sColor & Escape(c)
                    Else
                        sRTFText &= Escape(c)
                    End If
                    bOldBold = bBold
                    sOldColor = sColor

                    bBold = False
                Next

            Else
                'We just set the text as it is without any colouring
                '(but importantly WITH font styling - this is important
                'when data pasted from MS word for example)
                sRTFText &= Me.Escape(Me.Text)
            End If

            MyBase.SuspendLayout()
            mbMeChanging = True
            Me.Rtf = sRTFText & "\cf0\par}"
            mbMeChanging = False
            Me.SelectionStart = iSelectionStart
            MyBase.ResumeLayout(True)


        Catch ex As Exception
            'make sure we can root out this problem
            Debug.Assert(False, ex.Message)
            'set the text back without highlighting
            Me.Text = OriginalText
        Finally
            AddHandler MyBase.TextChanged, AddressOf MyBase_TextChanged
            mbMeChanging = False
        End Try

    End Sub

#End Region

#Region "Escape"

    Private Function Escape(ByVal c As Char) As String
        Select Case c
            Case "{"c, "}"c, "\"c
                Return "\" & c
            Case CChar(vbCrLf), CChar(vbCr), CChar(vbLf)
                Return "\par "
            Case Else
                If Convert.ToUInt32(c) <= &H7F Then
                    Return c
                Else
                    Return "\u" & Convert.ToUInt32(c).ToString() + "?"
                End If
        End Select
    End Function

    ''' <summary>
    ''' Escapes the characters in the supplied string, suitable
    ''' for use in rich text format.
    ''' </summary>
    ''' <param name="s">The string to escape.</param>
    ''' <returns>Returns the escaped equivalent string.</returns>
    Private Function Escape(ByVal s As String) As String
        Dim RetVal As String = String.Empty
        For Each c As Char In s.ToCharArray
            RetVal &= Escape(c)
        Next
        Return RetVal
    End Function


#End Region

#Region "CreateColourTable"
    Private Sub CreateColourTable()

        Dim AllColours() As Color = {mcOperatorColor, mcNumberColor, mcDataItemColor, mcTextColor}
        Dim sTempTable As String = "{\colortbl ;"

        For i As Integer = 0 To AllColours.Length - 1
            Dim cTemp As Color = AllColours(i)

            sTempTable &= "\red" & cTemp.R.ToString
            sTempTable &= "\green" & cTemp.G.ToString
            sTempTable &= "\blue" & cTemp.B.ToString & "; "
        Next

        For i As Integer = 0 To maBracketColors.Length - 1
            Dim cTemp As Color = maBracketColors(i)
            sTempTable &= "\red" & maBracketColors(i).R.ToString
            sTempTable &= "\green" & maBracketColors(i).G.ToString
            sTempTable &= "\blue" & maBracketColors(i).B.ToString & "; "
        Next

        sTempTable &= "\red" & mcParameterColor.R.ToString
        sTempTable &= "\green" & mcParameterColor.G.ToString
        sTempTable &= "\blue" & mcParameterColor.B.ToString & "; "

        sTempTable &= "}"
        mbColourChange = False
        colortable = sTempTable
    End Sub
#End Region

#Region "Undo and Redo"

    ''' <summary>
    ''' Restores the state of the textbox to the previous state in the undo buffer.
    ''' </summary>
    Private Sub UndoText()
        If Me.miUndoRedoIndex > 1 Then
            miUndoRedoIndex -= 1
            RestoreTextBoxStateToCurrentBufferIndex()
        End If
    End Sub

    ''' <summary>
    ''' Restores the state of the textbox to the next state in the undo buffer.
    ''' </summary>
    Private Sub RedoText()
        If Me.miUndoRedoIndex < Me.mUndoBuffer.Count Then
            miUndoRedoIndex += 1
            RestoreTextBoxStateToCurrentBufferIndex()
        End If
    End Sub

    ''' <summary>
    ''' Deletes all entries in the undo buffer from the current index forward.
    ''' </summary>
    Private Sub RemoveFutureFromUndoRedoBuffer()
        For i As Integer = Me.miUndoRedoIndex + 1 To Me.mUndoBuffer.Count
            Me.mUndoBuffer.Remove(Me.mUndoBuffer.Count)
        Next
    End Sub

    ''' <summary>
    ''' Applies the state indicated by the current index to the control.
    ''' </summary>
    Private Sub RestoreTextBoxStateToCurrentBufferIndex()
        Me.ApplyTextBoxState(CType(Me.mUndoBuffer.Item(miUndoRedoIndex), clsRichTextBoxState))
    End Sub

    ''' <summary>
    ''' Applies the supplied state to the control.
    ''' </summary>
    ''' <param name="State">The state to apply.</param>
    Public Sub ApplyTextBoxState(ByVal State As clsRichTextBoxState)
        RemoveHandler MyBase.TextChanged, AddressOf MyBase_TextChanged
        MyBase.SuspendLayout()
        MyBase.Rtf = State.RTF
        If State.SelectionStart >= 0 Then MyBase.SelectionStart = State.SelectionStart
        If State.SelectionLength >= 0 Then MyBase.SelectionLength = State.SelectionLength
        MyBase.ResumeLayout(True)
        AddHandler MyBase.TextChanged, AddressOf MyBase_TextChanged
    End Sub

    ''' <summary>
    ''' Clears all states from the undobuffer.
    ''' </summary>
    Public Sub ClearUndoBuffer()
        Me.mUndoBuffer = New Collection
        Me.miUndoRedoIndex = 0
        Me.AddCurentStateToUndoBuffer() 'the buffer works by always having the latest state in there
    End Sub

    ''' <summary>
    ''' Adds the current state of the control to the undo buffer at the next index
    ''' from the current one.
    ''' </summary>
    Public Sub AddCurentStateToUndoBuffer()
        RemoveFutureFromUndoRedoBuffer()
        Me.mUndoBuffer.Add(New clsRichTextBoxState(MyBase.Rtf, MyBase.SelectionStart, MyBase.SelectionLength))
        miUndoRedoIndex += 1
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region


End Class
