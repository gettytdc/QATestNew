Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.clsServer
Imports BluePrism.AutomateAppCore.Auth


''' Project  : Automate
''' Class    : ctlBusinessArea
''' 
''' <summary>
''' A control to display and manage Business Objects.
''' </summary>
Friend Class ctlExternalBusinessObjects
    Inherits System.Windows.Forms.UserControl
    Implements IHelp
    Implements IChild
    Implements IPermission

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

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
    Friend WithEvents llViewDetails As AutomateControls.BulletedLinkLabel
    Friend WithEvents llConfigureObject As AutomateControls.BulletedLinkLabel
    Friend WithEvents objBusinessObjects As AutomateUI.ctlBusinessObjectsView
    Friend WithEvents lBusinessObjects As System.Windows.Forms.Label
    Friend WithEvents llValidate As AutomateControls.BulletedLinkLabel
    Friend WithEvents txtAddNewObject As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents llAddObjectGo As AutomateControls.BulletedLinkLabel
    Friend WithEvents llReferences As AutomateControls.BulletedLinkLabel
    Friend WithEvents llDelete As AutomateControls.BulletedLinkLabel
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlExternalBusinessObjects))
        Me.llViewDetails = New AutomateControls.BulletedLinkLabel()
        Me.llConfigureObject = New AutomateControls.BulletedLinkLabel()
        Me.lBusinessObjects = New System.Windows.Forms.Label()
        Me.llValidate = New AutomateControls.BulletedLinkLabel()
        Me.txtAddNewObject = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.llAddObjectGo = New AutomateControls.BulletedLinkLabel()
        Me.llDelete = New AutomateControls.BulletedLinkLabel()
        Me.objBusinessObjects = New AutomateUI.ctlBusinessObjectsView()
        Me.llReferences = New AutomateControls.BulletedLinkLabel()
        Me.SuspendLayout()
        '
        'llViewDetails
        '
        resources.ApplyResources(Me.llViewDetails, "llViewDetails")
        Me.llViewDetails.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llViewDetails.Name = "llViewDetails"
        Me.llViewDetails.TabStop = True
        Me.llViewDetails.UseCompatibleTextRendering = True
        '
        'llConfigureObject
        '
        resources.ApplyResources(Me.llConfigureObject, "llConfigureObject")
        Me.llConfigureObject.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llConfigureObject.Name = "llConfigureObject"
        Me.llConfigureObject.TabStop = True
        Me.llConfigureObject.UseCompatibleTextRendering = True
        '
        'lBusinessObjects
        '
        Me.lBusinessObjects.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.lBusinessObjects, "lBusinessObjects")
        Me.lBusinessObjects.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lBusinessObjects.Name = "lBusinessObjects"
        '
        'llValidate
        '
        resources.ApplyResources(Me.llValidate, "llValidate")
        Me.llValidate.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llValidate.Name = "llValidate"
        Me.llValidate.TabStop = True
        Me.llValidate.UseCompatibleTextRendering = True
        '
        'txtAddNewObject
        '
        resources.ApplyResources(Me.txtAddNewObject, "txtAddNewObject")
        Me.txtAddNewObject.Name = "txtAddNewObject"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'llAddObjectGo
        '
        resources.ApplyResources(Me.llAddObjectGo, "llAddObjectGo")
        Me.llAddObjectGo.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llAddObjectGo.Name = "llAddObjectGo"
        Me.llAddObjectGo.TabStop = True
        Me.llAddObjectGo.UseCompatibleTextRendering = True
        '
        'llDelete
        '
        resources.ApplyResources(Me.llDelete, "llDelete")
        Me.llDelete.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llDelete.Name = "llDelete"
        Me.llDelete.TabStop = True
        Me.llDelete.UseCompatibleTextRendering = True
        '
        'objBusinessObjects
        '
        resources.ApplyResources(Me.objBusinessObjects, "objBusinessObjects")
        Me.objBusinessObjects.BackColor = System.Drawing.Color.White
        Me.objBusinessObjects.Name = "objBusinessObjects"
        '
        'llReferences
        '
        resources.ApplyResources(Me.llReferences, "llReferences")
        Me.llReferences.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llReferences.Name = "llReferences"
        Me.llReferences.TabStop = True
        Me.llReferences.UseCompatibleTextRendering = True
        '
        'ctlExternalBusinessObjects
        '
        Me.Controls.Add(Me.llReferences)
        Me.Controls.Add(Me.llDelete)
        Me.Controls.Add(Me.llAddObjectGo)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtAddNewObject)
        Me.Controls.Add(Me.llValidate)
        Me.Controls.Add(Me.llViewDetails)
        Me.Controls.Add(Me.llConfigureObject)
        Me.Controls.Add(Me.objBusinessObjects)
        Me.Controls.Add(Me.lBusinessObjects)
        Me.Name = "ctlExternalBusinessObjects"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Private Sub ctlBusinessObjects_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim sErr As String = Nothing
        If Not DesignMode Then
            Try
                mWebServices = gSv.GetWebServiceDefinitions()
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlExternalBusinessObjects_FailedToRetrieveAListOfWebServicesFromTheDatabaseErrorMessageWas0, ex.Message))
            End Try
        End If
    End Sub

    Private Sub llConfigureObject_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llConfigureObject.LinkClicked
        Me.objBusinessObjects.ConfigureObject(sender, e)
    End Sub

    Private Sub llViewDetails_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llViewDetails.LinkClicked
        Me.objBusinessObjects.ViewDetails(sender, e)
    End Sub

    Private Sub llReferences_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llReferences.LinkClicked
        If objBusinessObjects.lvBusinessObjects.SelectedItems.Count <> 1 Then
            UserMessage.Show(My.Resources.ctlExternalBusinessObjects_PleaseSelectASingleBusinessObject)
        Else
            Dim ObjName As String = objBusinessObjects.lvBusinessObjects.SelectedItems(0).SubItems(1).Text
            mParent.FindReferences(New clsProcessNameDependency(ObjName))
        End If

    End Sub

    ''' <summary>
    ''' Gets the name of the help file associated with the control.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpBusinessObjects.htm"
    End Function

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    ''' <summary>
    ''' A datatable to hold a list of webservices installed in the database.
    ''' </summary>
    Private mWebServices As ICollection(Of clsWebServiceDetails)

    ''' <summary>
    ''' Checks to see if there exists a web service having a friendly name
    ''' matching that of the name specified.
    ''' </summary>
    ''' <param name="sName">The name to check for.</param>
    ''' <returns>Returns true if such a name exists, false otherwise.</returns>
    Private Function ExistsWebServiceWithName(ByVal sName As String) As Boolean
        If Not mWebServices Is Nothing Then
            For Each ws As clsWebServiceDetails In mWebServices
                If ws.FriendlyName = sName Then Return True
            Next
        End If
        Return False
    End Function


    Private Sub llValidate_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llValidate.LinkClicked
        Me.objBusinessObjects.ValidateObject()
    End Sub

    Private Sub llAddObjectGo_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llAddObjectGo.LinkClicked
        If Not Me.txtAddNewObject.Text = "" Then
            Dim BO As New BluePrism.AutomateProcessCore.clsCOMBusinessObject(Me.txtAddNewObject.Text, "")
            If (Not BO Is Nothing) AndAlso (ExistsWebServiceWithName(BO.FriendlyName)) Then
                UserMessage.Show(My.Resources.ctlExternalBusinessObjects_YouMayNotChooseThisNameBecauseThereAlreadyExistsAWebServiceWithThisNameNoBusine)
                Exit Sub
            End If
            Dim configOptions = Options.Instance
            configOptions.AddObject(Me.txtAddNewObject.Text)
            Try
                configOptions.Save()
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlExternalBusinessObjects_UnableToSaveObjectDetails0, ex.Message))
                Exit Sub
            End Try
            gSv.BusinessObjectAdded(txtAddNewObject.Text)
            Me.objBusinessObjects.PopulateUsingLatestObjects()
            Me.txtAddNewObject.Text = ""
        Else
            UserMessage.Show(My.Resources.ctlExternalBusinessObjects_ErrorYouMustEnterTheNameOfABusinessObjectBeforeAttemptingToAddIt)
        End If
    End Sub

    Private Sub llDelete_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llDelete.LinkClicked
        Me.objBusinessObjects.DeleteSelectedObjects(sender, e)
    End Sub

    Private Sub ProcessLocateControl1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtAddNewObject.KeyDown
        Select Case e.KeyCode
            Case Keys.Enter
                Me.llAddObjectGo_LinkClicked(Nothing, Nothing)
        End Select
    End Sub

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Business Objects - External")
        End Get
    End Property
End Class
