@echo off

choice /C YN /M "This will clean unversioned files within your local repository. Continue?"
if %errorlevel%==2 exit /b

call msbuild build.targets /t:VeracodePackage /p:Configuration=Debug;BluePrismPlatforms=x64;LoginAgentPlatform=x64;UnitTestsEnabled=False;CleanBeforeBuildEnabled=True
pause