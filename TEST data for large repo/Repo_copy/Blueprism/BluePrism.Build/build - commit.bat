@echo off
call msbuild build.targets /t:Commit /p:Configuration=Debug;BluePrismPlatforms=x64
pause