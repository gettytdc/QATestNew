@echo off
pushd ..\bin
for /f "tokens=*" %%a in ('dir /b /od *.nupkg') do set newest=%%a
echo Pushing %newest%...
"%~dp0\..\CredentialProviderBundle\nuget.exe" sources Add -Name "BluePrism.Packages" -Source "https://pkgs.dev.azure.com/blueprism/_packaging/BluePrism/nuget/v3/index.json"
"%~dp0\..\CredentialProviderBundle\nuget.exe" push -Source "BluePrism.Packages" -ApiKey VSTS "%newest%"
popd
pause