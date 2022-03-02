#!/usr/bin/python
# -*- coding: utf-8 -*-
##############################################################################
#
# ....Blue Prism Tools module
#  Shared between Automate V3 and BP Application Manager release scripts
#

import sys
import os
import re

if __name__ == '__main__':
    print 'This module can only be imported'
    sys.exit(1)

# The following are available once initialised, and will contain the path
# to the relevant tool, or None if it is not installed.
compilerRoot = None
compiler = None
tester = None
nunitpath = None
helpcompiler = None
signtool = None
cscript = None
git = None


# Call this to initialise stuff before using the module
def init():

    # Find all our external tools...
    global compilerRoot, compiler, tester, nunitpath, helpcompiler, coop
    global signtool, cscript, coopexport, git

    compilerRoots = [
        r"C:\Program Files\Microsoft Visual Studio\2017\Professional\Common7",
        r"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7",
        r"C:\Program Files\Microsoft Visual Studio 12.0\Common7",
        r"D:\Microsoft Visual Studio 12.0\Common7"]
    compilerRoot = None
    for cr in compilerRoots:
        if os.path.exists(cr):
            compilerRoot = cr
            break

    if not compilerRoot is None:
        compiler = compilerRoot + r"\IDE\devenv.exe"
    else:
        compiler = None

    helpcompilerlocations = [
        r"C:\Program Files\HTML Help Workshop\hhc.exe",
        r"D:\Program Files\HTML Help Workshop\hhc.exe",
        r"C:\Program Files (x86)\HTML Help Workshop\hhc.exe"]
    helpcompiler = None
    for hhc in helpcompilerlocations:
        if os.path.exists(hhc):
            helpcompiler = hhc
            break

    git = r"C:\Program Files\Git\bin\git.exe"
    if not os.path.exists(git):
        git = None

    kitRoots = [
        r"C:\Program Files\Windows Kits\8.1\bin\x86",
        r"D:\Program Files\Windows Kits\8.1\bin\x86",
        r"C:\Program Files (x86)\Windows Kits\8.1\bin\x86"]
    kitRoot = None
    for cr in kitRoots:
        if os.path.exists(cr):
            kitRoot = cr
            break

    if kitRoot is not None:
        signtool = kitRoot + r"\signtool.exe"
        if not os.path.exists(signtool):
            signtool = None

    cscript = r"C:\WINDOWS\system32\cscript.exe"
    if not os.path.exists(cscript):
        cscript = None

    tester = r"C:\dotcover\dotcover.exe"
    if not os.path.exists(tester):
        tester = None

    nunitpath = r"C:\Program Files\NUnit 2.6.4\bin\nunit-console.exe"
    if not os.path.exists(nunitpath):
        nunitpath = None


# Update the given assembly info file with a new version number
def update_asminfo(file, version, novcs):

    with open(file, 'r') as f:
        txt = f.read()

    for pattern in [
            # AssemblyInfo.vb...
            r'(?m)^(<Assembly: AssemblyVersion\(")[^"]*',
            r'(?m)^(<Assembly: AssemblyFileVersion\(")[^"]*',
            # AssemblyInfo.cs...
            r'(?m)^(\[assembly: AssemblyVersion\(")[^"]*',
            r'(?m)^(\[assembly: AssemblyFileVersion\(")[^"]*'
            ]:
        txt = re.sub(pattern, r'\g<1>' + version, txt)

    with open(file, 'w') as f:
        f.write(txt)


def update_resource_file(file, version):
    # First we need to split the version number down, since it's helpfully
    # stored in a different format to the norm
    # Extract the numbers from the version string.
    regex = re.compile('\d+')
    ver = regex.findall(version)

    # make sure we have 4 numbers in there for our replace
    while len(ver) < 4:
        ver.append('0')

    # read the resource file into 'txt'
    f = open(file, 'r')
    txt = f.read()
    f.close()

    # replace any file or product version numbers, retain the rest of the
    # line and any spaces between the version components
    rx = re.compile(
        r'(?im)^(.*(?:FILE|PRODUCT)VERSION.*)\d+,( ?)\d+, ?\d+, ?\d+(.*)$')

    # \1 = Prefix to the version number
    # \2 = Space if version number has a space between components
    # \3 = Postfix to the version number
    txt = rx.sub(
        r'\g<1>' + ver[0] +
        ',\g<2>' + ver[1] +
        ',\g<2>' + ver[2] +
        ',\g<2>' + ver[3] + '\g<3>',
        txt)

    # write the file back out
    f = open(file, 'w')
    f.write(txt)
    f.close()
