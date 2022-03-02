// BpInjAgent64.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "initguid.h"
#include "mshtml.h"
#include "Exdisp.h"

using namespace std;

// Have 'DebugMessages' defined to generate extra messages across the
// pipe for debugging purposes. This is never done directly in this file,
// but the BPInjAgentD project does that and then includes this file, so
// we produce two versions of the DLL, one with diagnostics and one without.

// Maximum size of a pipe message we will send. Anything larger will get chunked.
#define MAX_PIPEMSG_SIZE 8192

// Have 'DebugLog' defined to enable some very crude and low level logging.
// Normally this should be switched off. The log file it creates is always
// at %COMMONAPPDATA%\Blue Prism Limited\bpinjagent.log.
//#define DebugLog

#if defined(DebugLog)
#include<ShlObj.h>
#include<KnownFolders.h>
ofstream LogFile;
HANDLE LogMutex = NULL;
WCHAR LogDest[MAX_PATH + 1];
void _Log(string msg)
{
    if (LogMutex == NULL) {
        LogMutex = CreateMutex(NULL, FALSE, NULL);
        PWSTR progdata = NULL;
        SHGetKnownFolderPath(FOLDERID_ProgramData, 0, NULL, &progdata);
        wcscpy_s(LogDest, progdata);
        wcscat_s(LogDest, L"\\Blue Prism Limited\\Blue Prism\\Logs\\BpInjAgent.txt");
        CoTaskMemFree(progdata);
        LogFile.open(LogDest, ios::out | ios::app);
    }
    if (WaitForSingleObject(LogMutex, 5000) != WAIT_OBJECT_0)
        return;
    try
    {
        LogFile << msg << endl;
    }
    catch (...)
    {
    }
    ReleaseMutex(LogMutex);
}
#define Log(msg) _Log(msg)
#else
#define Log(msg)
#endif

// The block of code we'll patch in that constitutes a 'hook', overwriting the
// start of the thing we're hooking. It's a jump to our replacement function.
// Also used as the jump back to the original.
#pragma pack(1)
typedef struct _HookBlock
{
    BYTE jmp1 = 0xff;
    BYTE jmp2 = 0x25;
    DWORD offset;
} HOOKBLOCK, *PHOOKBLOCK;
#pragma pack()
const int HOOKSIZE = sizeof(HOOKBLOCK);
const int MAXREPSIZE = HOOKSIZE + 16;

void UnhookAll(void);
void PipeMessage(LPCTSTR lpszMsg);
void PipeMemDump(const char* msg, BYTE* addr, int len);


wstring str_to_wstr(const string& str)
{
    wstring wstr(str.length(), 0);
    MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), &wstr[0], str.length());
    return wstr;
}
string wstr_to_str(const wstring& wstr)
{
    size_t size = wstr.length();
    string str(size, 0);
    WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), size, &str[0], size, NULL, NULL);
    return str;
}


// ***********************************************************************************
//
// Helpers used during command processing functions. They all return either true if
// successful, or false if they fail. If they fail, they have already piped the
// required FAILURE: message.
//


/// <summary>
/// Converts a local multibyte ANSI/OEM string to UTF8
/// </summary>
/// <param name="str">The string to convert</param>
/// <param name="len">The length of the string</param>
/// <param name="str_len">The length of the converted string</param>
LPCTSTR LocalToUtf8(const LPCTSTR str, int len, int& str_len)
{
    int wstr_len = MultiByteToWideChar(CP_ACP, 0, str, len, NULL, 0);
    WCHAR* wstr = (WCHAR*)malloc((wstr_len + 1) * sizeof(WCHAR));
    if (!wstr) return nullptr;
    MultiByteToWideChar(CP_ACP, 0, str, len, wstr, wstr_len);
    wstr[wstr_len] = L'\0';

    str_len = WideCharToMultiByte(CP_UTF8, 0, wstr, wstr_len, NULL, 0, NULL, NULL);
    CHAR* str_to = (CHAR*)malloc((str_len + 1) * sizeof(CHAR));
    if (!str_to) return nullptr;
    WideCharToMultiByte(CP_UTF8, 0, wstr, wstr_len, str_to, str_len, NULL, NULL);
    str_to[str_len] = '\0';

    free(wstr);
    return str_to;
}

/// <summary>
/// Converts a wide Unicode string to UTF8
/// </summary>
/// <param name="wstr">The wide string to convert</param>
/// <param name="wstr_len">The length of the wide string</param>
/// <param name="str_len">The length of the converted string</param>
LPCTSTR wideToUtf8(LPCWSTR wstr, int wstr_len, int& str_len)
{
    str_len = WideCharToMultiByte(CP_UTF8, 0, wstr, wstr_len, NULL, 0, NULL, NULL);
    CHAR * str_to = (CHAR*)malloc((str_len + 1) * sizeof(CHAR));
    if (!str_to) return nullptr;
    WideCharToMultiByte(CP_UTF8, 0, wstr, wstr_len, str_to, str_len, NULL, NULL);
    str_to[str_len] = '\0';
    return str_to;
}

/// <summary>
/// Appends data to the buffer replacing special characters with escape sequences
/// </summary>
/// <param name="buf">The buffer to append to</param>
/// <param name="buflen">The length of the buffer</param>
/// <param name="sz">The string to append</param>
/// <param name="numch">The number of characters, -1 will calculate the size automatically</param>
void EncodeAndAppend(LPTSTR buf, int buflen, LPCTSTR sz, int numch = -1)
{
    TCHAR ch;
    LPTSTR ptr;
    LPCTSTR ptr2;
    int len = strnlen_s(buf, buflen);
    // Functions like DrawText can be passed -1 as the nCount parameter
    // This means that the string should be treated as null terminated
    if (numch == -1)
        numch = strnlen_s(sz, buflen);

    buflen -= len;
    ptr = buf + len;
    ptr2 = sz;
    // Get some 'breathing room' in the buffer because then we
    // can overrun by a byte, which saves additional checking
    // within the loop.
    buflen -= 2;
    while (buflen > 0 && numch > 0)
    {
        ch = *(ptr2++);
        numch--;
        switch (ch)
        {
        case '\\':
            *(ptr++) = ch;
            *(ptr++) = ch;
            buflen -= 2;
            break;
        case '\n':
            *(ptr++) = '\\';
            *(ptr++) = 'n';
            buflen -= 2;
            break;
        case '\r':
            *(ptr++) = '\\';
            *(ptr++) = 'r';
            buflen -= 2;
            break;
        case ',':
            *(ptr++) = '\\';
            *(ptr++) = 'c';
            buflen -= 2;
            break;
        case '=':
            *(ptr++) = '\\';
            *(ptr++) = 'e';
            buflen -= 2;
            break;
        default:
            *(ptr++) = ch;
            buflen--;
            break;
        }
    }
    *ptr++ = '\0';
}

/// <summary>
/// Converts the data from a Wide string and appends data to the buffer replacing special
/// characters with escape sequences
/// </summary>
/// <param name="buf">The buffer to append to</param>
/// <param name="buflen">The length of the buffer</param>
/// <param name="str">The string to append</param>
/// <param name="numch">The number of characters</param>
void EncodeAndAppendW(LPTSTR buf, int buflen, LPCWSTR str, int numch)
{
    int str_len;
    LPCTSTR sz = wideToUtf8(str, numch, str_len);
    EncodeAndAppend(buf, buflen, sz, str_len);
    free((void*)sz);
}

/// <summary>
/// Converts the data from a local string and appends data to the buffer replacing special
/// characters with escape sequences
/// </summary>
/// <param name="buf">The buffer to append to</param>
/// <param name="buflen">The length of the buffer</param>
/// <param name="str">The string to append</param>
/// <param name="numch">The number of characters</param>
void EncodeAndAppendA(LPTSTR buf, int buflen, LPCTSTR str, int numch)
{
    int str_len;
    LPCTSTR sz = LocalToUtf8(str, numch, str_len);
    EncodeAndAppend(buf, buflen, sz, str_len);
    free((void*)sz);
}

LRESULT CALLBACK MsgHookProc(int nCode, WPARAM wParam, LPARAM lParam)
{

    TCHAR buf[2048];
    MSG* msg = (MSG*)lParam;
    LPNMHDR nmhdr;

    switch (msg->message)
    {
    case WM_COMMAND:
        _snprintf(buf, 2000, "HOOK.COMMAND(hWnd=%08X,ID=%04X,CODE=%04X)", lParam, LOWORD(wParam), HIWORD(wParam));
        PipeMessage(buf);
        break;
    case WM_NOTIFY:
        nmhdr = (LPNMHDR)msg->lParam;
        _snprintf(buf, 2000, "HOOK.NOTIFY(hWnd=%08X,ID=%04X,CODE=%04X)", nmhdr->hwndFrom, nmhdr->idFrom, nmhdr->code);
        PipeMessage(buf);
        break;
    default:
        _snprintf(buf, 2000, "HOOK.MSG(msg=%08X,hWnd=%08X,wParam=%08X,lParam=%08X)", msg->message, msg->hwnd, msg->wParam, msg->lParam);
        PipeMessage(buf);
        break;
    }

    return CallNextHookEx(NULL, nCode, wParam, lParam);
}


LRESULT CALLBACK CallWndProcHookProc(int nCode, WPARAM wParam, LPARAM lParam)
{

    TCHAR buf[2048];
    CWPSTRUCT* msg = (CWPSTRUCT*)lParam;
    LPNMHDR nmhdr;

    switch (msg->message)
    {
    case WM_COMMAND:
        _snprintf(buf, 2000, "HOOK.CWP.COMMAND(hWnd=%08X,ID=%04X,CODE=%04X)", msg->lParam, LOWORD(msg->wParam), HIWORD(msg->wParam));
        PipeMessage(buf);
        break;
    case WM_NOTIFY:
        nmhdr = (LPNMHDR)msg->lParam;
        _snprintf(buf, 2000, "HOOK.CWP.NOTIFY(hWnd=%08X,ID=%04X,CODE=%08X)", nmhdr->hwndFrom, nmhdr->idFrom, nmhdr->code);
        PipeMessage(buf);
        break;
    default:
        //          Can use this to see other messages, but for the sake of clarity and performance
        //          we're only sending the specific messages above that we're interested in.
        //          _snprintf(buf,2000,"HOOK.CWP.MSG(msg=%08X,wParam=%08X,lParam=%08X)",msg->message,msg->wParam,msg->lParam);
        //          PipeMessage(buf);
        break;
    }

    return CallNextHookEx(NULL, nCode, wParam, lParam);
}


LRESULT CALLBACK CbtHookProc(int nCode, WPARAM wParam, LPARAM lParam)
{

    TCHAR buf[2048];
    TCHAR classname[128];

    switch (nCode)
    {
    case HCBT_CREATEWND:
        int len;
        len = GetClassName((HWND)wParam, classname, 127);
        classname[len] = '\0';
        LPCBT_CREATEWND cw;
        cw = LPCBT_CREATEWND(lParam);
        _snprintf(buf, 2000, "HOOK.HCBT_CREATEWND(wParam=%08X,x=%d,y=%d,w=%d,h=%d,hWndParent=%08X,lpszName=\"", wParam, cw->lpcs->x, cw->lpcs->y, cw->lpcs->cx, cw->lpcs->cy, cw->lpcs->hwndParent);
        if (cw->lpcs->lpszName != NULL)
            EncodeAndAppend(buf, 2000, cw->lpcs->lpszName);
        strcat_s(buf, sizeof(buf)/sizeof(TCHAR), "\",classname=\"");
        EncodeAndAppend(buf, 2000, classname);
        strcat_s(buf, sizeof(buf)/sizeof(TCHAR), "\")");
        PipeMessage(buf);
        break;

    case HCBT_DESTROYWND:
        _snprintf(buf, 2048, "HOOK.HCBT_DESTROYWND(hWnd=%08X)", wParam);
        PipeMessage(buf);
        break;

    case HCBT_ACTIVATE:
        //CBTACTIVATESTRUCT ca=CBTACTIVATESTRUCT(lParam);
        _snprintf(buf, 2048, "HOOK.HCBT_ACTIVATE(hWnd=%08X)", wParam);
        PipeMessage(buf);
        break;

    case HCBT_CLICKSKIPPED:
        break;
    case HCBT_KEYSKIPPED:
        break;
    case HCBT_MINMAX:
        _snprintf(buf, 2048, "HOOK.HCBT_MINMAX(hWnd=%08X)", wParam);
        PipeMessage(buf);
        break;

    case HCBT_MOVESIZE:
    {
        RECT* mv = (RECT*)lParam;
        _snprintf(buf, 2048, "HOOK.HCBT_MOVESIZE(hWnd=%08X,X=%d,Y=%d,cx=%d,cy=%d)", wParam, mv->left, mv->top, mv->right - mv->left, mv->bottom - mv->top);
        PipeMessage(buf);
    }
    break;

    case HCBT_QS:
        break;
    case HCBT_SETFOCUS:
        break;
    case HCBT_SYSCOMMAND:
        break;
    }

    return CallNextHookEx(NULL, nCode, wParam, lParam);
}

// List of threads we have hooked...
class ThreadHookRecord
{
public:
    DWORD m_ThreadID;
    HHOOK m_HookHandle;
    HHOOK m_MsgHookHandle;
    HHOOK m_CallWndProcHookHandle;
    ThreadHookRecord* m_Next;
};
ThreadHookRecord* ThreadHookList = NULL;


// Set a windows hook on the current thread if necessary - i.e. if we haven't
// already hooked on this thread. This is called from all API entry points, and
// called from HookExistingThreads.
// 'id' is the ID of the thread to add the hooks for, or 0 for the current
// thread.
// Calling this from a remotely created thread (e.g. our Init function) will
// not work. The hooks will be created, but never called.
void HookThreadIfNecessary(int id = 0)
{
    // If no thread id was specified, use the current one.
    if (id == 0)
        id = GetCurrentThreadId();

    // Check if we've already hooked on this thread...
    ThreadHookRecord *th;
    ThreadHookRecord *last;
    th = ThreadHookList;
    last = NULL;
    while (th != NULL)
    {
        if (th->m_ThreadID == id)
            return;
        last = th;
        th = th->m_Next;
    }

    // Send message down pipe...
    TCHAR buf[1024];
    _snprintf(buf, 1024, "BPInjAgent: adding Windows Hooks on thread %08X", id);
    PipeMessage(buf);

    th = new ThreadHookRecord();
    th->m_ThreadID = id;
    if ((th->m_HookHandle = SetWindowsHookEx(WH_CBT, CbtHookProc, NULL, id)) == NULL)
    {
        _snprintf(buf, 1024, "BPInjAgent: Failed to set WH_CBT hook - %d", GetLastError());
        PipeMessage(buf);
    }
    if ((th->m_MsgHookHandle = SetWindowsHookEx(WH_GETMESSAGE, MsgHookProc, NULL, id)) == NULL)
        PipeMessage("BPInjAgent: Failed to set WH_GETMESSAGE hook");
    if ((th->m_CallWndProcHookHandle = SetWindowsHookEx(WH_CALLWNDPROC, CallWndProcHookProc, NULL, id)) == NULL)
        PipeMessage("BPInjAgent: Failed to set WH_CALLWNDPROC hook");
    th->m_Next = NULL;
    // Add new record to list...  ( ** TEMP ** needs access synchronising!)
    if (last == NULL)
        ThreadHookList = th;
    else
        last->m_Next = th;
}

// Hook all existing threads, expect the one currently executing. See the
// notes on HookThreadIfNecessary regarding when this can be called
void HookExistingThreads(void)
{
    DWORD procid = GetCurrentProcessId();

#if defined(DebugMessages)
    TCHAR buf[1024];
    _snprintf(buf, 1024, "BPInjAgent: Hooking existing threads for process %08X", procid);
    PipeMessage(buf);
#endif

    // Enumerate all threads (no way to restrict this to a process other than filtering
    // once we have them)...
    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
    if (snapshot != INVALID_HANDLE_VALUE)
    {
        THREADENTRY32 th;
        th.dwSize = sizeof(th);
        if (Thread32First(snapshot, &th))
        {
            do
            {
                if (th.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID) + sizeof(th.th32OwnerProcessID))
                {
                    if (th.th32OwnerProcessID == procid && th.th32ThreadID != GetCurrentThreadId())
                        HookThreadIfNecessary(th.th32ThreadID);
                }
                th.dwSize = sizeof(th);
            } while (Thread32Next(snapshot, &th));
        }

    }
    CloseHandle(snapshot);

}

void ReleaseHooks(void)
{
    ThreadHookRecord* th;
    ThreadHookRecord* last;
    th = ThreadHookList;
    last = NULL;
    while (th != NULL)
    {
        UnhookWindowsHookEx(th->m_HookHandle);
        UnhookWindowsHookEx(th->m_MsgHookHandle);
        UnhookWindowsHookEx(th->m_CallWndProcHookHandle);
        last = th;
        th = th->m_Next;
        delete last;
    }
    ThreadHookList = NULL;
}


HWND(WINAPI *CreateDialogIndirectParamATrampoline)(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit);
HWND WINAPI CreateDialogIndirectParamAReplaced(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: CreateDialogIndirectParamAReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the original API function first, because we need the
    // return value.
    HWND retval;
    retval = CreateDialogIndirectParamATrampoline(hInstance, lpTemplate, hWndParent, lpDialogFunc, lParamInit);
    return retval;
}


HWND(WINAPI *CreateDialogParamATrampoline)(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit);
HWND WINAPI CreateDialogParamAReplaced(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: CreateDialogParamAReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the original API function first, because we need the
    // return value.
    HWND retval;
    retval = CreateDialogParamATrampoline(hInstance, lpTemplate, hWndParent, lpDialogFunc, lParamInit);
    return retval;
}

HWND(WINAPI *DialogBoxIndirectParamATrampoline)(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit);
HWND WINAPI DialogBoxIndirectParamAReplaced(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: DialogBoxIndirectParamAReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the original API function first, because we need the
    // return value.
    HWND retval;
    retval = DialogBoxIndirectParamATrampoline(hInstance, lpTemplate, hWndParent, lpDialogFunc, lParamInit);
    return retval;
}


HWND(WINAPI *DialogBoxParamATrampoline)(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit);
HWND WINAPI DialogBoxParamAReplaced(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: DialogBoxParamAReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the original API function first, because we need the
    // return value.
    HWND retval;
    retval = DialogBoxParamATrampoline(hInstance, lpTemplate, hWndParent, lpDialogFunc, lParamInit);
    return retval;
}


HWND(WINAPI *CreateWindowExATrampoline)(
    DWORD dwExStyle,
    LPCTSTR lpClassName,
    LPCTSTR lpWindowName,
    DWORD dwStyle,
    int x,
    int y,
    int nWidth,
    int nHeight,
    HWND hWndParent,
    HMENU hMenu,
    HINSTANCE hInstance,
    LPVOID lpParam
);
HWND WINAPI CreateWindowExAReplaced(
    DWORD dwExStyle,
    LPCTSTR lpClassName,
    LPCTSTR lpWindowName,
    DWORD dwStyle,
    int x,
    int y,
    int nWidth,
    int nHeight,
    HWND hWndParent,
    HMENU hMenu,
    HINSTANCE hInstance,
    LPVOID lpParam
)
{

#if defined(DebugMessages)
    PipeMessage("DEBUG: CreateWindowExAReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the original API function first, because we need the
    // return value.
    HWND retval;
    retval = CreateWindowExATrampoline(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
#if defined(DebugMessages)
    PipeMessage("DEBUG: CreateWindowExAReplaced returning");
#endif

    return retval;
}

HWND(WINAPI *CreateWindowExWTrampoline)(
    DWORD dwExStyle,
    LPCTSTR lpClassName,
    LPCTSTR lpWindowName,
    DWORD dwStyle,
    int x,
    int y,
    int nWidth,
    int nHeight,
    HWND hWndParent,
    HMENU hMenu,
    HINSTANCE hInstance,
    LPVOID lpParam
    );
HWND WINAPI CreateWindowExWReplaced(
    DWORD dwExStyle,
    LPCTSTR lpClassName,
    LPCTSTR lpWindowName,
    DWORD dwStyle,
    int x,
    int y,
    int nWidth,
    int nHeight,
    HWND hWndParent,
    HMENU hMenu,
    HINSTANCE hInstance,
    LPVOID lpParam
)
{

#if defined(DebugMessages)
    PipeMessage("DEBUG: CreateWindowExWReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the original API function first, because we need the
    // return value.
    HWND retval;
    retval = CreateWindowExWTrampoline(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
    return retval;
}



HWND(WINAPI *SetParentTrampoline)(HWND hWndChild, HWND hWndNewParent);
HWND WINAPI SetParentReplaced(HWND hWndChild, HWND hWndNewParent)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: SetParentReplaced");
#endif
    HookThreadIfNecessary();

    HWND retval = SetParentTrampoline(hWndChild, hWndNewParent);

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "SetParent(hWndChild=%08X,hWndNewParent=%08X,retval=%08X)", hWndChild, hWndNewParent, retval);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: SetParent");
    }
    return retval;
}


int(WINAPI *FillRectTrampoline)(HDC hDC, CONST RECT *lprc, HBRUSH hbr);
int WINAPI FillRectReplaced(HDC hDC, CONST RECT *lprc, HBRUSH hbr)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: FillRectReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        if (lprc != NULL) {
            // Translate logical to device coordinates...
            POINT p[2];
            p[0].x = lprc->left;
            p[0].y = lprc->top;
            p[1].x = lprc->right;
            p[1].y = lprc->bottom;
            LPtoDP(hDC, LPPOINT(&p), 2);

            _snprintf(buf, 1024, "FillRect(hDC=%08X,lprc=%d;%d;%d;%d)", hDC, p[0].x, p[0].y, p[1].x, p[1].y);
        }
        else {
            _snprintf(buf, 1024, "FillRect(hDC=%08X,lprc=NULL)", hDC);
        }
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: FillRect");
    }

    // Execute the orignal API function before returning
    return FillRectTrampoline(hDC, lprc, hbr);
}

BOOL(WINAPI *EndPaintTrampoline)(HWND hWnd, CONST PAINTSTRUCT *lpPaint);
BOOL WINAPI EndPaintReplaced(HWND hWnd, CONST PAINTSTRUCT *lpPaint)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: EndPaintReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "EndPaint(hWnd=%08X,hDC=%08X)", hWnd, lpPaint == NULL ? 0 : lpPaint->hdc);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: EndPaint");
    }

#if defined(DebugMessages)
    PipeMessage("DEBUG: EndPaintReplaced - about to return");
#endif

    return EndPaintTrampoline(hWnd, lpPaint);
}

HDC(WINAPI *BeginPaintTrampoline)(HWND hwnd, LPPAINTSTRUCT lpPaint);
HDC WINAPI BeginPaintReplaced(HWND hWnd, LPPAINTSTRUCT lpPaint)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: BeginPaintReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the orignal API function first because we need the result...
    HDC retval;
    retval = BeginPaintTrampoline(hWnd, lpPaint);

#if defined(DebugMessages)
    PipeMessage("DEBUG: BeginPaintReplaced - original executed");
#endif

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "BeginPaint(hWnd=%08X,retval=%08X)", hWnd, retval);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: BeginPaint");
    }

#if defined(DebugMessages)
    PipeMessage("DEBUG: BeginPaintReplaced - about to return");
#endif

    return retval;
}


int(WINAPI *ReleaseDCTrampoline)(HWND hWnd, HDC hDC);
int WINAPI ReleaseDCReplaced(HWND hWnd, HDC hDC)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: ReleaseDCReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "ReleaseDC(hWnd=%08X,hDC=%08X)", hWnd, hDC);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: ReleaseDC");
    }

#if defined(DebugMessages)
    PipeMessage("DEBUG: ReleaseDCReplaced complete");
#endif

    // Execute the orignal API function before returning so we still
    // return the correct DC to the caller...
    return ReleaseDCTrampoline(hWnd, hDC);

}


int(WINAPI *DeleteDCTrampoline)(HDC hDC);
int WINAPI DeleteDCReplaced(HDC hDC)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: DeleteDCReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "DeleteDC(hDC=%08X)", hDC);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: DeleteDC");
    }

    // Execute the orignal API function before returning so we still
    // return the correct DC to the caller...
    return DeleteDCTrampoline(hDC);

}


HDC(WINAPI *GetDCTrampoline)(HWND hWnd);
HDC WINAPI GetDCReplaced(HWND hWnd)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: GetDCReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the orignal API function first because we need the result...
    HDC retval;
    retval = GetDCTrampoline(hWnd);

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "GetDC(hWnd=%08X,retval=%08X)", hWnd, retval);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: GetDC");
    }

    return retval;
}

HDC(WINAPI *CreateDCATrampoline)(
    LPCTSTR lpszDriver,        // driver name
    LPCTSTR lpszDevice,        // device name
    LPCTSTR lpszOutput,        // not used; should be NULL
    CONST DEVMODE* lpInitData);  // optional printer data
HDC WINAPI CreateDCAReplaced(LPCTSTR lpszDriver,        // driver name
    LPCTSTR lpszDevice,        // device name
    LPCTSTR lpszOutput,        // not used; should be NULL
    CONST DEVMODE* lpInitData)  // optional printer data
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: CreateDCAReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the orignal API function first because we need the result...
    HDC retval;
    retval = CreateDCATrampoline(lpszDriver, lpszDevice, lpszOutput, lpInitData);

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "CreateDCA(retval=%08X)", retval);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: CreateDCA");
    }

    return retval;
}

HDC(WINAPI *CreateCompatibleDCTrampoline)(HDC hdc);
HDC WINAPI CreateCompatibleDCReplaced(HDC hdc)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: CreateCompatibleDCReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the orignal API function first because we need the result...
    HDC retval = CreateCompatibleDCTrampoline(hdc);

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "CreateCompatibleDC(hDC=%08X,retval=%08X)", hdc, retval);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: CreateCompatibleDC");
    }

    return retval;
}

BOOL(WINAPI *BitBltTrampoline)(
    HDC hdcDest,
    int x,
    int y,
    int cx,
    int cy,
    HDC hdcSrc,
    int nXSrc,
    int nYSrc,
    DWORD rop
);
BOOL  WINAPI BitBltReplaced(
    HDC hdcDest,
    int nXDest,
    int nYDest,
    int cx,
    int cy,
    HDC hdcSrc,
    int nXSrc,
    int nYSrc,
    DWORD rop
)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: BitBltReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        // Translate logical to device coordinates...
        POINT p[3];
        p[0].x = nXDest;
        p[0].y = nYDest;
        p[1].x = nXSrc;
        p[1].y = nYSrc;
        p[2].x = cx;
        p[2].y = cy;
        LPtoDP(hdcDest, LPPOINT(&p), 1);
        LPtoDP(hdcSrc, LPPOINT(&p[1]), 2);

        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "BitBlt(hdcDest=%08X,nXDest=%d,nYDest=%d,hdcSrc=%08X,nXSrc=%d,nYSrc=%d,cx=%d,cy=%d)", hdcDest, p[0].x, p[0].y, hdcSrc, p[1].x, p[1].y, p[2].x, p[2].y);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: BitBlt");
    }

    return BitBltTrampoline(hdcDest, nXDest, nYDest, cx, cy, hdcSrc, nXSrc, nYSrc, rop);
}

BOOL(WINAPI *PatBltTrampoline)(
    HDC dc,
    int x,
    int y,
    int w,
    int h,
    DWORD rop
    );
BOOL  WINAPI PatBltReplaced(
    HDC dc,
    int x,
    int y,
    int w,
    int h,
    DWORD rop
)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: PatBltReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        // Translate logical to device coordinates...
        POINT p[2];
        p[0].x = x;
        p[0].y = y;
        p[1].x = w;
        p[1].y = h;
        LPtoDP(dc, LPPOINT(&p), 2);

        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "PatBlt(dc=%08X,x=%d,y=%d,w=%d,h=%d)", dc, p[0].x, p[0].y, p[1].x, p[1].y);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: PatBlt");
    }

    return PatBltTrampoline(dc, x, y, w, h, rop);
}

HDC(WINAPI *GetWindowDCTrampoline)(HWND hWnd);
HDC WINAPI GetWindowDCReplaced(HWND hWnd)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: GetWindowDCReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the orignal API function first because we need the result...
    HDC retval;
    retval = GetWindowDCTrampoline(hWnd);

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "GetWindowDC(hWnd=%08X,retval=%08X)", hWnd, retval);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: GetWindowDC");
    }

    return retval;
}


HDC(WINAPI *GetDCExTrampoline)(HWND hWnd, HRGN hrgnClip, DWORD flags);
HDC WINAPI GetDCExReplaced(HWND hWnd, HRGN hrgnClip, DWORD flags)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: GetDCExReplaced");
#endif
    HookThreadIfNecessary();

    // Execute the orignal API function first because we need the result...
    HDC retval;
    retval = GetDCExTrampoline(hWnd, hrgnClip, flags);

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "GetDCEx(hWnd=%08X,retval=%08X)", hWnd, retval);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: GetDCEx");
    }

    return retval;
}

int(WINAPI *DrawTextATrampoline)(
    HDC hDC,          // handle to DC
    LPCTSTR lpString, // text to draw
    int nCount,       // text length
    LPRECT lpRect,    // formatting dimensions
    UINT uFormat      // text-drawing options
);
int WINAPI DrawTextAReplaced(
    HDC hDC,          // handle to DC
    LPCTSTR lpString, // text to draw
    int nCount,       // text length
    LPRECT lpRect,    // formatting dimensions
    UINT uFormat      // text-drawing options
)
{

#if defined(DebugMessages)
    PipeMessage("DEBUG: DrawTextAReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        // Translate logical to device coordinates...
        POINT p[2];
        p[0].x = lpRect->left;
        p[0].y = lpRect->top;
        p[1].x = lpRect->right;
        p[1].y = lpRect->bottom;
        LPtoDP(hDC, LPPOINT(&p), 2);

        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "DrawTextA(hDC=%08X,lpRect=%d;%d;%d;%d,uFormat=%d,lpString=\"", hDC, p[0].x, p[0].y, p[1].x, p[1].y, uFormat);
        EncodeAndAppendA(buf, 1021, lpString, nCount);
        strcat_s(buf, sizeof(buf)/sizeof(TCHAR), "\")");
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: DrawTextA");
    }

    // Execute the orignal API function before returning, otherwise
    // the text won't get drawn....
    return DrawTextATrampoline(hDC, lpString, nCount, lpRect, uFormat);

}


int(WINAPI *DrawTextWTrampoline)(
    HDC hDC,          // handle to DC
    LPCWSTR lpString, // text to draw
    int nCount,       // text length
    LPRECT lpRect,    // formatting dimensions
    UINT uFormat      // text-drawing options
);
int WINAPI DrawTextWReplaced(
    HDC hDC,          // handle to DC
    LPCWSTR lpString, // text to draw
    int nCount,       // text length
    LPRECT lpRect,    // formatting dimensions
    UINT uFormat      // text-drawing options
)
{

#if defined(DebugMessages)
    PipeMessage("DEBUG: DrawTextWReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        // Translate logical to device coordinates...
        POINT p[2];
        p[0].x = lpRect->left;
        p[0].y = lpRect->top;
        p[1].x = lpRect->right;
        p[1].y = lpRect->bottom;
        LPtoDP(hDC, LPPOINT(&p), 2);

        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "DrawTextW(hDC=%08X,lpRect=%d;%d;%d;%d,uFormat=%d,lpString=\"", hDC, p[0].x, p[0].y, p[1].x, p[1].y, uFormat);
        EncodeAndAppendW(buf, 1021, lpString, nCount);
        strcat_s(buf, sizeof(buf)/sizeof(TCHAR), "\")");
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: DrawTextW");
    }

    // Execute the orignal API function before returning, otherwise
    // the text won't get drawn....
    return DrawTextWTrampoline(hDC, lpString, nCount, lpRect, uFormat);

}


BOOL(WINAPI *TextOutATrampoline)(
    HDC hdc,           // handle to DC
    int nXStart,       // x-coordinate of starting position
    int nYStart,       // y-coordinate of starting position
    LPCTSTR lpString,  // character string
    int cbString       // number of characters
);
BOOL WINAPI TextOutAReplaced(
    HDC hDC,           // handle to DC
    int nXStart,       // x-coordinate of starting position
    int nYStart,       // y-coordinate of starting position
    LPCTSTR lpString,  // character string
    int cbString       // number of characters
)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: TextOutAReplaced");
#endif
    HookThreadIfNecessary();

    try
    {

        //Get the extents of the text in the current font
        SIZE szSize;
        GetTextExtentPoint32(hDC, lpString, cbString, &szSize);

        // Translate logical to device coordinates...
        POINT p[2];
        p[0].x = nXStart;
        p[0].y = nYStart;
        GetViewportOrgEx(hDC, &p[1]);
        p[1].x += szSize.cx;
        p[1].y += szSize.cy;
        LPtoDP(hDC, LPPOINT(&p), 2);

        TEXTMETRICA metric;
        GetTextMetricsA(hDC, &metric);

        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "TextOutA(hDC=%08X,nXStart=%d,nYStart=%d,nWidth=%d,nHeight=%d,tmDescent=%d,lpString=\"", hDC, p[0].x, p[0].y, p[1].x, p[1].y, metric.tmDescent);
        EncodeAndAppendA(buf, 1021, lpString, cbString);
        strcat_s(buf, sizeof(buf)/sizeof(TCHAR), "\")");
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: TextOutA");
    }

    // Execute the orignal API function before returning, otherwise
    // the text won't get drawn....
    return TextOutATrampoline(hDC, nXStart, nYStart, lpString, cbString);
}

BOOL(WINAPI *ExtTextOutWTrampoline)(
    HDC     hdc,
    int     X,
    int     Y,
    UINT    fuOptions,
    const RECT    *lprc,
    LPCWSTR lpString,
    UINT    cbCount,
    const INT     *lpDx
    );
BOOL WINAPI ExtTextOutWReplaced(
    HDC     hdc,
    int     X,
    int     Y,
    UINT    fuOptions,
    const RECT    *lprc,
    LPCWSTR lpString,
    UINT    cbCount,
    const INT     *lpDx
)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: ExtTextOutWReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        //Get the extents of the text in the current font
        SIZE szSize;
        GetTextExtentPoint32W(hdc, lpString, cbCount, &szSize);

        // Translate logical to device coordinates...
        POINT p[2];
        p[0].x = X;
        p[0].y = Y;
        GetViewportOrgEx(hdc, &p[1]);
        p[1].x += szSize.cx;
        p[1].y += szSize.cy;
        LPtoDP(hdc, LPPOINT(&p), 2);

        TEXTMETRICW metric;
        GetTextMetricsW(hdc, &metric);

        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "ExtTextOutW(hDC=%08X,nXStart=%d,nYStart=%d,nWidth=%d,nHeight=%d,fuOptions=%d,tmDescent=%d,lpString=\"", hdc, p[0].x, p[0].y, p[1].x, p[1].y, fuOptions, metric.tmDescent);
        EncodeAndAppendW(buf, 1021, lpString, cbCount);
        strcat_s(buf, sizeof(buf)/sizeof(TCHAR), "\")");
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: ExtTextOutW");
    }

    // Execute the orignal API function before returning, otherwise
    // the text won't get drawn....
    return ExtTextOutWTrampoline(hdc, X, Y, fuOptions, lprc, lpString, cbCount, lpDx);
}


UINT(WINAPI *SetTextAlignTrampoline)(HDC hDC, UINT align);
UINT  WINAPI SetTextAlignReplaced(HDC hDC, UINT align)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: SetTextAlignReplaced");
#endif
    HookThreadIfNecessary();

    try
    {
        // Send message down pipe...
        TCHAR buf[1024];
        _snprintf(buf, 1024, "SetTextAlign(hDC=%08X,align=%d)", hDC, align);
        PipeMessage(buf);
    }
    catch (...)
    {
        PipeMessage("EXCEPTION: SetTextAlign");
    }

    // Execute the orignal API function before returning, otherwise
    // the text won't get drawn....
    return SetTextAlignTrampoline(hDC, align);
}

BOOL(WINAPI *SetWindowPosTrampoline)(
    HWND hWnd,
    HWND hWndInsertAfter,
    int X,
    int Y,
    int cx,
    int cy,
    UINT uFlags
    );
BOOL WINAPI SetWindowPosReplaced(
    HWND hWnd,
    HWND hWndInsertAfter,
    int X,
    int Y,
    int cx,
    int cy,
    UINT uFlags
)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: SetWindowPosReplaced");
#endif
    HookThreadIfNecessary();

    // Send message down pipe...
    TCHAR buf[1024];
    _snprintf(buf, 1024, "SetWindowPos(hWnd=%08X,X=%d,Y=%d,cx=%d,cy=%d,uFlags=%d)", hWnd, X, Y, cx, cy, uFlags);
    PipeMessage(buf);
    // Call the original API function and return the result...
    return SetWindowPosTrampoline(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
}

BOOL(WINAPI *ShowWindowTrampoline)(HWND hWnd, int nCMDShow);
BOOL WINAPI ShowWindowReplaced(HWND hWnd, int nCMDShow)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: ShowWindowReplaced");
#endif
    HookThreadIfNecessary();

    // Send message down pipe...
    TCHAR buf[1024];
    _snprintf(buf, 1024, "ShowWindow(hWnd=%08X)", hWnd);
    PipeMessage(buf);
    // Call the original API function and return the result...
    return ShowWindowTrampoline(hWnd, nCMDShow);
}


// A structure used to record information about a hook we have inserted. A list of these
// is maintained in HookRecords.
struct HookRecord
{
    BYTE* address;                              // The address we put the hook into.
    BYTE replacedcode[MAXREPSIZE];              // The code we overwrote.
    int replacedsize;                           // Amount we actually replaced.
    BYTE trampcode[MAXREPSIZE];                 // The same (or similar!) code for the
                                                // trampoline function.
    int trampsize;                              // Size of trampoline code.
};
vector<HookRecord> HookRecords;

// Reverse the effects of all the InterceptAPI() calls we have made since we started up.
void UnInterceptAPIS()
{
    DWORD dwOldProtect;
    vector<HookRecord>::iterator i;
    for (i = HookRecords.begin(); i != HookRecords.end(); ++i)
    {
        VirtualProtect((void *)i->address, i->replacedsize, PAGE_WRITECOPY, &dwOldProtect);
        memcpy_s(i->address, i->replacedsize, i->replacedcode, i->replacedsize);
        VirtualProtect((void *)i->address, i->replacedsize, dwOldProtect, &dwOldProtect);
    }
    HookRecords.clear();
    FlushInstructionCache(GetCurrentProcess(), NULL, NULL);
}

struct TrampolineBlock
{
    void* address;                              // The address of the block.
    int used;                                   // The number of 'slots' used.
};
const int TrampolineBlockSlotSize = 128;
const int TrampolineDist = 0x40000000;
void* TrampolineBlockMinAddr;
void* TrampolineBlockMaxAddr;
int TrampolineBlockPageSize;
int TrampolineBlockMaxSlots;
vector<TrampolineBlock> TrampolineBlocks;

void TrampolineBlockInit()
{
    SYSTEM_INFO si;
    GetSystemInfo(&si);
    TrampolineBlockMinAddr = si.lpMinimumApplicationAddress;
    TrampolineBlockMaxAddr = si.lpMaximumApplicationAddress;
    TrampolineBlockPageSize = si.dwAllocationGranularity;
    TrampolineBlockMaxSlots = TrampolineBlockPageSize / TrampolineBlockSlotSize;
#if defined(DebugLog)
    TCHAR msg[1024];
    _snprintf(msg, 1024, "TrampolineBlockInit: Page size: %08X, Min:%016llX, Max:%016llX",
        TrampolineBlockPageSize, TrampolineBlockMinAddr, TrampolineBlockMaxAddr);
    Log(msg);
#endif
}
void TrampolineBlockCleanup()
{
    vector<TrampolineBlock>::iterator i;
    for (i = TrampolineBlocks.begin(); i != TrampolineBlocks.end(); ++i)
        VirtualFree(i->address, TrampolineBlockPageSize, MEM_RELEASE);
}

// Allocate memory for a 'trampoline' function near the given address, where
// 'near' means within range of a 32-bit relative offset, to allow us to be
// able to patch instructions more easily.
void *AllocTrampoline(void* nearaddr, int size)
{
    // First see if we already have space in a nearby block we've already
    // allocated...
    vector<TrampolineBlock>::iterator i;
    for (i = TrampolineBlocks.begin(); i != TrampolineBlocks.end(); ++i)
    {
        int dist = (int)i->address - (int)nearaddr;
        if (dist < 0) dist = -dist;
        if(dist > TrampolineDist) continue;
        if (i->used >= TrampolineBlockMaxSlots) continue;
        Log("Allocated trampoline in existing block");
        return (BYTE*)i->address + i->used++ * TrampolineBlockSlotSize;
    }
    // Nope, we need a new block - search backwards first...
    MEMORY_BASIC_INFORMATION mi;
    void *checkaddress = (void*)((__int64)nearaddr - ((__int64)nearaddr % TrampolineBlockPageSize) - TrampolineBlockPageSize);
    void *min = (BYTE*)nearaddr - TrampolineDist;
    if (min < TrampolineBlockMinAddr) min = TrampolineBlockMinAddr;
    while (checkaddress > min) {
        if (VirtualQuery(checkaddress, &mi, sizeof(mi)) == 0) {
            Log("VirtualQuery failed");
            continue;
        }
        if (mi.State == MEM_FREE) {
            if (VirtualAlloc(checkaddress, TrampolineBlockPageSize, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE) == 0) {
                Log("VirtualAlloc failed");
                continue;
            }
            TrampolineBlock* ti = new TrampolineBlock();
            ti->address = checkaddress;
            ti->used = 1;
            TrampolineBlocks.push_back(*ti);
            Log("Allocated trampoline in new reverse-searched block");
            return checkaddress;
        }
        checkaddress = (BYTE*)checkaddress - TrampolineBlockPageSize;
    }
    // That didn't work - same again but searching forwards...
    checkaddress = (void*)((__int64)nearaddr - ((__int64)nearaddr % TrampolineBlockPageSize) + TrampolineBlockPageSize);
    void *max = (BYTE*)nearaddr + TrampolineDist;
    if (max > TrampolineBlockMaxAddr) max = TrampolineBlockMaxAddr;
    while (checkaddress < max) {
        if (VirtualQuery(checkaddress, &mi, sizeof(mi)) == 0) {
            Log("VirtualQuery failed");
            continue;
        }
        if (mi.State == MEM_FREE) {
            if (VirtualAlloc(checkaddress, TrampolineBlockPageSize, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE) == 0) {
                Log("VirtualAlloc failed");
                continue;
            }
            TrampolineBlock* ti = new TrampolineBlock();
            ti->address = checkaddress;
            ti->used = 1;
            TrampolineBlocks.push_back(*ti);
            Log("Allocated trampoline in new forward-searched block");
            return checkaddress;
        }
        checkaddress = (BYTE*)checkaddress + TrampolineBlockPageSize;
    }
    return NULL;
}

bool InterceptAPI(const char* c_szDllName, const char* c_szApiName,
    void* dwReplaced, void** ppTrampoline)
{
    TCHAR message[256];
    DWORD dwOldProtect;
    try
    {

#if defined(DebugMessages)
        _snprintf(message, 256, "BPInjAgent: Attaching to %s::%s", c_szDllName, c_szApiName);
        PipeMessage(message);
#endif

        BYTE* addressToIntercept = (BYTE*)GetProcAddress(GetModuleHandle((char*)c_szDllName), (char*)c_szApiName);
        if (addressToIntercept == NULL)
        {
            _snprintf(message, 256, "BPInjAgent: Could not GetProcAddress for %s::%s", c_szDllName, c_szApiName);
            PipeMessage(message);
            return false;
        }

        // Follow jump(s) to get to the real code (because there won't be enough
        // room for us to overwrite if there's only a jump there)...
        while (*addressToIntercept == 0xFF && *(addressToIntercept + 1) == 0x25) {
            Log("Following jump");
            INT64 offset = *((INT32*)(addressToIntercept + 2));
            addressToIntercept += offset + 6;
            addressToIntercept = *((BYTE**)addressToIntercept);
#if defined(DebugMessages)
            _snprintf(message, 256, "BPInjAgent: Followed jump offset %016llX to %016llX", offset, addressToIntercept);
            PipeMessage(message);
#endif
        }

        // Create a record to store details of what we've done, so we can put it back
        // later...
        Log("Creating hook record");
        HookRecord* hookrec = new HookRecord;
        hookrec->address = addressToIntercept;
        hookrec->trampsize = 0;
        hookrec->replacedsize = 0;
        BYTE* repout = hookrec->replacedcode;
        BYTE* trampout = hookrec->trampcode;

        // Allocate memory for a 'trampoline' function, which serves as a replacement
        // for the original and can be called in the same way. It's 8 + replacesize + HOOKSIZE
        // + 8, because we'll be putting there a) the address of the replacement function(
        // to allow a short jmp for the overwrite), b) the original (overwritten) first part
        // of the real function, c) a jump into the original, and d) the address of the original.
        int trampBufSize = 8 + MAXREPSIZE + HOOKSIZE + 8;
        void* bufTrampoline = AllocTrampoline(addressToIntercept, trampBufSize);
        if (bufTrampoline == NULL) {
            PipeMessage("BPInjAgent: ERROR: Unable to allocate trampoline block");
            return false;
        }
        void *trampStart = (BYTE*)bufTrampoline + 8; // Address trampoline function starts
        *(ppTrampoline) = trampStart;
        __int64 trampdist = (__int64)trampStart - (__int64)addressToIntercept;
#if defined(DebugMessages)
        _snprintf(message, 256, "BPInjAgent: Trampoline offset is %16llX", trampdist);
        PipeMessage(message);
#endif
        // Decide how much code we need to replace. This is going to be a MINIMUM
        // of HOOKSIZE, since we're overwriting that much, but potentially longer
        // so we're not jumping back in half way through an instruction! At the
        // same time, generate the equivalent (often the same, sometimes modified)
        // code for our trampoline function.
        BYTE* rip = addressToIntercept;
        PipeMemDump("BPInjAgent: Original function start:", rip, HOOKSIZE + 15);
        while (hookrec->replacedsize < HOOKSIZE) {
            int len = 0, prefixlen = 0;
            BYTE op1 = *rip;
            BYTE *mrip = rip;
            BYTE rex = 0;
            bool relocatedisp32 = false;
            int imm = 0;
            BYTE prefs[16];

            // Prefixes first...
            while (op1 == 0xf0 || op1 == 0xf2 || op1 == 0xf3 || op1 == 0x26 ||
                op1 == 0x2e || op1 == 0x36 || op1 == 0x3e || op1 == 0x64 ||
                op1 == 0x65 || op1 == 0x66 || op1 == 0x67) {
                prefs[prefixlen++] = op1;
                op1 = *++rip;
            }

            // REX prefixes...
            if ((op1 & 0xf0) == 0x40) {
                prefs[prefixlen++] = op1;
                rex = op1;
                op1 = *++rip;
            }

            BYTE op2 = *(rip + 1);
            BYTE op3 = *(rip + 2);
            BYTE op4 = *(rip + 3);
            if (op1 == 0xc7 && op2 == 0x44 && op3 == 0x24) {
                // mov [rsp+imm8],imm32
                len = 8;
            }
            else if (op1 == 0xe8 || op1 == 0xe9) {
                // call/jmp rip+imm32
                len = 5;
                relocatedisp32 = true;
            }
            else if (op1 == 0x8b && op2 == 0x05) {
                // mov rax,[rip+imm32]
                len = 6;
                relocatedisp32 = true;
            }
            else if ((op1 >= 0x00 && op1 <= 0x03) || (op1 >= 0x08 && op1 <= 0x0b) ||
                (op1 >= 0x10 && op1 <= 0x13) || (op1 >= 0x18 && op1 <= 0x1b) ||
                (op1 >= 0x20 && op1 <= 0x23) || (op1 >= 0x28 && op1 <= 0x2b) ||
                (op1 >= 0x30 && op1 <= 0x33) || (op1 >= 0x38 && op1 <= 0x3b) ||
                op1 == 0x62 || op1 == 0x63 || op1 == 0x69 || op1 == 0x6b ||
                (op1 >= 0x80 && op1 <= 0x8f || op1 == 0xff)) {
                // various  (op2 is R/M)
                int sib = 0, disp = 0;
                BYTE mod = (op2 >> 6) & 3;
                if (mod != 3) {
                    BYTE rm = op2 & 0x07;
                    if (mod == 2)
                        disp = 4;
                    else if (mod == 1)
                        disp = 1;
                    else if (rm == 5)
                        disp = 4;
                    sib = rm == 4 ? 1 : 0;
                    if (sib) {
                        if (op3 & 7 == 5) {
                            if (mod == 1)
                                disp++;
                            else
                                disp += 4;
                        }
                    }
                    if (mod == 0 && rm == 5) {
                        relocatedisp32 = true;
                    }
                }
                if (op1 == 0x80 || op1 == 0x83)
                    imm = 1;
                else if(op1 == 0x81)
                    imm = 4;
                len = 2 + sib + disp + imm;
#if defined(DebugLog)
                _snprintf(message, 256, "MOD:%d, SIB:%d, DISP:%d, IMM:%d", mod, sib, disp, imm);
                Log(message);
#endif
            }
            else if (op1 >= 0x70 && op1 <= 0x7f) {
                // Short conditional jumps
                // TODO: what if they're outside our range!!
                len = 2;
            }
            else if (op1 >= 0x50 && op1 <= 0x61) {
                // pushes and pops
                len = 1;
            }
            else if (op1 == 0xf6 && op2 == 0x04 && op3 == 0x25) {
                // test [imm64],imm8
                len = 8;
            }
            else if (op1 >= 0xb0 && op1 <= 0xb7) {
                // mov rrr, imm8
                len = 2;
            }
            else if (op1 >= 0xb8 && op1 <= 0xbf) {
                // mov rrr, imm32/64
                if (rex == 0x48)
                    len = 9;
                else
                    len = 5;
            }
            else if (op1 == 0xcc) {
                // int 3
                len = 1;
            }
            if (len == 0) {
                // We're unable to 'disassemble' this instruction, so we have no
                // choice but to abort interception of this API function.
                _snprintf(message, 256, "BPInjAgent: ERROR: Unable to decode instruction at offset %d", mrip - addressToIntercept);
                PipeMessage(message);
                return false;
            }
            rip += len;
            len += prefixlen;
            memcpy_s(repout, len, mrip, len);
            repout += len;
            hookrec->replacedsize += len;
            memcpy_s(trampout, len, mrip, len);
            trampout += len;
            hookrec->trampsize += len;
#if defined(DebugLog)
            _snprintf(message, 256, "Instruction len: %d", len);
            Log(message);
#endif
            if (relocatedisp32) {
                // The last 4 bytes of the instruction (or further back
                // if there's an immediate) we just wrote need
                // to be adjusted, as they are a relative 32 bit offset
                // from the instruction pointer.
                DWORD* disp32 = (DWORD*)((BYTE*)trampout - 4 - imm);
                *disp32 = *disp32 - trampdist;
            }
        }

        if (hookrec->replacedsize > MAXREPSIZE) {
            PipeMessage("BPInjAgent: ERROR: Unable to replace code - too large");
            return false;
        }

       // We can keep the HookRecord now, since we've decided we're able to do it
       // and we're about to start making actual changes...
       HookRecords.push_back(*hookrec);

        // Copy the code from the start of the target function to the
        // start of the trampoline function.
       memcpy_s(trampStart, hookrec->trampsize, hookrec->trampcode, hookrec->trampsize);

        // Add a jump back from the trampoline, going to the relevant offset in
        // the target function, i.e. after the bytes we have already copied.
        HOOKBLOCK jmpback;
        void* backaddress = addressToIntercept + hookrec->replacedsize;
        jmpback.offset = 0;
        memcpy_s((BYTE*)trampStart + hookrec->trampsize, HOOKSIZE, &jmpback, HOOKSIZE);
        *(void**)((BYTE*)trampStart + hookrec->trampsize + HOOKSIZE) = backaddress;
        // At the start is the address to be used by the jump-back...
        *(void**)bufTrampoline = dwReplaced;
        PipeMemDump("BPInjAgent: Trampoline buffer:", (BYTE*)bufTrampoline, trampBufSize);

        // Overwrite the start of the target function to make it jump straight
        // to our replacement function...
        VirtualProtect((void *)addressToIntercept, HOOKSIZE, PAGE_WRITECOPY, &dwOldProtect);
        jmpback.offset = (__int64)bufTrampoline - (__int64)((BYTE*)addressToIntercept + 6);
        memcpy_s(addressToIntercept, HOOKSIZE, &jmpback, HOOKSIZE);
        VirtualProtect((void *)addressToIntercept, HOOKSIZE, dwOldProtect, &dwOldProtect);
        PipeMemDump("BPInjAgent: Original entry overwritten as: ", addressToIntercept, HOOKSIZE + 1);

        FlushInstructionCache(GetCurrentProcess(), NULL, NULL);
        Log("Flushed instruction cache");

        return true;
    }
    catch (...)
    {
        _snprintf(message, 256, "BPInjAgent: Exception attaching to %s::%s", c_szDllName, c_szApiName);
        PipeMessage(message);
        return false;
    }
}


/**
 * Extracts the (potentially escaped) parameter from the given command, starting
 * from the specified index.
 *
 * After extraction, *index will be the start index of the next parameter or the
 * length of the string if there are no more parameters.
 *
 * Params:
 *  cmd : The command string from which to extract the parameter
 *  index : Pointer to the string index from which to start extracting the parameter.
 *          This will be set to index of the next parameter or the length of the
 *          string after the parameter has been extracted.
 *
 * Returns:
 *  The parameter found within the given command at the specified index.
 */
string ExtractParam(string cmd, int* index)
{
    int currindex = *index; // The current index
    int len = cmd.length(); // The length of the command

    string accum = ""; // The accumulator into which the parameter is appended

    // Loop over the commas appending a single comma where a double is found 
    // (ie. where ",," replace with ","; where "," end param and exit function)
    // If we hit the end of the string, return the accumulated param
    // Other return points are dealt with within the loop.
    while (currindex < len)
    {
        int nextComma = cmd.find(",", currindex);

        if (nextComma == -1) // no more commas - append the rest of cmd to the accum and return
        {
            *index = len;
            return accum.append(cmd, currindex, len - currindex);
        }
        // Check if comma is at end of string, or next character is not a comma
        // If either is the case, then we can just return what we've got and move on.
        if (nextComma == len - 1 || cmd[nextComma + 1] != ',')
        {
            *index = nextComma + 1; // ie. 'len' if comma is at EOS - next param otherwise.
            return accum.append(cmd, currindex, nextComma - currindex);
        }
        // now we're into escaping territory - start building up the accumulator
        // We want to append up to and including the first comma (currindex, 1+nextComma-currindex)
        // then, then move 'currindex' to after *both* commas and go round the loop again.
        accum.append(cmd, currindex, 1 + nextComma - currindex);
        // Now shift currindex to point to the next char after the two commas
        currindex = 2 + nextComma;
    }
    // If we're here, it's because we ran out of command to accumulate.
    // Set the index to the length of the command and return the accumulated value.
    *index = len;
    return accum;
}

/**
 * Process an incoming command received via the pipe from the application that
 * injected us.
 * Must result in a message being sent down the pipe starting with either "RESPONSE:"
 * or "FAILURE:".
 *
 * Commas can be escaped within parameters by doubling them in a similar style to
 * the escaping of double-quotes within VB - a double-comma within the command
 * string represents a single comma within the parameter, such that the string
 * "Hello,,World,Will,this,do?" represents 4 parameters, namely:-
 * "Hello,World"
 * "Will"
 * "this"
 * "do?"
 */
void ProcessIncomingCommand(string cmd)
{

    // Parse the command into its components...
    string op;
    vector<string> parms;
    int index = cmd.find(" ");
    if (index == -1)
    {
        op = cmd;
    }
    else
    {
        op = cmd.substr(0, index);
        index += 1;
        int len = cmd.length();
        while (index < len)
        {
            parms.push_back(ExtractParam(cmd, &index));
        }
    }

#if defined(DebugMessages)
    TCHAR buf[256];
    _snprintf(buf, 256, "DEBUG: Received command '%s'", cmd.c_str());
    PipeMessage(buf);
    for (unsigned int i = 0; i < parms.size(); i++)
    {
        _snprintf(buf, 256, "DEBUG: Command parameter '%s'", parms[i].c_str());
        PipeMessage(buf);
    }
#endif

    if (op == "quit")
    {
        UnhookAll();
        PipeMessage("RESPONSE:ok");
    }
    else
    {
        PipeMessage("FAILURE:Invalid agent command.");
    }
}

// A queue of outgoing messages which are waiting to be sent down the pipe
// to the host application.
queue<string> PipeMessageQueue;

// A mutex that synchronises access to the PipeMessageQueue.
HANDLE PipeMutex = NULL;

// The name of the pipe we will use for sending all messages. The process ID will
// be appended to create a unique name.
static LPCTSTR lpszPipename = "\\\\.\\pipe\\BluePrismInjectorPipe";
HANDLE hPipe = INVALID_HANDLE_VALUE;

HANDLE PipeSendEvent;
HANDLE PipeReadEvent;

// Initialise the pipe by setting things up locally and connecting to
// the server (host application). Returns true if successful, false
// otherwise
bool PipeInit()
{
    Log("PipeInit starting");

    // Generate the name for our pipe, which is a fixed name with our process ID
    // appended...
    TCHAR namebuf[256];
    TCHAR numbuf[9];
    strcpy_s(namebuf, sizeof(namebuf)/sizeof(TCHAR), lpszPipename);
    _snprintf(numbuf, 9, "%08X", GetCurrentProcessId());
    strcat_s(namebuf, sizeof(namebuf)/sizeof(TCHAR), numbuf);

    // Open pipe for reading and writing (i.e. connect to the server)
    hPipe = CreateFile(namebuf, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, NULL);
    if (hPipe == INVALID_HANDLE_VALUE)
    {
        Log("PipeInit failed to open pipe");
        return false;
    }
    // Set pipe mode...
    DWORD dwMode = PIPE_READMODE_MESSAGE;
    if (!SetNamedPipeHandleState(hPipe, &dwMode, NULL, NULL))
    {
        CloseHandle(hPipe);
        hPipe = INVALID_HANDLE_VALUE;
        Log("PipeInit failed to set pipe mode");
        return false;
    }

    // Create events for overlapped IO when sending and receiving
    PipeSendEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
    PipeReadEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

    return true;
}


void PipeClose()
{
    Log("PipeClose");
    // Close the pipe.
    if (hPipe != INVALID_HANDLE_VALUE)
        CloseHandle(hPipe);
}

// Place a message in the queue to be sent down the pipe to the server.
void PipeMessage(const char* msg)
{
    Log(msg);
    // Don't send zero length messages
    if (*msg == '\0')
        return;
    if (WaitForSingleObject(PipeMutex, 5000) != WAIT_OBJECT_0)
        return;
    string m = msg;
    PipeMessageQueue.push(move(m));
    ReleaseMutex(PipeMutex);
}

void PipeMemDump(const char* msg, BYTE* addr, int len)
{
#if defined(DebugMessages)
    TCHAR message[1024];
    TCHAR msg2[256];
    strncpy(message, msg, 1024);
    _snprintf(msg2, 256, "%08llX=", addr);
    strcat(message, msg2);

    TCHAR* ww = message + strlen(message);
    while(len--) {
        BYTE b = *(addr++);
        BYTE b1 = (b & 0xF0) >> 4;
        *(ww++) = b1 + (b1 <= 9 ? '0' : 'A' - 10);
        b1 = b & 0xF;
        *(ww++) = b1 + (b1 <= 9 ? '0' : 'A' - 10);
        *(ww++) = ' ';
    }
    *ww = '\0';
    PipeMessage(message);
#endif
}

// Flag used to tell our threads to terminate.
bool TerminatePipeThreads = false;



// Send a message down our named pipe. In the event of any failure,
// this function will silently abort, since there is nothing we can
// do about it anyway.
void PipeSendMessage(const char* msg, const size_t msglen)
{
    OVERLAPPED ov = { 0 };
    ov.hEvent = PipeSendEvent;

    if (hPipe != INVALID_HANDLE_VALUE)
    {
        try
        {
            DWORD cbWritten;
            // If the message is large, send it as several messages in chunks. Each partial
            // message is prefixed with '\0' so the receiver knows to stitch them all together.
            // This works around that fact that messages greater than the incoming buffer
            // size never appear to be received by the other end.
            int len;
            while ((len = strnlen_s(msg, msglen)) > MAX_PIPEMSG_SIZE)
            {
                char buf[MAX_PIPEMSG_SIZE + 1];
                *buf = '\0';
                memcpy_s(buf + 1, MAX_PIPEMSG_SIZE, msg, MAX_PIPEMSG_SIZE);
                if (!WriteFile(hPipe, buf, MAX_PIPEMSG_SIZE + 1, NULL, &ov))
                {
                    if (GetLastError() != ERROR_IO_PENDING)
                        return;
                    while (true)
                    {
                        if (WaitForSingleObject(PipeSendEvent, 100) == WAIT_OBJECT_0)
                            break;
                        if (TerminatePipeThreads)
                            return;
                    }
                    if (!GetOverlappedResult(hPipe, &ov, &cbWritten, FALSE))
                        return;
                    if (cbWritten == 0)
                        return;
                }
                msg += MAX_PIPEMSG_SIZE;
            }

            if (!WriteFile(hPipe, msg, len, NULL, &ov))
            {
                if (GetLastError() != ERROR_IO_PENDING)
                    return;
                while (true)
                {
                    if (WaitForSingleObject(PipeSendEvent, 100) == WAIT_OBJECT_0)
                        break;
                    if (TerminatePipeThreads)
                        return;
                }
            }
        }
        catch (...)
        {
            // Don't want to deal with any errors here - if we can't
            // send the message, we can't send it and that's that. But
            // we don't want to crash our host either!
        }
    }
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    return DefWindowProc(hWnd, message, wParam, lParam);
}

// Thread that listens for and responds to incoming messages on the pipe.
HANDLE hListenerThread;
DWORD WINAPI ListenerThread()
{
    Log("PipeListenerThread starting");

    const int buflen = 256;
    char buf[buflen];
    DWORD cbRead;
    bool bInitialised = false;

    OVERLAPPED ov = { 0 };
    ov.hEvent = PipeReadEvent;

    HINSTANCE hinst = GetModuleHandle(NULL);

    WNDCLASS wc;
    wc.style = 0;
    wc.lpfnWndProc = (WNDPROC)WndProc;
    wc.cbClsExtra = 0;
    wc.cbWndExtra = 0;
    wc.hInstance = hinst;
    wc.hIcon = NULL;
    wc.hCursor = NULL;
    wc.hbrBackground = NULL;
    wc.lpszMenuName = NULL;
    wc.lpszClassName = "BPInjAgentWndClass";
    if (!RegisterClass(&wc))
    {
#if defined(DebugMessages)
        PipeMessage("DEBUG:Failed to register window class");
#endif
    }

    HWND hwndMain = CreateWindow("BPInjAgentWndClass", "BPInjAgent Command Thread",
        WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT,
        CW_USEDEFAULT, CW_USEDEFAULT, (HWND)NULL,
        (HMENU)NULL, hinst, (LPVOID)NULL);
#if defined(DebugMessages)
    _snprintf(buf, buflen, "DEBUG:Created BPInjAgent window - %08X", hwndMain);
    PipeMessage(buf);
#endif

    while (!TerminatePipeThreads)
    {

        //      Temporarily disabling this as an experiment. Note that COM marshaling won't
        //      work without it though. And if this is disabled, all the window creation stuff
        //      above is unnecessary
        //
        //      MSG msg;
        //      while(bInitialised && PeekMessage(&msg,NULL,0,0,PM_REMOVE))
        //      {
        //          TranslateMessage(&msg);
        //          DispatchMessage(&msg);
        //      }

        if (!ReadFile(hPipe, buf, buflen, &cbRead, &ov))
        {
            if (GetLastError() != ERROR_IO_PENDING)
                break;
            while (true)
            {
                if (WaitForSingleObject(PipeReadEvent, 100) == WAIT_OBJECT_0)
                    break;
                if (TerminatePipeThreads)
                    return 0;
            }
            if (!GetOverlappedResult(hPipe, &ov, &cbRead, FALSE))
                return 0;
        }
        *(buf + cbRead) = '\0';
        string cmd = buf;
        if (!bInitialised)
        {
            CoInitialize(NULL);
            bInitialised = true;
        }
        ProcessIncomingCommand(cmd);
    }
    return 0;
}


// Thread that sends outgoing messages down the pipe
HANDLE hPipeSenderThread;
DWORD WINAPI PipeSenderThread()
{
    Log("PipeSenderThread starting");

    if (!PipeInit())
    {
        Log("PipeSenderThread exiting - could not initialise pipe");
        // Some unpleasant cleanup, might need improving...
        Sleep(1000);
        UnInterceptAPIS();
        ReleaseHooks();
        return 0;
    }

    // Hook any existing threads - only really relevant when we're attaching. See bug #4033.
    HookExistingThreads();

    Log("PipeSenderThread starting listener thread");

    // Start listener thread...
    DWORD dwThreadID;
    hListenerThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)ListenerThread, NULL, 0, &dwThreadID);

    Log("PipeSenderThread processing");

    try
    {
        while (!TerminatePipeThreads)
        {
            while (true)
            {
                if (TerminatePipeThreads)
                    break;
                if (WaitForSingleObject(PipeMutex, 100) == WAIT_OBJECT_0)
                {
                    if (PipeMessageQueue.empty())
                    {
                        ReleaseMutex(PipeMutex);
                        break;
                    }
                    string msg = PipeMessageQueue.front();
                    PipeMessageQueue.pop();
                    ReleaseMutex(PipeMutex);
                    Log("PipeSenderThread sending");
                    PipeSendMessage(msg.c_str(), msg.length());
                    Log("PipeSenderThread sent");
                }
            }
            Sleep(25);
        }
    }
    catch (...)
    {
        // Make sure the listener thread knows to stop too...
        TerminatePipeThreads = true;
        Log("PipeSenderThread exception");
    }

    WaitForSingleObject(hListenerThread, 1000);
    CloseHandle(hListenerThread);

    Log("PipeSenderThread terminating");

    return 0;
}


bool Unhooked = false;
// Unhook everything we have hooked, prior to detaching from the target application.
void UnhookAll(void)
{
    Log("UnhookAll");
    if (!Unhooked)
    {
        UnInterceptAPIS();
        ReleaseHooks();
        PipeMessage("BPInjAgent detached");
        // Try and get our outgoing message queue flushed so that the 'detached'
        // message gets through, since we want our host to see it.
        int retries = 0;
        while (true)
        {
            if (WaitForSingleObject(PipeMutex, 100) == WAIT_OBJECT_0)
            {
                bool empty = PipeMessageQueue.empty();
                ReleaseMutex(PipeMutex);
                if (empty)
                    break;
                Sleep(100);
            }
            retries++;
            if (retries > 25)
                break;
        }
        TerminatePipeThreads = true;
        // Give our threads a reasonable chance to terminate before closing
        // the pipe.
        WaitForSingleObject(hPipeSenderThread, 1000);
        CloseHandle(hPipeSenderThread);
        PipeClose();
        TrampolineBlockCleanup();
    }
    Unhooked = true;
    Log("UnhookAll finished");
}

// The only function we export - it initialises everything. We cause this to be
// called after we have injected and loaded the DLL. It's safe to do things here
// that can't be reliably done in DLLMain.
extern "C" __declspec(dllexport) void Init()
{
    Log("Init");

    // Create the mutex we use for synchronising access to the outgoing pipe
    // message queue.
    PipeMutex = CreateMutex(NULL, FALSE, NULL);
    if (!PipeMutex)
        return;

    TrampolineBlockInit();

    // Note that we can still use the pipe to send messages, even though we only just
    // started the thread that created it, because all we are doing is putting the
    // messages into a queue to be sent when possible.
    PipeMessage("BPInjAgent initialised : attaching");
    InterceptAPI("gdi32.dll", "CreateCompatibleDC", &CreateCompatibleDCReplaced, (void**)&CreateCompatibleDCTrampoline);
    InterceptAPI("gdi32.dll", "ExtTextOutW", &ExtTextOutWReplaced, (void**)&ExtTextOutWTrampoline);

    InterceptAPI("gdi32.dll", "TextOutA", &TextOutAReplaced, (void**)&TextOutATrampoline);
    InterceptAPI("user32.dll", "DrawTextA", &DrawTextAReplaced, (void**)&DrawTextATrampoline);
    InterceptAPI("user32.dll", "DrawTextW", &DrawTextWReplaced, (void**)&DrawTextWTrampoline);

    InterceptAPI("user32.dll", "GetDC", &GetDCReplaced, (void**)&GetDCTrampoline);
    InterceptAPI("user32.dll", "GetDCEx", &GetDCExReplaced, (void**)&GetDCExTrampoline);
    InterceptAPI("user32.dll", "BeginPaint", &BeginPaintReplaced, (void**)&BeginPaintTrampoline);
    InterceptAPI("user32.dll", "EndPaint", &EndPaintReplaced, (void**)&EndPaintTrampoline);
    InterceptAPI("user32.dll", "ReleaseDC", &ReleaseDCReplaced, (void**)&ReleaseDCTrampoline);
    InterceptAPI("gdi32.dll", "DeleteDC", &DeleteDCReplaced, (void**)&DeleteDCTrampoline);
    InterceptAPI("gdi32.dll", "BitBlt", &BitBltReplaced, (void**)&BitBltTrampoline);
    InterceptAPI("gdi32.dll", "PatBlt", &PatBltReplaced, (void**)&PatBltTrampoline);
    InterceptAPI("user32.dll", "FillRect", &FillRectReplaced, (void**)&FillRectTrampoline);
    InterceptAPI("user32.dll", "ShowWindow", &ShowWindowReplaced, (void**)&ShowWindowTrampoline);
    InterceptAPI("user32.dll", "SetParent", &SetParentReplaced, (void**)&SetParentTrampoline);
    InterceptAPI("user32.dll", "SetWindowPos", &SetWindowPosReplaced, (void**)&SetWindowPosTrampoline);
    InterceptAPI("gdi32.dll", "SetTextAlign", &SetTextAlignReplaced, (void**)&SetTextAlignTrampoline);

    // The following two also cover CreateWindowA and CreateWindowW as defined by the API, since they
    // are just aliases.
    InterceptAPI("user32.dll", "CreateWindowExA", &CreateWindowExAReplaced, (void**)&CreateWindowExATrampoline);
    InterceptAPI("user32.dll", "CreateWindowExW", &CreateWindowExWReplaced, (void**)&CreateWindowExWTrampoline);

    //  InterceptAPI("user32.dll", "CreateDialogIndirectParamA",(DWORD)CreateDialogIndirectParamAReplaced,(DWORD)CreateDialogIndirectParamATrampoline);
    //  InterceptAPI("user32.dll", "CreateDialogParamA",(DWORD)CreateDialogParamAReplaced,(DWORD)CreateDialogParamATrampoline);
    //  InterceptAPI("user32.dll", "DialogBoxParamA",(DWORD)DialogBoxParamAReplaced,(DWORD)DialogBoxParamATrampoline);

    // Start the pipe sender thread...
    DWORD dwThreadID;
    hPipeSenderThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)PipeSenderThread, NULL, 0, &dwThreadID);

    PipeMessage("BPInjAgent ready");

    Log("Init finished");

}

BOOL APIENTRY DllMain(HINSTANCE hInst, DWORD dwReason, LPVOID lpReserved)
{
    switch (dwReason)
    {
    case DLL_PROCESS_ATTACH:
        Log("DllMain: Attach");
        break;
    case DLL_PROCESS_DETACH:
        Log("DllMain: Detach");
        UnhookAll();
        break;
    }
    return TRUE;
}

