// BpInjAgent.cpp : Defines the entry point for the DLL application.
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
// at c:\bpinjagent.log.
// #define DebugLog

#if defined(DebugLog)
ofstream LogFile;
HANDLE LogMutex = NULL;
void _Log(string msg)
{
    if (LogMutex == NULL)
        LogMutex = CreateMutex(NULL, FALSE, NULL);
    if (WaitForSingleObject(LogMutex, 5000) != WAIT_OBJECT_0)
        return;
    try
    {
        LogFile.open("c:\\bpinjagent.log", ios::out | ios::app);
        LogFile << msg << '\n';
        LogFile.close();
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

// Class ID definitions for COM classes that we are interested in 'deep-hooking'.
// Any other COM objects will not be looked at too closely. This is due to issues
// such as one found when investigating bug #3742, where the target application
// was crashing when trying to create an ADO.Connection object.
// Specifically, when we see an IClassFactory being created, we will only pursue
// that class factory further if it is a factory for one of these classes. They
// are referenced in an if statement to that effect later in the file.
DEFINE_GUID(CLSID_ApexGrid, 0xA8C3B720, 0x0B5A, 0x101B, 0xB2, 0x2E, 0x00, 0xAA, 0x00, 0x37, 0xB2, 0xFC);
DEFINE_GUID(CLSID_MsFlexGrid, 0x6262D3A0, 0x531B, 0x11CF, 0x91, 0xF6, 0xC2, 0x86, 0x3C, 0x38, 0x5E, 0x30);
DEFINE_GUID(CLSID_DBGrid, 0x00028C00, 0x0000, 0x0000, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
DEFINE_GUID(CLSID_SSTabControl, 0xBDC217C5, 0xED16, 0x11CD, 0x95, 0x6C, 0x00, 0x00, 0xC0, 0x4E, 0x4C, 0x0A);
DEFINE_GUID(CLSID_DateTimePicker, 0x20DD1B9E, 0x87C4, 0x11D1, 0x8B, 0xE3, 0x00, 0x00, 0xF8, 0x75, 0x4D, 0xA1);
DEFINE_GUID(CLSID_MsListView50, 0x58DA8D8A, 0x9D6A, 0x101B, 0xAF, 0xC0, 0x42, 0x10, 0x10, 0x2A, 0x8D, 0xA7);
DEFINE_GUID(CLSID_MsListView60, 0xBDD1F04B, 0x858B, 0x11D1, 0xB1, 0x6A, 0x00, 0xC0, 0xF0, 0x28, 0x36, 0x28);
DEFINE_GUID(CLSID_TreeView, 0xC74190B6, 0x8589, 0x11D1, 0xB1, 0x6A, 0x00, 0xC0, 0xF0, 0x28, 0x36, 0x28);
DEFINE_GUID(CLSID_FarPoint, 0xB02F3641, 0x766B, 0x11CE, 0xAF, 0x28, 0xC3, 0xA2, 0xFB, 0xE7, 0x6A, 0x13);

// Class ID for an Internet Explorer Active X control
DEFINE_GUID(CLSID_InternetExplorer, 0x8856F961, 0x340A, 0x11D0, 0xA9, 0x6B, 0x00, 0xC0, 0x4F, 0xD7, 0x05, 0xA2);

// Defines a trampoline function. This needs maxoffset+5 bytes of code that
// the compiler can't optimise away, where maxoffset is the maximum offset as
// defined in the API signatures (apisigs) table below, and the 5 is the length
// of the jump instruction that will be appended to that.
#define TRAMPOLINE __asm {nop} __asm {nop} __asm {nop} __asm {nop} __asm {nop} __asm {nop} __asm {nop} __asm {nop} __asm {nop} __asm {nop} __asm {nop} __asm {ret}


// A table of API function signatures and the offset to use for interception
// when they are encountered. The first five bytes are the first five bytes of
// code in the function, and are used to identify it - the next byte is the
// offset to be used when jumping in to the function, which is also the number
// of bytes that need to be copied elsewhere. The highest of these offset figures
// is used to determine the maximum amount of space required for a trampoline
// function, by adding 5 to it. See TRAMPOLINE above.
const int num_apisigs = 30;
BYTE apisigs[num_apisigs][6] =
{
    {0xB8,0x87,0x11,0x00,0x00,5},
    {0xB8,0x2A,0x11,0x00,0x00,5},
    {0xB8,0x68,0x11,0x00,0x00,5},
    {0x56,0x8B,0x74,0x24,0x0C,5},
    {0x68,0x00,0x00,0x01,0x00,5},
    {0x55,0x8B,0xEC,0x83,0xEC,6},
    {0xB8,0x18,0x12,0x00,0x00,5},
    {0xB8,0xFE,0x11,0x00,0x00,5},
    {0xB8,0x0F,0x12,0x00,0x00,5},
    {0x55,0x8B,0xEC,0x6A,0x01,5},
    {0x55,0x8B,0xEC,0x6A,0x00,5},
    {0x55,0x8B,0xEC,0x83,0xEC,6},
    {0x55,0x8B,0xEC,0x33,0xC0,5},
    {0x8B,0xFF,0x55,0x8B,0xEC,5},
    {0x8B,0x4C,0x24,0x04,0x56,5},
    {0xB8,0x86,0x11,0x00,0x00,5},
    {0xB8,0x91,0x11,0x00,0x00,5},
    {0x8B,0xC0,0x56,0x8B,0x74,7},
    {0x8B,0xC0,0x55,0x8B,0xEC,5},
    {0xB8,0x59,0x11,0x00,0x00,5},
    {0xB8,0x63,0x11,0x00,0x00,5},
    {0xB8,0x2B,0x12,0x00,0x00,5},
    {0xB8,0x92,0x11,0x00,0x00,5},
    {0xB8,0x34,0x11,0x00,0x00,5},
    {0xB8,0xB7,0x11,0x00,0x00,5},
    {0x8B,0xFF,0x55,0x8B,0xEC,5},
    {0xB8,0x11,0x12,0x00,0x00,5},
    {0xB8,0x22,0x12,0x00,0x00,5},
    {0xB8,0x72,0x11,0x00,0x00,5}
};

#define FAR_JUMP_OPCODES            0x25ff
#define FAR_JUMP_OFFSET_SIZE        6
#define DEFAULT_OFFSET_SIZE         5

void UnhookAll(void);
void PipeMessage(LPCTSTR lpszMsg);


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

// Get the Dispatch ID of the given name from an IDispatch interface
bool GetDispID(IDispatch* id, string name, DISPID& dispid)
{
    HRESULT res;
    _bstr_t member = name.c_str();
    LPOLESTR pmember = (wchar_t*)member;
    res = id->GetIDsOfNames(IID_NULL, &pmember, 1, LOCALE_SYSTEM_DEFAULT, &dispid);
    if (FAILED(res))
    {
        TCHAR buf[256];
        _snprintf(buf, 256, "FAILURE:Failed to get dispid of %s - %08X", name.c_str(), res);
        PipeMessage(buf);
        return false;
    }
    return true;
}

bool SetProperty(IDispatch* id, string name, VARIANT* value)
{
    DISPID dispid;
    if (!GetDispID(id, name, dispid))
        return false;

    DISPPARAMS dparms;
    dparms.cArgs = 1;
    dparms.rgvarg = value;
    DISPID dispidNamed = DISPID_PROPERTYPUT;
    dparms.cNamedArgs = 1;
    dparms.rgdispidNamedArgs = &dispidNamed;
    HRESULT res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYPUT, &dparms, NULL, NULL, NULL);
    if (FAILED(res))
    {
        TCHAR buf[256];
        _snprintf(buf, 256, "FAILURE:Failed to invoke property setter - %08X", res);
        PipeMessage(buf);
        return false;
    }
    return true;
}

// Get the named property of the given IDispatch, with the result being another IDispatch...
bool GetPropertyAsIDispatch(IDispatch* id, string name, IDispatch**result)
{
    TCHAR buf[256];
    DISPID dispid;
    if (!GetDispID(id, name, dispid))
        return false;
    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VARIANT varResult;
    HRESULT res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to invoke property getter on %s - %08X", name.c_str(), res);
        PipeMessage(buf);
        return false;
    }
    if (varResult.vt != VT_DISPATCH)
    {
        _snprintf(buf, 256, "FAILURE:Value for %s was not IDispatch", name.c_str());
        PipeMessage(buf);
        return false;
    }
    *result = varResult.pdispVal;
    return true;
}

// Get the named property of the given IDispatch, with the result being another IDispatch.
// The property is indexed by a string parameter.
bool GetPropertyIndexedByStringAsIDispatch(IDispatch* id, string name, string index, IDispatch**result)
{
    TCHAR buf[256];
    DISPID dispid;
    if (!GetDispID(id, name, dispid))
        return false;
    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG nargs[1];
    nargs[0].vt = VT_BSTR;
    nargs[0].bstrVal = SysAllocString(str_to_wstr(index).c_str());
    dparms.cArgs = 1;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = &(nargs[0]);
    VARIANT varResult;
    HRESULT res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to find %s in %s - %08X", index.c_str(), name.c_str(), res);
        PipeMessage(buf);
        return false;
    }
    if (varResult.vt != VT_DISPATCH)
    {
        _snprintf(buf, 256, "FAILURE:Value for %s was not IDispatch", name.c_str());
        PipeMessage(buf);
        return false;
    }
    *result = varResult.pdispVal;
    return true;
}
// Get the named property of the given IDispatch, with the result being another IDispatch.
// The property is indexed by an int parameter. This function also takes a precalculated dispid
// which is usefull in loop optimisations.
bool GetPropertyIndexedByIntAsIDispatch(IDispatch* id, DISPID dispid, string name, int index, IDispatch**result)
{
    // Invoke the property getter...
    TCHAR buf[256];
    DISPPARAMS dparms;
    VARIANTARG nargs[1];
    nargs[0].vt = VT_INT;
    nargs[0].intVal = index;
    dparms.cArgs = 1;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = &(nargs[0]);
    VARIANT varResult;
    HRESULT res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to find item %d in %s - %08X", index, name.c_str(), res);
        PipeMessage(buf);
        return false;
    }
    if (varResult.vt != VT_DISPATCH)
    {
        _snprintf(buf, 256, "FAILURE:Value for %s was not IDispatch", name.c_str());
        PipeMessage(buf);
        return false;
    }
    *result = varResult.pdispVal;
    return true;
}

// Get the named property of the given IDispatch, with the result being another IDispatch.
// The property is indexed by an int parameter.
bool GetPropertyIndexedByIntAsIDispatch(IDispatch* id, string name, int index, IDispatch**result)
{
    DISPID dispid;
    if (!GetDispID(id, name, dispid))
        return false;

    if (!GetPropertyIndexedByIntAsIDispatch(id, dispid, name, index, result))
        return false;

    return true;
}

// Get the named property of the given IDispatch, with the result being an int...
bool GetPropertyAsInt(IDispatch* id, string name, int* result)
{
    TCHAR buf[256];
    DISPID dispid;
    if (!GetDispID(id, name, dispid))
        return false;
    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VARIANT varResult;
    HRESULT res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to invoke property getter on %s - %08X", name.c_str(), res);
        PipeMessage(buf);
        return false;
    }
    switch (varResult.vt)
    {
    case VT_I2:
    case VT_I4:
    case VT_INT:
        *result = varResult.intVal;
        return true;
    default:
        _snprintf(buf, 256, "FAILURE:Value for %s was an integer", name.c_str());
        PipeMessage(buf);
        return false;
    }
}

// Get the named property of the given IDispatch, with the result being a string...
bool GetPropertyAsString(IDispatch* id, string name, string* result)
{
    TCHAR buf[256];
    DISPID dispid;
    if (!GetDispID(id, name, dispid))
        return false;
    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VARIANT varResult;
    HRESULT res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to invoke property getter on %s - %08X", name.c_str(), res);
        PipeMessage(buf);
        return false;
    }
    switch (varResult.vt)
    {
    case VT_BSTR:
        *result = wstr_to_str(varResult.bstrVal);
        SysFreeString(varResult.bstrVal);
        return true;
    default:
        _snprintf(buf, 256, "FAILURE:Value for %s was not a string", name.c_str());
        PipeMessage(buf);
        return false;
    }
}

// Invoke the named method of the given IDispatch...
bool InvokeMethod(IDispatch* id, string name)
{
    TCHAR buf[256];
    DISPID dispid;
    if (!GetDispID(id, name, dispid))
        return false;
    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VARIANT varResult;
    HRESULT res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_METHOD, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to invoke method %s - %08X", name.c_str(), res);
        PipeMessage(buf);
        return false;
    }
    return true;
}

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


HWND WINAPI CreateDialogIndirectParamATrampoline(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit)
{
    TRAMPOLINE;
}

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


HWND WINAPI CreateDialogParamATrampoline(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit)
{
    TRAMPOLINE;
}

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

HWND WINAPI DialogBoxIndirectParamATrampoline(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit)
{
    TRAMPOLINE;
}

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


HWND WINAPI DialogBoxParamATrampoline(HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM lParamInit)
{
    TRAMPOLINE;
}

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


HWND WINAPI CreateWindowExATrampoline(
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
    TRAMPOLINE;
}


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

HWND WINAPI CreateWindowExWTrampoline(
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
    TRAMPOLINE;
}


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



HWND WINAPI SetParentTrampoline(HWND hWndChild, HWND hWndNewParent)
{
    TRAMPOLINE;
}

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


int WINAPI FillRectTrampoline(HDC hDC, CONST RECT *lprc, HBRUSH hbr)
{
    TRAMPOLINE;
}

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

int WINAPI ReleaseDCTrampoline(HWND hWnd, HDC hDC)
{
    TRAMPOLINE;
}


int WINAPI DeleteDCTrampoline(HDC hDC)
{
    TRAMPOLINE;
}

HDC WINAPI GetDCTrampoline(HWND hWnd)
{
    TRAMPOLINE;
}

HDC WINAPI CreateDCATrampoline(
    LPCTSTR lpszDriver,        // driver name
    LPCTSTR lpszDevice,        // device name
    LPCTSTR lpszOutput,        // not used; should be NULL
    CONST DEVMODE* lpInitData)  // optional printer data
{
    TRAMPOLINE;
}


HDC WINAPI CreateCompatibleDCTrampoline(HDC hdc)
{
    TRAMPOLINE;
}

HDC WINAPI GetWindowDCTrampoline(HWND hWnd)
{
    TRAMPOLINE;
}

BOOL WINAPI EndPaintTrampoline(HWND hWnd, CONST PAINTSTRUCT *lpPaint)
{
    TRAMPOLINE;
}

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

HDC WINAPI BeginPaintTrampoline(HWND hwnd, LPPAINTSTRUCT lpPaint)
{
    TRAMPOLINE;
}

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


HDC WINAPI GetDCExTrampoline(HWND hWnd, HRGN hrgnClip, DWORD flags)
{
    TRAMPOLINE;
}


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

BOOL  WINAPI BitBltTrampoline(
    HDC hdcDest,
    int x,
    int y,
    int cx,
    int cy,
    HDC hdcSrc,
    int nXSrc,
    int nYSrc,
    DWORD rop
)
{
    TRAMPOLINE;
}

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

BOOL  WINAPI PatBltTrampoline(
    HDC dc,
    int x,
    int y,
    int w,
    int h,
    DWORD rop
)
{
    TRAMPOLINE;
}

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

int WINAPI DrawTextATrampoline(
    HDC hDC,          // handle to DC
    LPCTSTR lpString, // text to draw
    int nCount,       // text length
    LPRECT lpRect,    // formatting dimensions
    UINT uFormat      // text-drawing options
)
{
    TRAMPOLINE;
}


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
        strcat(buf, "\")");
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


int WINAPI DrawTextWTrampoline(
    HDC hDC,          // handle to DC
    LPCWSTR lpString, // text to draw
    int nCount,       // text length
    LPRECT lpRect,    // formatting dimensions
    UINT uFormat      // text-drawing options
)
{
    TRAMPOLINE;
}


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
        strcat(buf, "\")");
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


BOOL WINAPI TextOutATrampoline(
    HDC hdc,           // handle to DC
    int nXStart,       // x-coordinate of starting position
    int nYStart,       // y-coordinate of starting position
    LPCTSTR lpString,  // character string
    int cbString       // number of characters
)
{
    TRAMPOLINE;
}

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
        strcat(buf, "\")");
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

BOOL WINAPI ExtTextOutWTrampoline(
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
    TRAMPOLINE;
}

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
        strcat(buf, "\")");
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


UINT  WINAPI SetTextAlignTrampoline(HDC hDC, UINT align)
{
    TRAMPOLINE;
}

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

BOOL WINAPI SetWindowPosTrampoline(
    HWND hWnd,
    HWND hWndInsertAfter,
    int X,
    int Y,
    int cx,
    int cy,
    UINT uFlags
)
{
    TRAMPOLINE;
}

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

BOOL WINAPI ShowWindowTrampoline(HWND hWnd, int nCMDShow)
{
    TRAMPOLINE;
}

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

struct IClassFactoryReplacement
{
    CLSID ClassID;
    void* Interface;        // The interface itself.
    void** Vtable;          // The vtable pointer, which is really always *Interface and
                            // just stored here to save insane levels of indirection.
    HRESULT(STDMETHODCALLTYPE *OriginalQueryInterface)(IUnknown *pCF, REFIID riid, void **ppv);
    HRESULT(STDMETHODCALLTYPE *OriginalCreateInstance)(IClassFactory* pCF, REFGUID riid, void** ppv);
    HRESULT(STDMETHODCALLTYPE *OriginalCreateInstanceLic)(IClassFactory* pCF, IUnknown* pOuter, IUnknown* pReserved, REFGUID riid, BSTR key, void** ppv);
};
vector<IClassFactoryReplacement> IClassFactoryReplacements;
void HookClassFactory(REFCLSID rclsid, void** intf, int type);
const int HookCF_IClassFactory = 0;
const int HookCF_IClassFactory2 = 1;
const int HookCF_IUnknown = 2;


// The following table contains information about all the COM objects we are aware of
// that have an IDispatch interface.
struct DispatchableObject
{
    void* Interface;
    CLSID ClassID;
};
vector<DispatchableObject> DispatchableObjects;

// Called when we see that a new 'interesting' COM object has been created, i.e. one
// that is not just a factory for other objects, or an IUnknown interface that we
// expect to be later queried for a real interface.
void NewCOMObject(REFCLSID rclsid, REFIID riid, void**intf)
{
    USES_CONVERSION;
    LPOLESTR cs;
    StringFromCLSID(rclsid, &cs);
    TCHAR* cst = OLE2T(cs);
    TCHAR buf[1024];

#if defined(DebugMessages)
    LPOLESTR iid;
    StringFromIID(riid, &iid);
    TCHAR* iidt = OLE2T(iid);
    _snprintf(buf, 1024, "DEBUG: NewCOMObject, clsid=%s, iid=%s, interface=%08X", cst, iidt, intf);
    PipeMessage(buf);
    CoTaskMemFree(iid);
#endif

    if (IsEqualIID(riid, IID_IDispatch))
    {
#if defined(DebugMessages)
        PipeMessage("DEBUG: NewCOMObject interface is IDispatch - storing");
#endif
        DispatchableObject obj;
        obj.ClassID = rclsid;
        obj.Interface = intf;
        DispatchableObjects.push_back(obj);
        _snprintf(buf, 1024, "IDispatch(clsid=%s,interface=%08X)", cst, intf);
        PipeMessage(buf);
    }
    else
    {
#if defined(DebugMessages)
        PipeMessage("DEBUG: NewCOMObject interface is not IDispatch so ignoring");
#endif
    }

    CoTaskMemFree(cs);
}


// Replacement function for the CreateInstance method of any IClassFactory that gets
// created during CoGetClassObject.
HRESULT STDMETHODCALLTYPE IClassFactory_CreateInstance_Hook(IClassFactory* pCF, REFGUID riid, void** ppv)
{
#if defined(DebugMessages)
    USES_CONVERSION;
    TCHAR buf[1024];
    LPOLESTR iid;
    StringFromIID(riid, &iid);
    TCHAR* iidt = OLE2T(iid);
    _snprintf(buf, 1024, "DEBUG: IClassFactory_CreateInstance_Hook for iid=%s", iidt);
    PipeMessage(buf);
    CoTaskMemFree(iid);
#endif

    try
    {

        vector<IClassFactoryReplacement>::const_iterator i;
        for (i = IClassFactoryReplacements.begin(); i != IClassFactoryReplacements.end(); ++i)
        {
            if (i->Vtable == *((void**)pCF))
            {
#if defined(DebugMessages)
                PipeMessage("DEBUG: IClassFactory_CreateInstance_Hook - found target");
#endif
                HRESULT result = i->OriginalCreateInstance(pCF, riid, ppv);
#if defined(DebugMessages)
                PipeMessage("DEBUG: IClassFactory_CreateInstance_Hook - called original");
#endif
                if (result == S_OK)
                {
#if defined(DebugMessages)
                    _snprintf(buf, 1024, "DEBUG: IClassFactory_CreateInstance_Hook - OriginalCreateInstance ppv=%08X", *ppv);
                    PipeMessage(buf);
#endif
                    if (IsEqualIID(riid, IID_IUnknown))
                    {
#if defined(DebugMessages)
                        PipeMessage("DEBUG: IClassFactory_CreateInstance_Hook - hooking created IUnknown");
#endif
                        HookClassFactory(i->ClassID, (void**)*ppv, HookCF_IUnknown);
                    }
                    else
                    {
                        NewCOMObject(i->ClassID, riid, (void**)*ppv);
                    }
                }
#if defined(DebugMessages)
                else
                {
                    PipeMessage("DEBUG: Original CreateInstance failed");
                }
#endif
                return result;
            }
        }
        // If it gets here, we have hooked an interface that we don't have a record of.
        // It should never happen, but if it does we have probably broken the target
        // application.
#if defined(DebugMessages)
        PipeMessage("DEBUG: IClassFactory_CreateInstance_Hook - no target");
#endif
        *ppv = NULL;
        return OLE_E_CANTCONVERT;
    }
    catch (...)
    {
#if defined(DebugMessages)
        PipeMessage("DEBUG: IClassFactory_CreateInstance_Hook - exception occurred");
#endif 
        *ppv = NULL;
        return OLE_E_CANTCONVERT;
    }
}

// Replacement function for the CreateInstance method of any IClassFactory2 that gets
// created during CoGetClassObject or is queried via IClassFactory. It's the same as
// IClassFactory_CreateInstance_Hook, except for the extra parameters.
HRESULT STDMETHODCALLTYPE IClassFactory2_CreateInstanceLic_Hook(IClassFactory* pCF, IUnknown* pOuter, IUnknown* pReserved, REFGUID riid, BSTR key, void** ppv)
{
#if defined(DebugMessages)
    USES_CONVERSION;
    TCHAR buf[1024];
    LPOLESTR iid;
    StringFromIID(riid, &iid);
    TCHAR* iidt = OLE2T(iid);
    _snprintf(buf, 1024, "DEBUG: IClassFactory_CreateInstanceLic_Hook for iid=%s", iidt);
    PipeMessage(buf);
    CoTaskMemFree(iid);
#endif

    vector<IClassFactoryReplacement>::const_iterator i;
    for (i = IClassFactoryReplacements.begin(); i != IClassFactoryReplacements.end(); ++i)
    {
        if (i->Vtable == *((void**)pCF) && i->OriginalCreateInstanceLic != NULL)
        {
#if defined(DebugMessages)
            PipeMessage("DEBUG: IClassFactory_CreateInstanceLic_Hook - found target");
#endif
            HRESULT result = i->OriginalCreateInstanceLic(pCF, pOuter, pReserved, riid, key, ppv);
            if (result == S_OK)
            {
                if (IsEqualIID(riid, IID_IUnknown))
                {
#if defined(DebugMessages)
                    PipeMessage("DEBUG: IClassFactory_CreateInstanceLic_Hook - hooking created IUnknown");
#endif
                    HookClassFactory(i->ClassID, (void**)*ppv, HookCF_IUnknown);
                }
                else
                {
                    NewCOMObject(i->ClassID, riid, (void**)*ppv);
                }
            }
            return result;
        }
    }
    // If it gets here, we have hooked an interface that we don't have a record of.
    // It should never happen, but if it does we have probably broken the target
    // application.
#if defined(DebugMessages)
    PipeMessage("DEBUG: IClassFactory_CreateInstanceLic_Hook - no target");
#endif
    *ppv = NULL;
    return OLE_E_CANTCONVERT;
}


// Replacement function for the QueryInterface method of any IClassFactory that gets
// created during CoGetClassObject, as well as other objects that we don't know the
// identitiy of but might want to, e.g. if an IUnknown is created via an IClassFactory(2)
HRESULT STDMETHODCALLTYPE IUnknown_QueryInterface_Hook(IUnknown* pCF, REFIID riid, void **ppv)
{
#if defined(DebugMessages)
    USES_CONVERSION;
    TCHAR buf[1024];
    LPOLESTR iid;
    StringFromIID(riid, &iid);
    TCHAR* iidt = OLE2T(iid);
    _snprintf(buf, 1024, "DEBUG: IUnknown_QueryInterface_Hook for iid=%s, pCF=%08X", iidt, pCF);
    PipeMessage(buf);
    CoTaskMemFree(iid);
#endif

    vector<IClassFactoryReplacement>::const_iterator i;
    for (i = IClassFactoryReplacements.begin(); i != IClassFactoryReplacements.end(); ++i)
    {
        if (i->Vtable == *((void**)pCF))
        {
#if defined(DebugMessages)
            PipeMessage("DEBUG: IUnknown_QueryInterface_Hook - found target");
#endif
            HRESULT result = i->OriginalQueryInterface(pCF, riid, ppv);
            if (result == S_OK)
            {
                if (IsEqualIID(riid, IID_IClassFactory2))
                {
#if defined(DebugMessages)
                    PipeMessage("DEBUG: Hooking queried IClassFactory2");
#endif
                    HookClassFactory(i->ClassID, (void**)*ppv, HookCF_IClassFactory2);
                }
                else
                {
                    NewCOMObject(i->ClassID, riid, (void**)*ppv);
                }
            }
            return result;
        }
    }
    // If it gets here, we have hooked an interface that we don't have a record of.
    // It should never happen, but if it does we have probably broken the target
    // application.
#if defined(DebugMessages)
    PipeMessage("DEBUG: IClassFactory_QueryInterface_Hook - no target");
#endif
    *ppv = NULL;
    return OLE_E_CANTCONVERT;
}

// Hook an IClassFactory interface so we can catch instances of the actual object
// being created from it. We hook QueryInterface as well as CreateInstance, because
// VB6 seems to call QueryInterface first, and only uses CreateInstance if that fails.
// 'intf' is the pointer to the interface.
// 'type' is the interface type, one of HookCF_xxxx.
void HookClassFactory(REFCLSID rclsid, void** intf, int type)
{

#if defined(DebugMessages)
    TCHAR buf[1024];
    _snprintf(buf, 1024, "DEBUG: HookClassFactory, intf(pCF)=%08X", intf);
    PipeMessage(buf);
#endif

    DWORD dwOldProtect;

    vector<IClassFactoryReplacement>::iterator i;
    for (i = IClassFactoryReplacements.begin(); i != IClassFactoryReplacements.end(); ++i)
    {
        if (i->Vtable == *intf)
        {
#if defined(DebugMessages)
            PipeMessage("DEBUG: HookClassFactory - already hooked");
#endif
            if (type == HookCF_IClassFactory2 && i->OriginalCreateInstanceLic == NULL)
            {
                // We have to hook additional functions that we didn't before.
                // This is equivalent to just a small portion of the code below, i.e.
                // doing the difference between IClassFactory and IClassFactory2.
#if defined(DebugMessages)
                PipeMessage("DEBUG: ...but was previously IClassFactory and is now IClassFactory2!");
#endif
                i->OriginalCreateInstanceLic = (HRESULT(STDMETHODCALLTYPE *)(IClassFactory* pCF, IUnknown* pOuter, IUnknown* pReserved, REFGUID riid, BSTR key, void** ppv))*(i->Vtable + 7);
                VirtualProtect(i->Vtable, 32, PAGE_EXECUTE_READWRITE, &dwOldProtect);
                *(i->Vtable + 7) = &IClassFactory2_CreateInstanceLic_Hook;
                VirtualProtect(i->Vtable, 32, dwOldProtect, &dwOldProtect);
            }
            return;
        }
    }

    // Create a record of what we've hooked...
    IClassFactoryReplacement rep;
    rep.ClassID = rclsid;
    rep.Interface = intf;
    rep.Vtable = (void**)*intf;
    rep.OriginalQueryInterface = (HRESULT(STDMETHODCALLTYPE *)(IUnknown *pCF, REFIID riid, void **ppvObject))*(rep.Vtable + 0);
    if (type == HookCF_IClassFactory || type == HookCF_IClassFactory2)
        rep.OriginalCreateInstance = (HRESULT(STDMETHODCALLTYPE *)(IClassFactory* pCF, REFGUID riid, void** ppv))*(rep.Vtable + 3);
    else
        rep.OriginalCreateInstance = NULL;
    if (type == HookCF_IClassFactory2)
        rep.OriginalCreateInstanceLic = (HRESULT(STDMETHODCALLTYPE *)(IClassFactory* pCF, IUnknown* pOuter, IUnknown* pReserved, REFGUID riid, BSTR key, void** ppv))*(rep.Vtable + 7);
    else
        rep.OriginalCreateInstanceLic = NULL;
    IClassFactoryReplacements.push_back(rep);

    // Hook it by overwriting the relevant vtable entries with the addresses of our own
    // handlers. These will look up the originals in the record we created above when
    // they need to call them...
    VirtualProtect(rep.Vtable, 32, PAGE_EXECUTE_READWRITE, &dwOldProtect);
    *(rep.Vtable + 0) = &IUnknown_QueryInterface_Hook;
    if (type == HookCF_IClassFactory || type == HookCF_IClassFactory2)
        *(rep.Vtable + 3) = &IClassFactory_CreateInstance_Hook;
    if (type == HookCF_IClassFactory2)
        *(rep.Vtable + 7) = &IClassFactory2_CreateInstanceLic_Hook;
    VirtualProtect(rep.Vtable, 32, dwOldProtect, &dwOldProtect);
}


STDAPI CoCreateInstanceTrampoline(IN REFCLSID rclsid, IN LPUNKNOWN pUnkOuter, IN DWORD dwClsContext, IN REFIID riid, OUT LPVOID *ppv)
{
    TRAMPOLINE;
}


// A list of the Internet Explorer ActiveX control objects we have seen being
// created.
// TODO: We are going to have to remove them from this list somehow when they
// get destroyed!
vector<LPVOID> InternetExplorers;

STDAPI CoCreateInstanceReplaced(IN REFCLSID rclsid, IN LPUNKNOWN pUnkOuter, IN DWORD dwClsContext, IN REFIID riid, OUT LPVOID *ppv)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: CoGetClassObjectReplaced");
#endif
    HookThreadIfNecessary();

    // Call the original API function...
    HRESULT retval;
    retval = CoCreateInstanceTrampoline(rclsid, pUnkOuter, dwClsContext, riid, ppv);

    USES_CONVERSION;

    // Send message down pipe...
    TCHAR buf[1024];
    if (retval == S_OK)
    {

        if (IsEqualCLSID(rclsid, CLSID_InternetExplorer))
        {
#if defined(DebugMessages)
            PipeMessage("DEBUG: Internet Explorer ActiveX control created");
#endif
            InternetExplorers.push_back(*ppv);
        }

        LPOLESTR cs;
        StringFromCLSID(rclsid, &cs);
        TCHAR* cst = OLE2T(cs);
        _snprintf(buf, 1024, "CoCreateInstance(clsid=%s,retval=%08X)", cst, retval);
        PipeMessage(buf);
        CoTaskMemFree(cs);
    }
    else
    {
#if defined(DebugMessages)
        _snprintf(buf, 1024, "DEBUG: CoCreateInstance failed, retval=%08X)", retval);
        PipeMessage(buf);
#endif
    }

    return retval;
}

STDAPI CoGetClassObjectTrampoline(IN REFCLSID rclsid, IN DWORD dwClsContext, IN LPVOID pvReserved, IN REFIID riid, OUT LPVOID *ppv)
{
    TRAMPOLINE;
}

STDAPI CoGetClassObjectReplaced(IN REFCLSID rclsid, IN DWORD dwClsContext, IN LPVOID pvReserved, IN REFIID riid, OUT LPVOID *ppv)
{
#if defined(DebugMessages)
    PipeMessage("DEBUG: CoGetClassObjectReplaced");
#endif
    HookThreadIfNecessary();

#if defined(DebugMessages)
    PipeMessage("DEBUG: CoGetClassObjectReplaced calling original");
#endif

    // Call the original API function...
    HRESULT retval;
    retval = CoGetClassObjectTrampoline(rclsid, dwClsContext, pvReserved, riid, ppv);

#if defined(DebugMessages)
    PipeMessage("DEBUG: CoGetClassObjectReplaced called original");
#endif

    USES_CONVERSION;

    // Send message down pipe...
    TCHAR buf[1024];
    if (retval == S_OK)
    {
        LPOLESTR cs, iid;
        StringFromCLSID(rclsid, &cs);
        StringFromIID(riid, &iid);
        TCHAR* cst = OLE2T(cs);
        TCHAR* iidt = OLE2T(iid);
        _snprintf(buf, 1024, "CoGetClassObject(clsid=%s,iid=%s,retval=%08X)", cst, iidt, retval);
        PipeMessage(buf);
        CoTaskMemFree(cs);
        CoTaskMemFree(iid);
    }
    else
    {
#if defined(DebugMessages)
        _snprintf(buf, 1024, "DEBUG: CoGetClassObject failed, retval=%08X)", retval);
        PipeMessage(buf);
#endif
    }

    // See if a class factory was created...
    if (retval == S_OK && IsEqualIID(riid, IID_IClassFactory))
    {
        // See if it is one we want to pursue further...
        if (IsEqualCLSID(rclsid, CLSID_MsFlexGrid) ||
            IsEqualCLSID(rclsid, CLSID_ApexGrid) ||
            IsEqualCLSID(rclsid, CLSID_DBGrid) ||
            IsEqualCLSID(rclsid, CLSID_SSTabControl) ||
            IsEqualCLSID(rclsid, CLSID_DateTimePicker) ||
            IsEqualCLSID(rclsid, CLSID_MsListView50) ||
            IsEqualCLSID(rclsid, CLSID_MsListView60) ||
            IsEqualCLSID(rclsid, CLSID_TreeView) ||
            IsEqualCLSID(rclsid, CLSID_FarPoint))
        {
#if defined(DebugMessages)
            PipeMessage("DEBUG: Hooking newly created IClassFactory");
#endif
            HookClassFactory(rclsid, (void**)*ppv, HookCF_IClassFactory);
        }
        else
        {
#if defined(DebugMessages)
            PipeMessage("DEBUG: New IClassFactory is not of a supported type, so not hooked");
#endif
        }
    }
    // This is the only way I can get the Farpoint control 'recognised'
    else if (retval == S_OK && IsEqualIID(riid, IID_IClassFactory2))
    {
        if (IsEqualCLSID(rclsid, CLSID_FarPoint))
        {
#if defined(DebugMessages)
            PipeMessage("DEBUG: Hooking newly created IClassFactory2");
#endif
            HookClassFactory(rclsid, (void**)*ppv, HookCF_IClassFactory2);
        }
        else
        {
#if defined(DebugMessages)
            PipeMessage("DEBUG: New IClassFactory2 is not of a supported type, so not hooked");
#endif
        }
    }

#if defined(DebugMessages)
    PipeMessage("DEBUG: CoGetClassObjectReplaced returning");
#endif
    return retval;
}

// A structure used to record information about a hook we have inserted. A list of these
// is maintained in HookRecords.
struct HookRecord
{
    BYTE* address;              // The address we put the hook into.
    BYTE replacedcode[5];       // The code we overwrote.
};
vector<HookRecord> HookRecords;

// Reverse the effects of all the InterceptAPI() calls we have made since we started up.
void UnInterceptAPIS()
{
    DWORD dwOldProtect;
    vector<HookRecord>::iterator i;
    for (i = HookRecords.begin(); i != HookRecords.end(); ++i)
    {
        VirtualProtect((void *)i->address, 5, PAGE_WRITECOPY, &dwOldProtect);
        for (int j = 0; j < 5; j++)
            i->address[j] = i->replacedcode[j];
        VirtualProtect((void *)i->address, 5, dwOldProtect, &dwOldProtect);
    }
    HookRecords.clear();
}

bool InterceptAPI(const char* c_szDllName, const char* c_szApiName,
    DWORD dwReplaced, DWORD dwTrampoline)
{

#if defined(DebugMessages)
    TCHAR message[256];
#endif
    try
    {

#if defined(DebugMessages)
        _snprintf(message, 256, "BPInjAgent: Attaching to %s::%s", c_szDllName, c_szApiName);
        PipeMessage(message);
#endif
        int i, j;

        DWORD dwOldProtect;
        DWORD dwAddressToIntercept = (DWORD)GetProcAddress(GetModuleHandle((char*)c_szDllName), (char*)c_szApiName);
        if (dwAddressToIntercept == NULL)
        {
#if defined(DebugMessages)
            _snprintf(message, 256, "BPInjAgent: Could not GetProcAddress for %s::%s", c_szDllName, c_szApiName);
            PipeMessage(message);
#endif
            return false;
        }

        BYTE* api;

        // See if the first instruction of the function we are hooking is a jump
        // jump. If so, there is not room us to insert our code there, but we can
        // insert it at the target address of the jump instead...
        api = (BYTE*)dwAddressToIntercept;
        int offset;
        while (*api == 0xEB || *api == 0xE9)
        {
            switch (*api)
            {
            case 0xEB:
                offset = *((signed char*)(api + 1));    // This should sign extend the byte
                api += 2 + offset;
#if defined(DebugMessages)
                PipeMessage("BPInjAgent: Followed 8 bit relative jump to new hook target");
#endif
                break;
            case 0xE9:
                offset = *((int*)(api + 1));
                api += 5 + offset;
#if defined(DebugMessages)
                PipeMessage("BPInjAgent: Followed 32 bit relative jump to new hook target");
#endif
                break;
            }
        }
        dwAddressToIntercept = (DWORD)api;

        // Create a record to store details of what we've done, so we can put it back
        // later...
        HookRecord* hookrec = new HookRecord;
        hookrec->address = (BYTE*)dwAddressToIntercept;
        api = (BYTE*)dwAddressToIntercept;
        for (j = 0; j < 5; j++)
            hookrec->replacedcode[j] = *api++;
        HookRecords.push_back(*hookrec);

        // Find the signature of the API function in our table so we can
        // determine the offset to hook it at...
        BYTE* sig;
        offset = 0;
        for (i = 0; i < num_apisigs; i++)
        {
            sig = apisigs[i];
            api = (BYTE*)dwAddressToIntercept;
            for (j = 0; j < 5; j++)
                if (*sig++ != *api++)
                    break;
            if (j == 5)
            {
                offset = *sig;
                break;
            }
        }
        if (offset == 0)
        {
#if defined(DebugMessages)
            PipeMessage("BPInjAgent: WARNING: Function signature not found - guessing!");
#endif
            api = (BYTE*)dwAddressToIntercept;
            // Since the Windows 10 Fall Creators Update, Windows adds its own
            // function hooks to many of the functions we are hooking. Because
            // this overwrites the functions, we can't recognise them with the
            // signatures. Also, because the far jump instruction is bigger than
            // the default offset, if we use the default then we just end up
            // trying to jump to invalid addresses.
            offset =
                (*(unsigned short*)api == FAR_JUMP_OPCODES)
                ? FAR_JUMP_OFFSET_SIZE
                : DEFAULT_OFFSET_SIZE;
        }
        else
        {
#if defined(DebugMessages)
            _snprintf(message, 256, "BPInjAgent: Using function signature %d, offset=%d", i, offset);
            PipeMessage(message);
#endif
        }

        BYTE *pbTargetCode = (BYTE*)dwAddressToIntercept;
        BYTE *pbReplaced = (BYTE*)dwReplaced;
        BYTE *pbTrampoline = (BYTE*)dwTrampoline;

        // Change the protection of the trampoline region so that we can overwrite the
        // first offset+5 bytes. (offset+5 because we are inserting 'offset' bytes of
        // code from the target function, plus a jump instruction)
#if defined(DebugMessages)
        _snprintf(message, 256, "BPInjAgent: About to change protection at %08X", dwTrampoline);
        PipeMessage(message);
#endif
        VirtualProtect((void *)dwTrampoline, offset + 5, PAGE_EXECUTE_READWRITE, &dwOldProtect);

        // Copy 'offset' bytes of code from the start of the target function to the
        // start of the trampoline function.
#if defined(DebugMessages)
        strcpy(message, "BPInjAgent: replaced code: ");
        int so = strlen(message);
#endif
        for (i = 0; i < offset; i++)
        {
            BYTE b = *pbTargetCode++;
#if defined(DebugMessages)
            // Write the code we are replacing to our message buffer in
            // hex...
            BYTE b1 = (b & 0xF0) >> 4;
            *(message + so + i * 3) = b1 + (b1 <= 9 ? '0' : 'A' - 10);
            b1 = b & 0xF;
            *(message + so + i * 3 + 1) = b1 + (b1 <= 9 ? '0' : 'A' - 10);
            *(message + so + i * 3 + 2) = ' ';
#endif
            *pbTrampoline++ = b;
        }
#if defined(DebugMessages)
        *(message + so + i * 3) = '\0';
        PipeMessage(message);
#endif

        // Add a jump instruction to the trampoline, going to the relevant offset in
        // the target function, i.e. after the bytes we have already copied.
        pbTargetCode = (BYTE*)dwAddressToIntercept;
        *pbTrampoline++ = 0xE9;        // jump rel32
        *((signed int *)(pbTrampoline)) = (pbTargetCode + offset) - (pbTrampoline + 4);
        // Restore the protection on the trampoline function...
        VirtualProtect((void *)dwTrampoline, offset + 5, dwOldProtect, &dwOldProtect);

        // Overwrite the first 5 bytes of the target function to make it jump straight
        // to our replacement function...
        VirtualProtect((void *)dwAddressToIntercept, 5, PAGE_WRITECOPY, &dwOldProtect);
        *pbTargetCode++ = 0xE9;        // jump rel32
        *((signed int *)(pbTargetCode)) = pbReplaced - (pbTargetCode + 4);
        VirtualProtect((void *)dwAddressToIntercept, 5, dwOldProtect, &dwOldProtect);

        // Flush the instruction cache to make sure 
        // the modified code is executed.
        FlushInstructionCache(GetCurrentProcess(), NULL, NULL);
        return true;
    }
    catch (...)
    {
#if defined(DebugMessages)
        _snprintf(message, 256, "BPInjAgent: Exception attaching to %s::%s", c_szDllName, c_szApiName);
        PipeMessage(message);
#endif
        return false;
    }
}


// Get a pointer to the DispatchableObject record that corresponds to the given
// window handle. Returns NULL if not found.
DispatchableObject* GetDispatchableObjectFromHWnd(HWND wnd)
{
    // Search all our dispatchable objects...
    vector<DispatchableObject>::iterator i;
    for (i = DispatchableObjects.begin(); i != DispatchableObjects.end(); ++i)
    {
        VARIANT varResult;
        VariantInit(&varResult);
        IDispatch* id = (IDispatch*)i->Interface;
        DISPID dispid_hWnd;
        HRESULT res;

        // Get the dispid of the hWnd property, which should exist on all IDispatch
        // objects...
        OLECHAR FAR* szMember = L"hWnd";
        res = id->GetIDsOfNames(IID_NULL, &szMember, 1, LOCALE_SYSTEM_DEFAULT, &dispid_hWnd);
        if (SUCCEEDED(res))
        {
            DISPPARAMS parms;
            parms.cArgs = 0;
            parms.cNamedArgs = 0;
            res = id->Invoke(dispid_hWnd, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &parms, &varResult, NULL, NULL);
            if (SUCCEEDED(res))
            {
                if ((void*)varResult.uintVal == wnd)
                    return &(*i);
            }
        }
    }
    // We didn't find it...
    return NULL;
}

void Do_statusbar_read(HWND wnd)
{
    TCHAR buf[256];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    IDispatch* id = (IDispatch*)obj->Interface;
    int style;
    if (!GetPropertyAsInt(id, "Style", &style))
        return;

    //Handle simple style text
    if (style == 1)
    {
        string simpletext;
        if (!GetPropertyAsString(id, "SimpleText", &simpletext))
            return;

        //// Send the result...
        _snprintf(buf, 256, "RESPONSE:1\t%d", simpletext);
        PipeMessage(buf);
        return;
    }
    //Handle complex panels
    else
    {
        IDispatch* panels;
        if (!GetPropertyAsIDispatch(id, "Panels", &panels))
            return;

        int numpanels;
        if (!GetPropertyAsInt(panels, "Count", &numpanels))
            return;

        // Build 'header' of response data, i.e. number of panels
        string responsedata;
        _snprintf(buf, 256, "RESPONSE:%d", numpanels);
        responsedata = buf;

        // Get the dispid of the Item property...
        DISPID dispid;
        if (!GetDispID(panels, "Item", dispid))
        {
            PipeMessage("FAILURE:Failed to get dispid of Item property");
            return;
        }

        for (int index = 1; index <= numpanels; index++)
        {
            IDispatch* panel;
            if (!GetPropertyIndexedByIntAsIDispatch(panels, dispid, "Item", index, &panel))
                return;

            string text;
            if (!GetPropertyAsString(panel, "Text", &text))
                return;

            // Add the result to the response data.
            responsedata.append("\t");
            responsedata.append(text);

        }

        PipeMessage(responsedata.c_str());
        return;
    }

}
void Do_treeview_children(HWND wnd, LPCTSTR nodetext)
{
    TCHAR buf[256];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    VariantInit(&varResult);
    IDispatch* id = (IDispatch*)obj->Interface;

    // Get the Nodes collection...
    IDispatch* nodes;
    if (!GetPropertyAsIDispatch(id, "Nodes", &nodes))
        return;

    // Find the node we are going to get the children of...
    int nodecount;
    if (!GetPropertyAsInt(nodes, "Count", &nodecount))
    {
        nodes->Release();
        return;
    }
    IDispatch* node;
    int nodenum;
    for (nodenum = 0; nodenum < nodecount; nodenum++)
    {
        if (!GetPropertyIndexedByIntAsIDispatch(nodes, "Item", nodenum + 1, &node))
        {
            nodes->Release();
            return;
        }
        string thisnodetext;
        if (!GetPropertyAsString(node, "Text", &thisnodetext))
        {
            nodes->Release();
            return;
        }
        if (!strcmp(nodetext, thisnodetext.c_str()))
            break;
    }
    nodes->Release();

    if (nodenum == nodecount)
    {
        PipeMessage("FAILURE:Could not find the requested node");
        return;
    }

    // Get the child count...
    int childcount;
    if (!GetPropertyAsInt(node, "Children", &childcount))
    {
        node->Release();
        return;
    }

    string responsedata;
    _snprintf(buf, 256, "RESPONSE:%d", childcount);
    responsedata = buf;

    if (childcount > 0)
    {
        IDispatch* node2;
        string text;
        // Get the first child.
        if (!GetPropertyAsIDispatch(node, "Child", &node2))
        {
            node->Release();
            return;
        }
        if (!GetPropertyAsString(node2, "Text", &text))
        {
            node->Release();
            node2->Release();
            return;
        }
        responsedata.append("\t");
        responsedata.append(text);

        // Get the rest of the children...
        for (int i = 0; i < childcount - 1; i++)
        {
            IDispatch* thischild;
            if (!GetPropertyAsIDispatch(node2, "Next", &thischild))
                return;
            node2->Release();
            node2 = thischild;
            if (!GetPropertyAsString(node2, "Text", &text))
            {
                node2->Release();
                node->Release();
                return;
            }
            responsedata.append("\t");
            responsedata.append(text);
        }
        node2->Release();
    }
    node->Release();

    PipeMessage(responsedata.c_str());
    return;

}


void Do_treeview_siblings(HWND wnd, LPCTSTR nodetext)
{
    TCHAR buf[256];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    VariantInit(&varResult);
    IDispatch* id = (IDispatch*)obj->Interface;

    // Get the Nodes collection...
    IDispatch* nodes;
    if (!GetPropertyAsIDispatch(id, "Nodes", &nodes))
        return;

    // Find the node we are going to get the siblings of...
    int nodecount;
    if (!GetPropertyAsInt(nodes, "Count", &nodecount))
    {
        nodes->Release();
        return;
    }
    IDispatch* node;
    int nodenum;
    for (nodenum = 0; nodenum < nodecount; nodenum++)
    {
        if (!GetPropertyIndexedByIntAsIDispatch(nodes, "Item", nodenum + 1, &node))
        {
            nodes->Release();
            return;
        }
        string thisnodetext;
        if (!GetPropertyAsString(node, "Text", &thisnodetext))
        {
            nodes->Release();
            return;
        }
        if (!strcmp(nodetext, thisnodetext.c_str()))
            break;
    }
    nodes->Release();

    if (nodenum == nodecount)
    {
        PipeMessage("FAILURE:Could not find the requested node");
        return;
    }

    string responsedata;
    int siblingcount = 1;

    IDispatch* node2;
    string text;
    // Get the first sibling.
    if (!GetPropertyAsIDispatch(node, "FirstSibling", &node2))
    {
        node->Release();
        return;
    }
    if (!GetPropertyAsString(node2, "Text", &text))
    {
        node->Release();
        node2->Release();
        return;
    }
    responsedata.append(text);

    // Get the rest of the siblings...
    while (true)
    {
        IDispatch* thischild;
        if (!GetPropertyAsIDispatch(node2, "Next", &thischild))
            return;
        if (thischild == NULL)
            break;
        node2->Release();
        node2 = thischild;
        if (!GetPropertyAsString(node2, "Text", &text))
        {
            node2->Release();
            node->Release();
            return;
        }
        responsedata.append("\t");
        responsedata.append(text);
        siblingcount++;
    }
    node2->Release();

    node->Release();

    _snprintf(buf, 256, "RESPONSE:%d\t", siblingcount);
    string resp2 = buf;
    resp2.append(responsedata);

    PipeMessage(resp2.c_str());
    return;

}

void Do_treeview_ensurevisible(HWND wnd, LPCTSTR nodetext)
{
    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    VariantInit(&varResult);
    IDispatch* id = (IDispatch*)obj->Interface;

    // Get the Nodes collection...
    IDispatch* nodes;
    if (!GetPropertyAsIDispatch(id, "Nodes", &nodes))
        return;

    // Find the node we are going to get the siblings of...
    int nodecount;
    if (!GetPropertyAsInt(nodes, "Count", &nodecount))
    {
        nodes->Release();
        return;
    }
    IDispatch* node;
    int nodenum;
    for (nodenum = 0; nodenum < nodecount; nodenum++)
    {
        if (!GetPropertyIndexedByIntAsIDispatch(nodes, "Item", nodenum + 1, &node))
        {
            nodes->Release();
            return;
        }
        string thisnodetext;
        if (!GetPropertyAsString(node, "Text", &thisnodetext))
        {
            nodes->Release();
            return;
        }
        if (!strcmp(nodetext, thisnodetext.c_str()))
            break;
    }
    nodes->Release();

    if (nodenum == nodecount)
    {
        PipeMessage("FAILURE:Could not find the requested node");
        return;
    }

    if (!InvokeMethod(node, "EnsureVisible"))
        return;
    node->Release();

    PipeMessage("RESPONSE:OK");
}

void Do_treeview_select(HWND wnd, LPCTSTR nodetext)
{
    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    VariantInit(&varResult);
    IDispatch* id = (IDispatch*)obj->Interface;

    // Get the Nodes collection...
    IDispatch* nodes;
    if (!GetPropertyAsIDispatch(id, "Nodes", &nodes))
        return;

    // Find the node we are going to get the siblings of...
    int nodecount;
    if (!GetPropertyAsInt(nodes, "Count", &nodecount))
    {
        nodes->Release();
        return;
    }
    IDispatch* node;
    int nodenum;
    for (nodenum = 0; nodenum < nodecount; nodenum++)
    {
        if (!GetPropertyIndexedByIntAsIDispatch(nodes, "Item", nodenum + 1, &node))
        {
            nodes->Release();
            return;
        }
        string thisnodetext;
        if (!GetPropertyAsString(node, "Text", &thisnodetext))
        {
            nodes->Release();
            return;
        }
        if (!strcmp(nodetext, thisnodetext.c_str()))
            break;
    }
    nodes->Release();

    if (nodenum == nodecount)
    {
        PipeMessage("FAILURE:Could not find the requested node");
        return;
    }

    VARIANT value;
    value.vt = VT_BOOL;
    value.boolVal = true;
    if (!SetProperty(node, "Selected", &value))
        return;
    node->Release();

    PipeMessage("RESPONSE:OK");
}


void Do_treeview_count(HWND wnd)
{
    TCHAR buf[256];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }
    IDispatch* id = (IDispatch*)obj->Interface;

    IDispatch* nodes;
    if (!GetPropertyAsIDispatch(id, "Nodes", &nodes))
        return;
    int num;
    if (!GetPropertyAsInt(nodes, "Count", &num))
    {
        nodes->Release();
        return;
    }
    nodes->Release();
    _snprintf(buf, 256, "RESPONSE:%d", num);
    PipeMessage(buf);
}

void Do_listview_count(HWND wnd)
{
    TCHAR buf[256];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    VariantInit(&varResult);
    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid;
    HRESULT res;

    // Get the dispid of the Columns property...
    if (!GetDispID(id, "ListItems", dispid))
        return;

    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }
    // Send the result...
    IDispatch* cols;
    switch (varResult.vt)
    {
    case VT_DISPATCH:
        cols = varResult.pdispVal;

        // Get the dispid of the Count property...
        if (!GetDispID(cols, "Count", dispid))
            return;

        res = cols->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
        if (FAILED(res))
        {
            _snprintf(buf, 256, "FAILURE:Failed to invoke count property getter - %08X", res);
            PipeMessage(buf);
            return;
        }
        // Send the result...
        switch (varResult.vt)
        {
        case VT_I2:
        case VT_I4:
        case VT_INT:
            _snprintf(buf, 256, "RESPONSE:%d", varResult.uintVal);
            break;
        case VT_BSTR:
            _snprintf(buf, 256, "RESPONSE:%S", (char*)varResult.bstrVal);
            SysFreeString(varResult.bstrVal);
            break;
        default:
            // If this message is received, we need to look at supporting other
            // possible returned data types.
            PipeMessage("FAILURE:Can't interpret returned property data type");
            return;
        }
        PipeMessage(buf);
        return;

    default:
        PipeMessage("FAILURE:Expected dispatch result for ListItems property");
        return;
    }
}


void Do_listview_getselecteditemtext(HWND wnd)
{
    TCHAR buf[256];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    VariantInit(&varResult);
    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid;
    HRESULT res;

    // Get the dispid of the Columns property...
    if (!GetDispID(id, "SelectedItem", dispid))
        return;

    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }
    // Send the result...
    IDispatch* cols;
    switch (varResult.vt)
    {
    case VT_DISPATCH:
        cols = varResult.pdispVal;

        // Get the dispid of the Text property...
        if (!GetDispID(cols, "Text", dispid))
            return;

        res = cols->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
        if (FAILED(res))
        {
            _snprintf(buf, 256, "FAILURE:Failed to invoke index property getter - %08X", res);
            PipeMessage(buf);
            return;
        }
        // Send the result...
        switch (varResult.vt)
        {
        case VT_I2:
        case VT_I4:
        case VT_INT:
            _snprintf(buf, 256, "RESPONSE:%d", varResult.uintVal);
            break;
        case VT_BSTR:
            _snprintf(buf, 256, "RESPONSE:%S", (char*)varResult.bstrVal);
            SysFreeString(varResult.bstrVal);
            break;
        default:
            // If this message is received, we need to look at supporting other
            // possible returned data types.
            PipeMessage("FAILURE:Can't interpret returned property data type");
            return;
        }
        PipeMessage(buf);
        return;

    default:
        PipeMessage("FAILURE:Expected dispatch result for SelectedItem property");
        return;
    }
}

// Read the entire contents of an ListView20WndClass object. This can be achieved by individual
// property get calls
// The returned data is tab separated, with the first two entries being the number of
// rows and columns. Everything thereafter is cell contents, left to right, top to bottom.
// Tab separation ought to be safe in this context, because Microsoft also do it, see:
//   http://msdn.microsoft.com/en-us/library/aa239882(VS.60).aspx
void Do_listview_readall(HWND wnd)
{
    TCHAR buf[2048];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid;
    HRESULT res;
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;

    // Get the number of rows...
    if (!GetDispID(id, "ListItems", dispid))
        return;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VariantInit(&varResult);
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 2048, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }

    // Now get the count property...
    IDispatch* listitems;
    listitems = varResult.pdispVal;
    if (!GetDispID(listitems, "Count", dispid))
        return;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VariantInit(&varResult);
    res = listitems->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 2048, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }
    int numrows = varResult.uintVal;

    // Get the number of columns...
    if (!GetDispID(id, "ColumnHeaders", dispid))
        return;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VariantInit(&varResult);
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 2048, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }

    // Now get the count property..
    IDispatch* cols;
    cols = varResult.pdispVal;
    if (!GetDispID(cols, "Count", dispid))
        return;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VariantInit(&varResult);
    res = cols->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 2048, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }
    int numcols = varResult.uintVal;

    // Build 'header' of response data, i.e. number of rows and columns...
    string responsedata;
    _snprintf(buf, 256, "RESPONSE:%d\t%d", numrows, numcols);
    responsedata = buf;

    DISPID itemdispid;
    if (!GetDispID(listitems, "Item", itemdispid))
        return;

    wstring ws;
    string ns;

    for (int row = 1; row <= numrows; row++)
    {
        VARIANTARG rowargs[1];
        rowargs[0].vt = VT_INT;
        rowargs[0].intVal = row;
        dparms.cArgs = 1;
        dparms.cNamedArgs = 0;
        dparms.rgvarg = &(rowargs[0]);
        VariantInit(&varResult);
        res = listitems->Invoke(itemdispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
        if (FAILED(res))
        {
            _snprintf(buf, 2048, "FAILURE:Failed to invoke Item property getter - %08X", res);
            PipeMessage(buf);
            return;
        }
        IDispatch* item;
        item = varResult.pdispVal;
        if (!GetDispID(item, "Text", dispid))
            return;
        dparms.cArgs = 0;
        dparms.cNamedArgs = 0;
        dparms.rgvarg = args;
        VariantInit(&varResult);
        res = item->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
        if (FAILED(res))
        {
            _snprintf(buf, 2048, "FAILURE:Failed to invoke Text property getter - %08X", res);
            PipeMessage(buf);
            return;
        }

        // Add the result to the response data.
        responsedata.append("\t");
        switch (varResult.vt)
        {
        case VT_I2:
        case VT_I4:
        case VT_INT:
            _snprintf(buf, 256, "%d", varResult.uintVal);
            responsedata.append(buf);
            break;
        case VT_BSTR:
            ws = varResult.bstrVal;
            // We are doing a dodgy unicode conversion here, but that's because
            // all BPInjAgent communications are not unicode. We are going to
            // have to address this issue at some point.
            ns.assign(ws.begin(), ws.end());
            responsedata.append(ns.c_str());
            SysFreeString(varResult.bstrVal);
            break;
        default:
            // If this message is received, we need to look at supporting other
            // possible returned data types.
            PipeMessage("FAILURE:Can't interpret returned property data type");
            return;
        }

        DISPID subitemdispid;
        if (!GetDispID(item, "SubItems", subitemdispid))
            return;

        // Inner loop needs to get items.subitems(i)
        for (int col = 1; col < numcols; col++)
        {
            VARIANTARG colargs[1];
            colargs[0].vt = VT_INT;
            colargs[0].intVal = col;
            dparms.cArgs = 1;
            dparms.cNamedArgs = 0;
            dparms.rgvarg = &(colargs[0]);
            VariantInit(&varResult);
            res = item->Invoke(subitemdispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
            if (FAILED(res))
            {
                _snprintf(buf, 2048, "FAILURE:Failed to invoke SubItems.Items property getter - %08X", res);
                PipeMessage(buf);
                return;
            }

            // Add the result to the response data.
            responsedata.append("\t");
            switch (varResult.vt)
            {
            case VT_I2:
            case VT_I4:
            case VT_INT:
                _snprintf(buf, 256, "%d", varResult.uintVal);
                responsedata.append(buf);
                break;
            case VT_BSTR:
                ws = varResult.bstrVal;
                // We are doing a dodgy unicode conversion here, but that's because
                // all BPInjAgent communications are not unicode. We are going to
                // have to address this issue at some point.
                ns.assign(ws.begin(), ws.end());
                responsedata.append(ns.c_str());
                SysFreeString(varResult.bstrVal);
                break;
            default:
                // If this message is received, we need to look at supporting other
                // possible returned data types.
                PipeMessage("FAILURE:Can't interpret returned property data type");
                return;
            }


        }


    }

    PipeMessage(responsedata.c_str());

}


void Do_apex_cols(HWND wnd)
{
    TCHAR buf[256];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    VariantInit(&varResult);
    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid;
    HRESULT res;

    // Get the dispid of the Columns property...
    if (!GetDispID(id, "Columns", dispid))
        return;

    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }
    // Send the result...
    IDispatch* cols;
    switch (varResult.vt)
    {
    case VT_DISPATCH:
        cols = varResult.pdispVal;

        // Get the dispid of the Count property...
        if (!GetDispID(cols, "Count", dispid))
            return;

        res = cols->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
        if (FAILED(res))
        {
            _snprintf(buf, 256, "FAILURE:Failed to invoke count property getter - %08X", res);
            PipeMessage(buf);
            return;
        }
        // Send the result...
        switch (varResult.vt)
        {
        case VT_I2:
        case VT_I4:
        case VT_INT:
            _snprintf(buf, 256, "RESPONSE:%d", varResult.uintVal);
            break;
        case VT_BSTR:
            _snprintf(buf, 256, "RESPONSE:%S", (char*)varResult.bstrVal);
            SysFreeString(varResult.bstrVal);
            break;
        default:
            // If this message is received, we need to look at supporting other
            // possible returned data types.
            PipeMessage("FAILURE:Can't interpret returned property data type");
            return;
        }
        PipeMessage(buf);
        return;

    default:
        PipeMessage("FAILURE:Expected dispatch result for Columns property");
        return;
    }
}

void Do_apex_readcurrow(HWND wnd)
{
    TCHAR buf[2048];
    TCHAR msgbuf[2048];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid, dispiditem;
    HRESULT res;

    // Get the dispid of the Columns property...
    if (!GetDispID(id, "Columns", dispid))
        return;

    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VariantInit(&varResult);
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 2048, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }
    // Send the result...
    IDispatch* cols;
    int numcols;
    switch (varResult.vt)
    {
    case VT_DISPATCH:
        cols = varResult.pdispVal;

        // Get the dispid of the Count property...
        if (!GetDispID(cols, "Count", dispid))
            return;
        // Get the dispid of the Item property...
        if (!GetDispID(cols, "Item", dispiditem))
            return;
        VariantInit(&varResult);
        res = cols->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
        if (FAILED(res))
        {
            _snprintf(buf, 2048, "FAILURE:Failed to invoke count property getter - %08X", res);
            PipeMessage(buf);
            return;
        }


        *msgbuf = '\0';

        // Get the column count and then iterate through each column...
        numcols = varResult.intVal;
#if defined(DebugMessages)
        _snprintf(buf, 2048, "DEBUG: %d columns", numcols);
        PipeMessage(buf);
#endif
        for (int col = 0; col < numcols; col++)
        {
            // Get the column object...
            VariantInit(&varResult);
            VARIANTARG itemarg;
            itemarg.vt = VT_I4;
            itemarg.uintVal = col;
            dparms.cArgs = 1;
            dparms.rgvarg = &itemarg;
            res = cols->Invoke(dispiditem, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
            if (FAILED(res))
            {
                _snprintf(buf, 2048, "FAILURE:Failed to invoke item property getter - %08X", res);
                PipeMessage(buf);
                return;
            }
            if (varResult.vt != VT_DISPATCH)
            {
                PipeMessage("FAILURE:Column item is not a dispatch interface");
                return;
            }
            IDispatch* icol;
            icol = varResult.pdispVal;

#if defined(DebugMessages)
            _snprintf(buf, 2048, "DEBUG: Got dispatch for column %d", col);
            PipeMessage(buf);
#endif
            // Get the dispid of the Text property...
            if (!GetDispID(icol, "Text", dispid))
                return;

            // Get the text.
            VariantInit(&varResult);
            dparms.cArgs = 0;
            args = NULL;
            res = icol->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
            if (FAILED(res))
            {
                _snprintf(buf, 2048, "FAILURE:Failed to invoke text property getter - %08X", res);
                PipeMessage(buf);
                return;
            }
            if (strnlen_s(msgbuf, 2048) > 0)
                strncat_s(msgbuf, 2048, "\t", 1);
            int len;
            switch (varResult.vt)
            {
            case VT_I2:
            case VT_I4:
            case VT_INT:
                _snprintf(buf, 2048, "%d", varResult.uintVal);
                len = strnlen_s(buf, 2048);
                strncat_s(msgbuf, 2048, buf, len);
                break;
            case VT_BSTR:
                _snprintf(buf, 2048, "%S", (char*)varResult.bstrVal);
                len = strnlen_s(buf, 2048);
                strncat_s(msgbuf, 2048, buf, len);
                SysFreeString(varResult.bstrVal);
                break;
            default:
                // If this message is received, we need to look at supporting other
                // possible returned data types.
                PipeMessage("FAILURE:Can't interpret returned property data type");
                return;
            }

        }

        _snprintf(buf, 2048, "RESPONSE:%s", msgbuf);
        PipeMessage(buf);
        return;

    default:
        PipeMessage("FAILURE:Expected dispatch result for Columns property");
        return;
    }
}

// Read the entire contents of an MSFlexGrid object. This can be achieved by individual
// property get calls (i.e. get number of rows and columns, then use the textmatrix
// property get for each cell) but that is not efficent. This does the same thing in a
// single command.
// The returned data is tab separated, with the first two entries being the number of
// rows and columns. Everything thereafter is cell contents, left to right, top to bottom.
// Tab separation ought to be safe in this context, because Microsoft also do it, see:
//   http://msdn.microsoft.com/en-us/library/aa239882(VS.60).aspx
void Do_flex_readall(HWND wnd)
{
    TCHAR buf[2048];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid;
    HRESULT res;
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;

    // Get the number of rows...
    if (!GetDispID(id, "Rows", dispid))
        return;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VariantInit(&varResult);
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 2048, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }
    int numrows = varResult.uintVal;

    // Get the number of columns...
    if (!GetDispID(id, "Cols", dispid))
        return;
    dparms.cArgs = 0;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    VariantInit(&varResult);
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (FAILED(res))
    {
        _snprintf(buf, 2048, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }
    int numcols = varResult.uintVal;

    // Build 'header' of response data, i.e. number of rows and columns...
    string responsedata;
    _snprintf(buf, 256, "RESPONSE:%d\t%d", numrows, numcols);
    responsedata = buf;

    // Get the dispid of the TextMatrix property - we'll reuse this repeatedly inside
    // the loop!
    if (!GetDispID(id, "TextMatrix", dispid))
        return;

    wstring ws;
    string ns;

    for (int row = 0; row < numrows; row++)
    {
        for (int col = 0; col < numcols; col++)
        {
            VARIANTARG args[2];
            args[0].vt = VT_INT;
            args[0].intVal = col;
            args[1].vt = VT_INT;
            args[1].intVal = row;
            dparms.cArgs = 2;
            dparms.cNamedArgs = 0;
            dparms.rgvarg = &(args[0]);
            res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
            if (FAILED(res))
            {
                _snprintf(buf, 256, "FAILURE:Failed to invoke property getter - %08X", res);
                PipeMessage(buf);
                return;
            }
            // Add the result to the response data.
            responsedata.append("\t");
            switch (varResult.vt)
            {
            case VT_I2:
            case VT_I4:
            case VT_INT:
                _snprintf(buf, 256, "%d", varResult.uintVal);
                responsedata.append(buf);
                break;
            case VT_BSTR:
                ws = varResult.bstrVal;
                // We are doing a dodgy unicode conversion here, but that's because
                // all BPInjAgent communications are not unicode. We are going to
                // have to address this issue at some point.
                ns.assign(ws.begin(), ws.end());
                responsedata.append(ns.c_str());
                SysFreeString(varResult.bstrVal);
                break;
            default:
                // If this message is received, we need to look at supporting other
                // possible returned data types.
                PipeMessage("FAILURE:Can't interpret returned property data type");
                return;
            }

        }
    }
    PipeMessage(responsedata.c_str());

}

// Convert a COM argument passed as a string in a pipe command into a VARIANT to be
// passed to the actual COM object.
void SetArgument(string value, VARIANTARG* parg)
{
    if (value.find("~S") == 0)
    {
        parg->vt = VT_I2;
        parg->iVal = atoi(value.c_str() + 2);
    }
    else if (value.find("~D") == 0)
    {
        SYSTEMTIME stime;
        char buf[22];
        if (value.length() != 21)
        {
            parg->vt = VT_EMPTY;
            return;
        }
        strcpy(buf, value.c_str());
        // The input, in this case, must be in the following format:
        //  "~Dyyyy-mm-dd hh:MM:SS"
        // We just do our best to parse the correct thing out of what is given, without
        // any real attempt at validation - it is assumed that the caller (Application
        // Manager) will have formed the command correctly.
        buf[6] = '\0';
        stime.wYear = atoi(buf + 2);
        buf[9] = '\0';
        stime.wMonth = atoi(buf + 7);
        buf[12] = '\0';
        stime.wDay = atoi(buf + 10);
        buf[15] = '\0';
        stime.wHour = atoi(buf + 13);
        buf[18] = '\0';
        stime.wMinute = atoi(buf + 16);
        stime.wSecond = atoi(buf + 19);
        stime.wMilliseconds = 0;

        parg->vt = VT_DATE;
        int res = SystemTimeToVariantTime(&stime, &(parg->date));
#if defined(DebugMessages)
        if (res > 0)
            PipeMessage("DEBUG: Successfully set variant time");
        else
            PipeMessage("DEBUG: Setting variant time failed");
#endif
    }
    else
    {
        parg->vt = VT_INT;
        parg->intVal = atoi(value.c_str());
    }
}


void Do_property_get(HWND wnd, string propname, vector<string> &parms)
{
    TCHAR buf[256];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    VARIANT varResult;
    VariantInit(&varResult);
    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid;
    HRESULT res;

    // Get the dispid of the requested property...
    if (!GetDispID(id, propname, dispid))
        return;

    // Invoke the property getter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    // If there are any parameters, package them up.
    if (parms.size() > 0)
    {
        args = (VARIANTARG*)malloc(sizeof(VARIANTARG)*parms.size());
        if (args == NULL)
        {
            PipeMessage("FAILURE:Could not allocate memory for arguments");
            return;
        }
        VARIANTARG* parg = args;
        // Note that the parameters go in backwards...
        for (vector<string>::reverse_iterator i = parms.rbegin(); i != parms.rend(); i++)
            SetArgument(*i, parg++);
    }
    dparms.cArgs = parms.size();
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &dparms, &varResult, NULL, NULL);
    if (args != NULL)
        free(args);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to invoke property getter - %08X", res);
        PipeMessage(buf);
        return;
    }
    // Send the result...
    switch (varResult.vt)
    {
    case VT_I2:
    case VT_I4:
    case VT_INT:
        _snprintf(buf, 256, "RESPONSE:%d", varResult.uintVal);
        break;
    case VT_BSTR:
        _snprintf(buf, 256, "RESPONSE:%S", (char*)varResult.bstrVal);
        SysFreeString(varResult.bstrVal);
        break;
    case VT_DATE:
        SYSTEMTIME stime;
        VariantTimeToSystemTime(varResult.date, &stime);
        _snprintf(buf, 256, "RESPONSE:%04d-%02d-%02d %02d:%02d:%02d", stime.wYear, stime.wMonth, stime.wDay, stime.wHour, stime.wMinute, stime.wSecond);
        break;
    default:
        // If this message is received, we need to look at supporting other
        // possible returned data types.
        _snprintf(buf, 256, "FAILURE:Can't interpret returned property data type of %d", varResult.vt);
    }
    PipeMessage(buf);
}


void Do_property_set(HWND wnd, string propname, string value, vector<string> &parms)
{
    TCHAR buf[256];

    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(wnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid;
    HRESULT res;

    // Get the dispid of the requested property...
    if (!GetDispID(id, propname, dispid))
        return;

    // Invoke the property setter...
    DISPPARAMS dparms;
    VARIANTARG* args = NULL;
    // If there are any parameters, package them up.
    args = (VARIANTARG*)malloc(sizeof(VARIANTARG)*(parms.size() + 1));
    if (args == NULL)
    {
        PipeMessage("FAILURE:Could not allocate memory for arguments");
        return;
    }
    VARIANTARG* parg = args;
    // The first argument we add is the new value for the property...
    SetArgument(value, parg++);
    // Then any actual arguments... (Note that the parameters go in backwards)
    if (parms.size() > 0)
        for (vector<string>::reverse_iterator i = parms.rbegin(); i != parms.rend(); i++)
            SetArgument(*i, parg++);
    dparms.cArgs = parms.size() + 1;
    dparms.rgvarg = args;
    DISPID dispidNamed = DISPID_PROPERTYPUT;
    dparms.cNamedArgs = 1;
    dparms.rgdispidNamedArgs = &dispidNamed;
    res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYPUT, &dparms, NULL, NULL, NULL);
    if (args != NULL)
        free(args);
    if (FAILED(res))
    {
        _snprintf(buf, 256, "FAILURE:Failed to invoke property setter - %08X", res);
        PipeMessage(buf);
        return;
    }
    // Send the result...
    PipeMessage("RESPONSE:Property set");
}

void Do_marshal_htmldocument(string docid)
{
    TCHAR buf[1024];
    unsigned char readbuf[500];

    vector<LPVOID>::iterator i;
    for (i = InternetExplorers.begin(); i != InternetExplorers.end(); ++i)
    {
        IUnknown* iu = (IUnknown*)(*i);
        HRESULT res;
        IDispatch* id;
        res = iu->QueryInterface(IID_IDispatch, (void **)&id);
        if (SUCCEEDED(res))
        {
            DISPID dispid;

            // Get the dispid of the Document property...
            if (!GetDispID(id, "Document", dispid))
            {
                return;
            }
            else
            {
                VARIANT varResult;
                VariantInit(&varResult);
                DISPPARAMS parms;
                parms.cArgs = 0;
                parms.cNamedArgs = 0;
                EXCEPINFO exinfo;
                res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &parms, &varResult, &exinfo, NULL);
                if (SUCCEEDED(res))
                {
                    _snprintf(buf, 256, "%08X", varResult.pvRecord);
                    if (!strcmp(buf, docid.c_str()))
                    {
                        IUnknown* ihtml;
                        res = ((IDispatch*)(varResult.pvRecord))->QueryInterface(IID_IHTMLDocument, (void**)&ihtml);
                        if (FAILED(res))
                        {
                            PipeMessage("FAILURE:Failed to get IHTMLDocument interface");
                            return;
                        }

                        id->Release();
                        LPSTREAM stream;
                        res = CreateStreamOnHGlobal(NULL, TRUE, &stream);
                        if (FAILED(res))
                        {
                            PipeMessage("FAILURE:Failed to create stream");
                            return;
                        }
                        res = CoMarshalInterface(stream, IID_IHTMLDocument, ihtml, MSHCTX_LOCAL, NULL, MSHLFLAGS_NORMAL);
                        if (FAILED(res))
                        {
                            PipeMessage("FAILURE:Marshaling failed");
                            return;
                        }
                        ULARGE_INTEGER len;
                        LARGE_INTEGER pos;
                        pos.LowPart = 0;
                        pos.HighPart = 0;
                        res = stream->Seek(pos, STREAM_SEEK_END, &len);
                        if (FAILED(res))
                        {
                            PipeMessage("FAILURE:Stream seek failed");
                            return;
                        }
                        if (len.LowPart > 500)
                        {
                            PipeMessage("FAILURE:Stream is longer than expected");
                            return;
                        }
                        ULONG bytesread;
                        stream->Seek(pos, STREAM_SEEK_SET, NULL);
                        stream->Read((void*)readbuf, len.LowPart, &bytesread);
                        strcpy(buf, "RESPONSE:");
                        char* resptr = buf + strlen(buf);
                        unsigned char* srcptr = readbuf;
                        for (ULONG i = 0; i < bytesread; i++)
                        {
                            unsigned char x = *(srcptr++);
                            *(resptr++) = ((x & 0xF0) >> 4) + (x >= 0xA0 ? 'A' - 0xA : '0');
                            x &= 0xF;
                            *(resptr++) = x + (x >= 0xA ? 'A' - 0xA : '0');
                        }
                        *(resptr) = '\0';
                        stream->Release();
                        PipeMessage(buf);
                        return;
                    }
                }
            }
            id->Release();
        }
    }
    PipeMessage("FAILURE:Document not found");
}

// Used internally by Do_gethtmlsource() - given an IDispatch that points to an HTML
// document, gets the source for that document and appends it to 'response', and then
// does the same for any frames within the document. This process is repeated recursively
// to cover nested frames.
// Returns true if successful - if false, an error has occurred and an appropriate
// response has been sent down the pipe.
bool AppendHTMLDocSource(IDispatch* disp, string &response)
{
    _bstr_t* src;
    HRESULT res;

#if defined(DebugMessages)
    PipeMessage("DEBUG: AppendHTMLDocSource: Getting HTML source for a document");
#endif

    // Do the source for the top level document...
    CComQIPtr<IHTMLDocument3> ihtml(disp);
    if (!ihtml)
    {
        PipeMessage("FAILURE:Failed to get IHTMLDocument3 interface");
        return false;
    }

    CComPtr<IHTMLElement> idocel;
    ihtml->get_documentElement(&idocel);
    if (idocel)
    {
        src = new bstr_t();
        idocel->get_outerHTML(&(src->GetBSTR()));
        response.append((const char*)*src);
        delete src;
    }

    // Do any frames...
    CComQIPtr<IHTMLDocument2> ihtml2(ihtml);
    if (!ihtml2)
    {
        PipeMessage("FAILURE:Failed to get IHTMLDocument2 interface");
        return false;
    }

    CComQIPtr<IOleContainer> container(ihtml2);
    if (!container)
    {
        PipeMessage("FAILURE:Failed to get ole container");
        return false;
    }

    CComPtr<IEnumUnknown> enumu;
    res = container->EnumObjects(OLECONTF_EMBEDDINGS, &enumu);
    if (!enumu)
    {
        PipeMessage("FAILURE:Failed to enumerate objects");
        return false;
    }

    IUnknown* funk;
    ULONG uFetched;
    bool found = false;
    for (UINT i = 0; enumu->Next(1, &funk, &uFetched) == S_OK; i++)
    {

#if defined(DebugMessages)
        char buf[256];
        _snprintf(buf, 256, "DEBUG: AppendHTMLDocSource: Getting HTML source for frame %d", i);
        PipeMessage(buf);
#endif

        CComQIPtr<IWebBrowser2> ibrowser(funk);
        funk->Release();
        if (!ibrowser)
        {
            PipeMessage("FAILURE:Failed to get embedded browser");
            return false;
        }

        CComPtr<IDispatch> docdisp;
        ibrowser->get_Document(&docdisp);
        if (!docdisp)
        {
            PipeMessage("FAILURE:Failed to get frame document");
            return false;
        }

        if (!AppendHTMLDocSource(docdisp, response))
            return false;

    }
    return true;
}

void Do_gethtmlsource(string docid)
{
    char buf[256];

    vector<LPVOID>::iterator i;
    for (i = InternetExplorers.begin(); i != InternetExplorers.end(); ++i)
    {
        IUnknown* iu = (IUnknown*)(*i);
        HRESULT res;
        CComQIPtr<IDispatch> id(iu);
        if (id)
        {
            DISPID dispid;

            // Get the dispid of the Document property...
            if (!GetDispID(id, "Document", dispid))
            {
                return;
            }
            else
            {

                VARIANT varResult;
                VariantInit(&varResult);
                DISPPARAMS parms;
                parms.cArgs = 0;
                parms.cNamedArgs = 0;
                EXCEPINFO exinfo;
                res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &parms, &varResult, &exinfo, NULL);
                if (SUCCEEDED(res))
                {
                    _snprintf(buf, 256, "%08X", varResult.pvRecord);
                    if (!strcmp(buf, docid.c_str()))
                    {
                        string source = "";
                        IDispatch* docidisp = (IDispatch*)(varResult.pvRecord);
                        bool result = AppendHTMLDocSource(docidisp, source);
                        docidisp->Release();
                        if (result)
                        {
                            string response;
                            response = "RESPONSE:" + source;
                            PipeMessage(response.c_str());
                        }
                        return;
                    }
                }
            }
        }
    }
    PipeMessage("FAILURE:Document not found");
}


void Do_get_htmldocuments()
{
    string resp = "RESPONSE:Documents:";
    TCHAR buf[256];

    int found = 0;
    vector<LPVOID>::iterator i;
    for (i = InternetExplorers.begin(); i != InternetExplorers.end(); ++i)
    {
        IUnknown* iu = (IUnknown*)(*i);
        HRESULT res;
        IDispatch* id;
        res = iu->QueryInterface(IID_IDispatch, (void **)&id);
        if (SUCCEEDED(res))
        {
            DISPID dispid;

            // Get the dispid of the Document property...
            if (!GetDispID(id, "Document", dispid))
            {
                return;
            }
            else
            {
                VARIANT varResult;
                VariantInit(&varResult);
                DISPPARAMS parms;
                parms.cArgs = 0;
                parms.cNamedArgs = 0;
                EXCEPINFO exinfo;
                res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_PROPERTYGET, &parms, &varResult, &exinfo, NULL);
                if (SUCCEEDED(res) && varResult.pvRecord != NULL)
                {
                    _snprintf(buf, 256, "%s%08X", found == 0 ? "" : ",", varResult.pvRecord);
                    resp.append(buf);
                    found++;
                }
            }
            id->Release();
        }
    }

    PipeMessage(resp.c_str());
}

/* Exception constants for the FARPOINT spreadsheet control handling */
#define FARPOINT_ERR_ALPHA_AT_END   -1
#define FARPOINT_ERR_MULTI_DIGITS   -2
#define FARPOINT_ERR_INVALID_CHAR   -3
#define FARPOINT_ERR_NO_ALPHA_SET   -4
#define FARPOINT_ERR_NULL_CELLREF   -5

/**
 * Convert the given cell reference into a row and column value.
 *
 * Params:
 *  cellref : The cell reference in the format "[alphas][digits]"
 *
 * Returns:
 *  POINT structure containing the column (POINT.x) and row (POINT.y)
 *  corresponding to the given cell reference.
 *  Note that this 1-based, such that the 1st row and 1st column returns {1,1}.
 *
 * Throws:
 *  FARPOINT_ERR_ALPHA_AT_END if an alpha character was found at end of cellref
 *  FARPOINT_ERR_MULTI_DIGITS if multiple sets of digits were found
 *  FARPOINT_ERR_INVALID_CHAR if an invalid character was discovered
 *  FARPOINT_ERR_NO_ALPHA_SET if no alphas were found in the cellref
 *  FARPOINT_ERR_NULL_CELLREF if the given cell ref was empty
 */
POINT Farpoint_CellRefToPoint(string cellref) /* throw (int) */
{
    if (cellref.length() == 0)
        throw FARPOINT_ERR_NULL_CELLREF;

    POINT cell = { 0,0 }; // The return value - (1-based) x=col/y=row of cell within spreadsheet

    // Column number generation : basically - A = 1; AA = 27; BA = 53; BB = 54;
    // Thus, each digit is : (index * (26 ^ [digit-posn-from-right]))
    // so A=(1*26^0)=1; AA=(1*26^1)+(1*26^0)=26+1=27; BA=(2*26^1)+(1*26^0)=52+1=53
    // Row number generation does similar, but with the '0'-'9' chars and in base 10.

    int colpwr = 1; // The current multiplicand for a column alpha
    int rowpwr = 1; // The current multiplicand for a row digit
    bool doneRows = false; // Whether the rows (decimals) have completed or not

    // Go through the cell ref from the end (the right) - take the digit and add
    // that digit raised to the appropriate power to the relevant variable
    // within the POINT structure.

    // Some basic validation :-
    // - If we reach alphas, assume that digits are done with; if we get any 
    //   more, it's an error (FARPOINT_ERR_MULTI_DIGITS)
    // - If we reach alphas and the current row value is zero (which is
    //   invalid) that's an error (FARPOINT_ERR_ALPHA_AT_END)
    // - Obviously any char not in [A-Za-z0-9] is an error (FARPOINT_ERR_INVALID_CHAR)
    for (int i = int(cellref.length()) - 1; i >= 0; --i)
    {
        int c = toupper(cellref[i]);
        if (c >= 'A' && c <= 'Z') // Column number
        {
            if (cell.y == 0) // no rows set... should be by now
                throw FARPOINT_ERR_ALPHA_AT_END;
            doneRows = true;
            cell.x += (1 + (c - 'A')) * colpwr;
            colpwr *= 26;
        }
        else if (c >= '0' && c <= '9') // Row number
        {
            if (doneRows)
                throw FARPOINT_ERR_MULTI_DIGITS;
            cell.y += (c - '0') * rowpwr;
            rowpwr *= 10;
        }
        else // not A-Z? not 0-9? I refuse to work with this rubbish.
        {
            throw FARPOINT_ERR_INVALID_CHAR;
        }
    }

    // This is the only point at which we can tell that there were 
    // no alpha chars entered
    if (cell.x == 0)
        throw FARPOINT_ERR_NO_ALPHA_SET;

    // We got here? It's all good
    return cell;
}

/**
 * Set the value in the cell referenced by the specified cell ref to the given value
 * Params:
 *  hwnd    : Window handle of the Farpoint spreadsheet control
 *  cellref : "[Alpha][Number]" reference specifying the column [Alpha] and the
 *            row [Number] to set the value in.
 *  value   : The string value to set within the cell
 */
void Do_farpoint_setvalue(HWND hwnd, string cellref, string value)
{
    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(hwnd);

    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    // Get the row and column number : 
    POINT point;
    try
    {
        point = Farpoint_CellRefToPoint(cellref);
    }
    catch (int err)
    {
        switch (err)
        {
        case FARPOINT_ERR_ALPHA_AT_END:
        case FARPOINT_ERR_MULTI_DIGITS:
        case FARPOINT_ERR_INVALID_CHAR:
        case FARPOINT_ERR_NO_ALPHA_SET:
            PipeMessage("FAILURE:Invalid Cell Reference provided - must be '[alphas][digits]'");
            break;
        case FARPOINT_ERR_NULL_CELLREF:
            PipeMessage("FAILURE:No Cell Reference provided");
            break;
        default:
            PipeMessage("FAILURE:Unrecognised Error");
        }
        return;
    }

    /*
    Okay, we now have 1-based row and column values.
    Syntax for SetText from :
    http://www.clubfarpoint.com/FarPointSupportSite/Modules/Resources/faqitem.aspx?pid=1&id=1 is:

    fpSpread.SetText(ByVal Col As Long, ByVal Row As Long, ByVal Var As Variant)

    Parameters:
    Col: Column number of cell to contain data
    Row: Row number of cell to contain data
    Var: Text being assigned to cell

    Returns:
    None
    */
    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid;
    if (!GetDispID(id, "SetText", dispid))
    {
        id->Release();
        return;
    }

    DISPPARAMS dparms;
    VARIANTARG args[3]; // SetText(point.x, point.y, value)
    {
        args[0].vt = VT_INT;
        args[0].intVal = point.x;
        args[1].vt = VT_INT;
        args[1].intVal = point.y;
        args[2].vt = VT_BSTR;
        args[2].bstrVal = SysAllocString(str_to_wstr(value).c_str());
    }
    dparms.cArgs = 3;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;

    VARIANT varResult;
    VariantInit(&varResult);
    HRESULT res = id->Invoke(dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_METHOD, &dparms, &varResult, NULL, NULL);

    if (FAILED(res))
    {
        TCHAR buf[256];
        _snprintf(buf, 256, "FAILURE:Failed to invoke method 'Set Text' - %08X", res);
        PipeMessage(buf);
    }

    // Clean up before we leave
    id->Release();
    SysFreeString(args[2].bstrVal);

}

/**
 * Get the value in the cell referenced by the specified cell ref
 * Params:
 *  hwnd:       Window handle of the Farpoint spreadsheet control
 *  cellref:    "[Alpha][Number]" reference specifying the column [Alpha] and the
 *              row [Number] to set the value in.
 *
 * Returns:
 *  The string value found within the specified Farpoint control at the given reference.
 */
void Do_farpoint_getvalue(HWND hwnd, string cellref)
{
    // Find the object...
    DispatchableObject* obj = GetDispatchableObjectFromHWnd(hwnd);
    if (obj == NULL)
    {
        PipeMessage("FAILURE:Could not find dispatchable object matching window");
        return;
    }

    // Get the row and column number : 
    POINT point;
    try
    {
        point = Farpoint_CellRefToPoint(cellref);
    }
    catch (int err)
    {
        switch (err)
        {
        case FARPOINT_ERR_ALPHA_AT_END:
        case FARPOINT_ERR_MULTI_DIGITS:
        case FARPOINT_ERR_INVALID_CHAR:
        case FARPOINT_ERR_NO_ALPHA_SET:
            PipeMessage("FAILURE:Invalid Cell Reference provided - must be '[alphas][digits]'");
            break;
        case FARPOINT_ERR_NULL_CELLREF:
            PipeMessage("FAILURE:No Cell Reference provided");
            break;
        default:
            PipeMessage("FAILURE:Unrecognised Error");
        }
        return;
    }

    /*
    Okay, we now have 1-based row and column values.
    Syntax for GetText from :
    http://www.clubfarpoint.com/FarPointSupportSite/Modules/Resources/faqitem.aspx?pid=1&id=32 is:

    fpSpread.GetText(ByVal Col As Long, ByVal Row As Long, Var As Variant) As Boolean

    Parameters:
    Col: Column number of cell
    Row: Row number of cell
    Var: Text in cell


    Returns:
    True if successful; otherwise, False.

    Note that 'Var' is ByRef (by omission).
    */
    IDispatch* id = (IDispatch*)obj->Interface;
    DISPID dispid;
    if (!GetDispID(id, "GetText", dispid))
    {
        id->Release();
        return;
    }

    DISPPARAMS dparms;
    VARIANTARG args[3]; // GetText(point.x, point.y, value)
    BSTR value; // byref output param
    {
        args[0].vt = VT_INT;
        args[0].intVal = point.x;
        args[1].vt = VT_INT;
        args[1].intVal = point.y;
        args[2].vt = VT_BSTR | VT_BYREF;
        args[2].pbstrVal = &value;
    }
    dparms.cArgs = 3;
    dparms.cNamedArgs = 0;
    dparms.rgvarg = args;

    VARIANT varResult;
    VariantInit(&varResult);
    HRESULT res = id->Invoke(
        dispid, IID_NULL, LOCALE_SYSTEM_DEFAULT, DISPATCH_METHOD, &dparms, &varResult, NULL, NULL);


    if (FAILED(res))
    {
        TCHAR buf[256];
        _snprintf(buf, 256, "FAILURE:Failed to invoke method 'Get Text' - %08X", res);
        PipeMessage(buf);
    }
    else if (varResult.boolVal)
    {
        string retval = "RESPONSE:" + wstr_to_str(value);
        PipeMessage(retval.c_str());
    }
    else
    {
        PipeMessage("FAILURE:Call to method 'Get Text' returned a fail response");
    }

    // Clean up before going bye bye
    id->Release();
    SysFreeString(value);

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

    if (op == "get_htmldocuments")
    {
        // We should not have any parameters...
        if (parms.size() != 0)
        {
            PipeMessage("FAILURE:No parameters required for get_htmldocuments");
            return;
        }
        Do_get_htmldocuments();
    }
    else if (op == "marshal_htmldocument")
    {
        // We should have one parameter...
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for marshal_htmldocument");
            return;
        }
        Do_marshal_htmldocument(parms[0]);
    }
    else if (op == "gethtmlsource")
    {
        // We should have one parameter...
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for gethtmlsource");
            return;
        }
        Do_gethtmlsource(parms[0]);
    }
    else if (op == "property_get")
    {
        // We should have at least two parameters - the window handle and the name of
        // the property, followed by any actual parameters for the property.
        if (parms.size() < 2)
        {
            PipeMessage("FAILURE:Two or more parameters required for property_get");
            return;
        }
        // Read our parameters and then remove them from the list, just leaving the
        // ones to be passed to the property getter.
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        string propname = parms[1];
        vector<string>::iterator itStart = parms.begin();
        vector<string>::iterator itEnd = itStart + 2;
        parms.erase(itStart, itEnd);
        Do_property_get(wnd, propname, parms);
    }
    else if (op == "property_set")
    {
        // We should have at least three parameters - the window handle and the name of
        // the property, the new value, followed by any actual parameters for the
        // property.
        if (parms.size() < 3)
        {
            PipeMessage("FAILURE:Three or more parameters required for property_set");
            return;
        }
        // Read our parameters and then remove them from the list, just leaving the
        // ones to be passed to the property setter.
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        string propname = parms[1];
        string value = parms[2];
        vector<string>::iterator itStart = parms.begin();
        vector<string>::iterator itEnd = itStart + 3;
        parms.erase(itStart, itEnd);
        Do_property_set(wnd, propname, value, parms);
    }
    else if (op == "statusbar_read")
    {
        // We should have one parameter - the window handle
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for statusbar_read");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_statusbar_read(wnd);
    }
    else if (op == "treeview_count")
    {
        // We should have one parameter - the window handle
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for treeview_count");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_treeview_count(wnd);
    }
    else if (op == "treeview_ensurevisible")
    {
        // We should have two parameters - the window handle and the node text
        if (parms.size() != 2)
        {
            PipeMessage("FAILURE:Two parameters required for treeview_ensurevisible");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_treeview_ensurevisible(wnd, parms[1].c_str());
    }
    else if (op == "treeview_select")
    {
        // We should have two parameters - the window handle and the node text
        if (parms.size() != 2)
        {
            PipeMessage("FAILURE:Two parameters required for treeview_select");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_treeview_select(wnd, parms[1].c_str());
    }
    else if (op == "treeview_children")
    {
        // We should have two parameter - the window handle, and the text of the node to get the
        // children of...
        if (parms.size() != 2)
        {
            PipeMessage("FAILURE:Two parameters required for treeview_count");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_treeview_children(wnd, parms[1].c_str());
    }
    else if (op == "treeview_siblings")
    {
        // We should have two parameter - the window handle, and the text of the node to get the
        // children of...
        if (parms.size() != 2)
        {
            PipeMessage("FAILURE:Two parameters required for treeview_count");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_treeview_siblings(wnd, parms[1].c_str());
    }
    else if (op == "listview_count")
    {
        // We should have one parameter - the window handle
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for listview_count");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_listview_count(wnd);
    }
    else if (op == "listview_getselecteditemtext")
    {
        // We should have one parameter - the window handle
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for listview_selecteditemtext");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_listview_getselecteditemtext(wnd);
    }
    else if (op == "listview_readall")
    {
        // We should have one parameter - the window handle
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for listview_readall");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_listview_readall(wnd);
    }
    else if (op == "apex_cols")
    {
        // We should have one parameter - the window handle
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for apex_cols");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_apex_cols(wnd);
    }
    else if (op == "apex_readcurrow")
    {
        // We should have one parameter - the window handle
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for apex_readcurrow");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_apex_readcurrow(wnd);
    }
    else if (op == "flex_readall")
    {
        // We should have one parameter - the window handle
        if (parms.size() != 1)
        {
            PipeMessage("FAILURE:One parameter required for flex_readall");
            return;
        }
        HWND wnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_flex_readall(wnd);
    }
    else if (op == "farpoint_setvalue")
    {
        // HWND + cellref + value = 3 params
        if (parms.size() != 3)
        {
            PipeMessage("FAILURE:Three parameters required for farpoint_setvalue");
            return;
        }
        HWND hwnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_farpoint_setvalue(hwnd, parms[1], parms[2]);
    }
    else if (op == "farpoint_getvalue")
    {
        // HWND + cellref = 2 params
        if (parms.size() != 2)
        {
            PipeMessage("FAILURE:Two parameters required for farpoint_getvalue");
            return;
        }
        HWND hwnd = (HWND)strtol(parms[0].c_str(), NULL, 16);
        Do_farpoint_getvalue(hwnd, parms[1]);
    }
    else if (op == "quit")
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
    strcpy(namebuf, lpszPipename);
    _snprintf(numbuf, 9, "%08X", GetCurrentProcessId());
    strcat(namebuf, numbuf);

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
    // Don't send zero length messages
    if (*msg == '\0')
        return;
    if (WaitForSingleObject(PipeMutex, 5000) != WAIT_OBJECT_0)
        return;
    string m = msg;
    PipeMessageQueue.push(m);
    ReleaseMutex(PipeMutex);
}

// Flag used to tell our threads to terminate.
bool TerminatePipeThreads = false;



// Send a message down our named pipe. In the event of any failure,
// this function will silently abort, since there is nothing we can
// do about it anyway.
void PipeSendMessage(const char* msg)
{
    OVERLAPPED ov = { 0 };
    ov.hEvent = PipeSendEvent;

    if (hPipe != INVALID_HANDLE_VALUE)
    {
        try
        {
            DWORD cbWritten;
            // If the message is large, send it as several messages in chunks. Each partial
            // message is prefixed with '\0' so the receiver knows to stich them all together.
            // This works around that fact that messages greater than the incoming buffer
            // size never appear to be received by the other end.
            int len;
            while ((len = strlen(msg)) > MAX_PIPEMSG_SIZE)
            {
                char buf[MAX_PIPEMSG_SIZE + 1];
                *buf = '\0';
                memcpy(buf + 1, msg, MAX_PIPEMSG_SIZE);
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
                    PipeSendMessage(msg.c_str());
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

    // Start the pipe sender thread...
    DWORD dwThreadID;
    hPipeSenderThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)PipeSenderThread, NULL, 0, &dwThreadID);

    // Note that we can still use the pipe to send messages, even though we only just
    // started the thread that created it, because all we are doing is putting the
    // messages into a queue to be sent when possible.
    PipeMessage("BPInjAgent initialised : attaching");

    InterceptAPI("gdi32.dll", "TextOutA", (DWORD)TextOutAReplaced, (DWORD)TextOutATrampoline);
    InterceptAPI("gdi32.dll", "ExtTextOutW", (DWORD)ExtTextOutWReplaced, (DWORD)ExtTextOutWTrampoline);
    InterceptAPI("user32.dll", "DrawTextA", (DWORD)DrawTextAReplaced, (DWORD)DrawTextATrampoline);
    InterceptAPI("user32.dll", "DrawTextW", (DWORD)DrawTextWReplaced, (DWORD)DrawTextWTrampoline);

    InterceptAPI("gdi32.dll", "CreateCompatibleDC", (DWORD)CreateCompatibleDCReplaced, (DWORD)CreateCompatibleDCTrampoline);
    InterceptAPI("user32.dll", "GetDC", (DWORD)GetDCReplaced, (DWORD)GetDCTrampoline);
    InterceptAPI("user32.dll", "GetDCEx", (DWORD)GetDCExReplaced, (DWORD)GetDCExTrampoline);
    InterceptAPI("user32.dll", "BeginPaint", (DWORD)BeginPaintReplaced, (DWORD)BeginPaintTrampoline);
    InterceptAPI("user32.dll", "EndPaint", (DWORD)EndPaintReplaced, (DWORD)EndPaintTrampoline);
    InterceptAPI("user32.dll", "ReleaseDC", (DWORD)ReleaseDCReplaced, (DWORD)ReleaseDCTrampoline);
    InterceptAPI("gdi32.dll", "DeleteDC", (DWORD)DeleteDCReplaced, (DWORD)DeleteDCTrampoline);

    InterceptAPI("gdi32.dll", "BitBlt", (DWORD)BitBltReplaced, (DWORD)BitBltTrampoline);
    InterceptAPI("gdi32.dll", "PatBlt", (DWORD)PatBltReplaced, (DWORD)PatBltTrampoline);
    InterceptAPI("user32.dll", "FillRect", (DWORD)FillRectReplaced, (DWORD)FillRectTrampoline);
    InterceptAPI("user32.dll", "ShowWindow", (DWORD)ShowWindowReplaced, (DWORD)ShowWindowTrampoline);
    InterceptAPI("user32.dll", "SetParent", (DWORD)SetParentReplaced, (DWORD)SetParentTrampoline);
    InterceptAPI("user32.dll", "SetWindowPos", (DWORD)SetWindowPosReplaced, (DWORD)SetWindowPosTrampoline);
    InterceptAPI("gdi32.dll", "SetTextAlign", (DWORD)SetTextAlignReplaced, (DWORD)SetTextAlignTrampoline);

    // The following two also cover CreateWindowA and CreateWindowW as defined by the API, since they
    // are just aliases.
    InterceptAPI("user32.dll", "CreateWindowExA", (DWORD)CreateWindowExAReplaced, (DWORD)CreateWindowExATrampoline);
    InterceptAPI("user32.dll", "CreateWindowExW", (DWORD)CreateWindowExWReplaced, (DWORD)CreateWindowExWTrampoline);

    InterceptAPI("ole32.dll", "CoGetClassObject", (DWORD)CoGetClassObjectReplaced, (DWORD)CoGetClassObjectTrampoline);
    InterceptAPI("ole32.dll", "CoCreateInstance", (DWORD)CoCreateInstanceReplaced, (DWORD)CoCreateInstanceTrampoline);

    //  InterceptAPI("user32.dll", "CreateDialogIndirectParamA",(DWORD)CreateDialogIndirectParamAReplaced,(DWORD)CreateDialogIndirectParamATrampoline);
    //  InterceptAPI("user32.dll", "CreateDialogParamA",(DWORD)CreateDialogParamAReplaced,(DWORD)CreateDialogParamATrampoline);
    //  InterceptAPI("user32.dll", "DialogBoxParamA",(DWORD)DialogBoxParamAReplaced,(DWORD)DialogBoxParamATrampoline);

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

