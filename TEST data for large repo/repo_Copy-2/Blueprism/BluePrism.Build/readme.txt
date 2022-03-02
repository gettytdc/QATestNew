# BluePrism MSBuild Files

The BluePrism.Build directory contains MSBuild scripts used to build the solution. 

These are used mainly by our CI server but can also be run manually during development.

## Running MSBuild

These scripts should be run using MSBuild 15.0. MSBuild 15.0 is installed with Visual Studio 2017 and should be at one of the following locations:

%programfiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe
%programfiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe

## Running build.targets

The build.targets script builds the core product and runs additional tasks such as unit tests and signing the installers.

build.targets can be run from the command line as follows:

1. Open a command prompt
2. cd to the BluePrism.Build directory
3. Run msbuild.exe with build.targets file and any other parameters to control the build

For example, the following runs a typical commit build, which compiles the solution in debug configuration and runs any unit tests:

> cd C:\MyAutomateRepo\BluePrism.Build
> "%programfiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe" build.targets /t:Commit /p:Configuration=Debug;BluePrismPlatforms=x64

The BluePrism.Build directory also contains a useful msbuild.bat file which you can use to avoid typing the full path to msbuild.exe:

> msbuild build.targets /t:Commit /p:Configuration=Debug;BluePrismPlatforms=x64

See the "Build parameters" section within build.targets for full details of the properties available to control the build.

Build properties can be set as parameters on the command line as follows:

/p:Configuration=Release;BluePrismPlatforms="x86;x64"

Verbosity and logging to file can be enabled via the following parameters (type msbuild /? for a full list of options):

/verbosity:detailed /filelogger

The build that runs on push to the Git repo can be run as follows:

msbuild build.targets /t:Commit /p:Configuration=Debug;BluePrismPlatforms=x64

You can also run the above script by running "build - commit.bat"

The build used to create installers for both platforms can be run as follows (with certificate installed and the SigningCertificateHash property updated):

msbuild build.targets /t:Build /p:Configuration=Release;BluePrismPlatforms="x86;x64";VersionRevisionNumber=1234;SemanticVersionSuffix=alpha;SigningCertificateHash=52221052603b344c70b8f5dc214c06b4b62b0b8b;RevertSourceCodeChangesEnabled=True

The command used to create a package to upload to Veracode:

msbuild build.targets /t:VeracodePackage /p:Configuration=Debug;BluePrismPlatforms=x64;LoginAgentPlatform=x64;UnitTestsEnabled=False

You can also run the above script by running "build - veracode package.bat"

## Generate certificate for signing on your development machine

1) Create a new self-signed certificate and install it into the Trusted Root using the the makecert tool installed with the SDK:

   %programfiles(x86)%\Windows Kits\10\bin\10.0.15063.0\x86\makecert.exe" -r -pe -ss Root -sky exchange -n CN=BluePrismSignTest.cer <output folder>\BluePrismSignTest.cer

2) Open MMC to manage the current user's certificates and find BluePrismSignTest.cer in the Trusted Root Certification store. Right click the certificate and click All Tasks > Export. Export the certificate, ensuring that in the wizard:
	- You choose to export the Private Key with the certificate
	- You specify a password

3) Install the exported .pfx certificate into the current user's Personal certificate store. You will need to enter the password. As the self-signed certificate is installed into your root store, the imported certificate is implicitly trusted. As a result, signing your msi with this certificate will ensure the msi is trusted when run on your local machine.

4) Use the Thumbprint of the CN=BluePrismSignTest.cer file as the SigningCertificateHash in the msbuild command

## Running escrow.targets

This file creates a copy of the source code for providing to 3rd parties under our escrow agreement.

escrow.targets can be run from the command line as follows:

"%programfiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe" escrow.targets /p:VersionLabel=6.2

## Other Notes

The .targets and associated files are managed as a Class Library project to enable easy editing in Visual Studio. 

It also allows us to define the NuGet packages for tools used by the build.
