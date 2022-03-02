Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Groups
Imports ProcessAttributes = BluePrism.AutomateProcessCore.ProcessAttributes
Imports clsProcess = BluePrism.AutomateProcessCore.clsProcess
Imports BluePrism.BPCoreLib

Public Class ctlChooseWebExposeName

    Friend Sub Setup(ByVal mode As ProcessType)
        Select Case mode
            Case ProcessType.Process
                lblName.Text = My.Resources.ctlChooseWebExposeName_ChooseTheExposedNameForTheProcess
            Case ProcessType.BusinessObject
                lblName.Text = My.Resources.ctlChooseWebExposeName_ChooseTheExposedNameForTheBusinessObject
        End Select
    End Sub

    Public Sub Setup(mem As ProcessBackedGroupMember)
        Dim gProcessID As Guid = mem.IdAsGuid
        Dim wsDetails As WebServiceDetails = New WebServiceDetails(gSv.GetProcessWSDetails(gProcessID))

        Dim name As String = wsDetails.WebServiceName

        If String.IsNullOrEmpty(name) Then
            name = gSv.GetProcessNameByID(gProcessID)
            name = clsProcess.GetSafeName(name)
        End If

        txtExposeName.Text = name
        SetValid()
    End Sub

    ''' <summary>
    ''' Validates the given name, checking that it is valid for use as a web service
    ''' name and, optionally, checking to ensure that there is no web service exposed
    ''' in this environment with the same name.
    ''' </summary>
    ''' <param name="name">The name to test for validity</param>
    ''' <param name="full">True to check for a duplicate exposed web service on the
    ''' database; False to only check for name validity.</param>
    ''' <returns>An error message if an invalid name was found, null if the name was
    ''' found to be valid, according to the checks made.</returns>
    Private Function ValidateName(name As String, full As Boolean) As String
        If name <> GetSafeName(name) Then Return My.Resources.ctlChooseWebExposeName_TheNameIsInvalid
        If full AndAlso AlreadyExists(name) Then _
            Return My.Resources.ctlChooseWebExposeName_AProcessOrObjectIsAlreadyExposedUsingThatName
        Return Nothing
    End Function

    ''' <summary>
    ''' Handles the name text box validating, ensuring that a correct non-duplicate
    ''' name has been entered
    ''' </summary>
    Private Sub HandleNameValidating(sender As Object, e As CancelEventArgs) Handles txtExposeName.Validating

        Dim name As String = txtExposeName.Text

        If String.IsNullOrEmpty(name) Then
            lblInvalid.Visible = False
            txtExposeName.BackColor = Color.Empty
            NavigateNext = False
            UpdateNavigate()
            Return
        End If

        Dim msg As String = ValidateName(name, True)
        If msg Is Nothing Then
            SetValid()
        Else
            SetInvalid(msg, e)
        End If

    End Sub

    ''' <summary>
    ''' Handles the text changing in the textbox, checking in realtime that the name
    ''' is valid. Note that this will not check for duplicates - that is only done
    ''' when the textbox validates.
    ''' </summary>
    Private Sub HandleNameTextChanged(sender As Object, e As EventArgs) _
     Handles txtExposeName.TextChanged
        Dim msg As String = ValidateName(txtExposeName.Text, False)
        If msg Is Nothing Then
            SetValid()
        Else
            SetInvalid(msg, Nothing)
        End If
    End Sub

    Private Function AlreadyExists(ByVal name As String) As Boolean
        Dim gProcessID As Guid = gSv.GetProcessIDByWSName(name, True)
        If Not gProcessID = Guid.Empty Then
            Dim attrs As ProcessAttributes
            attrs = gSv.GetProcessAttributes(gProcessID)
            If CBool(attrs And ProcessAttributes.PublishedWS) Then
                Return True
            End If
        End If
        Return False
    End Function

    Private Sub SetInvalid(ByVal reason As String, validatingArgs As CancelEventArgs)
        btnCorrect.Enabled = True
        lblInvalid.Visible = True
        lblInvalid.Text = reason
        txtExposeName.BackColor = Color.Red
        NavigateNext = False
        If validatingArgs IsNot Nothing Then validatingArgs.Cancel = True
        UpdateNavigate()
    End Sub

    Private Sub SetValid()
        btnCorrect.Enabled = False
        lblInvalid.Visible = False
        txtExposeName.BackColor = Color.Empty
        NavigateNext = True
        UpdateNavigate()
    End Sub

    Private Sub btnCorrect_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCorrect.Click
        txtExposeName.Text = BPUtil.FindUnique(
         GetSafeName(txtExposeName.Text) & "{0}", True, Function(n) AlreadyExists(n))
    End Sub

    Public Shared Function GetSafeName(ByVal name As String) As String
        Dim safeName As New StringBuilder()
        For Each c As Char In name
            If Char.IsLetterOrDigit(c) _
             OrElse "_-.".IndexOf(c) <> -1 Then safeName.Append(c)
        Next
        Dim result As String = safeName.ToString()
        If result.Length > 0 Then
            If Char.IsDigit(result(0)) Then result = "_" & result
        End If
        Return result
    End Function

    Private Sub forceSoapDocumentCheckbox_CheckedChanged(sender As Object, e As EventArgs) Handles forceSoapDocumentCheckbox.CheckedChanged

        ' If the Encoding Type is set to Document/Literal, this states it is a complex type,
        '  and thus the schema namespace can not be changed. 
        If forceSoapDocumentCheckbox.Checked Then
            UseLegacyNamespaceCheckbox.Enabled = False

            ' Make sure the checkbox is not checked.
            UseLegacyNamespaceCheckbox.Checked = False
        Else
            UseLegacyNamespaceCheckbox.Enabled = True
        End If

    End Sub
End Class
