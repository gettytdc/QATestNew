Imports AutomateControls.Forms
''' Project  : Automate
''' Class    : Form
''' 
''' <summary>
''' A System.Windows.Forms.Form subclass used at a base class for Automate forms.
''' </summary>
Public Class frmForm : Inherits AutomateForm : Implements IChild

    Private Sub Form_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        If Not DesignMode Then
            clsFont.SetFont(Me)
        End If
    End Sub

    Private Sub frmForm_ControlAdded(ByVal sender As Object, ByVal e As ControlEventArgs) Handles Me.ControlAdded
        If Not DesignMode Then
            clsFont.SetFont(e.Control)
        End If
    End Sub

    Protected mParent As frmApplication
    Friend Overridable Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the application form currently set as the 'parent' of this form
    ''' </summary>
    ''' <returns>The application form 'parent' of this form or null if none is set.
    ''' </returns>
    Public Function GetParent() As frmApplication
        Return mParent
    End Function

End Class
