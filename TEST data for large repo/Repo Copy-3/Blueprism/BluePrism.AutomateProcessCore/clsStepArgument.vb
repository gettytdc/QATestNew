Imports System.Xml

''' Class    : clsStepArgument
''' 
''' <summary>
''' This class is used to represent the value for a particular Argument to an
''' Action, as used in a Navigate step. Currently other step types don't make
''' use of this functionality.
''' </summary>
Public Class clsStepArgument

    ''' <summary>
    ''' Identifier for this argument eg "targx". This ID is supplied by AMI
    ''' as part of clsArgumentInfo, and should be used to reference any further
    ''' information, in conjunction with the action ID, which the owner of this
    ''' clsStepArgument object should know.
    ''' </summary>
    Public ReadOnly Property ID() As String
        Get
            Return msID
        End Get
    End Property
    Private msID As String

    ''' <summary>
    ''' The value of the argument, which is an expression to be evaluated at run time.
    ''' </summary>
    Public Property Value() As String
        Get
            Return msValue
        End Get
        Set(ByVal value As String)
            msValue = value
        End Set
    End Property
    Private msValue As String

    ''' <summary>
    ''' Constructor, to be used only within AMI itself. Clients of AMI receive
    ''' instances of this class only by calling the relevant clsAMI methods.
    ''' </summary>
    ''' <param name="sID">ID of the argument</param>
    Public Sub New(ByVal sID As String)
        msID = ID
    End Sub

    ''' <summary>
    ''' Performs a deep clone of this argumentinfo.
    ''' </summary>
    Public Function Clone() As clsStepArgument
        Dim copy As New clsStepArgument(Me.ID)
        copy.Value = Me.Value
        Return copy
    End Function

    ''' <summary>
    ''' Generates xml corresponding to this instance.
    ''' </summary>
    ''' <param name="ParentDocument">The document to own the generated
    ''' element.</param>
    ''' <returns>Returns an xml element representing this instance.
    ''' This element name will be "Argument"</returns>
    Public Function ToXML(ByVal ParentDocument As XmlDocument) As XmlElement
        Dim retval As XmlElement = ParentDocument.CreateElement("argument")
        Dim child As XmlElement = Nothing

        If Not Me.ID = "" Then
            child = ParentDocument.CreateElement("id")
            child.InnerText = Me.ID
            retval.AppendChild(child)
        End If

        If Not Me.Value = "" Then
            child = ParentDocument.CreateElement("value")
            child.InnerText = Me.Value
            retval.AppendChild(child)
        End If

        Return retval
    End Function


    ''' <summary>
    ''' Deserialises an xml element and returns the
    ''' argumentinfo object represented.
    ''' </summary>
    ''' <param name="e">The xml element representing an argument
    ''' info object. Element name must be "Argument".</param>
    ''' <returns>Returns new argumentinfo object,, or nothing if the xml element
    ''' is not suitable.</returns>
    Public Shared Function FromXML(ByVal e As XmlElement) As clsStepArgument
        Dim retval As clsStepArgument = Nothing

        If e.Name = "argument" Then
            Dim id As String = ""
            Dim value As String = ""

            For Each child As XmlElement In e.ChildNodes
                Select Case child.Name
                    Case "id"
                        id = child.InnerText
                    Case "value"
                        value = child.InnerText
                End Select
            Next

            retval = New clsStepArgument(id)
            retval.Value = value
        End If

        Return retval
    End Function


End Class
