//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// BluePrismCredential is our implementation of ICredentialProviderCredential.
// ICredentialProviderCredential is what LogonUI uses to let a credential
// provider specify what a user tile looks like and then tell it what the
// user has entered into the tile.  ICredentialProviderCredential is also
// responsible for packaging up the users credentials into a buffer that
// LogonUI then sends on to LSA.
//
// Since the Blue Prism Credential is not visible, this just really holds
// the logon information (domain, username & password) for passing to LSA

#ifndef WIN32_NO_STATUS
#include <ntstatus.h>
#define WIN32_NO_STATUS
#endif
#include <unknwn.h>
#include "BluePrismCredential.h"
#include "guid.h"

/////////////////////////////////////////////////////////////////////////////////////
// Blue Prism Credential constructor and destructor
/////////////////////////////////////////////////////////////////////////////////////

// Constructor
BluePrismCredential::BluePrismCredential(
    __in ICredentialProvider* bpprov) :
_cRef(1),
_pCredProvCredentialEvents(NULL)
{
    DllAddRef();

    ZeroMemory(_rgCredProvFieldDescriptors, sizeof(_rgCredProvFieldDescriptors));
    ZeroMemory(_rgFieldStatePairs, sizeof(_rgFieldStatePairs));
    ZeroMemory(_rgFieldStrings, sizeof(_rgFieldStrings));
    _pBPProvider = bpprov;

    LogDebug(L"BluePrismCredential instance created");
}

// Destructor
BluePrismCredential::~BluePrismCredential()
{
    this->ClearPassword();
    for (int i = 0; i < ARRAYSIZE(_rgFieldStrings); i++)
    {
        CoTaskMemFree(_rgFieldStrings[i]);
        CoTaskMemFree(_rgCredProvFieldDescriptors[i].pszLabel);
    }

    DllRelease();
    LogDebug(L"BluePrismCredential instance destroyed");
}

// Zeroes the memory used by the password - note that this does
// not free the memory. That must be done by the caller.
void BluePrismCredential::ClearPassword()
{
    if (_rgFieldStrings[SFI_PASSWORD])
    {
        size_t lenPassword = lstrlen(_rgFieldStrings[SFI_PASSWORD]);
        SecureZeroMemory(_rgFieldStrings[SFI_PASSWORD], lenPassword * sizeof(*_rgFieldStrings[SFI_PASSWORD]));
    }
}

// Initializes one credential with the field information passed in.
// Set the value of the SFI_USERNAME field to pwzUsername.
HRESULT BluePrismCredential::Initialize(
    __in CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
    __in const CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR* rgcpfd,
    __in const FIELD_STATE_PAIR* rgfsp)
{
    LogDebug(L"Initializing Blue Prism credential: %d", cpus);
    HRESULT hr = S_OK;

    _cpus = cpus;

    // Copy the field descriptors for each field. This is useful if you want to vary the field
    // descriptors based on what Usage scenario the credential was created for.
    for (DWORD i = 0; SUCCEEDED(hr) && i < ARRAYSIZE(_rgCredProvFieldDescriptors); i++)
    {
        _rgFieldStatePairs[i] = rgfsp[i];
        hr = FieldDescriptorCopy(rgcpfd[i], &_rgCredProvFieldDescriptors[i]);
    }

    // Initialize the String value of all the fields.
    if (SUCCEEDED(hr))
    {
        hr = SHStrDupW(L"", &_rgFieldStrings[SFI_DOMAIN]);
    }
    if (SUCCEEDED(hr))
    {
        hr = SHStrDupW(L"", &_rgFieldStrings[SFI_USERNAME]);
    }
    if (SUCCEEDED(hr))
    {
        hr = SHStrDupW(L"", &_rgFieldStrings[SFI_PASSWORD]);
    }
    if (SUCCEEDED(hr))
    {
        hr = SHStrDupW(L"Submit", &_rgFieldStrings[SFI_SUBMIT_BUTTON]);
    }

    return hr;
}

// Sets the login credentials for this credential object
HRESULT BluePrismCredential::CopyLoginCredentials(
    __in PWSTR domain,
    __in PWSTR username,
    __in PWSTR password)
{
    LogDebug(L"Setting login information into credential");
    HRESULT hr = S_OK;

    if (SUCCEEDED(hr))
    {
        LogDebug(L"Setting Domain: %s", domain);
        CoTaskMemFree(_rgFieldStrings[SFI_DOMAIN]);
        hr = SHStrDupW(domain, &_rgFieldStrings[SFI_DOMAIN]);
    }
    if (SUCCEEDED(hr))
    {
        LogDebug(L"Setting Username: %s", username);
        CoTaskMemFree(_rgFieldStrings[SFI_USERNAME]);
        hr = SHStrDupW(username, &_rgFieldStrings[SFI_USERNAME]);
    }
    if (SUCCEEDED(hr))
    {
        LogDebug(L"Setting Password: ********");
        ClearPassword();
        CoTaskMemFree(_rgFieldStrings[SFI_PASSWORD]);
        hr = SHStrDupW(password, &_rgFieldStrings[SFI_PASSWORD]);
    }
    return hr;
}

// LogonUI calls this in order to give us a callback in case we need to notify it of anything.
HRESULT BluePrismCredential::Advise(
    __in ICredentialProviderCredentialEvents* pcpce)
{
    LogTrace(L"Credential callback enabled");
    if (_pCredProvCredentialEvents != NULL)
    {
        _pCredProvCredentialEvents->Release();
    }
    _pCredProvCredentialEvents = pcpce;
    _pCredProvCredentialEvents->AddRef();
    LogTrace(L"Getting window handle");
    
    LogTrace(L"Finished Getting window handle");
    return S_OK;
}

// LogonUI calls this to tell us to release the callback.
HRESULT BluePrismCredential::UnAdvise()
{
    LogTrace(L"Credential callback disabled");
    if (_pCredProvCredentialEvents)
    {
        _pCredProvCredentialEvents->Release();
    }
    _pCredProvCredentialEvents = NULL;
    return S_OK;
}

// LogonUI calls this function when our tile is selected (zoomed)
// If you simply want fields to show/hide based on the selected state,
// there's no need to do anything here - you can set that up in the 
// field definitions.  But if you want to do something
// more complicated, like change the contents of a field when the tile is
// selected, you would do it here.
HRESULT BluePrismCredential::SetSelected(
    __out BOOL* pbAutoLogon)
{
    LogTrace(L"BluePrismCredential::SetSelected()");
    UNREFERENCED_PARAMETER(pbAutoLogon);
    return S_OK;
}

// Similarly to SetSelected, LogonUI calls this when your tile was selected
// and now no longer is.
HRESULT BluePrismCredential::SetDeselected()
{
    LogTrace(L"BluePrismCredential::SetDeselected()");
    return S_OK;
}

// Get info for a particular field of a tile. Called by logonUI to get information to 
// display the tile.
HRESULT BluePrismCredential::GetFieldState(
    __in DWORD dwFieldID,
    __in CREDENTIAL_PROVIDER_FIELD_STATE* pcpfs,
    __in CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE* pcpfis)
{
    LogTrace(L"BluePrismCredential::GetFieldState()");
    HRESULT hr;

    if (dwFieldID < ARRAYSIZE(_rgFieldStatePairs) && pcpfs && pcpfis)
    {
        *pcpfis = _rgFieldStatePairs[dwFieldID].cpfis;
        *pcpfs = _rgFieldStatePairs[dwFieldID].cpfs;

        hr = S_OK;
    }
    else
    {
        hr = E_INVALIDARG;
    }
    return hr;
}

// Sets ppwsz to the string value of the field at the index dwFieldID.
HRESULT BluePrismCredential::GetStringValue(
    __in DWORD dwFieldID,
    __deref_out PWSTR* ppwsz)
{
    LogTrace(L"BluePrismCredential::GetStringValue()");
    HRESULT hr;

    // Check to make sure dwFieldID is a legitimate index.
    if (dwFieldID < ARRAYSIZE(_rgCredProvFieldDescriptors) && ppwsz)
    {
        // Make a copy of the string and return that. The caller
        // is responsible for freeing it.
        hr = SHStrDupW(_rgFieldStrings[dwFieldID], ppwsz);
    }
    else
    {
        hr = E_INVALIDARG;
    }

    return hr;
}

// Get the image to show in the user tile.
HRESULT BluePrismCredential::GetBitmapValue(
    __in DWORD dwFieldID,
    __out HBITMAP* phbmp)
{
    LogTrace(L"BluePrismCredential::GetBitmapValue()");
    UNREFERENCED_PARAMETER(dwFieldID);
    UNREFERENCED_PARAMETER(phbmp);
    return S_OK;
}

// Sets pdwAdjacentTo to the index of the field the submit button should be 
// adjacent to. We recommend that the submit button is placed next to the last
// field which the user is required to enter information in. Optional fields
// should be below the submit button.
HRESULT BluePrismCredential::GetSubmitButtonValue(
    __in DWORD dwFieldID,
    __out DWORD* pdwAdjacentTo
    )
{
    LogTrace(L"BluePrismCredential::GetSubmitButtonValue()");
    HRESULT hr;

    if (SFI_SUBMIT_BUTTON == dwFieldID && pdwAdjacentTo)
    {
        // pdwAdjacentTo is a pointer to the fieldID you want the submit button to 
        // appear next to.
        *pdwAdjacentTo = SFI_PASSWORD;
        hr = S_OK;
    }
    else
    {
        hr = E_INVALIDARG;
    }
    return hr;
}

// Sets the value of a field which can accept a string as a value.
// This is called on each keystroke when a user types into an edit field
HRESULT BluePrismCredential::SetStringValue(
    __in DWORD dwFieldID,
    __in PCWSTR pwz)
{
    LogTrace(L"BluePrismCredential::SetStringValue()");
    UNREFERENCED_PARAMETER(dwFieldID);
    UNREFERENCED_PARAMETER(pwz);
    return E_NOTIMPL;
}

//------------- 
// The following methods are for logonUI to get the values of various UI elements and then communicate
// to the credential about what the user did in that field.  However, these methods are not implemented
// because our tile doesn't contain these types of UI elements
HRESULT BluePrismCredential::GetCheckboxValue(
    __in DWORD dwFieldID,
    __out BOOL* pbChecked,
    __deref_out PWSTR* ppwszLabel)
{
    UNREFERENCED_PARAMETER(dwFieldID);
    UNREFERENCED_PARAMETER(pbChecked);
    UNREFERENCED_PARAMETER(ppwszLabel);
    return E_NOTIMPL;
}

HRESULT BluePrismCredential::GetComboBoxValueCount(
    __in DWORD dwFieldID,
    __out DWORD* pcItems,
    __out_range(< , *pcItems) DWORD* pdwSelectedItem)
{
    UNREFERENCED_PARAMETER(dwFieldID);
    UNREFERENCED_PARAMETER(pcItems);
    UNREFERENCED_PARAMETER(pdwSelectedItem);
    return E_NOTIMPL;
}

HRESULT BluePrismCredential::GetComboBoxValueAt(
    __in DWORD dwFieldID,
    __in DWORD dwItem,
    __deref_out PWSTR* ppwszItem)
{
    UNREFERENCED_PARAMETER(dwFieldID);
    UNREFERENCED_PARAMETER(dwItem);
    UNREFERENCED_PARAMETER(ppwszItem);
    return E_NOTIMPL;
}

HRESULT BluePrismCredential::SetCheckboxValue(
    __in DWORD dwFieldID,
    __in BOOL bChecked)
{
    UNREFERENCED_PARAMETER(dwFieldID);
    UNREFERENCED_PARAMETER(bChecked);
    return E_NOTIMPL;
}

HRESULT BluePrismCredential::SetComboBoxSelectedValue(
    __in DWORD dwFieldId,
    __in DWORD dwSelectedItem)
{
    UNREFERENCED_PARAMETER(dwFieldId);
    UNREFERENCED_PARAMETER(dwSelectedItem);
    return E_NOTIMPL;
}

HRESULT BluePrismCredential::CommandLinkClicked(__in DWORD dwFieldID)
{
    UNREFERENCED_PARAMETER(dwFieldID);
    return E_NOTIMPL;
}
//------ end of methods for controls we don't have in our tile ----//

// Collect the username and password into a serialized credential for the correct usage scenario 
// (logon/unlock is what's demonstrated in this sample).  LogonUI then passes these credentials 
// back to the system to log on.
HRESULT BluePrismCredential::GetSerialization(
    __out CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE* pcpgsr,
    __out CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs,
    __deref_out_opt PWSTR* ppwszOptionalStatusText,
    __out CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon)
{
    LogTrace(L"Serializing Credential");
    UNREFERENCED_PARAMETER(ppwszOptionalStatusText);
    UNREFERENCED_PARAMETER(pcpsiOptionalStatusIcon);

    KERB_INTERACTIVE_LOGON kil;
    ZeroMemory(&kil, sizeof(kil));

    HRESULT hr;

    PWSTR pwzProtectedPassword;

    LogDebug(L"Serializing Domain: %s", _rgFieldStrings[SFI_DOMAIN]);
    LogDebug(L"Serializing Username: %s", _rgFieldStrings[SFI_USERNAME]);
    LogDebug(L"Serializing Password: ********");

    // If we don't have a username set, we have no credential
    if (!_rgFieldStrings[SFI_USERNAME])
    {
        *pcpgsr = CPGSR_NO_CREDENTIAL_FINISHED;
        return S_OK;
    }

    hr = ProtectIfNecessaryAndCopyPassword(_rgFieldStrings[SFI_PASSWORD], _cpus, &pwzProtectedPassword);

    if (!SUCCEEDED(hr))
    {
        LogFail(L"Failed to call ProtectIfNecessaryAndCopyPassword: %d", hr);
    }
    else
    {
        KERB_INTERACTIVE_UNLOCK_LOGON kiul;

        // Initialize kiul with weak references to our credential.
        hr = KerbInteractiveUnlockLogonInit(_rgFieldStrings[SFI_DOMAIN], _rgFieldStrings[SFI_USERNAME], pwzProtectedPassword, _cpus, &kiul);

        if (!SUCCEEDED(hr))
        {
            LogFail(L"Failed to call KerbInteractiveUnlockLogonInit: %d", hr);
        }
        else
        {
            // We use KERB_INTERACTIVE_UNLOCK_LOGON in both unlock and logon scenarios.  It contains a
            // KERB_INTERACTIVE_LOGON to hold the creds plus a LUID that is filled in for us by Winlogon
            // as necessary.
            hr = KerbInteractiveUnlockLogonPack(kiul, &pcpcs->rgbSerialization, &pcpcs->cbSerialization);

            if (!SUCCEEDED(hr))
            {
                LogFail(L"Failed to call KerbInteractiveUnlockLogonPack: %d", hr);
            }
            else
            {
                ULONG ulAuthPackage;
                hr = RetrieveNegotiateAuthPackage(&ulAuthPackage);
                if (!SUCCEEDED(hr))
                {
                    LogFail(L"Failed to call RetrieveNegotiateAuthPackage: %d", hr);
                }
                else
                {
                    pcpcs->ulAuthenticationPackage = ulAuthPackage;
                    pcpcs->clsidCredentialProvider = CLSID_BluePrismProvider;

                    // At this point the credential has created the serialized credential used for logon
                    // By setting this to CPGSR_RETURN_CREDENTIAL_FINISHED we are letting logonUI know
                    // that we have all the information we need and it should attempt to submit the 
                    // serialized credential.
                    *pcpgsr = CPGSR_RETURN_CREDENTIAL_FINISHED;

                    // We also want to clear the memory used for the field strings
                    CoTaskMemFree(_rgFieldStrings[SFI_DOMAIN]);
                    _rgFieldStrings[SFI_DOMAIN] = NULL;
                    CoTaskMemFree(_rgFieldStrings[SFI_USERNAME]);
                    _rgFieldStrings[SFI_USERNAME] = NULL;
                    CoTaskMemFree(_rgFieldStrings[SFI_PASSWORD]);
                    _rgFieldStrings[SFI_PASSWORD] = NULL;
                }
            }
        }

        CoTaskMemFree(pwzProtectedPassword);
    }

    return hr;
}

HRESULT BluePrismCredential::ReportResult(
    __in NTSTATUS ntsStatus,
    __in NTSTATUS ntsSubstatus,
    __deref_out_opt PWSTR* ppwszOptionalStatusText,
    __out CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon)
{
    LogDebug(L"Reporting Logon Result");

    *ppwszOptionalStatusText = NULL;
    *pcpsiOptionalStatusIcon = CPSI_NONE;
    
    LogDebug(L"ntStatus : %d", ntsStatus);
    LogDebug(L"ntSubstatus : %d", ntsSubstatus);

    BluePrismProvider* bpp = dynamic_cast<BluePrismProvider*>(_pBPProvider);
    if (SUCCEEDED(HRESULT_FROM_NT(ntsStatus)))
    {
        // If logon was successful then pass a success message back to the Provider
        bpp->SendReply(L"OK", true);
        return S_OK;
    }
    else
    {
        // If logon was not successful then return error message and exit this WinLogon
        // process (so a new WinLogon process is available for subsequent attempts)
        bpp->SendReply(GetReply(ntsStatus), true);
        exit(0);
    }
}

LPCTSTR BluePrismCredential::GetReply(
    __in NTSTATUS ntsStatus)
{
    LPVOID lpDisplayBuffer;
    LPVOID lpMessageBuffer;
    HMODULE Hand = LoadLibrary(L"ntdll.dll");
    if (Hand == NULL) LogFail(L"LoadLibrary failed with 0x%x\n", GetLastError());

    // Get description for the error code
    if (!FormatMessage(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_FROM_HMODULE,
        Hand, ntsStatus, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR)&lpMessageBuffer, 0, NULL))
        LogFail(L"Format message failed with 0x%x\n", GetLastError());

    lpDisplayBuffer = (LPVOID)LocalAlloc(LMEM_ZEROINIT,
        (lstrlen((LPCTSTR)lpMessageBuffer) + 40) * sizeof(TCHAR));

    StringCchPrintf((LPTSTR)lpDisplayBuffer,
        LocalSize(lpDisplayBuffer) / sizeof(TCHAR),
        TEXT("ERROR\n0x%x\n%s\n"),
        ntsStatus, lpMessageBuffer);

    // Free the buffer allocated by the system.
    LocalFree(lpMessageBuffer);
    FreeLibrary(Hand);

    return (LPCTSTR)lpDisplayBuffer;
}

