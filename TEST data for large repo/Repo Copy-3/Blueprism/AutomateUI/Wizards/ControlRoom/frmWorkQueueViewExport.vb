Imports BluePrism.AutomateAppCore
Imports System.IO
Imports WorkQueueEventCode = BluePrism.AutomateAppCore.WorkQueueEventCode
Imports BluePrism.BPCoreLib

Friend Class frmWorkQueueViewExport
    Inherits frmWizard

#Region " Designer-generated code "

    Friend WithEvents llInitialPrompt As System.Windows.Forms.Label
    Friend WithEvents rdoAsOnScreen As AutomateControls.StyledRadioButton
    Friend WithEvents rdoFreshAllPages As AutomateControls.StyledRadioButton
    Friend WithEvents pnlChoosePath As System.Windows.Forms.Panel
    Friend WithEvents mPathFinder As ctlPathFinder
    Friend WithEvents rdoFreshCurrentPage As AutomateControls.StyledRadioButton
    Friend WithEvents pnlChooseData As System.Windows.Forms.Panel
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmWorkQueueViewExport))
        Me.pnlChooseData = New System.Windows.Forms.Panel()
        Me.rdoFreshAllPages = New AutomateControls.StyledRadioButton()
        Me.rdoFreshCurrentPage = New AutomateControls.StyledRadioButton()
        Me.rdoAsOnScreen = New AutomateControls.StyledRadioButton()
        Me.llInitialPrompt = New System.Windows.Forms.Label()
        Me.pnlChoosePath = New System.Windows.Forms.Panel()
        Me.mPathFinder = New AutomateUI.ctlPathFinder()
        Me.pnlChooseData.SuspendLayout()
        Me.pnlChoosePath.SuspendLayout()
        Me.SuspendLayout()
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'pnlChooseData
        '
        Me.pnlChooseData.Controls.Add(Me.rdoFreshAllPages)
        Me.pnlChooseData.Controls.Add(Me.rdoFreshCurrentPage)
        Me.pnlChooseData.Controls.Add(Me.rdoAsOnScreen)
        Me.pnlChooseData.Controls.Add(Me.llInitialPrompt)
        resources.ApplyResources(Me.pnlChooseData, "pnlChooseData")
        Me.pnlChooseData.Name = "pnlChooseData"
        '
        'rdoFreshAllPages
        '
        resources.ApplyResources(Me.rdoFreshAllPages, "rdoFreshAllPages")
        Me.rdoFreshAllPages.Checked = True
        Me.rdoFreshAllPages.Name = "rdoFreshAllPages"
        Me.rdoFreshAllPages.TabStop = True
        Me.rdoFreshAllPages.UseVisualStyleBackColor = True
        '
        'rdoFreshCurrentPage
        '
        resources.ApplyResources(Me.rdoFreshCurrentPage, "rdoFreshCurrentPage")
        Me.rdoFreshCurrentPage.Name = "rdoFreshCurrentPage"
        Me.rdoFreshCurrentPage.UseVisualStyleBackColor = True
        '
        'rdoAsOnScreen
        '
        resources.ApplyResources(Me.rdoAsOnScreen, "rdoAsOnScreen")
        Me.rdoAsOnScreen.Name = "rdoAsOnScreen"
        Me.rdoAsOnScreen.UseVisualStyleBackColor = True
        '
        'llInitialPrompt
        '
        resources.ApplyResources(Me.llInitialPrompt, "llInitialPrompt")
        Me.llInitialPrompt.Name = "llInitialPrompt"
        '
        'pnlChoosePath
        '
        Me.pnlChoosePath.Controls.Add(Me.mPathFinder)
        resources.ApplyResources(Me.pnlChoosePath, "pnlChoosePath")
        Me.pnlChoosePath.Name = "pnlChoosePath"
        '
        'mPathFinder
        '
        resources.ApplyResources(Me.mPathFinder, "mPathFinder")
        Me.mPathFinder.Filter = Nothing
        Me.mPathFinder.InitialDirectory = Nothing
        Me.mPathFinder.Mode = AutomateUI.ctlPathFinder.PathModes.Save
        Me.mPathFinder.Name = "mPathFinder"
        Me.mPathFinder.SuggestedFilename = Nothing
        '
        'frmWorkQueueViewExport
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.pnlChoosePath)
        Me.Controls.Add(Me.pnlChooseData)
        Me.Name = "frmWorkQueueViewExport"
        Me.Controls.SetChildIndex(Me.pnlChooseData, 0)
        Me.Controls.SetChildIndex(Me.pnlChoosePath, 0)
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.pnlChooseData.ResumeLayout(False)
        Me.pnlChooseData.PerformLayout()
        Me.pnlChoosePath.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

    Public Sub New()
        MyBase.New()
        Me.InitializeComponent()

        SetMaxSteps(1)
        mPathFinder.Filter = clsFileSystem.GetFileFilterString(clsFileSystem.FileExtensions.CSV)
    End Sub

    ''' <summary>
    ''' The data shown on screen. This should be populated by the caller
    ''' in case the user picks the 'export the data on screen' option.
    ''' </summary>
    Private mWorkQueueItems As ICollection(Of clsWorkQueueItem)

    ''' <summary>
    ''' The collection of work queue items that is shown on screen
    ''' </summary>
    Public Property WorkQueueItems() As ICollection(Of clsWorkQueueItem)
        Get
            Return mWorkQueueItems
        End Get
        Set(ByVal value As ICollection(Of clsWorkQueueItem))
            mWorkQueueItems = value
        End Set
    End Property

    ''' <summary>
    ''' The filter to use when fetching data from the database. This should
    ''' be populated by the caller, in case the user picks the "get fresh
    ''' data from the database" option.
    ''' </summary>
    Public QueryFilter As WorkQueueFilter

    ''' <summary>
    ''' The ID of the queue of interest. This should
    ''' be populated by the caller, in case the user picks the "get fresh
    ''' data from the database" option.
    ''' </summary>
    Public Property QueueID() As Guid
        Get
            Return mQueueID
        End Get
        Set(ByVal value As Guid)
            mQueueID = value
            gSv.WorkQueueGetQueueName(mQueueID, msQueueName)
        End Set
    End Property
    Private mQueueID As Guid
    Private msQueueName As String


    Protected Overrides Sub UpdatePage()
        Select Case Me.GetStep
            Case 0
                'Show initial page
                Me.ShowPage(Me.pnlChooseData)
            Case 1
                'Time to choose file path
                If Not (Me.rdoAsOnScreen.Checked OrElse Me.rdoFreshAllPages.Checked OrElse Me.rdoFreshCurrentPage.Checked) Then
                    UserMessage.Show(My.Resources.frmWorkQueueViewExport_PleaseChooseOneOfTheOptionsPresented)
                    MyBase.Rollback()
                    Exit Sub
                End If

                Me.mPathFinder.SuggestedFilename = String.Format(My.Resources.frmWorkQueueViewExport_QueueReport01Csv, msQueueName, DateTime.Now.ToString("yyyy-MM-dd HH-mm"))
                Me.ShowPage(Me.pnlChoosePath)
            Case 2


                'Time to do export
                Dim DataWritten As Boolean
                Select Case True
                    Case Me.rdoAsOnScreen.Checked
                        Try
                            If Me.mWorkQueueItems IsNot Nothing Then
                                DataWritten = WriteData(mWorkQueueItems)
                            Else
                                UserMessage.Show(String.Format(My.Resources.frmWorkQueueViewExport_CannotExportScreenDataDueToAnUnanticipatedConfigurationErrorScreenDataAbsentPle, ApplicationProperties.CompanyName))
                            End If
                        Catch ex As Exception
                            UserMessage.Show(String.Format(My.Resources.frmWorkQueueViewExport_UnexpectedError0, ex.Message), ex)
                        End Try
                    Case Me.rdoFreshAllPages.Checked
                        'modify the filter to include all pages
                        QueryFilter.StartIndex = 0
                        QueryFilter.MaxRows = Integer.MaxValue

                        DataWritten = WriteLatestDatabaseData(QueryFilter)
                    Case Me.rdoFreshCurrentPage.Checked
                        DataWritten = WriteLatestDatabaseData(QueryFilter)
                    Case Else
                        UserMessage.Show(My.Resources.frmWorkQueueViewExport_UnexpectedConfigPleaseSelectOneOfTheRadioOptions)
                End Select

                If DataWritten Then
                    gSv.AuditRecordWorkQueueEvent(WorkQueueEventCode.ExportFromQueue, QueueID, "", String.Format(My.Resources.frmWorkQueueViewExport_DataWrittenToFile0UsingGraphicalExportWizard, Me.mPathFinder.ChosenFile))
                    Me.Close()
                Else
                    MyBase.Rollback()
                    Exit Sub
                End If
            Case Is > 2
                Me.Close()
        End Select
    End Sub

    ''' <summary>
    ''' Writes the latest database data to the file chosen by the user.
    ''' </summary>
    ''' <param name="Filter">The filter to use when fetching data from the
    ''' database.</param>
    ''' <returns>Returns true if the data is written, false on error.</returns>
    Private Function WriteLatestDatabaseData(ByVal Filter As WorkQueueFilter) As Boolean
        Dim sErr As String = Nothing, Total As Integer
        Dim results As ICollection(Of clsWorkQueueItem) = Nothing
        Try
            gSv.WorkQueuesGetQueueFilteredContents(QueueID, Filter, Total, results)
            Return WriteData(results)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmWorkQueueViewExport_UnableToFetchContentFromDatabase0, ex.Message))
            MyBase.Rollback()
        End Try

        Return False
    End Function

    ''' <summary>
    ''' Writes the supplied data to the file chosen by the user.
    ''' </summary>
    ''' <param name="data">The data to be written</param>
    ''' <returns>Returns true if the data is written, false on error.</returns>
    Private Function WriteData(ByVal data As ICollection(Of clsWorkQueueItem)) As Boolean
        If data Is Nothing Then
            Throw New ArgumentNullException(NameOf(data), My.Resources.frmWorkQueueViewExport_NoCollectionOfWorkQueueItemsPassed)
        End If

        'Validate file path
        If String.IsNullOrEmpty(Me.mPathFinder.ChosenFile()) Then
            UserMessage.Show(My.Resources.frmWorkQueueViewExport_PleaseChooseADestinationFileForTheExportedReport)
            Return False
        End If
        If File.Exists(Me.mPathFinder.ChosenFile) Then
            If UserMessage.YesNo(My.Resources.frmWorkQueueViewExport_AFileAlreadyExistsAtTheChosenLocationDoYouWishToOverwriteIt) = MsgBoxResult.No Then
                Return False
            End If
        End If

        'Write the data
        Dim sr As System.IO.StreamWriter = Nothing
        Try
            sr = New System.IO.StreamWriter(Me.mPathFinder.ChosenFile, False)
            clsWorkQueueItem.ToCsv(data, sr)
            Return True
        Finally
            If sr IsNot Nothing Then
                sr.Close()
            End If
        End Try

        Return False
    End Function

    Public Overrides Function GetHelpFile() As String
        Return "frmWorkQueueViewExport.htm"
    End Function

End Class
