Imports AutomateControls
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmDataItemWatches : Implements IEnvironmentColourManager

    ''' <summary>
    ''' The process owning this form
    ''' </summary>
    Private moProcess As clsProcess

    ''' <summary>
    ''' The moListView on the data item watch control.
    ''' </summary>
    Private WithEvents moListView As AutomateControls.TreeList.TreeListView

    ''' <summary>
    ''' Process studio
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents moProcessStudioParent As frmProcess


    Public Sub New(ByVal ProcessStudioParent As frmProcess)

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        Me.objBluebar.Title = My.Resources.frmDataItemWatches_DragAndDropDataItemsOnToTheRightHandWatchList
        moProcessStudioParent = ProcessStudioParent
        moListView = Me.ctlDataItemWatches.objDataList
        Me.ResizeRedraw = True
        Me.CenterToParent()

    End Sub

    ''' <summary>
    ''' Sets the process owning this form.
    ''' </summary>
    ''' <param name="Process">The process.</param>
    Public Sub SetProcess(ByVal Process As clsProcess)
        Me.moProcess = Process
        Me.ctlDataItemTreeView.Populate(Process)
        AddHandler moProcess.ProcessXMLReloaded, AddressOf HandleProcessChanged
        AddHandler moProcess.StageAdded, AddressOf HandleProcessChanged
        AddHandler moProcess.StageDeleted, AddressOf HandleProcessChanged
        AddHandler moProcess.StageModified, AddressOf HandleProcessChanged

        Me.Text = String.Format(My.Resources.frmDataItemWatches_DataItemWatchesFor0, moProcess.Name)
    End Sub

    ''' <summary>
    ''' Finds the stage object contained in the DragEventArgs object.
    ''' </summary>
    ''' <param name="e">The drageventargs.</param>
    ''' <returns>Returns the stage object from the args, or nothing
    ''' if none is present.</returns>
    Private Function GetStageDataFromDragEventArgs(ByVal e As DragEventArgs) As clsProcessStage
        Dim stage As clsProcessStage = Nothing

        Select Case True
            Case e.Data.GetDataPresent(GetType(String))
                stage = moProcess.GetStage(CStr(e.Data.GetData(GetType(String))))

            Case e.Data.GetDataPresent(GetType(TreeNode))
                Dim TN As TreeNode = CType(e.Data.GetData(GetType(TreeNode)), TreeNode)
                If (TypeOf TN.Tag Is clsProcessStage) Then
                    stage = CType(TN.Tag, clsProcessStage)
                ElseIf (TypeOf TN.Tag Is clsCollectionFieldInfo) Then
                    stage = CType(TN.Tag, clsCollectionFieldInfo).Parent.ParentStage
                Else
                    stage = Nothing
                End If

            Case Else
                stage = Nothing
        End Select

        'we may return null here. This is fine.
        Return stage
    End Function

    Private Sub ctlDataItemWatches_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles moListView.DragDrop
        Dim stage As clsProcessStage = Me.GetStageDataFromDragEventArgs(e)
        If Not stage Is Nothing AndAlso (stage.StageType = StageTypes.Data OrElse stage.StageType = StageTypes.Collection) Then
            Me.ctlDataItemWatches.AddStage(stage)
        End If
    End Sub

    Private Sub moListView_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles moListView.DragEnter
        Dim objStage As clsProcessStage = GetStageDataFromDragEventArgs(e)
        If Not objStage Is Nothing AndAlso (objStage.StageType = StageTypes.Data OrElse objStage.StageType = StageTypes.Collection) Then
            e.Effect = DragDropEffects.Move
        End If
    End Sub

    Private Sub btnHelp_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnHelp.Click
        Try
            OpenHelpFile(Me, GetHelpFile)
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Sub btnClose_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    Private Sub moProcessStudioParent_Closing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles moProcessStudioParent.FormClosing

        RemoveHandler Me.FormClosing, AddressOf Me.Me_FormClosing
        Me.Close()

    End Sub

    Private Sub Me_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        e.Cancel = True
        Me.Visible = False

    End Sub

    Private Sub HandleProcessChanged(ByVal objstage As clsProcessStage)
        Me.HandleProcessChanged()
    End Sub

    Private Sub HandleProcessChanged()
        Me.ctlDataItemTreeView.Populate(moProcess)
    End Sub

    Private Sub HandleProcessChanged(ByVal objstage1 As clsProcessStage, ByVal objstage2 As clsProcessStage)
        Me.HandleProcessChanged()
    End Sub

    ''' <summary>
    ''' The help file
    ''' </summary>
    ''' <returns>Help file string.</returns>
    Public Function GetHelpFile() As String
        Return "frmDataItemWatches.htm"
    End Function

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return objBluebar.BackColor
        End Get
        Set(value As Color)
            objBluebar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return objBluebar.TitleColor
        End Get
        Set(value As Color)
            objBluebar.TitleColor = value
        End Set
    End Property
End Class
