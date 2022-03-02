Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports System.DirectoryServices.ActiveDirectory
Imports System.Security.Principal
Imports System.DirectoryServices.AccountManagement
Imports System.ComponentModel

''' <summary>
''' User control for configuring single sign-on via Active Directory
''' </summary>
Public Class ActiveDirectorySettings
    ''' <summary>
    ''' Specifies whether the control should show exisiting values from the database 
    ''' or populate with defaults for a new connection
    ''' </summary>
    <Browsable(True), Category("Behaviour"), DefaultValue("False"),
    Description("Indicates whether the control is being used to create/configure a " _
        & "new database (True) or to retrieve stored values for an existing " _
        & "database (False)")>
    Public Property IsNewConnection As Boolean

    Private mAdminGroupSid As SecurityIdentifier = Nothing
    Public ReadOnly Property AdminGroupSid() As SecurityIdentifier
        Get
            Return mAdminGroupSid
        End Get
    End Property

    Public ReadOnly Property AdminGroupName() As String
        Get
            Return txtAdminGroupName.Text
        End Get
    End Property

    Public ReadOnly Property DomainName() As String
        Get
            Return txtActiveDirectoryDomain.Text
        End Get
    End Property

    Private mConnectionIsVerified As Boolean = False
    Private mOriginalFQDN As String

    ''' <summary>
    ''' Populates the user interface with the domain and Admin Group.
    ''' If being used to create or configure a new database, will populate with
    ''' the name of the domain that the current machine is a member of. 
    ''' Otherwise, populates the user interface with the current settings,
    ''' as read from the database
    ''' </summary>
    Public Sub Populate()

        If IsNewConnection Then
            'this is creating a new db so we don't want to use the gSv object for the existing connection
            If Me.txtActiveDirectoryDomain.Text = "" Then
                Try
                    Dim d As Domain = Domain.GetComputerDomain()
                    Me.txtActiveDirectoryDomain.Text = d.Name
                Catch ex As ActiveDirectoryObjectNotFoundException
                    'no domain found, so leave blank
                    Me.txtActiveDirectoryDomain.Text = ""
                End Try
            End If
        Else

            'retrieve the values for the current connection from the db
            Dim AdminGroup = ""
            gSv.GetSignonSettings(Me.txtActiveDirectoryDomain.Text, AdminGroup)

            Try
                'Record the original FQDN in case it is changed and we
                'need to remove all the other role/roup mappings
                mOriginalFQDN = GetDomainControllerFQDN()
            Catch
                'Ignore any errors here
            End Try

            Me.txtAdminGroupName.Text = AdminGroup
            If AdminGroup IsNot Nothing Then
                Try
                    mAdminGroupSid = New SecurityIdentifier(AdminGroup)
                Catch ex As Exception
                    'Still stored as the group name in the database, rather than a SID
                    ChangeAdminGroupIdentifierToSID(AdminGroup)
                End Try
                PopulateAdminGroup()
            End If
        End If
        AddHandler txtActiveDirectoryDomain.TextChanged, AddressOf DomainChangedByUser
    End Sub

    ''' <summary> 
    ''' Attempts to find the domain controller and return its Fully Qualified
    ''' Domain Name.
    ''' </summary>
    ''' <returns>Fully-qualified domain name as a string</returns>
    ''' <remarks>Throws an exception if the domain cannot be found (left unhandled to be
    ''' handled in the calling method)</remarks>
    Private Function GetDomainControllerFQDN() As String
        Dim context As New DirectoryContext(DirectoryContextType.Domain, Me.txtActiveDirectoryDomain.Text)
        Dim dom As Domain = Domain.GetDomain(context)
        Return dom.Name
    End Function

    ''' <summary> 
    ''' Returns the user name of the current logged in user
    ''' </summary>
    ''' <returns> User name as string</returns>
    Private Function GetUserName() As String
        Return UserPrincipal.Current.UserPrincipalName
    End Function

    ''' <summary>
    ''' Tests that a connection can be made to the specified domain and reports any
    ''' failure to the user 
    ''' </summary>
    ''' <returns>Boolean value stating whether the connection was verified</returns>
    ''' <remarks> Where the connection can be made, updates the UI with the fully 
    ''' qualified domain name </remarks>
    Public Function VerifyDomain() As Boolean
        If mConnectionIsVerified Then Return True
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim fqdn As String = Nothing
            Dim OldDomainValue As String = txtActiveDirectoryDomain.Text
            Try
                'The following method will throw an error if the domain is not found
                fqdn = GetDomainControllerFQDN()
            Catch ex As Exception
                MessageBox.Show(String.Format(
                                My.Resources.AConnectionCouldNotBeEstablishedToTheDomain0FromThisDevice1User211VerifyThatThe,
                                 txtActiveDirectoryDomain.Text, vbCrLf, GetUserName()), My.Resources.ConnectionFailed)
                Return False
            End Try
            If fqdn <> Nothing Then
                RemoveHandler txtActiveDirectoryDomain.TextChanged, AddressOf DomainChangedByUser
                txtActiveDirectoryDomain.Text = fqdn
                AddHandler txtActiveDirectoryDomain.TextChanged, AddressOf DomainChangedByUser
            End If
            'Raise domain changed event if the previous domain (non-FQDN) was updated
            'with the FQDN
            If Not String.IsNullOrEmpty(fqdn) AndAlso fqdn <> OldDomainValue Then
                RaiseEvent DomainChanged(Me, EventArgs.Empty)
            End If

            Return True
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Function

    ''' <summary>
    ''' Sets the AD group indetifier to be its SID instead of the specified
    ''' Group Name and saves the SID in the database
    ''' </summary>
    Private Sub ChangeAdminGroupIdentifierToSID(GroupName As String)

        'Retrieve the security group from Active Directory using the SID
        'and save to the database
        Using searcher As New ADGroupSearcher()
            Dim principal As Principal = searcher.GetADGroupByName(
                DomainName, Nothing, GroupName)

            If principal IsNot Nothing AndAlso principal.Sid.IsAccountSid() Then
                mAdminGroupSid = principal.Sid
            Else
                MessageBox.Show(
                    String.Format(My.Resources.TheGroup0CannotBeFoundAsItMayHaveBeenRemovedFromThisDomainPleaseContactYourActi,
                                  GroupName))
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Event firing when the Admin Group has been populated
    ''' </summary>
    Public Event AdminGroupPopulated As EventHandler

    ''' <summary>
    ''' Populate the admin group user interface with the data from the database
    ''' </summary>
    Private Sub PopulateAdminGroup()
        'Get the group name and path from active directory and populate text fields
        If mAdminGroupSid IsNot Nothing Then
            Using searcher As New ADGroupSearcher()
                Dim group As GroupPrincipal =
                    searcher.GetADGroupBySid(DomainName, Nothing, mAdminGroupSid)
                If group IsNot Nothing Then
                    txtAdminGroupName.Text = group.Name
                    txtAdminGroupPath.Text = group.DistinguishedName
                    SetAdminGroupTextBoxTooltips()
                Else
                    MessageBox.Show(String.Format(My.Resources.TheGroup0CannotBeFoundAsItMayHaveBeenRemovedFromThisDomainPleaseContactYourActi, mAdminGroupSid.ToString))
                End If
            End Using
        Else
            txtAdminGroupName.Text = ""
            txtAdminGroupPath.Text = ""
            SetAdminGroupTextBoxTooltips()
        End If

        RaiseEvent AdminGroupPopulated(Me, EventArgs.Empty)

    End Sub


    ''' <summary>
    ''' Returns whether the group specified in the txtAdminGroup does actually exist 
    ''' in the specified domain
    ''' </summary>
    Public Function VerifyAdminGroup() As Boolean
        Try
            Me.Cursor = Cursors.WaitCursor
            Try
                Using searcher As New ADGroupSearcher()
                    Dim principal As Principal = searcher.GetADGroupByDN(
                        DomainName, Nothing, txtAdminGroupPath.Text)
                    If principal Is Nothing Then
                        Throw New InvalidOperationException
                    End If
                End Using
            Catch ex As InvalidOperationException
                MessageBox.Show(
                    String.Format(My.Resources.TheGroup0CannotBeFoundInTheDomain12PleaseContactYourActiveDirectoryDomainAdmini,
                                  AdminGroupName, DomainName, vbCrLf))
                Return False
            End Try

            Return True
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Function

    ''' <summary>
    ''' Commits the configuration in the user interface to the database. If the UI 
    ''' holds the short name for the domain, the method first converts it to the 
    ''' fully-qualified domain name
    ''' </summary>
    ''' <returns>Returns true on success</returns>
    Public Function CommitConfiguration(ByRef domainChanged As Boolean) As Boolean

        If VerifyDomain() AndAlso VerifyAdminGroup() Then
            Dim currentFQDN = Me.txtActiveDirectoryDomain.Text
            domainChanged = Not String.IsNullOrEmpty(mOriginalFQDN) AndAlso (mOriginalFQDN <> currentFQDN)
            mOriginalFQDN = currentFQDN
            Return True
        Else
            Return False
        End If

    End Function

    ''' <summary>
    ''' Set the tooltips on the admin groups text box, so the contents of the text 
    ''' boxes can been seen when hovering in case the text is truncated.
    ''' </summary>
    Private Sub SetAdminGroupTextBoxTooltips()
        ToolTip.SetToolTip(txtAdminGroupName, txtAdminGroupName.Text)
        ToolTip.SetToolTip(txtAdminGroupPath, txtAdminGroupPath.Text)
    End Sub


    ''' <summary>
    ''' Sets and removes the "verified" status of the specified domain on the form so 
    ''' the property mConnectionIsVerified can be checked as required
    ''' </summary>
    ''' <param name="isVerified">Boolean stating whether the domain has been verified
    '''  or not</param>
    Private Sub SetVerified(ByVal isVerified As Boolean)
        If isVerified Then
            lblVerified.Show()
            pbTick.Show()
            lblUnverified.Hide()

            lblVerified.BringToFront()
            pbTick.BringToFront()
            mConnectionIsVerified = True
        Else
            lblVerified.Hide()
            pbTick.Hide()
            lblUnverified.Show()

            lblUnverified.BringToFront()
            pbTick.SendToBack()
            mConnectionIsVerified = False
        End If
    End Sub

#Region "Events"

    Private Sub btnVerifyDomain_Click(sender As Object, e As EventArgs) Handles btnVerifyDomain.Click
        If VerifyDomain() Then
            SetVerified(True)
            MessageBox.Show(String.Format(My.Resources.AConnectionWasSuccessfullyEstablishedToTheDomain0FromThisDevice1User2, txtActiveDirectoryDomain.Text, vbCrLf, GetUserName()),
                                              My.Resources.ConnectionSuccessful)
        End If
    End Sub

    Private Sub btnAdminGroup_Click(sender As Object, e As EventArgs) Handles btnAdminGroup.Click
        If VerifyDomain() Then
            SetVerified(True)
            'Open the form that can be used to select groups from active directory
            Using frmGroupSelector As New ADGroupSelectorForm(
             txtActiveDirectoryDomain.Text, mAdminGroupSid, True, Role.DefaultNames.SystemAdministrators)
                'Set environment colour (if available)
                Dim parent = TryCast(ParentForm, IEnvironmentColourManager)
                If parent IsNot Nothing Then
                    frmGroupSelector.EnvironmentBackColor = parent.EnvironmentBackColor
                    frmGroupSelector.EnvironmentForeColor = parent.EnvironmentForeColor
                End If

                frmGroupSelector.ShowInTaskbar = False
                If frmGroupSelector.ShowDialog = DialogResult.OK Then
                    'Set the selected group to be the Blue Prism System Administrator role
                    mAdminGroupSid = frmGroupSelector.SelectedGroup
                    'Update the user interface
                    PopulateAdminGroup()
                End If
            End Using
        End If

    End Sub


    ''' <summary>
    ''' Event firing when the Admin Group has been populated
    ''' </summary>
    Public Event DomainChanged As EventHandler

    Private Sub DomainChangedByUser(sender As Object, e As EventArgs)
        SetVerified(False)
        'Only enable the admin group button if there is a domain to search
        btnAdminGroup.Enabled = Not (txtActiveDirectoryDomain.Text = String.Empty)
        'Clear the existing admin group (as the previously selected group will no be 
        'invalid
        If AdminGroupSid IsNot Nothing Then
            mAdminGroupSid = Nothing
            'Update the user interface
            PopulateAdminGroup()
        End If

        RaiseEvent DomainChanged(Me, EventArgs.Empty)

    End Sub

#End Region

End Class
