// Splash.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "Splash.h"
#include "Shellapi.h"
#include "atlstr.h"
#include <cctype>
#include <sstream>
using namespace std;

#pragma warning(disable : 4996)
#pragma warning(disable : 4244)

// Global Variables:
HBITMAP hbmSplashScreen;
bool BPAWindowExists;
PROCESS_INFORMATION pi;

// Forward declarations of functions included in this code module:
//BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK    About(HWND, UINT, WPARAM, LPARAM);
BOOL CALLBACK       EnumWindowsProc(HWND, LPARAM);
BOOL                CaseInsensitiveStringCompare(std::string &, std::string &);
BOOL                CompareChar(CHAR &, CHAR &);

//char comparer predicate for checking the command line args
HCURSOR cursors[3];
int current_cursor = 2;
int APIENTRY _tWinMain(HINSTANCE hInstance,
    HINSTANCE hPrevInstance,
    LPTSTR    lpCmdLine,
    int       nCmdShow)
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);
    
    HCURSOR cursors[3];

    cursors[0] = LoadCursor(NULL, IDC_ARROW);    // default cursor
    cursors[1] = LoadCursor(NULL, IDC_CROSS);    // other cursor
    cursors[2] = LoadCursor(NULL, IDC_WAIT);     // wait cursor

    //Search the arguments for the /nosplash switch
    LPWSTR *szArglist;
    int nArgs = 0;
    szArglist = CommandLineToArgvW(GetCommandLineW(), &nArgs);
    bool showSplash = true;
    std::string searchString = "/nosplash";

    for (int i = 0; i < nArgs; i++) {
        std::string a = CW2A(szArglist[i]);
        if(!a.compare(searchString))
            showSplash = false;
    }
  
    MSG msg;

    // Register window class...
    WNDCLASSEX wcex;
    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = WndProc;
    wcex.cbClsExtra = 0;
    wcex.cbWndExtra = 0;
    wcex.hInstance = hInstance;
    wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_ICON));
    wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
    wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW);
    wcex.lpszMenuName = NULL;
    wcex.lpszClassName = _T("AutomateStarting");
    wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_ICON));
    RegisterClassEx(&wcex);

    // Generate the filename of the real Automate, which is in the same directory as this
    // splash screen launcher...
    TCHAR szFileName[MAX_PATH];
    GetModuleFileName(hInstance, szFileName, MAX_PATH);
    TCHAR *dest = _tcsrchr(szFileName, '\\');
    int destpos = dest - szFileName + 1;
    szFileName[destpos] = '\0';
    _tcscat(szFileName, _T("Automate.exe"));

    // Load the splash screen bitmap (first deciding whether to load the real one
    // or the 'unbranded' one...
    TCHAR szUBFileName[MAX_PATH];
    GetModuleFileName(hInstance, szUBFileName, MAX_PATH);
    TCHAR *ubdest = _tcsrchr(szUBFileName, '\\');
    int ubdestpos = ubdest - szUBFileName + 1;
    szUBFileName[ubdestpos] = '\0';
    _tcscat(szUBFileName, _T("unbranded"));
    DWORD dwAttrib = GetFileAttributes(szUBFileName);
    BOOL unbranded = FALSE;
    if (dwAttrib != INVALID_FILE_ATTRIBUTES &&
        !(dwAttrib & FILE_ATTRIBUTE_DIRECTORY)) {
        hbmSplashScreen = LoadBitmap(GetModuleHandle(NULL), MAKEINTRESOURCE(IDB_SPLASHN));
        unbranded = true;
    }
    else {
        hbmSplashScreen = LoadBitmap(GetModuleHandle(NULL), MAKEINTRESOURCE(IDB_SPLASH));
    }

    //Get the size of the canvas from the bitmap dimensions
    BITMAP bm;
    ::GetObject(hbmSplashScreen, sizeof(bm), &bm);
    int width = bm.bmWidth;
    int height = bm.bmHeight;

    //if the product is branded then we want to write the version number under the BluePrism logo 
    DWORD  verHandle = 0;
    UINT   size = 0;
    LPBYTE lpBuffer = NULL;
    DWORD  verSize = GetFileVersionInfoSize(szFileName, &verHandle);
    wchar_t istr[32];
    if (verSize != NULL)
    {
        LPSTR versionData = new char[verSize];

        if (GetFileVersionInfo(szFileName, verHandle, verSize, versionData))
        {
            if (VerQueryValue(versionData, _T("\\"), (VOID FAR* FAR*)&lpBuffer, &size))
            {
                if (size)
                {
                    VS_FIXEDFILEINFO *verInfo = (VS_FIXEDFILEINFO *)lpBuffer;
                    if (verInfo->dwSignature == 0xfeef04bd)
                    {
                        auto major = verInfo->dwFileVersionMS >> 16;
                        auto minor = verInfo->dwFileVersionMS & 0xffff;
                        auto patch = verInfo->dwFileVersionLS >> 16;

                        std::string versionString = unbranded ? "Version " : "";
                        versionString += std::to_string(major);
                        versionString += ".";
                        versionString += std::to_string(minor);
                        if (std::to_string(patch) != "0")
                        {
                            versionString += ".";
                            versionString += std::to_string(patch);
                        }

                        //  Create a new Bitmap, then draw our text and the original bitmap onto it
                        auto hdc = GetDC(0);
                        long lfHeight = -MulDiv(24, GetDeviceCaps(hdc, LOGPIXELSY), 72);

                        auto memdc = CreateCompatibleDC(hdc);
                        SetTextColor(memdc, RGB(255, 255, 255));
                        SetBkMode(memdc, TRANSPARENT);
                        auto oldbmp = SelectObject(memdc, hbmSplashScreen);

                        auto hfont = CreateFont(lfHeight, 0, 0, 0, 0, FALSE, 0, 0, 0, 0, 0, 0, 0, _T("Roboto"));

                        SelectObject(memdc, hfont);
                        std::stringstream stream;
                        stream << versionString;
                        int x = unbranded ? 12 : 655;
                        int y = unbranded ? 43 : 420;

                        TextOutA(memdc, x, y, stream.str().c_str(), versionString.length());
                        SelectObject(memdc, oldbmp);

                        DeleteDC(memdc);
                        ReleaseDC(0, hdc);
                    }
                }
            }
        }
    }

    // Get size of desktop so we can centre the splash screen window...
    HWND hDesktop = GetDesktopWindow();
    RECT desktop;
    int splashX;
    int splashY;
    POINT pt;
    GetCursorPos(&pt);
    HMONITOR currentMonitor = MonitorFromPoint(pt, NULL);
    MONITORINFO mi = { sizeof(mi) };
    
    if (GetMonitorInfo(currentMonitor, &mi)) {
        desktop = mi.rcMonitor;

        if (!showSplash) {
            width = 0;
            height = 0;
        }

        splashX= desktop.left + (desktop.right - desktop.left) / 2 - width / 2;
        splashY = desktop.top + (desktop.bottom - desktop.top) / 2 - height / 2;
    }
    else
    {//just show on main desktop
        GetWindowRect(hDesktop, &desktop);
        splashX = desktop.right / 2 - width / 2;
        splashY = desktop.bottom / 2 - height / 2;
    }    

    // Create splash screen window...
    HWND hWndMain = CreateWindowEx(WS_EX_STATICEDGE | WS_EX_TOOLWINDOW, _T("AutomateStarting"), _T("Automate Starting"),
        WS_POPUP, splashX, splashY, width, height, NULL, NULL, hInstance, NULL);
    if (!hWndMain)
        return FALSE;

    ShowWindow(hWndMain, nCmdShow);

    // Generate a full command-line made up of the path to the real Automate and all the
    // command-line options passed straight through. The path is quoted in case it contains
    // spaces...
    LPTSTR cmdline = (LPTSTR)malloc(sizeof(TCHAR)*(_tcslen(lpCmdLine) + _tcslen(szFileName) + 4));
    _tcscpy(cmdline, _T("\""));
    _tcscat(cmdline, szFileName);
    _tcscat(cmdline, _T("\" "));
    _tcscat(cmdline, lpCmdLine);

    // Launch the real Automate...
    STARTUPINFO si;
    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    ZeroMemory(&pi, sizeof(pi));
    CreateProcess(NULL, cmdline, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi);

    // Free the memory allocated above.
    free(cmdline);    

    // Set a timer, which will post a WM_TIMER message to our message queue.
    // Each time the timer ticks, we check to see if the BPA window exists.
    // When this window exists we quit.
    SetTimer(hWndMain, 0, 100, NULL);

    
    // Main message loop...
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    DeleteObject(hbmSplashScreen);

    return (int)msg.wParam;
}

static int loopCounter = 0;

// WndProc for the splash screen window...
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    PAINTSTRUCT ps;
    HDC hdc;
    HBITMAP hOld;
    HDC hdcMem;

    switch (message)
    {
    case WM_PAINT:
        // Draw our splash screen bitmap...
        hdc = BeginPaint(hWnd, &ps);
        BITMAP b;
        hdcMem = CreateCompatibleDC(hdc);
        hOld = (HBITMAP)SelectObject(hdcMem, hbmSplashScreen);
        GetObject(hbmSplashScreen, sizeof(b), &b);
        BitBlt(hdc, 0, 0, b.bmWidth, b.bmHeight, hdcMem, 0, 0, SRCCOPY);
        SelectObject(hdcMem, hOld);
        DeleteDC(hdcMem);
        EndPaint(hWnd, &ps);
        break;
    case WM_TIMER:
        //Check for the automate window, and exit if it is present
        EnumDesktopWindows(NULL, EnumWindowsProc, NULL);
        loopCounter++;
        //loopCounter set to 30 limits the splash screen to 3 seconds
        if (BPAWindowExists || loopCounter > 30) {
            SetCursor(cursors[0]);
            PostQuitMessage(0);
        }
        break;
    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    case WM_SETCURSOR:
        SetCursor(cursors[current_cursor]);
        break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}

BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam)
{
    //If we have found a visible window within the correct
    //process then we can give up the search
    DWORD ProcID;
    GetWindowThreadProcessId(hWnd, &ProcID);
    if (ProcID == pi.dwProcessId)
    {
        if (IsWindowVisible(hWnd) > 0)
        {
            BPAWindowExists = TRUE;
            return FALSE;
        }
    }

    //Otherwise keep looking
    return TRUE;
}