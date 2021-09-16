#!/usr/bin/python
# -*- coding: utf-8 -*-
##############################################################################
#
# ....Automate Build/Release Script
#     --help for options
#     Return code is 0 if successful and non-zero otherwise
#

import os
import sys
import re
import subprocess
import uuid
import string
import shutil
import time
import zipfile
import tempfile
import glob
import traceback
from optparse import OptionParser
import bptools
import xml.etree.cElementTree as ET

# The prefix we use for labelling this project in the history:
labelprefix = 'Automate Version '


# Dump the log file to the output
def dump_log():
    if not os.path.exists(logfile):
        print "NO LOG FILE!"
        return
    f = open(logfile, 'r')
    print f.read()
    f.close()

# Exports source code to directory and creates zip archive to supply to 3rd party
def make_escrow_deposit(parentdir):
    print 'Producing Escrow deposit'
    if not parentdir.endswith('\\'):
        parentdir += '\\'

    # Set up directories and paths
	outputdirname = "EscrowDeposit-" + options.outputversion
    exportdir = os.path.join(parentdir, outputdirname)
    escrowsourcedir = os.path.join(exportdir, "Source Code", "BluePrism")
    escrowdocsdir = os.path.join(exportdir, "Documentation")
    os.makedirs(escrowdocsdir)

    sourcecode = os.path.abspath("..\\")

    # Copy source code
    print 'Copying from ' + sourcecode + ' to ' + escrowsourcedir
    shutil.copytree(sourcecode, escrowsourcedir, ignore=shutil.ignore_patterns('.git*',authenticoderoot))

    # Create a readme
    fout = open(os.path.join(escrowdocsdir, 'readme.txt'), 'w')
    fout.write(get_escrow_readme_text())
    fout.close()

    # Zip contents of export directory
    zippath = os.path.join(parentdir, outputdirname) # .zip extension added by make_archive
    print 'Creating zip archive ' + zippath 
    shutil.make_archive(zippath, 'zip', exportdir)

def get_escrow_readme_text():
    return """######################### Blue Prism source code #############################

###Important Note
The software product known as "Blue Prism" was formerly known as "Blue Prism Automate" (or
simply "Automate"). Any references to "Automate" in the source code, or related documentation
applies to the product "Blue Prism".


###Installing the software
This package includes a ready-built installer for the Blue Prism software.

1. Visit the directory BluePrism.Automate\Setup\Release
2. Run the file "Automate.msi"
3. Complete the wizard accepting all default options. Blue Prism should now be available from
the start menu.
4. Run the main application. Enter details of your database server from the "File -> Connections"
menu option. Refer to the help menu for further advice.


### Building the software (producing a Release)
1. Install the python scripting language. The version used by Blue Prism at the time of writing is version 2.7
2. From within a command prompt, change directory to "BluePrism.Automate\Release"
3. Issue the command "Release.py --novcs --quiet --release --build V5" This will build a new release.


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
"""


# Set the current directory to the release directory (where the script is) so we
# can use relative paths for everything.
os.chdir(sys.path[0])
if not os.path.exists('Release.py'):
    print 'Something is wrong with the current directory!'
    sys.exit(1)

# Paths to login agent solution files
loginagentroot = r"..\LoginAgent"
loginagent = loginagentroot + r"\LoginAgent.sln"
loginagentsetup_x86 = loginagentroot + r"\Setup32\Setup32.vdproj"
loginagentsetup_x64 = loginagentroot + r"\Setup64\Setup64.vdproj"
loginagentbindir = loginagentroot + r"\bin"

# Paths to solution files etc.
bindir = r"..\bin"
apcroot = r"..\BluePrism.AutomateProcessCore"
apc = apcroot + r"\AutomateProcessCore.sln"
corelibroot = r"..\BPCoreLib"
corelib = corelibroot + r"\BPCoreLib.sln"
bparoot = r"..\BluePrism.Automate"
bpa = bparoot + r"\Automate.sln"
authenticoderoot = r"Authenticode"
bpaasminfo = bparoot + r"\AssemblyInfo.vb"
appcoreasminfo = bparoot + r"\AutomateAppCore\My Project\AssemblyInfo.vb"
bpserverasminfo = bparoot + r"\BPServer\Properties\AssemblyInfo.cs"
bpserviceasminfo = bparoot + r"\BPServerService\Properties\AssemblyInfo.cs"
bpaproj = bparoot + r"\BluePrism.Automate.vbproj"
appcoreproj = bparoot + r"\AutomateAppCore\AutomateAppCore.vbproj"
bpasetup_x86 = bparoot + r"\Setup_x86\Setup_x86.vdproj"
bpasetup_x64 = bparoot + r"\Setup_x64\Setup_x64.vdproj"
bpasetupdir_x86 = bparoot + r"\Setup_x86\Release"
bpasetupdir_x64 = bparoot + r"\Setup_x64\Release"
bpasplashrc = bparoot + r"\Splash\Splash.rc"
automatecroot = r"..\BluePrism.Automate\AutomateC"
automatecasminfo = automatecroot + r"\My Project\AssemblyInfo.vb"
automatecproj = automatecroot + r"\AutomateC.vbproj"
bpahelp = bparoot + r"\Help\AutomateHelp.hhp"
appmanroot = r"..\ApplicationManager"
appman = appmanroot + r"\ApplicationManager.sln"
dbroot = r"..\AutomateDB"
vboroot = r"..\VBO"
controlsasminfo = r"..\AutomateControls\AutomateControls\AssemblyInfo.vb"

# Other constants
logfile = os.path.join(tempfile.gettempdir(), 'build.txt')

# Parse command line...
# The version string we supply in the following constructor (and which is
# automatically accessible via the --version option) is used when building
# to determine the build type and requirements (such as VM and VS version)
# for the build.
parser = OptionParser(version="automate")
parser.add_option(
    '-S', '--novcs', action='store_true', default=False,
    help='Do not use version control system'
    )
parser.add_option(
    '--outputversion', default=None,
    help="Specify version to build - e.g. 5.0.16 or 5.1.0. "
         "The version is required, unless you're doing a "
         "local development build with --make."
    )
parser.add_option(
    '-r', '--release', action='store_true', default=False,
    help='Build release version'
    )
parser.add_option(
    '-b', '--buildtype', default='V5',
    help='Specify what to build - only V5 currently'
    )
parser.add_option(
    '-q', '--quiet', action='store_true', default=False,
    help="Don't ask questions"
    )
parser.add_option(
    '-n', '--nocommit', action='store_true', default=False,
    help="Don't commit to version control"
    )
parser.add_option(
    '-x', '--nopublish', action='store_true', default=False,
    help="Don't publish to R drive"
    )
parser.add_option(
    '-m', '--make', dest='makeonly', action='store_true', default=False,
    help='Just build everything. (Debug version, including appman!)'
    )
parser.add_option(
    '-R', '--makerelmode', action='store_true', default=False,
    help='Use with makeonly for Release configuration'
    )
parser.add_option(
    '-c', '--clean', action='store_true', default=False,
    help='Use in conjunction with --make to force clean before build'
    )
parser.add_option(
    '-E', '--escrow', default=None,
    help='Output escrow deposit of source code to this directory. A new directory containing the exported code and a zip archive will be created at this location.'
    )
parser.add_option(
    '-D', '--dev', action='store_true', default=False,
    help='Set up build files for current development version'
    )
parser.add_option(
    '--sign',
    help='Set password for signing. Without this, the output is not signed. '
         'The --pfx option must also be used.'
    )
parser.add_option(
    '--pfx',
    help='Use with --sign to supply the path to a pfx file to use for signing.'
    )
parser.add_option(
    '', '--unittest', action="store_true", default=False,
    help='Perform unit tests against built code.'
         'Implies --make (makeonly)'
    )
parser.add_option(
    '', '--coveragereport', action="store_true", default=False,
    help="Perform unit tests and coverage reports on built code."
         "Implies --unittest, which implies --makeonly."
    )
(options, args) = parser.parse_args()

# Force makeonly if unit tests are requested
if options.coveragereport:
    options.unittest = True

if options.unittest:
    options.makeonly = True

if options.makeonly:
    options.novcs = True

# Initialise git if needed
if options.novcs:
    # novcs implies nocommit which in turn requires nopublish
    options.nocommit = True
    options.nopublish = True

# Initialise the bptools module.
bptools.init()

gitrepo = None

# Validate parameter combinations...
if options.escrow is not None:
    options.nocommit = True
    options.nopublish = True
    options.quiet = True

if (options.sign and not options.pfx) or (options.pfx and not options.sign):
    print 'Both --sign and --pfx are needed for signing'
    sys.exit(1)

if options.outputversion is None:
    if options.makeonly:
        options.outputversion = '999.999.999-internal'
    else:
        print("Version needs to be specified")
        sys.exit(1)

if options.nocommit and not options.nopublish:
    print 'If you are publishing release files to the R drive, you must commit to version control'
    sys.exit(1)

if options.buildtype != 'V5':
    print 'Invalid build type - only V5 is currently supported.'
    sys.exit(1)

# Make sure we have all the external tools we need for the options supplied...
if bptools.compilerRoot == None or bptools.compiler == None:
    print "Can't find Visual Studio"
    sys.exit(1)
if bptools.helpcompiler == None:
    print "Can't find Help Compiler"
    sys.exit(1)
if not options.makeonly:  # Note that signing tools are not required to make only
    if not bptools.signtool:
        print "Can't find code signing tools"
        sys.exit(1)
if not options.makeonly:
    if bptools.cscript == None:
        print "Can't find MSI Vista Fix tool (cscript.exe)"
        sys.exit(1)

buildversion = options.outputversion
pversion = buildversion
if len(buildversion.split('-')) == 2:
    internalversion = True
    pversion = buildversion.split('-')[0]
else:
    internalversion = False

if not options.novcs:

    from git import *
    gitrepo = Repo('..')
    if not hasattr(gitrepo.is_dirty, '__call__'):
        print('You appear to have version 0.1 of git-python. You need a newer version!')
        sys.exit(1)

    # Check if the revision we're building has already got a tag - if so, refuse to build again, as it will fail
    if buildversion in gitrepo.tags:
        print("Unable to continue - {0} already exists as a tag in the repository".format(buildversion))
        sys.exit(4)

    if not options.makeonly:

        # Check that project is in fit state to have release built (resulting in
        # check-in)
        if not options.nocommit and not options.dev:
            if gitrepo.is_dirty():
                msg = \
                    "Cannot build release with a dirty repository. Use '--nocommit' or '--makeonly' options if this is what you really intend."
                print(msg)
                sys.exit(1)

        # If we're actually going to make a commit when we're done, we need to make sure we have the latest repo version.
        if not options.nocommit and not options.makeonly and not options.dev:
            msg = 'Pulling changes from origin'
            print(msg)
            git = gitrepo.git
            git.pull('origin')

# Copy source code (including the newly built release) to escrow directory
if options.escrow is not None:
    make_escrow_deposit(options.escrow)
    print('Complete')
    sys.exit(0)

# Do some pre-compile validation that the compiler won't do for us...
acdir = os.path.join(bparoot, 'AutomateAppCore')
gsvChecks = [os.path.join(acdir, 'clsServer.vb')]
pcdir = os.path.join(acdir, 'clsServerPartialClasses')
for fn in os.listdir(pcdir):
    gsvChecks.append(os.path.join(pcdir, fn))
gSvcount = 0
for fn in gsvChecks:
    with open(fn, 'r') as f:
        for line in f.readlines():
            if line.find('gSv') != -1:
                gSvcount += 1
if gSvcount != 1:
    print('Pre-compile validation failed - gSv can only be mentioned once in clsServer (and its partial classes)')
    sys.exit(1)

# Do the build...

# The things we're building...
# Solutions are (path, platform-specific) where platform-specific is True
# if the solution has x86/x64 configurations, or False for just "Any CPU".
solutions = [
    (corelib, True),
    (appman, True),
    (apc, False),
    (bpa, True)
    ]

asminfos = []
resourcefiles = []
for p, d, f in os.walk(".."):
    for fn in f:
        if fn in ['AssemblyInfo.cs', 'AssemblyInfo.vb']:
            asminfos.append(os.path.join(p, fn))
        if fn.lower().endswith('.rc'):
            resourcefiles.append(os.path.join(p, fn))
    for sd in ['NetSpell.2.1.7', 'qa', 'BPJabInstaller']:
        if sd in d:
            d.remove(sd)

# Delete the log file...
if os.path.exists(logfile):
    os.remove(logfile)

# Confirm what we're about to do
if not options.quiet and not options.dev and not options.makeonly:
    print '--------------------------------------------------------------------'
    print 'About to build version ' + buildversion + ' of Automate'
    print 'in ' + options.buildtype + ' configuration'
    print ''
    print 'Press Ctrl-C if there is a problem, otherwise any key to continue'
    raw_input('>')

if options.dev:
    msg = 'Setting development environment'
elif options.makeonly:
    msg = 'Building locally'
else:
    msg = 'Building version ' + buildversion + \
          ' in configuration ' + options.buildtype
print(msg)

# Determine which config we're building...
if options.makeonly or options.dev:
    if options.makerelmode:
        buildconfig = 'Release'
    else:
        buildconfig = 'Debug'
else:
    buildconfig = 'Release'

# Configure according to the type of build we are doing...
f = open(bpaproj, 'r')
txt = f.read()
f.close()
f = open(appcoreproj, 'r')
txt2 = f.read()
f.close()
f = open(automatecproj, 'r')
txt3 = f.read()
f.close()
if options.buildtype == 'V5':
    constants = \
        'ExceptionHandling=True,WorkQueues=True,ActiveDirectory=True,ProcessGroups=True,MultipleCalculation=True,Credentials=True,ResourcePools=True,Scheduler=True'
else:
    print 'Something went wrong with the build type!'
    sys.exit(1)
cregex = re.compile('<DefineConstants>[A-Za-z0-9=,]*</DefineConstants>')
txt = cregex.sub('<DefineConstants>' + constants + '</DefineConstants>',
                 txt)
if txt.find(constants) == -1:
    print('Something went wrong with the configuration - aborting')
    sys.exit(1)
txt2 = cregex.sub('<DefineConstants>' + constants + '</DefineConstants>',
                  txt2)
if txt2.find(constants) == -1:
    print('Something went wrong with the configuration - aborting')
    sys.exit(1)
txt3 = cregex.sub('<DefineConstants>' + constants + '</DefineConstants>',
                  txt3)
if txt3.find(constants) == -1:
    print('Something went wrong with the configuration - aborting')
    sys.exit(1)
f = open(bpaproj, 'w')
f.write(txt)
f.close()
f = open(appcoreproj, 'w')
f.write(txt2)
f.close()
f = open(automatecproj, 'w')
f.write(txt3)
f.close()

def clean_solution(sln):
    print 'Cleaning ' + sln
    if subprocess.call([
        bptools.compiler,
        sln,
        '/clean',
        buildconfig,
        '/out',
        logfile,
        ]) > 0:
        dump_log()
        sys.exit(1)

def build_solution(sln, config):
    print 'Building ' + sln + ' in ' + config + ' configuration'
    if subprocess.call([
        bptools.compiler,
        sln,
        '/build',
        config,
        '/out',
        logfile,
        ]) > 0:
        msg = 'Build failed on ' + sln
        print(msg)
        dump_log()
        print "Log is at " + logfile
        sys.exit(1)

def update_setup_project(sf, title=None):
    prodcode = string.upper(str(uuid.uuid1()))
    packcode = string.upper(str(uuid.uuid1()))
    f = open(sf, 'r')
    txt = f.read()
    f.close()
    if title:
        txt = re.compile('"Title" = "8:' + title + '"'
                      ).sub('"Title" = "8:' + title + ' - ' + buildversion + '"', txt)
    txt = re.compile('"ProductVersion" = "8:[\.0-9]*"'
                      ).sub('"ProductVersion" = "8:' + pversion + '"', txt)
    txt = re.compile('"ProductCode" = "[\{\}:a-zA-Z0-9-]*"'
                      ).sub('"ProductCode" = "8:{' + prodcode + '}"', txt)
    txt = re.compile('"PackageCode" = "[\{\}:a-zA-Z0-9-]*"'
                     ).sub('"PackageCode" = "8:{' + packcode + '}"', txt)
    f = open(sf, 'w')
    f.write(txt)
    f.close()

def apply_msi_fixes(msidir, msiname):
    # Fix the MSI file so it will install on vista
    print 'Running Vista MSI Fix Script'
    if subprocess.call([bptools.cscript, 'CustomAction_NoImpersonate.js',
                       os.path.join(msidir, msiname)]) > 0:
        print 'Failed Applying Fix'
        sys.exit(1)

    # Fix the MSI file so uninstalls/installs in the right order (like VS2005 did)
    print 'Running MSI fix action sequence script'
    if subprocess.call([bptools.cscript, 'MSI_SetActionSequence.js',
                       os.path.join(msidir, msiname),
                       'InstallExecuteSequence',
                       'RemoveExistingProducts',
                       '1525']) > 0:
        print 'Failed applying action sequence fix'
        sys.exit(1)

def sign_msi(msidir, msiname):
    print('Signing and timestamping MSI file')
    if subprocess.call([
        bptools.signtool,
        'sign',
        '/f',
        options.pfx,
        '/p',
        options.sign,
        '/d',
        'Blue Prism',
        '/du',
        'http://www.blueprism.com',
        os.path.join(msidir, msiname),
        ]) > 0:
        msg = 'Failed signing msi file'
        print(msg)
        sys.exit(1)
    timestamped = False
    timestampretries = 0
    timestampretrydelay = 60
    while not timestamped:
        if subprocess.call([bptools.signtool, 'timestamp', '/t',
                           'http://timestamp.digicert.com'
                           , os.path.join(msidir, msiname)]) == 0:
            timestamped = True
        else:
            if timestampretries > 4:
                msg = 'Failed to timestamp MSI file after ' \
                    + str(timestampretries) + ' attempts. Aborting release'
                print(msg)
                sys.exit(1)
            msg = 'Failed to timestamp MSI file - will retry in ' \
                + str(timestampretrydelay) + ' seconds'
            print(msg)
            time.sleep(timestampretrydelay)
            timestampretrydelay += 120
            timestampretries += 1

def build_loginagent(x64=False):

    # Update login agent setup project...
    if not options.makeonly and not options.dev:
        print 'Updating login agent setup project...'
        update_setup_project(loginagentsetup_x64 if x64 else loginagentsetup_x86, "Blue Prism Login Agent")

    # Clean solution
    if options.novcs:
        clean_solution(loginagent)
    else:
        if gitrepo:
            git = gitrepo.git
            print 'Cleaning LoginAgent using git'
            git.clean("LoginAgent/", d=True, f=True, x=True)
        else:
            if not options.makeonly or options.clean:
                print 'Git repo access is required for cleaning'
                sys.exit(1)

    # Build login agent
    buildconfigplatform = buildconfig + ('|x64' if x64 else '|x86')
    build_solution(loginagent, buildconfigplatform)

    #Apply fixes to the login agent msi file
    msidir = loginagentbindir
    msiname = 'LoginAgent_x64.msi' if x64 else 'LoginAgent_x86.msi'
    apply_msi_fixes(msidir, msiname)

    #Sign the login agent msi file
    if options.sign:
        sign_msi(msidir, msiname)

    # Copy file to automate bin dir
    shutil.copyfile(os.path.join(msidir, msiname),
    os.path.join(bindir, msiname))

# Clean, build, etc. When building releases, this will leave an Automate.msi
# or Automate64.msi as the result, depending on the 'x64' parameter. This
# will be signed, timestamped, etc, ready for publishing.
# To allow multiple calls, the resulting msi is also copied to the system
# temp directory.
def buildit(x64=False):

    # Update assembly infos & resources
    # This doesn't like version numbers with text in, i.e. pre-release versions.
    regex = re.compile('\d+')
    ver = regex.findall(buildversion)
    notextversion = '.'.join(ver)
    print '--------------------------------------------------------------------'
    if not options.makeonly or options.dev:
        for asm in asminfos:
            print 'Updating ' + asm
            bptools.update_asminfo(asm, notextversion, options.novcs)
        for res in resourcefiles:
            print 'Updating ' + res
            bptools.update_resource_file(res, buildversion)

    # Update VBO versions
    if not options.makeonly or options.dev:
        for file in glob.glob(os.path.join(vboroot, '*.xml')):
            print 'Updating VBO version ' + file
            with open(file, 'rb') as f:
                txt = f.read()
            txt = re.sub(r'(<process .*version=")[0-9\.]*', '\g<1>' +
                    notextversion, txt)
            with open(file, 'wb') as f:
                f.write(txt)

    # Update setup project...
    if not options.makeonly and not options.dev:
        print 'Updating setup project...'
        update_setup_project(bpasetup_x64 if x64 else bpasetup_x86)

    # If we were just setting up the development environment files, we can exit now.
    if options.dev:
        sys.exit(0)

    # Clean each solution in turn, checking for failure at each stage
    if not options.makeonly or options.clean:
        if options.novcs:
          for sln, ps in solutions:
             clean_solution(sln)
        else:
            if gitrepo:
                git = gitrepo.git
                print 'Cleaning using git'
                git.clean(d=True, f=True, x=True)
            else:
                if not options.makeonly or options.clean:
                    print 'Git repo access is required for cleaning'
                    sys.exit(1)

    #Build Login agent for the required platforms.
    if not options.makeonly and not options.dev:
        if not x64:
            build_loginagent(False)
            build_loginagent(True)
        else:
            build_loginagent(True)

    # Compile the HTML help...
    print 'Compiling Automate html help ...'
    retvalue = subprocess.call([bptools.helpcompiler, bpahelp])
    # hhc helpfully returns 1 for success and 0 for errors (?!)
    if (retvalue == 0): # ie. failure
        print 'Help Compilation Failed'
        sys.exit(1)

    # Build each solution in turn, checking for failure at each stage
    for sln, ps in solutions:
        buildconfigplatform = buildconfig
        if ps:
            buildconfigplatform += '|x64' if x64 else '|x86'
        else:
            buildconfigplatform += '|Any CPU'
        build_solution(sln, buildconfigplatform)

    # If we're just building, exit now, we're done:
    if options.makeonly:
        testfailed = False
        if options.unittest:
            print ("Local build complete. Commencing unit testing.")
            testfailed = False

            if options.coveragereport:
                # Run unit tests through dotcover, and report coverage.
                if not os.path.exists('coverage_reports'):
                    os.mkdir('coverage_reports')

                # Because of how dotcover works, the easiest way seems to be
                # XML generation to produce dynamic configuration.
                xmlroot = ET.Element("AnalyseParams")
                ET.SubElement(xmlroot, "TargetExecutable").text = bptools.nunitpath
                # We can map the solution into nunit, rather than trying to capture every DLL.
                ET.SubElement(xmlroot, "TargetArguments").text = os.path.join(sys.path[0], '..\BluePrism.sln')
                ET.SubElement(xmlroot, "TargetWorkingDir").text = os.path.join(sys.path[0], bindir)
                ET.SubElement(xmlroot, "Output").text = os.path.join(sys.path[0], "coveragereport.xml")
                filters = ET.SubElement(xmlroot, "AttributeFilters")
                ET.SubElement(filters, "AttributeFilterEntry").text = "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute"
                ET.SubElement(filters, "AttributeFilterEntry").text = "NUnit.Framework.TestFixtureAttribute"
                ET.SubElement(xmlroot, "ReportType").text = "XML"

                xmltree = ET.ElementTree(xmlroot)
                xmltree.write("C:\dotcover\dotcover-config.xml", xml_declaration=True)

                test = subprocess.Popen(
                        [bptools.tester,
                        'analyse',
                        'dotcover-config.xml'], cwd=r"C:\dotcover", stdout=subprocess.PIPE)
                teststdout = test.communicate()[0]
                print(teststdout)
                if test.returncode != 0:
                    testfailed = True

                # Now read the report in and print the overall test coverage back to Gitlab
                reportdata = ET.parse('coveragereport.xml')
                reportroot = reportdata.getroot()
                print("Test Coverage: " + reportroot.attrib['CoveragePercent'])
            else:
                # Standard nunit tests
                test = subprocess.Popen(
                    [bptools.nunitpath,
                    os.path.join(sys.path[0], '..\BluePrism.sln')], stdout=subprocess.PIPE)
                teststdout = test.communicate()[0]
                print (teststdout)

                if test.returncode != 0:
                    testfailed = True
        print("Local build complete. Don't forget not to commit things YOU didn't change!")
        sys.exit(1 if testfailed else 0)

    msidir = bpasetupdir_x64 if x64 else bpasetupdir_x86
    msiname = 'Automate.msi'
    apply_msi_fixes(msidir, msiname)

    # Digitally sign the MSI file.
    if options.sign:
        sign_msi(msidir, msiname)

    # Copy file to temporary output directory so it doesn't get 'cleaned'
    # out of the way...
    destname = 'Automate64.msi' if x64 else 'Automate.msi'
    shutil.copyfile(os.path.join(msidir, msiname),
        os.path.join(tempfile.gettempdir(), destname))

print 'Building for x86...'
buildit(x64=False)

print 'Building for x64...'
buildit(x64=True)

# Check for R drive existence before commmiting...
if not options.nopublish:
    if options.buildtype == 'V5':
        reldir = 'Automate {0}'.format(options.outputversion[0])
    else:
        print('Something went wrong with the build type!')
        sys.exit(1)
    relpath = os.path.join("R:\\", reldir)
    if not os.path.exists(relpath):
        print('Cannot find ' + relpath)
        sys.exit(1)

# Commit changes to version control...
if not options.nocommit:

    if not options.quiet:
        print 'ARE YOU SURE YOU WANT TO COMMIT CHANGES?'
        print '  (ctrl-C if not!)'
        raw_input('>')
    # Tag this release with its version number
    gitrepo.git.tag('-f', buildversion)
    gitrepo.git.push('--tags', 'origin')

# Publish output to R drive
if not options.nopublish:
    print('Publishing files to ' + relpath)
    retries = 0
    retrydelay = 10
    while True:
        try:
            if not internalversion:
                outdir = os.path.join(relpath, 'Automate ' + buildversion)
                # Might already exist, if we're retrying!
                if not os.path.exists(outdir):
                    os.mkdir(outdir)
            else:
                outdir = os.path.join(relpath, "Internal")
            for suffix, msiname in [('_x86', 'Automate.msi'),
                                    ('_x64', 'Automate64.msi')]:
                if internalversion:
                    shutil.copyfile(os.path.join(tempfile.gettempdir(), msiname),
                                    os.path.join(relpath, 'Internal\\Automate'
                                    + buildversion + suffix + '.msi'))
                else:
                    shutil.copyfile(os.path.join(tempfile.gettempdir(), msiname),
                                    os.path.join(outdir, 'BluePrism' + buildversion
                                    + suffix + '.msi'))

            # Generate database script...
            automatec = os.path.join(bindir, "AutomateC.exe")
            p = subprocess.Popen([automatec, '/getdbversion'], stdout=subprocess.PIPE)
            dbversion = p.communicate()[0].strip()

            p = subprocess.Popen([automatec, '/getdbscript'], stdout=subprocess.PIPE)
            script = p.communicate()[0]
            outfile = os.path.join(outdir, "BluePrismDBCreate_R" + dbversion + ".sql")
            with open(outfile, 'w') as f:
                f.write(script)

            p = subprocess.Popen([automatec, '/getdbscript', '/fromrev', '10'], stdout=subprocess.PIPE)
            script = p.communicate()[0]
            outfile = os.path.join(outdir, "BluePrismDBUpgrade_R" + dbversion + ".sql")
            with open(outfile, 'w') as f:
                f.write(script)
                                
            break
        except Exception, e:
            print("...exception while publishing: %s"%(traceback.format_exc()))
        retries += 1
        if retries > 10:
            print('...too many retries')
            sys.exit(1)
        print('...publish to R drive failed - retrying')
        time.sleep(retrydelay)
        retrydelay += 20

# Allow Runtest.py to determine what we built...
print('Build complete for:' + buildversion)

print('Complete')
sys.exit(0)
