Imports AutomateControls
Imports AutomateControls.Forms
Imports AutomateUI.Classes
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore

Public Class frmPerformDatabaseConversion : Inherits AutomateForm : Implements IChild, IEnvironmentColourManager

    Private Sub InitializeComponent()
        Me.mComponents = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPerformDatabaseConversion))
        Me.titleBar = New AutomateControls.TitleBar()
        Me.lblStatusDescription = New System.Windows.Forms.Label()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton(Me.mComponents)
        Me.SuspendLayout()
        '
        'titleBar
        '
        resources.ApplyResources(Me.titleBar, "titleBar")
        Me.titleBar.Name = "titleBar"
        Me.titleBar.SubtitleFont = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.titleBar.TitleFont = New System.Drawing.Font("Segoe UI", 12.0!)
        '
        'lblStatusDescription
        '
        resources.ApplyResources(Me.lblStatusDescription, "lblStatusDescription")
        Me.lblStatusDescription.Name = "lblStatusDescription"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.BackColor = System.Drawing.Color.White
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = False
        '
        'frmPerformDatabaseConversion
        '
        resources.ApplyResources(Me, "$this")
        Me.ControlBox = False
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.lblStatusDescription)
        Me.Controls.Add(Me.titleBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmPerformDatabaseConversion"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents titleBar As TitleBar
    Friend WithEvents lblStatusDescription As Label
    Friend WithEvents btnOK As Buttons.StandardStyledButton
    Private mComponents As IContainer
    Private ReadOnly mFrmUserMessage As IUserMessage = New UserMessageWrapper()
    Private mConversionSuccessful As Boolean
    Private ReadOnly mNativeAdminUser As NativeAdminUserModel

    Public Sub New(nativeAdminUser As NativeAdminUserModel)
        mNativeAdminUser = nativeAdminUser
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        lblStatusDescription.Text = frmPerformDatabaseConversion_Resources.lblStatus_ConvertingDatabase
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

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return titleBar.BackColor
        End Get
        Set(value As Color)
            titleBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return titleBar.TitleColor
        End Get
        Set(value As Color)
            titleBar.TitleColor = value
        End Set
    End Property

    Public ReadOnly Property ConversionSuccessful As Boolean
        Get
            Return mConversionSuccessful
        End Get
    End Property

    Public Function GetParent() As frmApplication
        Return mParent
    End Function

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Close()
    End Sub

    Private Sub frmPerformDatabaseConversion_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        btnOK.Hide()

        Dim conversionResultMessage As String = String.Empty

        Try
            Dim task = New TaskFactory().StartNew(Function()
                                                      Return gSv.ConvertDatabaseFromAdSsoToMappedAd(mNativeAdminUser, conversionResultMessage)
                                                  End Function,
                                              TaskCreationOptions.LongRunning)
            BeginInvoke(Sub()
                            ConversionTaskCompleted(task.Result, conversionResultMessage)
                        End Sub)
        Catch ex As Exception
            BeginInvoke(Sub()
                            ConversionTaskFailed(ex)
                        End Sub)
        End Try

    End Sub

    Private Sub ConversionTaskCompleted(success As Boolean, conversionResultMessage As String)

        If success Then
            mConversionSuccessful = True
            titleBar.Title = frmPerformDatabaseConversion_Resources.titleBar_ConversionSuccess

            If String.IsNullOrEmpty(conversionResultMessage) Then
                lblStatusDescription.Text =
                    frmPerformDatabaseConversion_Resources.lblStatus_ConversionSuccessYouWillBeLoggedOut
            Else
                lblStatusDescription.Text =
                    String.Format(frmPerformDatabaseConversion_Resources.lblStatus_ConversionSuccessPleaseAmendFollowing0_Template,
                                  conversionResultMessage)
            End If
        Else
            titleBar.Title = frmPerformDatabaseConversion_Resources.titleBar_ConversionFailure
            lblStatusDescription.Text = conversionResultMessage
        End If
        Cursor.Current = Cursors.Default
        btnOK.Show()
    End Sub
    Private Sub ConversionTaskFailed(exception As Exception)
        titleBar.Title = frmPerformDatabaseConversion_Resources.titleBar_ConversionFailure
        lblStatusDescription.Text = frmPerformDatabaseConversion_Resources.lblStatus_ConversionProcessingException
        btnOK.Show()
        mFrmUserMessage.ShowError(exception, frmPerformDatabaseConversion_Resources.lblStatus_ConversionProcessingException)
    End Sub

End Class
