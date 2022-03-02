Imports LocaleTools

''' <summary>
''' Encapsulates a license.
''' </summary>
Public NotInheritable Class LicenseInfo

    ''' <summary>
    ''' The set of keys represented by this instance.
    ''' </summary>
    Private mLicenseKeys As ICollection(Of KeyInfo)

    ''' <summary>
    ''' Creates a new LicenseInfo based around the given keys.
    ''' </summary>
    ''' <param name="keys">The keys which make up the compound licence represented
    ''' by this object. A null or empty collection effectively creates a default
    ''' license info - ie. containing a single licence, that being
    ''' the <see cref="KeyInfo.DefaultLicense">default licence</see>.</param>
    Private Sub New(keys As ICollection(Of KeyInfo))
        If keys Is Nothing OrElse keys.Count = 0 Then keys = {KeyInfo.DefaultLicense}
        mLicenseKeys = keys
    End Sub

    ''' <summary>
    ''' Cached value of the /license/transactionmodel node value
    ''' </summary>
    Private mTransactionModel As Nullable(Of Boolean) = Nothing

    ''' <summary>
    ''' Gets the first active key in this compound licence, or the default licence key if
    ''' there are no keys held in this licence.
    ''' </summary>
    ReadOnly Property FirstKey As KeyInfo
        Get
            Dim key = LicenseKeys.Where(Function(x) not x.Expired).FirstOrDefault()
            If key Is Nothing Then key = LicenseKeys.FirstOrDefault()
            Return If(key, KeyInfo.DefaultLicense)
        End Get
    End Property

    ''' <summary>
    ''' The type of license.
    ''' </summary>
    Public ReadOnly Property LicenseType() As LicenseTypes
        Get
            'If there are no active licences this will be the first in the database.
            Return FirstKey.LicenseType
        End Get
    End Property

    ''' <summary>
    ''' Create an LicenseInfo instance from a set of license keys.
    ''' </summary>
    ''' <param name="keys">The keys.</param>
    ''' <returns>The instance</returns>
    ''' <exception cref="Exception">If any errors occur while attempting to decrypt
    ''' or parse the license data.</exception>
    ''' <remarks>If given a null or empty list, this will return the
    ''' <see cref="DefaultLicense"/></remarks>
    Public Shared Function FromLicenseKeys(keys As ICollection(Of KeyInfo)) As LicenseInfo

        If keys Is Nothing OrElse keys.Count = 0 Then Return DefaultLicense

        Dim retval As LicenseInfo = LoadKeys(keys)

        If retval.LicenseType <> LicenseTypes.None Then
            For Each nd As KeyInfo In retval.LicenseKeys
                nd.Verify()
            Next
        End If
        Return retval
    End Function

    Public Shared Function LoadKeys(keys As ICollection(Of KeyInfo)) As LicenseInfo

        Dim retval As New LicenseInfo(keys)
        'Because the String conversion reads all the fields, we can use it to ensure
        'we're not somehow getting malformed data.
        Try
            Dim test As String = retval.ToString()
        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.LicenseInfo_TheLicenseDataIsInvalid)
        End Try
        Return retval
    End Function

    ''' <summary>
    ''' A default license, distributed with the product, that doesn't let you do
    ''' anything at all.
    ''' </summary>
    Friend Shared ReadOnly Property DefaultLicense() As LicenseInfo
        Get
            Return LoadKeys({KeyInfo.DefaultLicense})
        End Get
    End Property

    ''' <summary>
    ''' The license keys from which this license info was generated.
    ''' </summary>
    Public ReadOnly Property LicenseKeys() As IEnumerable(Of KeyInfo)
        Get
            Return mLicenseKeys
        End Get
    End Property

    ''' <summary>
    ''' The license keys which are currently in effect as described in this licence
    ''' </summary>
    Public ReadOnly Property EffectiveLicenseKeys As IEnumerable(Of KeyInfo)
        Get
            If Me.LicenseType = LicenseTypes.None Then
                Return {KeyInfo.DefaultLicense}
            End If
            Return mLicenseKeys.Where(Function(x) x.Effective)
        End Get
    End Property

    ''' <summary>
    ''' Gets a string representation of this license info.
    ''' </summary>
    ''' <returns>This object in string form</returns>
    Public Overrides Function ToString() As String
        Dim expiry = If(EffectiveLicenseKeys.Any(), ExpiryDate, Date.MinValue)
        Return String.Format(
            My.Resources.LicenseInfo_Type0Owner1StartDate2DExpiryDate3D,
            LicenseType, LicenseOwner, Date.MinValue, expiry)
    End Function

    ''' <summary>
    ''' Returns an informational (or warning) message about forthcoming changes to
    ''' the overall license entitlement. If there are no changes then an empty
    ''' string is returned.
    ''' </summary>
    ''' <param name="warning">True if the message is a warning</param>
    Public Function GetLicenseChangeMessage(ByRef warning As Boolean) As String
        ' Get a list of forthcoming license changes
        Dim changes As New LicenseChanges(LicenseKeys)
        If changes.Count = 0 Then Return String.Empty

        Dim daysToNext = changes.DaysToNextChange
        Dim daysToLast = changes.DaysToLastChange

        ' Return warning if all licenses are about to expire
        If changes.AllLicensesExpireSoon Then
            warning = True
            Return LTools.Format(My.Resources.LicenseInfo_plural_will_expire_within_days, "COUNT", daysToLast)
        End If

        ' Multiple pending changes, so return informational summary
        If changes.Count > 1 Then
            warning = False
            Return LTools.Format(My.Resources.LicenseInfo_plural_license_changes_will_occur_within, "COUNT", daysToLast, "NEXT", daysToNext)
        End If

        'Single license change pending
        Select Case changes.NextChangeType
            Case LicenseChangeEventType.Expires
                warning = True
                Return LTools.Format(My.Resources.LicenseInfo_plural_license_entitlement_decreased, "COUNT", daysToNext)
            Case LicenseChangeEventType.GracePeriodEnds
                warning = True
                If EffectiveLicenseKeys.First().Standalone = True Then
                    Return LTools.Format(My.Resources.LicenseInfo_plural_standalone_license_needs_activating, "COUNT", daysToNext)
                Else
                    Return LTools.Format(My.Resources.LicenseInfo_plural_license_entitlement_decreased, "COUNT", daysToNext)
                End If
            Case LicenseChangeEventType.RequiresActivation
                warning = True
                Return My.Resources.LicenseInfo_plural_license_requires_activation
            Case Else
                warning = False
                If changes.IsNextChangeStandalone = True Then
                    Return LTools.Format(My.Resources.LicenseInfo_plural_standalone_license_starts_soon, "COUNT", daysToNext)
                Else
                    Return LTools.Format(My.Resources.LicenseInfo_plural_license_entitlement_increased, "COUNT", daysToNext)
                End If
        End Select
    End Function

#Region "Aggregation"

    ''' <summary>
    ''' The date on which this license started to take effect. In effect, this is the
    ''' earliest start date of any of the licences held within this license info.
    ''' </summary>
    Public ReadOnly Property StartDate As Date
        Get
            Return EffectiveLicenseKeys.Min(Function(k) k.StartDate)
        End Get
    End Property

    ''' <summary>
    ''' The date at which this license expires. In effect, this is the latest expiry
    ''' date of any of the licences held within this license info.
    ''' </summary>
    Public ReadOnly Property ExpiryDate() As Date
        Get
            Return EffectiveLicenseKeys.Max(Function(k) k.ExpiryDate)
        End Get
    End Property

    ''' <summary>
    ''' Get an Icon to override the product icon with, if the license specifies one.
    ''' </summary>
    ''' <returns>The Base64-encoded icon to use, or Nothing if the license doesn't
    ''' override it.</returns>
    ''' <remarks>This is attempting to aggregate over all the installed licenses. It
    ''' basically returns the latest active override it finds, if any.</remarks>
    Public ReadOnly Property OverrideIcon() As String
        Get
            Return (
                From k In EffectiveLicenseKeys
                Where k.OverrideIcon IsNot Nothing
                Order By (k.SetAt) Descending
                Select (k.OverrideIcon)
            ).FirstOrDefault()
        End Get
    End Property

    ''' <summary>
    ''' Get an Image to override the large product logo with, if the license
    ''' specifies one.
    ''' </summary>
    ''' <returns>The Base64-encoded image to use, or Nothing if the license doesn't
    ''' override it.</returns>
    ''' <remarks>This is attempting to aggregate over all the installed licenses. It
    ''' basically returns the latest active override it finds, if any.</remarks>
    Public ReadOnly Property OverrideLargeLogo() As String
        Get
            Return (
                From k In EffectiveLicenseKeys
                Where k.OverrideLargeLogo IsNot Nothing
                Order By (k.SetAt) Descending
                Select (k.OverrideLargeLogo)
            ).FirstOrDefault()
        End Get
    End Property

    ''' <summary>
    ''' Get some text to override the product title bar with, if the license
    ''' specifies one.
    ''' </summary>
    ''' <returns>The text to use, or Nothing if the license doesn't override it.
    ''' </returns>
    ''' <remarks>This is attempting to aggregate over all the installed licenses. It
    ''' basically returns the latest active override it finds, if any.</remarks>
    Public ReadOnly Property OverrideTitle() As String
        Get
            Return (
                From k In EffectiveLicenseKeys
                Where k.OverrideTitle IsNot Nothing
                Order By (k.SetAt) Descending
                Select (k.OverrideTitle)
            ).FirstOrDefault()
        End Get
    End Property

    ''' <summary>
    ''' Name of the recipient of this license.
    ''' </summary>
    ''' <remarks>This is attempting to aggregate over all the installed licenses. It
    ''' basically returns the latest active owner it finds, if any.</remarks>
    Public ReadOnly Property LicenseOwner() As String
        Get
            Return (
                From k In EffectiveLicenseKeys
                Where k.LicenseType <> LicenseTypes.None
                Order By (k.SetAt) Descending
                Select (k.LicenseOwner)
            ).FirstOrDefault()
        End Get
    End Property

    ''' <summary>
    ''' Property to indicate whether this installation uses the transaction licence
    ''' model (i.e. billed according to work queue activity).
    ''' </summary>
    ''' <remarks>Aggregating this doesn't seem to make a lot of sense, but it has to
    ''' be done. If any of the licenses specify transaction model, this says yes.
    ''' </remarks>
    Public ReadOnly Property TransactionModel() As Boolean
        Get
            Return EffectiveLicenseKeys.Any(Function(k) k.TransactionModel)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this set of licences allows unlimited sessions.
    ''' </summary>
    Public ReadOnly Property AllowsUnlimitedSessions() As Boolean
        Get
            Return EffectiveLicenseKeys.Any(Function(k) k.AllowsUnlimitedSessions)
        End Get
    End Property

    ''' <summary>
    ''' The number of concurrent sessions that this set of licenses permits
    ''' to be run.
    ''' </summary>
    Public ReadOnly Property NumConcurrentSessions() As Integer
        Get
            Dim num As Integer
            For Each key In EffectiveLicenseKeys
                If key.AllowsUnlimitedSessions Then Return Integer.MaxValue
                num += key.NumConcurrentSessions
            Next
            Return num
        End Get
    End Property

    ''' <summary>
    ''' Gets the label to use for the number of concurrent sessions allowed by this
    ''' set of licenses.
    ''' </summary>
    Public ReadOnly Property NumConcurrentSessionsLabel() As String
        Get
            Return KeyInfo.GetLabel(AllowsUnlimitedSessions, NumConcurrentSessions)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this licence allows unlimited resources
    ''' </summary>
    Public ReadOnly Property AllowsUnlimitedResourcePCs() As Boolean
        Get
            Return EffectiveLicenseKeys.Any(Function(k) k.AllowsUnlimitedResourcePCs)
        End Get
    End Property

    ''' <summary>
    ''' The number of (unretired) resource PCs that this license permits
    ''' to be registered.
    ''' </summary>
    Public ReadOnly Property NumResourcePCs() As Integer
        Get
            Dim num As Integer
            For Each key In EffectiveLicenseKeys
                If key.AllowsUnlimitedResourcePCs Then Return Integer.MaxValue
                num += key.NumResourcePCs
            Next
            Return num
        End Get
    End Property

    ''' <summary>
    ''' Gets the label to use for the number of resources allowed by this set of
    ''' licenses.
    ''' </summary>
    Public ReadOnly Property NumResourcePCsLabel() As String
        Get
            Return KeyInfo.GetLabel(AllowsUnlimitedResourcePCs, NumResourcePCs)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this set of licences allows an unlimited number of process alerts
    ''' apps running.
    ''' </summary>
    Public ReadOnly Property AllowsUnlimitedProcessAlertsPCs() As Boolean
        Get
            Return EffectiveLicenseKeys.Any(
                Function(k) k.AllowsUnlimitedProcessAlertsPCs)
        End Get
    End Property

    ''' <summary>
    ''' The number of machines on which this set of licenses permits process alerts
    ''' to be run.
    ''' </summary>
    Public ReadOnly Property NumProcessAlertsPCs() As Integer
        Get
            Dim num As Integer
            For Each key In EffectiveLicenseKeys
                If key.AllowsUnlimitedProcessAlertsPCs Then Return Integer.MaxValue
                num += key.NumProcessAlertsPCs
            Next
            Return num
        End Get
    End Property

    ''' <summary>
    ''' Gets the label to use for the number of process alert PCs allowed by this
    ''' set of licenses.
    ''' </summary>
    Public ReadOnly Property NumProcessAlertsPCsLabel As String
        Get
            Return KeyInfo.GetLabel(AllowsUnlimitedProcessAlertsPCs, NumProcessAlertsPCs)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this licence allows unlimited published processes.
    ''' </summary>
    Public ReadOnly Property AllowsUnlimitedPublishedProcesses() As Boolean
        Get
            Return EffectiveLicenseKeys.Any(
                Function(k) k.AllowsUnlimitedPublishedProcesses)
        End Get
    End Property

    ''' <summary>
    ''' The number of processes that this license permits to be published.
    ''' </summary>
    Public ReadOnly Property NumPublishedProcesses() As Integer
        Get
            Dim num As Integer
            For Each key In EffectiveLicenseKeys
                If key.AllowsUnlimitedPublishedProcesses Then Return Integer.MaxValue
                num += key.NumPublishedProcesses
            Next
            Return num
        End Get
    End Property

    ''' <summary>
    ''' Gets the label to use for the number of published processes allowed by this
    ''' set of licenses.
    ''' </summary>
    Public ReadOnly Property NumPublishedProcessesLabel As String
        Get
            Return KeyInfo.GetLabel(AllowsUnlimitedPublishedProcesses, NumPublishedProcesses)
        End Get
    End Property

#End Region

#Region "Permission Checks"

    ''' <summary>
    ''' Checks if the current licence allows usage of a particular aspect of the
    ''' system. This is a broader check than checking allowed numbers.
    ''' </summary>
    ''' <param name="use">The usage to check to see if the current licence permits.
    ''' </param>
    ''' <returns>True if the current licence permits the use of the specified aspect
    ''' of the application; False otherwise.</returns>
    ''' <remarks>In practical terms, this checks the <see cref="LicenseType"/> to see
    ''' if the licence allows access to the required part of the app. Generally,
    ''' <see cref="LicenseTypes.Enterprise">enterprise licences</see> allow access to
    ''' the whole application, anything else (default licence,
    ''' <see cref="LicenseTypes.NHS">NHS licences</see>) may have some limitations.
    ''' </remarks>
    Public Function CanUse(use As LicenseUse) As Boolean
        If Not IsLicensed Then Return False

        If LicenseType = LicenseTypes.Education AndAlso use = LicenseUse.BPServer Then Return False

        Return LicenseType <> LicenseTypes.NHS AndAlso LicenseType <> LicenseTypes.None
    End Function

    ''' <summary>
    ''' Checks if a licence of any kind is actually installed in this runtime.
    ''' </summary>
    Public ReadOnly Property IsLicensed() As Boolean
        Get
            Dim first = EffectiveLicenseKeys.FirstOrDefault
            If first IsNot Nothing AndAlso first.LicenseType <> LicenseTypes.None Then
                Return True
            End If
            Return False
        End Get
    End Property

#End Region

End Class

