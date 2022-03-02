/*******************************************************************************
 *
 * Logging
 * -------
 * This file provides the implementation for a basic logging API. There is no
 *  thread management here, so it can't be described as thread safe.
 *
 * The LogTrace(), LogDebug() and LogFail() macros are probably the best way
 *  to log info - they each take a printf-style (unicode) format string and a
 *  series of arguments for that string. The LogDebug() calls are preprocessed
 *  out if _DEBUG is not defined, the LogTrace() calls are preprocessed out if
 *  either _DEBUG or _TRACE is not defined.
 *
 * TODO: This really needs a critical section around the writing to the text
 *   file - there's no members to worry about; everything is passed in as local
 *   arguments, but the file writing (ie. the writeLong(const wchar_t *) method)
 *   needs to be protected from other threads.
 *
 *******************************************************************************/

#include "logging.h"
#include "Shlwapi.h"

int Log::loggingLevel = 0;
PCWSTR Log::s_logfilePath = DEFAULT_LOGFILE_PATH;

void Log::writeLog(const wchar_t* msg, const size_t msgSize)
{
    size_t lenString = wcsnlen_s(msg, msgSize);
    if (lenString == msgSize) return;

    HANDLE h = CreateFile(
        s_logfilePath, GENERIC_WRITE, FILE_SHARE_READ, 0, OPEN_ALWAYS, 0, 0);

    if (INVALID_HANDLE_VALUE == h)
        return;

    if (SetFilePointer(h, 0, 0, FILE_END) != INVALID_SET_FILE_POINTER)
    {
        DWORD cb = DWORD(lenString) * sizeof *msg;
        WriteFile(h, msg, cb, &cb, 0);
    }
    CloseHandle(h);
}

void Log::simpleLog(const wchar_t* sourceFile,
    int lineNumber,
    const wchar_t* level,
    const wchar_t* formatString,
    va_list args)
{
    const size_t MsgSize = 512;
    wchar_t msg[MsgSize];
    if (FormatLogEntry(msg,
        sizeof msg / sizeof *msg, sourceFile, lineNumber, level, formatString, args))
    {
        writeLog(msg, MsgSize);
    }
}

void Log::InitClass(const wchar_t* filePrefix)
{
    // Start with a default - if anything fails after this point, at least we
    // have somewhere to write log entries to.
    loggingLevel = 0;
    s_logfilePath = DEFAULT_LOGFILE_PATH;

    HKEY hKey;

    DWORD result = RegOpenKeyExW(
        HKEY_LOCAL_MACHINE, L"SOFTWARE\\Blue Prism Limited\\LoginAgent",
        0L, KEY_QUERY_VALUE, &hKey);

    if (result != ERROR_SUCCESS)
        return;

    DWORD valueType;
    DWORD dwBuf(sizeof(DWORD));
    DWORD dwVal(0);
    result = RegQueryValueExW(
        hKey, L"LogLevel", NULL, &valueType, reinterpret_cast<LPBYTE>(&dwVal), &dwBuf);
    if (result == ERROR_SUCCESS)
    {
        loggingLevel = dwVal;
    }

    DWORD cbData;
    result = RegQueryValueExW(
        hKey, L"LogFileDir", NULL, &valueType, NULL, &cbData);
    if (result == ERROR_SUCCESS && cbData > 0)
    {
        LPWSTR pathLogfile = new wchar_t[cbData]; // don't need to +1 for null
        if (pathLogfile)
        {
            result = RegQueryValueExW(
                hKey, L"LogFileDir", NULL, &valueType, reinterpret_cast<LPBYTE>(pathLogfile), &cbData);

            if (result == ERROR_SUCCESS)
                s_logfilePath = pathLogfile;
        }
        // we don't clean up the string here - it exists until the DLL
        // unloads
    }

    if (hKey) RegCloseKey(hKey);

    // Generate unique logfile name for this session:
    // {prefix}_{yyyymmdd}_{hhmmss}_{pid}.log
    //
    // e.g. BluePrismCredentialProvider_20160815_073513_2508.log
    SYSTEMTIME now;
    GetSystemTime(&now);

    static wchar_t fnam[512];
    StringCchPrintf(fnam, sizeof fnam / sizeof *fnam, L"%s\\%s_%d%02d%02d_%02d%02d%02d_%d.log",
        s_logfilePath, filePrefix,
        now.wYear, now.wMonth, now.wDay,
        now.wHour, now.wMinute, now.wSecond,
        GetCurrentProcessId());
    s_logfilePath = fnam;
}

void Log::FinalizeClass()
{
    // if we're not set at the (#defined) default, clean it up
    if (lstrcmp(s_logfilePath, DEFAULT_LOGFILE_PATH) != 0)
    {
        delete[] s_logfilePath;
        // Log shouldn't really be used after this finalize, but set
        // a reasonable default just in case that isn't the case
        s_logfilePath = DEFAULT_LOGFILE_PATH;
    }
}

void Log::Reset()
{
    if (PathFileExistsW(s_logfilePath))
    {
        DeleteFileW(s_logfilePath);
    }
}

void Log::Error(const wchar_t* sourceFile,
    int lineNumber,
    const wchar_t* formatString,
    ...)
{
    // Ignore if error logging not enabled
    if (!(loggingLevel & LOG_ERROR)) return;

    va_list args;
    va_start(args, formatString);
    simpleLog(sourceFile, lineNumber, L"ERROR", formatString, args);
}

void Log::Debug(const wchar_t* sourceFile,
    int lineNumber,
    const wchar_t* formatString,
    ...)
{
    // Ignore if debug logging not enabled
    if (!(loggingLevel & LOG_DEBUG)) return;

    va_list args;
    va_start(args, formatString);
    simpleLog(sourceFile, lineNumber, L"DEBUG", formatString, args);
}

void Log::Trace(const wchar_t* sourceFile,
    int lineNumber,
    const wchar_t* formatString,
    ...)
{
    // Ignore if trace logging not enabled
    if (!(loggingLevel & LOG_TRACE)) return;

    va_list args;
    va_start(args, formatString);
    simpleLog(sourceFile, lineNumber, L"TRACE", formatString, args);
}

const wchar_t* Log::seekToFileName(const wchar_t* sourceFile)
{
    if (!*sourceFile) return sourceFile; // empty string

    const wchar_t* begin = sourceFile;
    const wchar_t* it = begin;
    while (*(++it))
        ; // just find the end of the string

    // iterator points to null terminator now - search back for first backslash
    while (it != begin) {
        if (L'\\' == *it) {
            ++it;
            break;
        }
        --it;
    }
    return it;
}


bool Log::FormatLogEntry(wchar_t* msg,
    int cch,
    const wchar_t* sourceFile,
    int lineNumber,
    const wchar_t* level,
    const wchar_t* formatString,
    va_list args)
{
    if (0 == cch) return false;
    msg[0] = L'\0';

    SYSTEMTIME systemTime;
    GetSystemTime(&systemTime);

    DWORD tsSessionId;
    if (!ProcessIdToSessionId(GetCurrentProcessId(), &tsSessionId)) tsSessionId = 0;

    const wchar_t* tssDesc =
        GetSystemMetrics(SM_REMOTESESSION) ? L"TSS(Remote)" : L"TSS(Local)";

    HRESULT hr = StringCchPrintf(msg, cch, L"%s %d: %02d:%02d:%02d:%04d %s %s(%d): ",
        tssDesc,
        tsSessionId,
        systemTime.wHour,
        systemTime.wMinute,
        systemTime.wSecond,
        systemTime.wMilliseconds,
        level,
        seekToFileName(sourceFile),
        lineNumber);
    if (FAILED(hr)) return false;

    wchar_t suffix[256];
    hr = StringCchVPrintf(suffix, sizeof suffix / sizeof *suffix, formatString, args);
    if (FAILED(hr)) return false;

    hr = StringCchCat(msg, cch, suffix);
    if (FAILED(hr)) return false;

    hr = StringCchCat(msg, cch, L"\r\n");
    if (FAILED(hr)) return false;

    return true;
}

bool Log::LookupErrorMessage(wchar_t* buf,
    int cch,
    DWORD err)
{
    if (FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, 0, err, 0, buf, cch, 0))
    {
        return true;
    }

    LogDebug(L"FormatMessage failed: %d", GetLastError());
    StringCchPrintf(buf, cch, (err < 15000) ? L"Error number: %d" :
        L"Error number: 0x%08X", err);
    return false;
}
