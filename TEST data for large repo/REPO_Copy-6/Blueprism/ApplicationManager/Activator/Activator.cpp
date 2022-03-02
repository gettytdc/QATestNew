#include "Activator.h"

// Function which we copy into remote process memory and then
// execute. Gets function pointer and paramters out of a INJFUNCTSTRUCT
static LRESULT DoSetWindowForeground (INJFUNCTSTRUCT *pFuncStruct)
{
    return pFuncStruct->pfnSetFGWindow( pFuncStruct->hwnd);
}

// Dummy function to denote end position of the previous function
// By observing the offset between the two functions we can calculate
// the size of the first function, in bytes
static int NextFunction (void)
{
    //Need some dummy instructions that can't be optimised away
    __nop(); // intrinsic, since x64 doesn't support asm blocks
    return 1;
}

static int wrapForceWindowActive(HWND hwnd)
{
    try
    {
        ForceWindowActive(hwnd);
        return 0;
    }
    catch (const std::string &errmsg)
    {
        GeneralError();
        return 1;
    }
    catch (...) // any other errors - what else can we do, it's an EXE
    {
        GeneralError();
        return 2;
    }
}

int activator_main(int argc, _TCHAR* argv[])
{
     // argc == 2 - ie. program name + 1 param
    if (argc != 2 || (argv[1][0] == '/' && argv[1][1] == '?'))
    {
        _tprintf(_T(
            "Usage: %s {hwnd}\n"), argv[0]
        );
        _tprintf(
            _T("Forces the window with the given handle to be activated.\n")
            _T("The hwnd can be in hex format (beginning with 0x) or in decimal.\n")
        );

        return 0;
    }

    // The only parameter should be a window handle
    TCHAR *strHwnd = argv[1];
    HWND hwnd;
    // If the string starts with 0x, treat it as hex
    if (strHwnd[0] == '0' && (strHwnd[1] == 'x' || strHwnd[1] == 'X'))
    {
        TCHAR *endOfNum = NULL; // we don't really need where the number ends.
        hwnd = (HWND) _tcstoi64(strHwnd + 2, &endOfNum, 16);
    }
    else // Otherwise, assume that it's decimal
    {
        hwnd = (HWND) _ttoi64(strHwnd);
    }

    __try
    {
        return wrapForceWindowActive(hwnd);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        GeneralError();
        return 3;
    }
}

void PrintDebug(TCHAR *str)
{
    // _ftprintf(stderr, str);
    OutputDebugString(str);
}

void GeneralError()
{
    DWORD lastErr = GetLastError();
    TCHAR fmtErr[255] = { 0 };
    _stprintf_s(fmtErr, 255, TEXT("Error thrown - Last win32 Error: 0x%08x\n"), lastErr);
    PrintDebug(fmtErr);
}

// Invokes SetForegroundWindow in remote memory, thereby overcoming
// operating system restrictions
//          -   hWnd is a handle to the window which is to be the new
//              foreground window. This can be a window belonging to
//              any process.
static int WINAPI ForceWindowActive(HWND hWnd)
{
    // the remote address to which we will copy our injected function struct
    INJFUNCTSTRUCT *pRemoteFuncStruct;
    // the remote address to which we will copy our DoSetWindowForeground function
    DWORD       *pfnRemoteFG;
    // Handle to the process which owns the fg window
    HANDLE      hProc = NULL;
    // remote thread which we create in the remote process to execute our code
    HANDLE      hRemoteThread = NULL;
    // the id of the remote thread
    DWORD       dwThreadId = 0;
    // return value of remote call to SetForegroundWindow
    DWORD       resp = 0;
    // The message indicating the error encountered, if one is encountered
    std::string errmsg;
    // Buffer to hold debug strings for debug output
    TCHAR debugStr[255] = {0};

    try
    {
        if (hWnd == NULL) throw std::runtime_error("No window handle");

        // Get the process that owns the current foreground window.
        // First get the foreground window
        HWND fgHwnd = GetForegroundWindow();

        // if this is the window we're trying to activate, return now
        if (fgHwnd == hWnd) return 0;

        DWORD procId = 0;
        // We don't need the thread ID - just the proc ID
        /* DWORD fgThreadId = */ GetWindowThreadProcessId(fgHwnd, &procId);

        if (procId == 0) throw std::runtime_error(
            "Failed to identify process owning the current foreground window");

        // Get a handle to the process which we can then use to inject a new thread
        HANDLE hProc = OpenProcess(
            PROCESS_CREATE_THREAD
            | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION
            | PROCESS_VM_WRITE | PROCESS_VM_READ,
            FALSE, procId);

        if (hProc==NULL) throw std::runtime_error(
            "Failed to open handle to currently active process");

        HMODULE hUser32 = GetModuleHandle(TEXT("user32"));
        if (hUser32 == NULL) throw std::runtime_error(
            "Failed to obtain handle to user32 module");

		// Create the function structure in the remote process
		INJFUNCTSTRUCT fnStruct = {
			hWnd,
			(SETWINDOWFOREGROUND)GetAddressForSetForegroundWindow(procId, hProc)
		};
        if(fnStruct.pfnSetFGWindow == NULL) throw std::runtime_error(
            "Failed to find SetForegroundWindow function");

        pRemoteFuncStruct = (INJFUNCTSTRUCT*) VirtualAllocEx(
            hProc, 0, sizeof(INJFUNCTSTRUCT), MEM_COMMIT, PAGE_READWRITE);
        if (pRemoteFuncStruct == NULL) throw std::runtime_error(
            "Failed to allocate remote memory (1)");

        WriteProcessMemory(hProc,
            pRemoteFuncStruct, &fnStruct, sizeof(INJFUNCTSTRUCT), NULL);

        // Calculate the number of bytes that ThreadFunc occupies
        const size_t cbFunctionCodeSize =
            ((LPBYTE) NextFunction - (LPBYTE) DoSetWindowForeground);

        _stprintf_s(debugStr, 255, TEXT("Allocating memory: %d bytes\n"),
             cbFunctionCodeSize);
        PrintDebug(debugStr);

        //inject function into remote memory
        pfnRemoteFG = (PDWORD) VirtualAllocEx(
            hProc, 0, cbFunctionCodeSize, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
        if (pfnRemoteFG == NULL) throw std::runtime_error(
            "Failed to allocate remote memory (2)");

        PrintDebug(TEXT("About to write memory to remote process\n"));
        WriteProcessMemory(hProc,
            pfnRemoteFG, &DoSetWindowForeground, cbFunctionCodeSize, NULL);

        PrintDebug(TEXT("About to create remote thread\n"));
        // Start our function running in remote thread
        hRemoteThread = CreateRemoteThread(
            hProc, NULL, 0, (LPTHREAD_START_ROUTINE) pfnRemoteFG,
            pRemoteFuncStruct, 0 , &dwThreadId);
        if (hRemoteThread == NULL) throw std::runtime_error(
            "Failed to create remote thread");

        WaitForSingleObject(hRemoteThread, INFINITE);
    }
    catch (const std::exception &ex)
    {
        DWORD lastErr = GetLastError();
        TCHAR fmtErr[255] = {0};
        _stprintf_s(fmtErr, 255, TEXT("Error thrown: %hs; Last Error: 0x%08x\n"),
             ex.what(), GetLastError());
        PrintDebug(fmtErr);
        errmsg = ex.what();
    }

    // Free all the memory we (may have) built up
    if (pRemoteFuncStruct != 0)
    {
        VirtualFreeEx( hProc, pRemoteFuncStruct, 0, MEM_RELEASE);
    }

    if (pfnRemoteFG != 0)
    {
        VirtualFreeEx( hProc, pfnRemoteFG, 0, MEM_RELEASE);
    }

    if (hRemoteThread != NULL)
    {
        GetExitCodeThread(hRemoteThread, &resp);
        CloseHandle(hRemoteThread);
    }

    if (hProc!=NULL)
    {
        CloseHandle(hProc);
    }

    // Rethrow the error as a std::string if we have an error stored
    if (errmsg.length() > 0)
    {
        throw errmsg;
    }

    // Pass on the return value from the remote function
    return resp;
}

//us-8335 bg-4659
//user32.dll - should be shared between all processes at the same address even if ASLR active, however we have come across a case
//such that the dll is loaded a second time at a different address, the original shared dll delinked (now memory access violation), 
//and the new address dll used - this may be a new behaviour by windows 10 in certain circumstances or some other explanation
static BYTE* GetAddressForSetForegroundWindow(DWORD dwPID, HANDLE hProc)
{
	const BYTE*  mySetForegroundWindowAddress = (BYTE*)SetForegroundWindow;
	const BYTE*  myUser32Address = (BYTE*)GetModuleHandle(TEXT("user32"));
	BYTE*        theirFunctionAddress = (BYTE*)mySetForegroundWindowAddress;
	const size_t myFunctionOffset = mySetForegroundWindowAddress - myUser32Address;
	BOOL         foundTheirAddress = FALSE;
	const size_t debugStrLength = 256;
	TCHAR        debugStr[debugStrLength] = { 0 };
	DWORD        lastError = 0;

	HANDLE hModuleSnap = INVALID_HANDLE_VALUE;
	MODULEENTRY32 me32;

	hModuleSnap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, dwPID);
	if (hModuleSnap == INVALID_HANDLE_VALUE)
	{
		lastError = GetLastError();
		if( _stprintf_s(debugStr, debugStrLength, TEXT("CreateToolhelp32Snapshot error 0x%08x\n"), lastError) != -1)
			PrintDebug(debugStr);

		return theirFunctionAddress; //fallback to old method
	}

	me32.dwSize = sizeof(MODULEENTRY32);
	if (!Module32First(hModuleSnap, &me32))
	{
		lastError = GetLastError();
		if( _stprintf_s(debugStr, debugStrLength, TEXT("Module32First error 0x%08x\n"), lastError) != -1)
			PrintDebug(debugStr);

		CloseHandle(hModuleSnap);
		return theirFunctionAddress; //fallback to old method
	}

	do
	{
		if (_wcsicmp(me32.szModule, L"user32.dll") == 0)
		{
			theirFunctionAddress = me32.modBaseAddr + myFunctionOffset;

			const SIZE_T bytesToReadLength = 10;
			SIZE_T bytesReadLength = 0;
			BYTE memBuffer[bytesToReadLength];

            std::fill(memBuffer, memBuffer+bytesToReadLength, 0);
			BOOL readMem = ReadProcessMemory(hProc, theirFunctionAddress, memBuffer, bytesToReadLength, &bytesReadLength);
			if (readMem == TRUE)
			{
				PrintDebug(TEXT("ReadProcessMemory success\n"));

				int compMem = memcmp(memBuffer, mySetForegroundWindowAddress, bytesToReadLength);
				if (compMem == 0)
				{
					foundTheirAddress = TRUE;

					if( _stprintf_s(debugStr, debugStrLength, TEXT("their address 0x%p\n"), theirFunctionAddress) != -1)
						PrintDebug(debugStr);

					break;
				}
				else
					PrintDebug(TEXT("memcmp match failed\n"));
			}
			else
			{
				lastError = GetLastError();
				if( _stprintf_s(debugStr, debugStrLength, TEXT("ReadProcessMemory error 0x%08x\n"), lastError) != -1)
					PrintDebug(debugStr);
			}
		}
	} while (Module32Next(hModuleSnap, &me32));

	CloseHandle(hModuleSnap);

	if (foundTheirAddress == FALSE)
	{
		theirFunctionAddress = (BYTE*)mySetForegroundWindowAddress; //fallback to old method
		PrintDebug(TEXT("fallback to old method of their address\n"));
	}

	return theirFunctionAddress;
}
