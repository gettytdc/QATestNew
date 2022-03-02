Imports System.Text.RegularExpressions
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports AutomateControls

''' Project  : Automate
''' Class    : frmStagePropertiesLoopStart
''' 
''' <summary>
''' The loop start properties form.
''' </summary>
Friend Class frmStagePropertiesLoopStart
    Inherits frmProperties

    ''' <summary>
    ''' The loop start stage that this properties dialog is editing
    ''' </summary>
    Protected ReadOnly Property Stage() As clsLoopStartStage
        Get
            Return TryCast(ProcessStage, clsLoopStartStage)
        End Get
    End Property

    ''' <summary>
    ''' Creates a new empty loop start properties dialog.
    ''' </summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Handles the loop start stage properties dialog being loaded.
    ''' </summary>
    Private Sub frmStagePropertiesData_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load

        'Make sure we have a valid stage object
        Dim stg As clsLoopStartStage = Me.Stage
        If stg Is Nothing Then
            UserMessage.Show(My.Resources.frmStagePropertiesLoopStart_PropertiesDialogIsNotProperlyConfigured)
            Exit Sub
        End If

        'Populate the collection list...
        cmbCollections.Sorted = True
        cmbCollections.Items.Add(New ComboBoxItem(My.Resources.frmStagePropertiesLoopStart_None))
        For Each collStg As clsCollectionStage In stg.Process.GetStages(StageTypes.Collection)
            Dim enabled As Boolean = collStg.IsInScope(mProcessStage)
            cmbCollections.Items.Add(New ComboBoxItem(collStg.GetName(), collStg, enabled))
            ' Add any nested collections within the stage...
            BuildCollection(cmbCollections, collStg, enabled)
        Next

        'Fill in all the fields...
        txtName.Text = stg.GetName()
        txtDescription.Text = stg.GetNarrative()
        cmbCollections.Text = stg.LoopData

        ' There is only one loop type at this time. Defaults to this.
        Stage.LoopType = "ForEach"
    End Sub

    ''' <summary>
    ''' Recursively builds the collection from the given collection definition
    ''' manager into the specified combo box, setting it (and all descendants)
    ''' enabled or disabled as specified
    ''' </summary>
    ''' <param name="cb">The combo box into which the collection and its
    ''' descendants should be added.</param>
    ''' <param name="defn">The definition from which to draw the collection 
    ''' fields. If this is null, the method simply returns without doing
    ''' anything</param>
    ''' <param name="enabled">True to enable all collection fields found within
    ''' the given manager; False to disable them all.</param>
    Private Sub BuildCollection(
     ByVal cb As ComboBox, ByVal defn As ICollectionDefinitionManager, ByVal enabled As Boolean)

        If defn Is Nothing Then Return
        For Each fld As clsCollectionFieldInfo In defn.FieldDefinitions
            If fld.DataType = DataType.collection Then
                cmbCollections.Items.Add(New ComboBoxItem(fld.FullyQualifiedName, fld, enabled))
                If fld.HasChildren() Then BuildCollection(cb, fld.Children, enabled)
            End If
        Next

    End Sub

    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean
        Dim bNameChanged As Boolean
        bNameChanged = ProcessStage.GetName <> Me.txtName.Text

        If Not MyBase.ApplyChanges() Then Return False

        ' reg ex to trim the collection name and allow stripping of extraneous
        ' square brackets.
        Static rx As New Regex("^\[?(.*?)\]?$")

        ' Save the changes... first, a little manipulation
        ' The user can freeform specify a nested collection - some basic stuff,
        ' trim the name, strip the square brackets surrounding the name if there.
        ' Understandable, but unnecessary.
        Dim name As String = rx.Match(cmbCollections.Text.Trim()).Groups(1).ToString()

        If cmbCollections.Text = My.Resources.frmStagePropertiesLoopStart_None Then
            Stage.LoopData = ""
        Else
            Stage.LoopData = name
        End If

        If bNameChanged Then
            Dim e As clsProcessStage = Process.GetLoopEnd(Stage)
            If e IsNot Nothing Then
                e.Name = Me.txtName.Text
            End If
        End If
        Return True

    End Function


    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesLoopStart.htm"
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

End Class
