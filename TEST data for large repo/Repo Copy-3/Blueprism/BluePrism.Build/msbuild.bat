@echo off

REM Convenience bat file used when running build from command line 
REM to avoid typing full path to msbuild.exe. It locates where 
REM MSBuild 15.0 is installed, then runs msbuild.exe with any 
REM supplied command-line arguments.

set msbuild="%programfiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe"
if not exist %msbuild% set msbuild="%programfiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe"
if not exist %msbuild% goto :msbuildnotfound

%msbuild% %*
goto :eof

:msbuildnotfound
echo msbuild.exe could not be found in any of the expected locations
exit /B 1