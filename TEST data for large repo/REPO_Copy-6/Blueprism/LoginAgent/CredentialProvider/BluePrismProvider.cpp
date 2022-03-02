//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// BluePrismProvider implements ICredentialProvider, which is the main
// interface that logonUI uses to decide which tiles to display.
// A single tile is managed by this provider and it is not displayed
// on the Logon screen, since the credentials are passed to us by a
// Blue Prism process running on a resource in the System Account (rather
// than being entered manually).
// Logon requests and responses are communicated via a named pipe.
// 
// Note: Several instances of the Credential Providers can be created on a
// device since there can be multiple LogonUI processes (Remote Desktop
// connections etc.), however the Blue Prism Credential Provider only
// listens for requests when running in the Windows Console session.
//

#include <credentialprovider.h>
#include "BluePrismCredential.h"
#include <sstream>
#include <AccCtrl.h>
#include <Aclapi.h>

DWORD WINAPI ListenerThread(BluePrismProvider* that);

#define szPipeName L"\\\\.\\pipe\\BluePrismCredentialProviderPipe"

/////////////////////////////////////////////////////////////////////////////////////
// Blue Prism Credential Provider constructor and destructor
/////////////////////////////////////////////////////////////////////////////////////

// Constructor
BluePrismProvider::BluePrismProvider() :
_cRef(1)
{
    DllAddRef();

    // Initialise variables
    _pcpe = NULL;
    _pCredential = NULL;
    _haveCredentials = false;
}

// Destructor
BluePrismProvider::~BluePrismProvider()
{
    if (_threadCreated)
    {
        // Attempt to close the Listener thread if still running
        if (!CancelSynchronousIo(_hListenerThread))
        {
            if (GetLastError() != ERROR_NOT_FOUND)
                LogFail(L"Failed to cancel listener thread I/O: %d", GetLastError());
        }
        else
        {
            if (WaitForSingleObject(_hListenerThread, INFINITE) == WAIT_FAILED)
                LogFail(L"Wait for Listener Thread failed: %d", GetLastError());
        }
        CloseHandle(_hListenerThread);
        _hListenerThread = NULL;
        LogDebug(L"Listener Thread Completed"); 
    }
    if (_pCredential != NULL)
    {
        _pCredential->Release();
        _pCredential = NULL;
    }
    DllRelease();
}

/////////////////////////////////////////////////////////////////////////////////////
// Thread to listen for incoming requests
/////////////////////////////////////////////////////////////////////////////////////

// Main listener thread
DWORD WINAPI ListenerThread(
    BluePrismProvider* that)
{
    LogDebug(L"Listener Thread - starting");
    DWORD sid = 0;
    ProcessIdToSessionId(GetCurrentProcessId(), &sid);

    // Only create pipe if this is a console session
    if (sid == WTSGetActiveConsoleSessionId())
    {

        int retryCount = 0;
        bool success = false;

        while (retryCount < 20 && !success)
        {
            if (!that->CreatePipe())
            {
                LogFail(L"Failed to create named pipe. retrying.... - %d", GetLastError());
                Sleep(2000);
                retryCount++;
            }
            else
            {
                success = true;
            }
        }
        
        if (success)
        {
            that->WaitForRequest();
        }
        else
        {
            LogFail(L"Failed to create named pipe after 20 attempts %d", GetLastError());
        }
    }
    else
    {
        LogDebug(L"This appears to be a remote desktop session.");
    }

    LogDebug(L"Listener Thread - finishing");
    return 0;
}

/////////////////////////////////////////////////////////////////////////////////////
// Pipe handling methods
/////////////////////////////////////////////////////////////////////////////////////

EXPLICIT_ACCESS CreateExplicitAccessForWellKnownSid(ACCESS_MODE accessType, PSID pSid)
{
    EXPLICIT_ACCESS explicitAccess;

    ZeroMemory(&explicitAccess, sizeof(EXPLICIT_ACCESS));
    explicitAccess.grfAccessPermissions = SPECIFIC_RIGHTS_ALL | STANDARD_RIGHTS_ALL;
    explicitAccess.grfAccessMode = accessType;
    explicitAccess.grfInheritance = NO_INHERITANCE;
    explicitAccess.Trustee.TrusteeForm = TRUSTEE_IS_SID;
    explicitAccess.Trustee.TrusteeType = TRUSTEE_IS_WELL_KNOWN_GROUP;
    explicitAccess.Trustee.ptstrName = (LPWSTR)pSid;

    return explicitAccess;
}

// Initialise the pipe by setting things up locally and connecting to
// the server (host application). Returns true if successful, false
// otherwise
bool BluePrismProvider::CreatePipe()
{
    LogDebug(L"Creating pipe");

    DWORD resp = ERROR_SUCCESS;
    
    SID_IDENTIFIER_AUTHORITY networkSidAuthority = SECURITY_NT_AUTHORITY;
    PSID networkSid = NULL;
    if (!AllocateAndInitializeSid(&networkSidAuthority, 1, SECURITY_NETWORK_RID,
        0, 0, 0, 0, 0, 0, 0, &networkSid))
    {
        LogFail(L"AllocateAndInitializeSid failed: %d", GetLastError());
        return false;
    }
    EXPLICIT_ACCESS denyNetworkAccess =
        CreateExplicitAccessForWellKnownSid(DENY_ACCESS, networkSid);

    SID_IDENTIFIER_AUTHORITY worldSidAuthority = SECURITY_WORLD_SID_AUTHORITY;
    PSID everyoneSid = NULL;
    if (!AllocateAndInitializeSid(&worldSidAuthority, 1, SECURITY_WORLD_RID,
        0, 0, 0, 0, 0, 0, 0, &everyoneSid))
    {
        LogFail(L"AllocateAndInitializeSid failed: %d", GetLastError());
        return false;
    }
    EXPLICIT_ACCESS allowEveryoneAccess =
        CreateExplicitAccessForWellKnownSid(SET_ACCESS, everyoneSid);

    EXPLICIT_ACCESS explicitAccessArray[2] = 
     { denyNetworkAccess, allowEveryoneAccess };

    PACL acl = NULL;
    resp = SetEntriesInAcl(2, explicitAccessArray, NULL, &acl);
    if (resp != ERROR_SUCCESS)
    {
        LogFail(L"SetEntriesInAcl failed: %d", resp);
        return false;
    }

    PSECURITY_DESCRIPTOR sd = (PSECURITY_DESCRIPTOR)LocalAlloc(LPTR,
        SECURITY_DESCRIPTOR_MIN_LENGTH);
    if (!InitializeSecurityDescriptor(sd, SECURITY_DESCRIPTOR_REVISION))
    {
        LogFail(L"InitializeSecurityDescriptor failed: %d", GetLastError());
        return false;
    }

    if (!SetSecurityDescriptorDacl(sd, TRUE, acl, FALSE))
    {
        LogFail(L"SetSecurityDescriptorDacl failed: %d", GetLastError());
        return false;
    }

    SECURITY_ATTRIBUTES sa;
    sa.nLength = sizeof(SECURITY_ATTRIBUTES);
    sa.lpSecurityDescriptor = sd;
    sa.bInheritHandle = FALSE;

    // Create pipe for reading and writing
    _hPipe = CreateNamedPipe(szPipeName, PIPE_ACCESS_DUPLEX, PIPE_TYPE_MESSAGE, 1, 8192, 65536, 0, &sa);
    if (_hPipe == INVALID_HANDLE_VALUE)
    {
        LogFail(L"CreateNamedPipe failed: %d", GetLastError());
        return false;
    }

    if (FreeSid(networkSid) != NULL)
    {
        LogFail(L"FreeSid(networkSid) failed");
        return false;
    }

    if (FreeSid(everyoneSid) != NULL)
    {
        LogFail(L"FreeSid(everyoneSid) failed");
        return false;
    }

    if (LocalFree(sd) != NULL)
    {
        LogFail(L"LocalFree(sd) failed");
        return false;
    }

    if (LocalFree(acl) != NULL)
    {
        LogFail(L"LocalFree(acl) failed");
        return false;
    }
    return true;
}

// Thread that listens for and responds to incoming messages on the pipe.
void BluePrismProvider::WaitForRequest()
{
    LogDebug(L"Waiting for client connection");

    bool listen = true;
    while (listen)
    {
        // Wait for client to connect to other end of pipe
        _haveCredentials = false;
        if (!ConnectNamedPipe(_hPipe, NULL) && GetLastError() != ERROR_PIPE_CONNECTED)
        {
            LogFail(L"ConnectNamedPipe failed: %d", GetLastError());
            break;
        }

        const int buflen = 256;
        char buf[buflen];
        DWORD cbRead;

        // Read the request
        LogDebug(L"Client connected, reading request");
        if (!ReadFile(_hPipe, buf, buflen, &cbRead, NULL))
        {
            LogFail(L"Failed to read request: %d", GetLastError());
            this->SendReply(L"ERROR\n0\nReadFile failed\n", false);
        }
        else
        {
            *(buf + cbRead) = '\0';
            LogDebug(L"Client request received. Bytes read: %d", cbRead);

            // Action the request - only LOGON and PING are valid requests, anything else
            // is ignored.
            std::stringstream ss(buf);
            std::string request;
            std::getline(ss, request);
            if (strcmp(request.c_str(), "LOGON") == 0)
            {
                // If it's a request to logon then this will happen asynchronously and
                // will either succeed or the WinLogon process will terminate - either
                // way we won't be receiving any further requests so can close this thread.
                if (this->HandleLogonRequest(ss))
                {
                    break;
                }
            }
            else if (strcmp(request.c_str(), "PING") == 0)
            {
                // If it's a simple ping then reply and await next connection
                this->SendReply(L"OK\n", false);
            }
            else
            {
                LogDebug(L"Unknown request %s", request.c_str());
                this->SendReply(L"ERROR\n0\nUnknown request\n", false);
            

            }
        }

        // Clear the pipe ready for next connection
        FlushFileBuffers(_hPipe);
        DisconnectNamedPipe(_hPipe);
    }
}

bool BluePrismProvider::HandleLogonRequest(
    std::stringstream& ss)
{
    std::string domain;
    std::getline(ss, domain);
    // Get the length of the domain (including null terminator)
    int domainlen = 1 + (int)domain.size();
    PWSTR dname = new WCHAR[domainlen];
    MultiByteToWideChar(CP_UTF8, NULL, domain.c_str(), domainlen, dname, domainlen);

    std::string username;
    std::getline(ss, username);
    // Get the length of the username (including null terminator)
    int unamelen = 1 + (int)username.size();
    PWSTR uname = new WCHAR[unamelen];
    MultiByteToWideChar(CP_UTF8, NULL, username.c_str(), unamelen, uname, unamelen);

    std::string password;
    std::getline(ss, password);
    // Get the length of the password (including null terminator)
    int pwordlen = 1 + (int)password.size();
    PWSTR pword = new WCHAR[pwordlen];
    MultiByteToWideChar(CP_UTF8, NULL, password.c_str(), pwordlen, pword, pwordlen);
    LogDebug(L"Credentials received. Domain: %ls, Username: %ls, Password: ********", dname, uname);
     
    if (_pcpe == NULL)
    {
        this->SendReply(L"ERROR\n0\nLogonUI callbacks not disabled\n", false);
        return false;
    }

    // Populate credential with login data
    _pCredential->CopyLoginCredentials(dname, uname, pword);

    delete[] dname;
    delete[] uname;
    delete[] pword;

    // Instruct logonUI to re-enumerate credentials
    _haveCredentials = true;
    _pcpe->CredentialsChanged(_upAdviseContext);
    LogDebug(L"Credentials made available to Windows");
    return true;
}

void BluePrismProvider::SendReply(
    __in LPCTSTR reply,
    __in bool closePipe)
{
    // Write reply to pipe
    LogDebug(L"Sending reply to client: %s", reply);
    DWORD cbWritten;
    if (!WriteFile(_hPipe, reply, (lstrlen(reply) * sizeof(TCHAR)) + 1, &cbWritten, NULL))
    {
        LogFail(L"Failed to send reply: %d", GetLastError());
    }

    // Close pipe if required
    if (closePipe)
    {
        FlushFileBuffers(_hPipe);
        DisconnectNamedPipe(_hPipe);
        CloseHandle(_hPipe);
        _hPipe = NULL;
    }
    return;
}

// SetUsageScenario is the provider's cue that it's going to be asked for tiles
// in a subsequent call.
HRESULT BluePrismProvider::SetUsageScenario(
    __in CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
    __in DWORD dwFlags)
{
    UNREFERENCED_PARAMETER(dwFlags);
    HRESULT hr = S_OK;

    // Decide which scenarios to support here. Returning E_NOTIMPL simply tells the caller
    // that we're not designed for that scenario.
    switch (cpus)
    {
    case CPUS_LOGON:
    case CPUS_UNLOCK_WORKSTATION:
        LogDebug(L"Setting provider usage scenario: %s", (cpus == 1) ? L"Logon" : L"Unlock");
        _cpus = cpus;
        
        // Start listener thread for the named pipe
        DWORD dwThreadID;
        _hListenerThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)ListenerThread, this, 0, &dwThreadID);
        if (_hListenerThread == NULL)
        {
            LogFail(L"Failed to create listener thread: %d", GetLastError());
        }
        else
        {
            _threadCreated = true;
        }

        // Setup our hidden credential
        _pCredential = new BluePrismCredential(this);
        hr = _pCredential->Initialize(_cpus, s_rgCredProvFieldDescriptors, s_rgFieldStatePairs);
        LogDebug(L"BluePrismProvider instance created");

        break;

    case CPUS_CREDUI:
    case CPUS_CHANGE_PASSWORD:
        hr = E_NOTIMPL;
        break;

    default:
        hr = E_INVALIDARG;
        break;
    }

    return hr;
}

// SetSerialization takes the kind of buffer that you would normally return to LogonUI for
// an authentication attempt.  It's the opposite of ICredentialProviderCredential::GetSerialization.
// GetSerialization is implement by a credential and serializes that credential.  Instead,
// SetSerialization takes the serialization and uses it to create a tile.
//
// SetSerialization is called for two main scenarios.  The first scenario is in the credui case
// where it is prepopulating a tile with credentials that the user chose to store in the OS.
// The second situation is in a remote logon case where the remote client may wish to 
// prepopulate a tile with a username, or in some cases, completely populate the tile and
// use it to logon without showing any UI.
//
// If you wish to see an example of SetSerialization, please see either the SampleCredentialProvider
// sample or the SampleCredUICredentialProvider sample.  [The logonUI team says, "The original sample that
// this was built on top of didn't have SetSerialization.  And when we decided SetSerialization was
// important enough to have in the sample, it ended up being a non-trivial amount of work to integrate
// it into the main sample.  We felt it was more important to get these samples out to you quickly than to
// hold them in order to do the work to integrate the SetSerialization changes from SampleCredentialProvider 
// into this sample.]
HRESULT BluePrismProvider::SetSerialization(
    __in const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs)
{
    UNREFERENCED_PARAMETER(pcpcs);
    return E_NOTIMPL;
}

// Called by LogonUI to give you a callback. Providers often use the callback if they
// some event would cause them to need to change the set of tiles that they enumerated
HRESULT BluePrismProvider::Advise(
    __in ICredentialProviderEvents* pcpe,
    __in UINT_PTR upAdviseContext)
{
    LogDebug(L"Provider callback enabled");
    if (_pcpe != NULL)
    {
        _pcpe->Release();
    }
    _pcpe = pcpe;
    _pcpe->AddRef();
    _upAdviseContext = upAdviseContext;
    return S_OK;
}

// Called by LogonUI when the ICredentialProviderEvents callback is no longer valid.
HRESULT BluePrismProvider::UnAdvise()
{
    LogDebug(L"Provider callback disabled");
    if (_pcpe != NULL)
    {
        _pcpe->Release();
        _pcpe = NULL;
    }
    return S_OK;
}

// Called by LogonUI to determine the number of fields in your tiles.
HRESULT BluePrismProvider::GetFieldDescriptorCount(
    __out DWORD* pdwCount)
{
    *pdwCount = SFI_NUM_FIELDS;
    return S_OK;
}

// Gets the field descriptor for a particular field.
HRESULT BluePrismProvider::GetFieldDescriptorAt(
    __in DWORD dwIndex,
    __deref_out CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd)
{
    HRESULT hr;

    // Verify dwIndex is a valid field.
    if ((dwIndex < SFI_NUM_FIELDS) && ppcpfd)
    {
        hr = FieldDescriptorCoAllocCopy(s_rgCredProvFieldDescriptors[dwIndex], ppcpfd);
    }
    else
    {
        hr = E_INVALIDARG;
    }
    return hr;
}

// We only have a tile if we are handling a logon request
HRESULT BluePrismProvider::GetCredentialCount(
    __out DWORD* pdwCount,
    __out_range(< , *pdwCount) DWORD* pdwDefault,
    __out BOOL* pbAutoLogonWithDefault)
{
    if (_haveCredentials)
        *pdwCount = 1;
    else
        *pdwCount = 0;
    *pdwDefault = 0;
    *pbAutoLogonWithDefault = TRUE;
    LogDebug(L"Getting Credential Count: %d", *pdwCount);
    return S_OK;
}

// Returns the credential at the index specified by dwIndex. This function is called
// to enumerate the tiles.
HRESULT BluePrismProvider::GetCredentialAt(
    __in DWORD dwIndex,
    __deref_out ICredentialProviderCredential** ppcpc)
{
    LogDebug(L"Getting Credential At: %d", dwIndex);
    HRESULT hr;
    // Make sure the parameters are valid.
    if ((dwIndex == 0) && ppcpc)
    {
        hr = _pCredential->QueryInterface(IID_ICredentialProviderCredential, reinterpret_cast<void**>(ppcpc));
    }
    else
    {
        hr = E_INVALIDARG;
    }
    return hr;
}

// Boilerplate method to create an instance of our provider. 
HRESULT BluePrismProvider_CreateInstance(
    __in REFIID riid,
    __in void** ppv)
{
    Log::InitClass(L"BluePrismCredentialProvider");
    Log::Reset();

    HRESULT hr;

    BluePrismProvider* pProvider = new BluePrismProvider();

    if (pProvider)
    {
        hr = pProvider->QueryInterface(riid, ppv);
        pProvider->Release();
    }
    else
    {
        hr = E_OUTOFMEMORY;
    }
    return hr;
}


