// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

// The following is to get it to include the correct libraries to go
// with comutil.h without messing with the compiler switches. See
// http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=145979&SiteID=1
#ifdef _DEBUG
# pragma comment(lib, "comsuppwd.lib")
#else
# pragma comment(lib, "comsuppw.lib")
#endif
# pragma comment(lib, "wbemuuid.lib")

#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers
#define _CRT_SECURE_NO_WARNINGS
// Windows Header Files:
#include <windows.h>
#include <string>
#include <sstream>
#include <vector>
#include <queue>
#include <fstream>
#include "Wtypes.h"
#include "objbase.h"
#include "atlbase.h"
#include "atlconv.h"
#include "atlstr.h"
#include "comutil.h"
#include "tlhelp32.h"
