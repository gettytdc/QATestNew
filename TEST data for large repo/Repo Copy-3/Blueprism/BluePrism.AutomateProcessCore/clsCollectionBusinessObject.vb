''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionBusinessObject
''' 
''' <summary>
''' This class represents the Automate - Collections Internal Business Object
''' </summary>
Public Class clsCollectionBusinessObject
    Inherits clsInternalBusinessObject

    ''' <summary>
    ''' The new constructor just creates the Internal Business Object Actions
    ''' </summary>
    ''' <param name="process">A reference to the process calling the object</param>
    ''' <param name="session">The session the object is running under</param>
    Public Sub New(ByVal process As clsProcess, ByVal session As clsSession)
        MyBase.New(process, session,
          "Blueprism.AutomateProcessCore.clsCollectionActions",
          My.Resources.Resources.Collections)

        AddAction(New clsCollectionInsertRow(Me))
        AddAction(New clsCollectionRemoveRow(Me))
        AddAction(New clsCollectionCountRow(Me))
        AddAction(New clsCollectionCountColumn(Me))
        AddAction(New clsCollectionRemoveAll(Me))
        AddAction(New clsCollectionCopyRows(Me))
    End Sub

    ''' <summary>
    ''' Handles anything that must be done to dispose the object.
    ''' </summary>
    Public Overrides Sub DisposeTasks()
        'Nothing required here
    End Sub

    Public Overrides Function CheckLicense() As Boolean
        Return True
    End Function


End Class
