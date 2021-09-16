/*******************************************************************************
 *
 * Logging
 * -------
 * See logging.cpp for documentation.
 *
 *******************************************************************************/

#pragma once
#include <stdlib.h>
#include <windows.h>

#define STRSAFE_NO_CB_FUNCTIONS
#include <strsafe.h>

// the following three lines define __WFILE__, a Unicode version of __FILE__
#define WIDEN2(x) L ## x
#define WIDEN(x) WIDEN2(x)
#define __WFILE__ WIDEN(__FILE__)

#define DEFAULT_LOGFILE_PATH L"C:\\Temp"
#define LOG_ERROR 1
#define LOG_DEBUG 2
#define LOG_TRACE 4

class Log {
public:
	static void InitClass(
		const wchar_t* filePrefix);

	static void FinalizeClass();

	static void Reset();
	static void Error(
		const wchar_t* sourceFile,
		int lineNumber,
		const wchar_t* formatString,
		...);

	static void Debug(
		const wchar_t* sourceFile,
		int lineNumber,
		const wchar_t* formatString,
		...);

	static void Trace(
		const wchar_t* sourceFile,
		int lineNumber,
		const wchar_t* formatString,
		...);

	static bool FormatLogEntry(
		wchar_t* msg,
		int cch,
		const wchar_t* sourceFile,
		int lineNumber,
		const wchar_t* level,
		const wchar_t* formatString,
		va_list args);

	static bool LookupErrorMessage(
		wchar_t* buf,
		int cch,
		DWORD err);

private:
	static int loggingLevel;
	static LPCWSTR s_logfilePath;

	Log() {} // not meant to be instantiated
	static void simpleLog(
		const wchar_t* sourceFile,
		int lineNumber,
		const wchar_t* level,
		const wchar_t* formatString,
		va_list args);

	static void writeLog(const wchar_t* msg, const size_t msgSize);

	static const wchar_t* seekToFileName(const wchar_t* sourceFile);
};

//
// Logging macros
//
#define LogFail(...) Log::Error(__WFILE__, __LINE__, __VA_ARGS__)
#define LogDebug(...) Log::Debug(__WFILE__, __LINE__, __VA_ARGS__)
#define LogTrace(...) Log::Trace(__WFILE__, __LINE__, __VA_ARGS__)
