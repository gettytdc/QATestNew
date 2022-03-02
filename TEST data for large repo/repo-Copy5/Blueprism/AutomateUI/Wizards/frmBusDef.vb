
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore

''' Project  : Automate
''' Class    : frmBusDef
''' 
''' <summary>
''' A wizard to define Business Objects.
''' </summary>
Friend Class frmBusDef
    Inherits frmWizard


#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

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
    Friend WithEvents cmbObject As AutomateControls.StyledComboBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmBusDef))
        Me.cmbObject = New AutomateControls.StyledComboBox()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        '
        'btnBack
        '
        resources.ApplyResources(Me.btnBack, "btnBack")
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'cmbObject
        '
        resources.ApplyResources(Me.cmbObject, "cmbObject")
        Me.cmbObject.Name = "cmbObject"
        '
        'frmBusDef
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.cmbObject)
        Me.Name = "frmBusDef"
        Me.Controls.SetChildIndex(Me.cmbObject, 0)
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private mobjExplorer As Object
    Private mobjDocument As Object
    Private msTemplateXSL As String
    ''' <summary>
    ''' Moves the wizard along to the next step.
    ''' </summary>
    Protected Overrides Sub UpdatePage()
        Dim obj = TryCast(cmbObject.SelectedComboBoxItem.Tag, clsBusinessObject)
        If obj IsNot Nothing Then
            clsBOD.OpenAPIDocumentation(obj)
        End If
    End Sub

    Private Sub UpdateObjectsList()
        Using objects As New clsGroupBusinessObject(Options.Instance.GetExternalObjectsInfo())
            For Each obr In objects.Children
                DescendChildren(obr, 0)
            Next
            cmbObject.SelectedIndex = 0
        End Using
    End Sub

    Private Sub DescendChildren(obj As clsBusinessObject, indent As Integer)
        Const increment = 16

        If Not obj.Valid Then Return

        Dim name As String = obj.FriendlyName

        Dim item As New AutomateControls.ComboBoxItem(name, obj)
        item.Indent = indent
        cmbObject.Items.Add(item)

        Dim group = TryCast(obj, clsGroupBusinessObject)
        If group IsNot Nothing Then
            item.Style = FontStyle.Bold
            item.Tag = Nothing
            For Each childObj In group.Children
                DescendChildren(childObj, indent + increment)
            Next
        End If

    End Sub

    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        UpdateObjectsList()
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetHelpFile() As String
        Return "helpBusinessObjects"
    End Function
End Class

