Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateAppCore.Groups

<DebuggerDisplay("ProcessType: {ModeString}")>
Public Class ProcessType

    Private Shared mBusinessObject As ProcessType
    Private Shared mProcess As ProcessType

    Shared Sub New()
        mBusinessObject = New ProcessType(DiagramType.Object)
        mProcess = New ProcessType(DiagramType.Process)
    End Sub

    Public Shared ReadOnly Property BusinessObject() As ProcessType
        Get
            Return mBusinessObject
        End Get
    End Property

    Public Shared ReadOnly Property Process() As ProcessType
        Get
            Return mProcess
        End Get
    End Property

    Private mMode As DiagramType

    Private Sub New(ByVal mode As DiagramType)
        mMode = mode
    End Sub
    ''' <summary>
    ''' A string representation of the current mode of this control as a plural.
    ''' ie. "Processes" or "Business Objects" as appropriate
    ''' 
    ''' Made private as this is not localized and should only be used for permissions
    ''' </summary>
    Private ReadOnly Property ModeStringPlural() As String
        Get
            If mMode = DiagramType.Process Then Return "Processes"
            Return "Business Objects"
        End Get
    End Property

    ''' <summary>
    ''' Assembles a permission string based on the current mode
    ''' </summary>
    Public Function GetPermissionString(ByVal s As String) As String
        ' I18n Pluralization - should not be localized as is permission string?
        Return String.Format("{0} - {1}", ModeStringPlural, s)
    End Function

    ''' <summary>
    ''' A string representation of the current mode of this control.
    ''' ie. "Process" or "Business Object" as appropriate
    ''' </summary>
    Public ReadOnly Property ModeString() As String
        Get
            If mMode = DiagramType.Process Then Return My.Resources.ProcessType_ProcessC
            Return My.Resources.ProcessType_BusinessObjectC
        End Get
    End Property

    Public ReadOnly Property ModeStringLowerCase() As String
        Get
            If mMode = DiagramType.Process Then Return My.Resources.ProcessType_processL
            Return My.Resources.ProcessType_businessobjectL
        End Get
    End Property

    Public Function IsProcess() As Boolean
        Return mMode = DiagramType.Process
    End Function

    Public Function IsBusinessObject() As Boolean
        Return mMode = DiagramType.Object
    End Function

    Public Function TreeType() As GroupTreeType
        Select Case mMode
            Case DiagramType.Object
                Return GroupTreeType.Objects
            Case DiagramType.Process
                Return GroupTreeType.Processes
            Case Else
                Return GroupTreeType.None
        End Select
    End Function

    Private Sub ChangeText(ByVal c As Control, ByVal s1 As String, ByVal s2 As String)

        For Each child As Control In c.Controls
            ChangeText(child, s1, s2)
        Next
        c.Text = c.Text.Replace(s1, s2)

    End Sub

    Public Sub ChangeText(ByVal c As Control)
        If IsBusinessObject() Then
            ChangeText(c, My.Resources.ProcessType_processesL, My.Resources.ProcessType_businessobjectsL)
            ChangeText(c, My.Resources.ProcessType_ProcessesC, My.Resources.ProcessType_BusinessObjectsC)
            ChangeText(c, My.Resources.ProcessType_processL, My.Resources.ProcessType_businessobjectL)
            ChangeText(c, My.Resources.ProcessType_ProcessC, My.Resources.ProcessType_BusinessObjectC)
        Else
            ChangeText(c, My.Resources.ProcessType_businessobjectsL, My.Resources.ProcessType_processesL)
            ChangeText(c, My.Resources.ProcessType_BusinessObjectsC, My.Resources.ProcessType_ProcessesC)
            ChangeText(c, My.Resources.ProcessType_businessobjectL, My.Resources.ProcessType_processL)
            ChangeText(c, My.Resources.ProcessType_BusinessObjectC, My.Resources.ProcessType_ProcessC)
        End If
    End Sub

    Public Function FormatString(msg As String) As String
        Return String.Format(msg, ModeString.ToLower)
    End Function

    Public Shared Operator =(ByVal left As ProcessType, ByVal right As ProcessType) As Boolean
        If left Is Nothing AndAlso right Is Nothing Then Return True
        If left Is Nothing OrElse right Is Nothing Then Return False

        Return left.mMode = right.mMode
    End Operator

    Public Shared Operator <>(ByVal left As ProcessType, ByVal right As ProcessType) As Boolean
        Return Not (left = right)
    End Operator


    ''' <summary>
    ''' Converts a 'process type' into a 'process type', which you might think is
    ''' redundant and pointless, but is not because reasons.
    ''' </summary>
    ''' <param name="pt">The type of a process</param>
    ''' <returns>The process type</returns>
    Public Shared Widening Operator CType(pt As DiagramType) As ProcessType
        Select Case pt
            Case DiagramType.Object : Return mBusinessObject
            Case DiagramType.Process : Return mProcess
            Case Else : Return Nothing
        End Select
    End Operator

    ''' <summary>
    ''' Converts a 'process type' into a 'process type', which you might think is
    ''' redundant and pointless, but is not because reasons.
    ''' </summary>
    ''' <param name="pt">The process type</param>
    ''' <returns>The type of a process</returns>
    Public Shared Widening Operator CType(pt As ProcessType) As DiagramType
        If pt Is mBusinessObject Then Return DiagramType.Object
        If pt Is mProcess Then Return DiagramType.Process
        Return DiagramType.Unset
    End Operator

End Class