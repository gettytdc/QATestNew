#!/usr/bin/python
# -*- coding: utf-8 -*-
##############################################################################
#
# ....Automate Help Update script - updates help, etc
#     Return code is 0 if successful and non-zero otherwise
#

import os
import sys
import subprocess

def do_update(file, newtext):
    f = open(file, 'r')
    pagetext = f.read()
    f.close()

    preindex = pagetext.find('<!-- BEGIN AUTO-GENERATED DOCUMENTATION -->')
    if preindex == -1:
        print >> sys.stderr, 'Failed to find start of auto-generated block in help'
        sys.exit(1)

    postindex = pagetext.find('<!-- END AUTO-GENERATED DOCUMENTATION -->')
    if preindex == -1:
        print >> sys.stderr, 'Failed to find end of auto-generated block in help'
        sys.exit(1)

    pretext = pagetext[:preindex]
    oldtext = pagetext[preindex:postindex]
    posttext = pagetext[postindex:]
    newtext = '<!-- BEGIN AUTO-GENERATED DOCUMENTATION -->\n' \
        + newtext.replace('\r', '')

    if newtext == oldtext:
        print >> sys.stderr, '...unchanged'
    else:
        newpagetext = pretext + newtext + posttext
        f = open(file, 'w')
        f.write(newpagetext)
        f.close()
        print >> sys.stderr, '...updated'


# Set the current directory to the release directory (where the script is) so we
# can use relative paths for everything.
os.chdir(sys.path[0])
if not os.path.exists('UpdateHelp.py'):
    print >> sys.stderr, 'Something is wrong with the current directory!'
    sys.exit(1)

# Paths to files
automatec_exe = '../bin/automatec.exe'

msg = 'Running UpdateHelp.py with '
for arg in sys.argv[1:]:
    msg += arg + ' '
print >> sys.stderr, msg

try:

    # ##################################################################################
    #
    # Resource PC Protocol documentation
    #

    print >> sys.stderr, 'Updating Resource PC Protocol documentation'
    p = subprocess.Popen([automatec_exe, '/getresprothtmldocs'],
                         stdout=subprocess.PIPE)
    result = p.communicate()[0]
    if p.wait() != 0:
        print >> sys.stderr, \
          'Failed to get Resource PC Protocol documentation:\n' + result
        sys.exit(1)

    do_update('..\\BluePrism.Automate\\Help\\helpResourcePCCommands.htm', result)

    print >> sys.stderr, 'Complete'
    sys.exit(0)

except Exception, e:

    print >> sys.stderr, 'Exception%s' % e
    sys.exit(1)

