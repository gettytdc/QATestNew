''' Project  : Automate
''' Class    : frmProcess.DockedChildInfo
''' 
''' <summary>
''' Info about the child controls being docked. Used by iDockingParent
''' controls to remember attributes of the docked children so that
''' they can be restored before undocking.
''' </summary>
Public Class clsDockedChildInfo

    ''' <summary>
    ''' Counts the number of instances of the class.
    ''' </summary>
    Private Shared CreationCount As Long

    Public Sub New()
        clsDockedChildInfo.CreationCount += 1
        UniqueID = clsDockedChildInfo.CreationCount
    End Sub

    ''' <summary>
    ''' Unique ID ensuring unique hash when this object is added to a hashtable.
    ''' </summary>
    Private UniqueID As Long

    ''' <summary>
    ''' The size of the child, before docking.
    ''' </summary>
    Public ChildSize As Size
    ''' <summary>
    ''' The location of the child, before docking.
    ''' </summary>
    Public ChildLocation As Point

    ''' <summary>
    ''' The parent of the child, before docking.
    ''' </summary>
    Public ChildParent As Control

    ''' <summary>
    ''' The Anchor style of the child, before docking.
    ''' </summary>
    Public Anchor As AnchorStyles

    ''' <summary>
    ''' The Dock style of the child, before docking.
    ''' </summary>
    Public Dock As DockStyle

End Class