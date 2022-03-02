Imports System.Globalization

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

''' Project  : Automate
''' Class    : ctlUserSettingsExpiry
''' 
''' <summary>
''' A control to display and manage user expiry settings.
''' </summary>
Friend Class ctlUserSettingsExpiry
    Inherits System.Windows.Forms.UserControl

    ' Whether to use the third party culture aware calendar as windows form version cannot have locale set
    Dim mUseCultureCal As Boolean

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        mUseCultureCal = Options.Instance.UseCultureCalendar
        If (mUseCultureCal) Then
            cultureCalUserExpiry.Location = calUserExpiry.Location
            cultureCalPasswordExpiry.Location = calPasswordExpiry.Location
            cultureCalPasswordExpiry.Culture = New CultureInfo(CultureInfo.CurrentCulture.Name)
            calUserExpiry.Visible = False
            calPasswordExpiry.Visible = False
        Else
            cultureCalUserExpiry.Visible = False
            cultureCalPasswordExpiry.Visible = False
        End If

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
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtUserExpiry As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents calUserExpiry As System.Windows.Forms.MonthCalendar
    Friend WithEvents txtPasswordExpiry As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents cmbPasswordDuration As System.Windows.Forms.ComboBox
    Private WithEvents mSplitter As System.Windows.Forms.SplitContainer
    Friend WithEvents cultureCalUserExpiry As CustomControls.MonthCalendar
    Friend WithEvents cultureCalPasswordExpiry As CustomControls.MonthCalendar
    Friend WithEvents calPasswordExpiry As System.Windows.Forms.MonthCalendar
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlUserSettingsExpiry))
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtUserExpiry = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.calUserExpiry = New System.Windows.Forms.MonthCalendar()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cmbPasswordDuration = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtPasswordExpiry = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.calPasswordExpiry = New System.Windows.Forms.MonthCalendar()
        Me.mSplitter = New System.Windows.Forms.SplitContainer()
        Me.cultureCalUserExpiry = New CustomControls.MonthCalendar()
        Me.cultureCalPasswordExpiry = New CustomControls.MonthCalendar()
        CType(Me.mSplitter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mSplitter.Panel1.SuspendLayout()
        Me.mSplitter.Panel2.SuspendLayout()
        Me.mSplitter.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'txtUserExpiry
        '
        resources.ApplyResources(Me.txtUserExpiry, "txtUserExpiry")
        Me.txtUserExpiry.Name = "txtUserExpiry"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'calUserExpiry
        '
        resources.ApplyResources(Me.calUserExpiry, "calUserExpiry")
        Me.calUserExpiry.Name = "calUserExpiry"
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'cmbPasswordDuration
        '
        resources.ApplyResources(Me.cmbPasswordDuration, "cmbPasswordDuration")
        Me.cmbPasswordDuration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPasswordDuration.FormattingEnabled = True
        Me.cmbPasswordDuration.Items.AddRange(New Object() {resources.GetString("cmbPasswordDuration.Items"), resources.GetString("cmbPasswordDuration.Items1"), resources.GetString("cmbPasswordDuration.Items2"), resources.GetString("cmbPasswordDuration.Items3"), resources.GetString("cmbPasswordDuration.Items4"), resources.GetString("cmbPasswordDuration.Items5"), resources.GetString("cmbPasswordDuration.Items6"), resources.GetString("cmbPasswordDuration.Items7"), resources.GetString("cmbPasswordDuration.Items8"), resources.GetString("cmbPasswordDuration.Items9")})
        Me.cmbPasswordDuration.Name = "cmbPasswordDuration"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'txtPasswordExpiry
        '
        resources.ApplyResources(Me.txtPasswordExpiry, "txtPasswordExpiry")
        Me.txtPasswordExpiry.Name = "txtPasswordExpiry"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'calPasswordExpiry
        '
        resources.ApplyResources(Me.calPasswordExpiry, "calPasswordExpiry")
        Me.calPasswordExpiry.Name = "calPasswordExpiry"
        '
        'mSplitter
        '
        resources.ApplyResources(Me.mSplitter, "mSplitter")
        Me.mSplitter.Name = "mSplitter"
        '
        'mSplitter.Panel1
        '
        Me.mSplitter.Panel1.Controls.Add(Me.cultureCalUserExpiry)
        Me.mSplitter.Panel1.Controls.Add(Me.calUserExpiry)
        Me.mSplitter.Panel1.Controls.Add(Me.Label1)
        Me.mSplitter.Panel1.Controls.Add(Me.txtUserExpiry)
        Me.mSplitter.Panel1.Controls.Add(Me.Label2)
        '
        'mSplitter.Panel2
        '
        Me.mSplitter.Panel2.Controls.Add(Me.cultureCalPasswordExpiry)
        Me.mSplitter.Panel2.Controls.Add(Me.cmbPasswordDuration)
        Me.mSplitter.Panel2.Controls.Add(Me.Label5)
        Me.mSplitter.Panel2.Controls.Add(Me.Label3)
        Me.mSplitter.Panel2.Controls.Add(Me.calPasswordExpiry)
        Me.mSplitter.Panel2.Controls.Add(Me.Label4)
        Me.mSplitter.Panel2.Controls.Add(Me.txtPasswordExpiry)
        Me.mSplitter.TabStop = False
        '
        'cultureCalUserExpiry
        '
        resources.ApplyResources(Me.cultureCalUserExpiry, "cultureCalUserExpiry")
        Me.cultureCalUserExpiry.Name = "cultureCalUserExpiry"
        '
        'cultureCalPasswordExpiry
        '
        resources.ApplyResources(Me.cultureCalPasswordExpiry, "cultureCalPasswordExpiry")
        Me.cultureCalPasswordExpiry.Name = "cultureCalPasswordExpiry"
        '
        'ctlUserSettingsExpiry
        '
        Me.Controls.Add(Me.mSplitter)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlUserSettingsExpiry"
        Me.mSplitter.Panel1.ResumeLayout(False)
        Me.mSplitter.Panel1.PerformLayout()
        Me.mSplitter.Panel2.ResumeLayout(False)
        Me.mSplitter.Panel2.PerformLayout()
        CType(Me.mSplitter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mSplitter.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Member Variables "

    ' The user object whose expiry settings are being changed
    Private mUser As User

    ' Flag indicating if the user expiry date has changed
    Private mUserExpiryChange As Boolean

    ' Flag indicating if the password expiry date has changed
    Private mPasswordExpiryChanged As Boolean

    ' Flag indicating if the password duration value has been changed
    Private mPasswordDurationChanged As Boolean

#End Region

#Region "methods"

    ''' <summary>
    ''' Checks that all fields have valid values.
    ''' </summary>
    ''' <returns>True if all valid</returns>
    Public Function AllFieldsValid() As Boolean
        'check for blank dates
        If txtUserExpiry.Text = "" Then _
         Return UserMessage.Err(My.Resources.ctlUserSettingsExpiry_PleaseEnterAUserExpiryDate)

        If txtPasswordExpiry.Text = "" Then _
         Return UserMessage.Err(My.Resources.ctlUserSettingsExpiry_PleaseEnterAPasswordExpiryDate)

        If cmbPasswordDuration.SelectedIndex = -1 Then _
         Return UserMessage.Err(My.Resources.ctlUserSettingsExpiry_PleaseSelectAPasswordDuration)

        'check that dates can be parsed
        Dim dummy As Date
        If Not Date.TryParse(txtPasswordExpiry.Text, dummy) Then _
         Return UserMessage.Err(My.Resources.ctlUserSettingsExpiry_PasswordExpiryDateAppearsToBeInvalidPleaseReviewTheDateEnteredIntoThePasswordTe)

        If Not Date.TryParse(txtUserExpiry.Text, dummy) Then _
         Return UserMessage.Err(My.Resources.ctlUserSettingsExpiry_UserExpiryDateAppearsToBeInvalidPleaseReviewTheDateEnteredIntoTheExpiryTextbox)

        Return True
    End Function

#End Region

#Region "Event Code"


    Private Sub calPasswordExpiry_DateSelected(ByVal sender As Object, ByVal e As DateRangeEventArgs) Handles calPasswordExpiry.DateSelected, cultureCalPasswordExpiry.DateSelected
        mUser.PasswordExpiry = e.End
        txtPasswordExpiry.Text = mUser.PasswordExpiryDisplay
    End Sub

    Private Sub txtPasswordExpiry_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtPasswordExpiry.TextChanged
        Dim result As Date
        If Date.TryParse(txtPasswordExpiry.Text, result) Then
            mUser.PasswordExpiry = result
        End If
        mPasswordExpiryChanged = True
    End Sub

    Private Sub calUserExpiry_DateSelected(ByVal sender As Object, ByVal e As DateRangeEventArgs) Handles calUserExpiry.DateSelected, cultureCalUserExpiry.DateSelected
        mUser.Expiry = e.End
        txtUserExpiry.Text = mUser.ExpiryDisplay
    End Sub

    Private Sub txtUserExpiry_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtUserExpiry.TextChanged
        Dim result As Date
        If Date.TryParse(txtUserExpiry.Text, result) Then
            mUser.Expiry = result
        End If
        mUserExpiryChange = True
    End Sub

    Public Property User() As User
        Get
            Return mUser
        End Get
        Set(ByVal value As User)
            mUser = value
            If value IsNot Nothing Then
                cmbPasswordDuration.SelectedIndex = mUser.PasswordDurationWeeks - 1

                calPasswordExpiry.SetDate(mUser.PasswordExpiry)
                cultureCalPasswordExpiry.SetDate(mUser.PasswordExpiry)

                txtPasswordExpiry.Text = mUser.PasswordExpiryDisplay

                calUserExpiry.SetDate(mUser.Expiry)
                cultureCalUserExpiry.SetDate(mUser.Expiry)

                txtUserExpiry.Text = mUser.ExpiryDisplay

                mPasswordExpiryChanged = False
                mUserExpiryChange = False

            End If
        End Set
    End Property


#End Region

    Private Sub HandlePasswordDurationChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbPasswordDuration.SelectedIndexChanged
        mPasswordDurationChanged = True
        mUser.PasswordDurationWeeks = cmbPasswordDuration.SelectedIndex + 1
    End Sub
End Class
