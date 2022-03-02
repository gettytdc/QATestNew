//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

#pragma once

#include <credentialprovider.h>
#include <windows.h>
#include <strsafe.h>

#pragma warning(disable: 4995)
#include <iostream>
#include <fstream>

#include "CommandWindow.h"
#include "BluePrismCredential.h"
#include "helpers.h"
//#include "logging.h"
#include <Windows.h>
#include <string>

#define VERSION_STRING L"Blue Prism Login Agent 2.0.0"

using namespace std;

// Forward references for classes used here.
class CCommandWindow;
class BluePrismCredential;
class CMessageCredential;

class BluePrismProvider : public ICredentialProvider
{
public:
	// IUnknown
	IFACEMETHODIMP_(ULONG) AddRef()
	{
		return ++_cRef;
	}

	IFACEMETHODIMP_(ULONG) Release()
	{
		LONG cRef = --_cRef;
		if (!cRef)
		{
			delete this;
		}
		return cRef;
	}

	IFACEMETHODIMP QueryInterface(
		__in REFIID riid,
		__deref_out void** ppv)
	{
		static const QITAB qit[] =
		{
			QITABENT(BluePrismProvider, ICredentialProvider), // IID_ICredentialProvider
			{ 0 },
		};
		return QISearch(this, qit, riid, ppv);
	}

public:
	// ICredentialProvider
	IFACEMETHODIMP SetUsageScenario(
		__in CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
		__in DWORD dwFlags);

	IFACEMETHODIMP SetSerialization(
		__in const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs);

	IFACEMETHODIMP Advise(
		__in ICredentialProviderEvents* pcpe,
		__in UINT_PTR upAdviseContext);

	IFACEMETHODIMP UnAdvise();

	IFACEMETHODIMP GetFieldDescriptorCount(
		__out DWORD* pdwCount);

	IFACEMETHODIMP GetFieldDescriptorAt(
		__in DWORD dwIndex,
		__deref_out CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd);

	IFACEMETHODIMP GetCredentialCount(
		__out DWORD* pdwCount,
		__out_range(< , *pdwCount) DWORD* pdwDefault,
		__out BOOL* pbAutoLogonWithDefault);

	IFACEMETHODIMP GetCredentialAt(
		__in DWORD dwIndex,
		__deref_out ICredentialProviderCredential** ppcpc);

    
public:
	// BluePrismCredentialProvider
	friend HRESULT BluePrismProvider_CreateInstance(
		__in REFIID riid,
		__deref_out void** ppv);

	bool CreatePipe();

	void WaitForRequest();

    bool HandleLogonRequest(
		std::stringstream& ss);

	void SendReply(
		__in LPCTSTR reply,
		__in bool closePipe);

protected:
	BluePrismProvider();

	__override ~BluePrismProvider();

private:
    bool SendSASSignal();
	void PipeSendMessage(const char* msg);

	LONG _cRef;									// Reference counter.
	BluePrismCredential *_pCredential;			// Our hidden credential.
	ICredentialProviderEvents *_pcpe;			// Used to tell our owner to re-enumerate credentials.
	UINT_PTR _upAdviseContext;					// Used to tell our owner who we are when asking to re-enumerate credentials.
	CREDENTIAL_PROVIDER_USAGE_SCENARIO _cpus;	// The scenario the credential provider has been created in
	HANDLE _hPipe;								// The named pipe used to communicate with the Login Process
	HANDLE _hListenerThread=NULL;				// Thread listening for requests on the pipe
	bool _haveCredentials;						// Indicates that we have been passed some credentials
	bool _threadCreated = false;
};
