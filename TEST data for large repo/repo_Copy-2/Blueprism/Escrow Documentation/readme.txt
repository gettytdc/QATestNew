######################### Blue Prism source code #############################

###Important Note
The software product known as "Blue Prism" was formerly known as "Blue Prism Automate" (or
simply "Automate"). Any references to "Automate" in the source code, or related documentation
applies to the product "Blue Prism".


### Building and Installing the software

This package can be used to build an installer for the Blue Prism software.

### Build Machine Setup

Set up the machine on which you will be building Blue Prism with the following software:

#### Visual Studio 2017
Visual Studio Enterprise or Visual Studio Professional. The following workloads and components should be selected:
1. .Net desktop development
2. ASP.Net and web development
3. Desktop development with C++. Include components:
    * Visual C++ ATL for x86 and x64
    * Visual C++ MFC for x86 and x64

#### NET Framework 4.7
This may be installed with Visual Studio 2017. If necessary, it can be installed separately by downloading the 4.7.2 Developer pack from the Microsoft website https://dotnet.microsoft.com/download/dotnet-framework/net472.

#### .NET Core 2.2
This may be installed with Visual Studio 2017. Can also be downloaded from Microsoft.

#### Windows SDK 8.1
The "Windows SDK 8.1" Visual Studio component should be installed during installation. 

#### Windows SDK 10.1.15063
This can be installed from https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk. There is a bug with the 10.1.15063 installer that might prevent it from being installed alongside a later version. In this case you will need to first uninstall the later version, then re-install it after installing version 10.1.15063.

#### Microsoft Build Tools 2015
This can be installed from https://www.microsoft.com/en-us/download/details.aspx?id=48159. The Wix framework, which is used to build Blue Prism installers, has a dependency on an earlier version of MSBuild. This requirement may be removed in the future.

#### Wix Toolset Visual Studio 2017 Extension
The "Wix Toolset Visual Studio 2017 Extension" can be installed via Extensions and Updates in Visual Studio.

#### Wix build tools 
The following software needs to be installed prior to building. http://wixtoolset.org/releases/  
version 3.11.1

#### HTML Help Workshop 1.3
This can be installed from https://www.microsoft.com/en-us/download/details.aspx?id=21138 or using the Chocolatey package html-help-workshop. After installation, hhc.exe needs to be available at "C:\Program Files (x86)\HTML Help Workshop\hhc.exe"

#### ScintillaNET
ScintillaNET is a text editing control that is used on certain Blue Prism controls. If you try and open the Visual Studio Form Designer for a form or control that contains a ScintillaNET control, you will get an error unless you have the Path variable in your Windows Environmental Variables set to a folder that contains the two ScintillaNET Lexer dlls. These files are held in the "ScintillaNET" folder in the automate repository.

#### Node.js Installation / Configuration
Node.js can be installed from https://nodejs.org/en/ - the LTS version is sufficient.

Node is used during the build process to transpile and obscurify the browser plugin.

After installation, configure npm as follows to ensure that it uses Visual Studio 2017 build tools to compile native modules:

> npm config set msvs_version 2017 --global

The npm modules used by Blue Prism include native addon modules, which include C++ code that is compiled during package installation.

### Building the software (producing a Release)

Build the installer as follows:

1. Open a command prompt and change to the "Source Code\BluePrism\BluePrism.Build" directory within the extracted deposit folder.
2. Inspect the msbuild.bat file in the BluePrism.Build directory. This attempts to find msbuild.exe at a number of expected locations but you may need to amend it depending on where Visual Studio 2017 is installed.
3. Run the build using the following command:

msbuild build.targets /t:Build /p:Configuration=Release;BluePrismPlatforms="x86;x64"

Other build parameters are available to control signing and version numbers - please refer to documented properties wihtin build.targets.

### Installing the Software

1. Visit the "Source Code\BluePrism\Publish" directory within the extracted deposit folder
2. Run one of the installer files (Automate<version>_x86.msi or Automate<version>_x64.msi)
3. Complete the wizard accepting all default options. Blue Prism should now be available from
the start menu.
4. Run the main application. Enter details of your database server from the "File -> Connections"
menu option. Refer to the help menu for further advice.


### Summary of relevant source code directories

    Directory Name                  Description
    ----------------------------------------------------------------------------------------------------------------

    BluePrism.Automate                    Main Blue Prism user interface code. This includes the main application
                                    and its related user interfaces: Process Studio, Control Room, etc.

                                    This directory also contains much of the end-user help, in html format.
                                    See the nested "Help" directory. This help is also available from within
                                    the Blue Prism user interface (via the help menu).

    BluePrism.AutomateProcessCore         A library of core functionality. This includes central data structures,
                                    and the runtime engine, which executes a process flowchart according to the
                                    user's wishes.

    AutomateControls                A library of user interface controls, used by the Automate project (BluePrism.Automate).

    AutomateDB                      A collection of database scripts, used by the Blue Prism product to create its
                                    database. It should not be necessary to run these manually - the product installs
                                    its own database (refer to end-user html help).

    ApplicationManager              Module for interacting with target applications, including browser (HTML)
                                    applications, windows applications, java and mainframe. The main Blue Prism
                                    product delegates its Object Studio interactions to this module.

    BPCoreLib                       A library of shared functionality required by both the Automate project (see
                                    BluePrism.Automate) and Application Manager.
                                    
#### Source Code for NuGet Packages
BluePrism.sln references certain NuGet packages that are developed internally at Blue Prism. The source code for these is included for reference in the following directories:

Source Code\document-processing
Source Code\utilities

Note that Document Processing (aka Decipher) functionality is not active in 6.5. The source code has been included for completeness but is not intended to provide working functionality.


