@echo off
call msbuild build.targets /t:Commit /p:Configuration=Release;UpdateVersionNumbersInSourceCodeEnabled=False
pause