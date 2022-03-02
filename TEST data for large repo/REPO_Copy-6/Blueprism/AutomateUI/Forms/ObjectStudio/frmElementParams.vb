Imports BluePrism.AMI
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmElementParams
    Inherits frmWizard

    ''' <summary>
    ''' The stage to which this set of parameters relates.
    ''' Used as the scope stage for populating the treeview.
    ''' </summary>
    ''' <remarks>See doc associated with corresponding
    ''' parameter on PopulateElement method for more info.</remarks>
    Private mobjProcessStage As clsProcessStage

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        MyBase.SetMaxSteps(0)
        Me.ctlParamsList.MinimumColumnWidth = 20

        'Add listview columns
        Me.ctlParamsList.Columns.Add(My.Resources.Name, My.Resources.TheNameOfTheAttributeToBeSpecified, 95)
        Me.ctlParamsList.Columns.Add(My.Resources.DataType, My.Resources.TheDataTypeOfTheNamedAttribute, 60)
        Me.ctlParamsList.Columns.Add(My.Resources.MatchType, My.Resources.TheTypeOfComparisonToBeMadeAgainstTheValueColumn, 120)
        Me.ctlParamsList.Columns.Add(My.Resources.Value, My.Resources.TheValueToBeComparedAgainstTheTrueAttributeValuesOfElements)

        Me.ctlParamsList.LastColumnAutoSize = True
        Me.objBluebar.Title = My.Resources.ProvideDynamicDataFromYourProcessToIdentifyTheCurrentApplicationElement
    End Sub


    ''' <summary>
    ''' Populates the user interface with the dynamic
    ''' attributes of the supplied element.
    ''' </summary>
    ''' <param name="Element">The element to collect parameters
    ''' for. Naturally this element should have some dynamic
    ''' attributes.</param>
    ''' <param name="Parameters">Parameters that may be applied to
    ''' the element. Names of parameters will be matched against
    ''' names of attributes.</param>
    ''' <param name="objProcessStage">A stage with which to populate
    ''' the data item treeview.</param>
    Public Sub PopulateElement(ByVal Element As clsApplicationElement, ByVal Parameters As List(Of clsApplicationElementParameter), ByVal objProcessStage As clsProcessStage)
        Me.mobjProcessStage = objProcessStage
        Me.ctlParamsList.Rows.Clear()
        For Each a As clsApplicationAttribute In Element.Attributes
            If a.InUse AndAlso a.Dynamic Then Me.ctlParamsList.Rows.Add(Me.CreateRowForAttribute(a, Parameters))
        Next
        Me.ctlParamsList.UpdateView()

        Me.CtlDataItemTreeView1.Populate(objProcessStage)
    End Sub

    ''' <summary>
    ''' Creates a listrow corresponding to the supplied attribute.
    ''' The user will be able to edit the last column, corresponding
    ''' to the value of the attribute.
    ''' </summary>
    ''' <param name="attr">The attribute to be represented.</param>
    ''' <returns>A new listrow.</returns>
    ''' <remarks>The value represented in the user interface
    ''' can be retrieved by calling GetParameters.</remarks>
    Private Function CreateRowForAttribute(ByVal attr As clsApplicationAttribute, _
     ByVal params As List(Of clsApplicationElementParameter)) As clsListRow

        'If a parameter passed matches our attribute then populate it
        Dim ParamToPopulate As clsApplicationElementParameter = Nothing
        If params IsNot Nothing Then
            For Each p As clsApplicationElementParameter In params
                If p.Name = attr.Name Then ParamToPopulate = p : Exit For
            Next
        End If

        If ParamToPopulate Is Nothing Then _
         ParamToPopulate = New clsApplicationElementParameter( _
         attr.Name, attr.Value.DataType, Nothing, clsAMI.ComparisonTypes.Equal)

        Return New clsApplicationElementParameterListRow( _
         ctlParamsList, ParamToPopulate, mobjProcessStage)

    End Function

    ''' <summary>
    ''' Gets the help file for this form.
    ''' </summary>
    ''' <returns>Filename of help file.</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmElementParams.htm"
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


    ''' <summary>
    ''' The parameters represented in this user interface.
    ''' </summary>
    ''' <returns>A collection of parameter objects.</returns>
    Public Function GetParameters() As List(Of clsApplicationElementParameter)
        Dim List As New List(Of clsApplicationElementParameter)
        For Each r As clsApplicationElementParameterListRow In Me.ctlParamsList.Rows
            List.Add(r.Parameter)
        Next

        Return List
    End Function

    Protected Overrides Sub UpdatePage()
        Dim i As Integer = MyBase.GetStep
        Select Case i
            Case 0
            Case Else
                Me.ctlParamsList.EndEditing()
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()
        End Select
    End Sub

End Class