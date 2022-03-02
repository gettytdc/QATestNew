Imports AutomateControls
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore.Auth

Friend Class frmActiveDirectoryUserSearch
    Inherits AutomateControls.Forms.AutomateForm
    Implements IEnvironmentColourManager


    Sub New(user As User)
  
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ctlSingleUserActiveDirectorySearch.FreezeUserDetails(user)

        tBar.Title = ActiveDirectoryUserSearch_Resources.UserSearcherUpdateUserTitle
        Text = ActiveDirectoryUserSearch_Resources.MainWindowTitle
    End Sub


#Region "IEnvironmentColourManager implementation"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return tBar.BackColor
        End Get
        Set(value As Color)
            tBar.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return tBar.TitleColor
        End Get
        Set(value As Color)
            tBar.TitleColor = value
        End Set
    End Property
#End Region

End Class
