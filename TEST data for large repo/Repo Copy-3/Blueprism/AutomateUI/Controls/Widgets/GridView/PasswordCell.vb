Imports AutomateControls
Imports BluePrism.BPCoreLib
Imports BluePrism.Common.Security

'''<summary>
''' A DataGridView cell which can hold passwords
''' </summary>
Public Class PasswordCell
    Inherits DataGridViewTextBoxCell


    Public Sub New()
        Value = New SafeString()
    End Sub


    Private mNewPassword As SafeString = New SafeString()
    ''' <summary>
    ''' The new password entered into this datagrid cell by the user.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NewPassword As SafeString
        Get
            Return mNewPassword
        End Get
        Private Set(value As SafeString)
            mNewPassword = value
        End Set
    End Property

    ''' <summary>
    ''' Flag which determines whether the password has been changed.
    ''' </summary>
    ''' <remarks></remarks>
    Private mHasPasswordChanged As Boolean = False
    Public Property HasPasswordChanged As Boolean
        Get
            Return mHasPasswordChanged
        End Get
        Private Set(value As Boolean)
            mHasPasswordChanged = value
        End Set
    End Property

    ''' <summary>
    ''' Override editing control
    ''' </summary>
    Public Overrides ReadOnly Property EditType As Type
        Get
            Return GetType(SecurePasswordEditingControl)
        End Get
    End Property


    ''' <summary>
    ''' This function should return:
    ''' A string of masking characters of default length if there is an existing 
    ''' password which hasn't been changed by the user.
    ''' An empty string if this is a new password which hasn't been changed by the
    ''' user.
    ''' A string of masking characters with the same length of the new password, 
    ''' if the password has been changed by the user.
    ''' </summary>
    ''' <param name="value">The value to be formatted</param>
    ''' <param name="rowIndex">The index of this value's row</param>
    ''' <param name="cellStyle">The style applied to this cell</param>
    ''' <param name="valueTypeConverter">A type converter</param>
    ''' <param name="formattedValueTypeConverter">A formatted type converter</param>
    ''' <param name="context">Some context</param>
    ''' <returns>A formatted value for the given value</returns>
    Protected Overrides Function GetFormattedValue(value As Object, rowIndex As Integer,
            ByRef cellStyle As DataGridViewCellStyle,
            valueTypeConverter As TypeConverter,
            formattedValueTypeConverter As TypeConverter,
            context As DataGridViewDataErrorContexts) As Object

        Dim oldPassword = DirectCast(Me.Value, SafeString)
        If NewPassword.IsEmpty AndAlso (oldPassword Is Nothing OrElse
                                        oldPassword.IsEmpty) Then
            Return ""
        End If

        If mHasPasswordChanged Then
            Return New String(BPUtil.PasswordChar, NewPassword.Length)
        End If

        Return New String(BPUtil.PasswordChar, SecurePasswordTextBox.PlaceHolderLength)

    End Function

    Private Sub HandlePasswordChanged(sender As Object, e As EventArgs)
        Dim pwd = DirectCast(sender, SecurePasswordEditingControl)
        mHasPasswordChanged = pwd.SecurePassword.Length > 0
    End Sub

    ''' <summary>
    ''' Open the editing control for this value
    ''' </summary>
    ''' <param name="rowIndex">The cell's row index</param>
    ''' <param name="initialFormattedValue">Initial formatted value</param>
    ''' <param name="dataGridViewCellStyle">Cell style</param>
    Public Overrides Sub InitializeEditingControl(rowIndex As Integer,
            initialFormattedValue As Object,
            dataGridViewCellStyle As DataGridViewCellStyle)

        MyBase.InitializeEditingControl(rowIndex, initialFormattedValue,
                                        dataGridViewCellStyle)

        Dim ctl = DirectCast(DataGridView.EditingControl, SecurePasswordEditingControl)
        ctl.SecurePassword = DirectCast(NewPassword, SafeString)
        AddHandler ctl.TextChanged, AddressOf HandlePasswordChanged

    End Sub

    ''' <summary>
    ''' Close the editing control for this value
    ''' </summary>
    Public Overrides Sub DetachEditingControl()
        MyBase.DetachEditingControl()
        Dim ctl = DirectCast(DataGridView.EditingControl, SecurePasswordEditingControl)
        RemoveHandler ctl.TextChanged, AddressOf HandlePasswordChanged
        If (ctl.SecurePassword.Length > 0) Then
            NewPassword = ctl.SecurePassword
        End If

    End Sub

End Class
