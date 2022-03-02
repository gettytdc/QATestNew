Function Test-CommandExists # Taken from https://devblogs.microsoft.com/scripting/use-a-powershell-function-to-see-if-a-command-exists/
{
  Param ($command)
  $previousErrorActionPreference = $ErrorActionPreference
  $ErrorActionPreference = 'stop'
  try {if(Get-Command $command){return $true}}
  catch {return $false}
  finally {$ErrorActionPreference = $previousErrorActionPreference}
}

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

if(-not (Test-Path .\temp)) {
  mkdir .\temp > $nul
}

if(-not (Test-CommandExists msbuild)) {
  if(Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin")
  {
    $env:Path += ";C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin"
  }
  elseif(-not (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin"))
  {
    Invoke-WebRequest -Uri https://aka.ms/vs/15/release/vs_buildtools.exe -OutFile .\temp\vs_buildtools.exe
    .\temp\vs_buildtools.exe --wait --quiet --norestart --nocache --layout .\BluePrism.Build\BuildTools --add Microsoft.Net.Component.4.7.TargetingPack --add Microsoft.Net.ComponentGroup.4.7.DeveloperTools --add Microsoft.VisualStudio.Component.NuGet.BuildTools --add Microsoft.VisualStudio.Component.NuGet | Out-Null # Out-Null forces the script to wait for the process to exit

    $env:Path += ";C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin"
  }
}

if(-not (Test-CommandExists nuget)) {
  Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile .\temp\nuget.exe
  $absoluteTempPath = Resolve-Path .\temp
  $env:Path += ";" + $absoluteTempPath.Path
}

nuget restore .\BluePrism.sln

msbuild.exe BluePrism.Build\build-api.targets /t:PublishApi /p:BuildApiStandalone=True

Remove-Item -r .\temp