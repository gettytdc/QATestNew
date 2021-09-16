#pragma once

#include <tchar.h>
#include <Windows.h>
#include <stdio.h>
#include <exception>
#include <stdexcept>
#include <tlhelp32.h>

#if _MSC_VER >= 1400
// Following 8 lines: workaround for a bug in some older SDKs
// Solution taken from http://blog.assarbad.net/20120425/annoyance-in-the-windows-sdk-headers/
#   pragma push_macro("_interlockedbittestandset")
#   pragma push_macro("_interlockedbittestandreset")
#   pragma push_macro("_interlockedbittestandset64")
#   pragma push_macro("_interlockedbittestandreset64")
#   define _interlockedbittestandset _local_interlockedbittestandset
#   define _interlockedbittestandreset _local_interlockedbittestandreset
#   define _interlockedbittestandset64 _local_interlockedbittestandset64
#   define _interlockedbittestandreset64 _local_interlockedbittestandreset64
#   include <intrin.h> // to force the header not to be included elsewhere
#   pragma pop_macro("_interlockedbittestandreset64")
#   pragma pop_macro("_interlockedbittestandset64")
#   pragma pop_macro("_interlockedbittestandreset")
#   pragma pop_macro("_interlockedbittestandset")
#endif

typedef LRESULT		(WINAPI *SETWINDOWFOREGROUND)(HWND);

// Structure to contain function pointer and paramters. We copy an instance
// of this struct into remote memory
typedef struct {
	HWND hwnd;                          // Handle of window we're making foreground
	SETWINDOWFOREGROUND	pfnSetFGWindow; // Pointer to user32.SetForegroundWindow
} INJFUNCTSTRUCT, *PINJFUNCTSTRUCT;

static LRESULT DoSetWindowForeground(INJFUNCTSTRUCT *pFuncStruct);
static int NextFunction(void);

static BYTE* GetAddressForSetForegroundWindow(DWORD dwPID, HANDLE hProc);

/**
 * Forces the window with the given handle to the front of the current desktop
 * @param hwnd The handle of the window which should be activated
 * @return 0 indicating success of the call
 * @throws std::string containing the error message encountered.
 */
static int WINAPI ForceWindowActive(HWND hWnd);

/**
 * Wraps the forcing of the active window. This handles C++ style exceptions
 * internally, returning an error response if such an exception occurs.
 * @param hwnd The handle of the window which should be activated
 * @return 0 if the activate was successful; 1 if a checked exception occurs;
 *     2 if an unchecked C++ exception occurs.
 */
static int wrapForceWindowActive(HWND hwnd);

/**
 * Provides a main class which interprets the command line and activates the
 * required window, handling Windows C Style exceptions and provides an error
 * code if one is detected.
 * @param argc The number of arguments provided
 * @param argv The arguments
 * @return 0: success; 1: Checked (C++) exception; 2: Unchecked (C++) exception;
 *     3: Unhandled (C SEH) exception.
 */
int activator_main(int argc, _TCHAR* argv[]);

void GeneralError();
