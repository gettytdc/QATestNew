#!/usr/bin/python
# -*- coding: utf-8 -*-
##############################################################################
#
#  Automate Wiki Update script - updates documentation, etc
#     --help for options
#     Return code is 0 if successful and non-zero otherwise
#

import os
import sys
import subprocess
import traceback
from optparse import OptionParser
import tempfile

# Set the current directory to the release directory (where the script is) so we
# can use relative paths for everything.
# NOTE: Do this before importing mwclient, because it messes up sys.path!
os.chdir(sys.path[0])
if not os.path.exists('UpdateWiki.py'):
    print 'Something is wrong with the current directory!'
    sys.exit(1)

import mwclient


def do_update(
    site,
    pagename,
    newtext,
    publish
    ):

    # Add to global list.
    global updatedpages, reallyupdatedpages
    updatedpages.append(pagename)

    page = site.Pages[pagename]
    pagetext = page.edit()

    preindex = pagetext.find('<!-- BEGIN AUTO-GENERATED DOCUMENTATION -->')
    if preindex == -1:
        print('Failed to find start of auto-generated block on wiki')
        sys.exit(1)
    postindex = pagetext.find('<!-- END AUTO-GENERATED DOCUMENTATION -->')
    if preindex == -1:
        print('Failed to find end of auto-generated block on wiki')
        sys.exit(1)

    pretext = pagetext[:preindex]
    oldtext = pagetext[preindex:postindex]
    posttext = pagetext[postindex:]
    newtext = '<!-- BEGIN AUTO-GENERATED DOCUMENTATION -->\n' \
        + newtext.replace('\r', '')

    if newtext == oldtext:
        print('...unchanged')
    else:
        if publish:
            print('...updating')
            newpagetext = pretext + newtext + posttext
            page.save(newpagetext, summary='Updated auto-generated docs')
            reallyupdatedpages.append(pagename)
        else:
            print('...changed')



# Paths to files
automatec_exe = '../bin/automatec.exe'
uiscript_exe = '../bin/UIScript.exe'

# Parse command line...
parser = OptionParser()
parser.add_option(
    '-p', '--publish', action='store_true', default=False,
    help='Publish the changes to the wiki',
    )
parser.add_option(
    '-a', '--all', action='store_true', default=False,
    help='Do everything - i.e. build code and create database'
    )
parser.add_option(
    '-X', '--xmpp', action='store_true', default=False,
    help='Notify relevant actions via xmpp'
    )
parser.add_option(
    '--qadir', default=os.path.join('..', '..', 'qacontrol', 'qa'),
    help="Specify qa directory (where runtests.py is) if not in the default location"
    )
(options, args) = parser.parse_args()

msg = 'Running UpdateWiki.py with '
for arg in sys.argv[1:]:
    msg += arg + ' '
print(msg)

try:

    if options.all:
        if subprocess.call([sys.executable, 'Release.py', '-m', '-b', 'V5'],
            shell=True) != 0:
            print('Failed to build')
            sys.exit(1)
        p = subprocess.Popen([sys.executable, os.path.join(options.qadir, 'runtests.py'), '-w', '-o',
            '--automate_projpath', os.path.abspath('..')],
            shell=True, cwd=options.qadir, stdout=subprocess.PIPE)
        output = p.communicate()[0]
        if p.returncode != 0:
            print('Failed to set up database - ' + output)
            sys.exit(1)

    # Prepare wiki objects
    server = ('https', 'portal.blueprism.com')
    site = mwclient.Site(server, path='/wiki/')
    site.login('TestManager', 'TestManagerLogin')

    # This will be a list of all the pages we update, for generating the index...
    updatedpages = []
    # And this will be the same, but only ones we actually changed this time...
    reallyupdatedpages = []

    # ##################################################################################
    #
    # Internal business objects
    #

    # These are the internal business objects we'll update documentation for.
    #   (name, clsid, wiki page name)
    ibos = [
        ("Work Queues", "Blueprism.Automate.clsWorkQueuesActions", "Work Queues"),
        ("Credentials", "Blueprism.Automate.clsCredentialsActions", "Credentials Management"),
        ("Calendars", "clsCalendarsBusinessObject", "Calendars Internal Business Object"),
        ("Encryption", "EncryptionBusinessObject", "Encryption Internal Business Object"),
        ("Environment Locking", "BluePrism.AutomateAppCore.clsEnvironmentLockingBusinessObject", "Environment Locking"),
        ("Collections", "Blueprism.AutomateProcessCore.clsCollectionActions", "Collections")
    ]
    for ibo in ibos:
        name, clsid, wikipage = ibo
        print('Updating ' + name + ' internal business object')
        p = subprocess.Popen([
            automatec_exe,
            '/getbod',
            clsid,
            '/user',
            'admin',
            'admin',
            ], stdout=subprocess.PIPE)
        result = p.communicate()[0]
        if p.wait() != 0:
            print('Failed to get ' + name + ' Internal Business Object documentation:\n'
                              + result)
            sys.exit(1)

        do_update(site, wikipage, result, options.publish)


    # ##################################################################################
    #
    # Utility VBOs
    #

    # TODO: It would be better if we could auto-generate this list...
    # Also note that these objects all need to be loaded - the 'utility' QA
    # test makes this happen, so in addition to updating this list, a new
    # object would also have to be added to that test's dependencies in the
    # test definition.
    utilities = [
        'Utility - Collection Manipulation',
        'Utility - Date and Time Manipulation',
        'Utility - Encryption',
        'Utility - Environment',
        'Utility - File Management',
        'Utility - Foreground Locker',
        'Utility - General',
        'Utility - HTTP',
        'Utility - Image Manipulation',
        'Utility - Locking',
        'Utility - Numeric Operations',
        'Utility - Strings',
        ]

    print('Updating utility VBOs')
    docs = ''
    for vbo in utilities:
        p = subprocess.Popen([
            automatec_exe,
            '/getbod',
            vbo,
            '/user',
            'admin',
            'admin',
            ], stdout=subprocess.PIPE)
        result = p.communicate()[0]
        if p.wait() != 0:
            print('Failed to get ' + vbo + ' documentation:\n'
                              + result)
            sys.exit(1)
        docs += result + '\n'

    do_update(site, 'Utility VBOs', docs, options.publish)

    # ##################################################################################
    #
    # AutomateC command-line options
    #

    print('Updating AutomateC command-line help')
    p = subprocess.Popen([automatec_exe, '/help'], stdout=subprocess.PIPE)
    result = p.communicate()[0]
    if p.wait() != 0:
        print('Failed to get AutomateC help:\n' + result)
        sys.exit(1)

    result = '<pre>\n' + result + '''
</pre>
'''
    do_update(site, 'AutomateC', result, options.publish)

    # ##################################################################################
    #
    # Application Manager Query Language documentation
    #

    print('Updating Application Manager Query Language documentation'
                      )
    p = subprocess.Popen([uiscript_exe, '--qldocs'], stdout=subprocess.PIPE)
    result = p.communicate()[0]
    if p.wait() != 0:
        print('Failed to get Application Manager Query Language documentation:\n'
                           + result)
        sys.exit(1)

    do_update(site, 'ApplicationManagerQueryLanguage', result, options.publish)

    # ##################################################################################
    #
    # AMI reference
    #

    print('Updating AMI reference')
    p = subprocess.Popen([uiscript_exe, '--amidocs'], stdout=subprocess.PIPE)
    result = p.communicate()[0]
    if p.wait() != 0:
        print('Failed to get AMI reference:\n' + result)
        sys.exit(1)

    do_update(site, 'AMI Reference Manual', result, options.publish)

    # ##################################################################################
    #
    # Database documentation
    #

    print('Updating database documentation')
    p = subprocess.Popen([automatec_exe, '/getdbdocs', 'x'],
                         stdout=subprocess.PIPE)
    result = p.communicate()[0]
    if p.wait() != 0:
        print('Failed to get database documentation:\n' + result)
        sys.exit(1)

    do_update(site, 'Blue Prism Database', result, options.publish)

    # ##################################################################################
    #
    # Resource PC Protocol documentation
    #

    print('Updating Resource PC Protocol documentation')
    p = subprocess.Popen([automatec_exe, '/getresprotdocs'],
                         stdout=subprocess.PIPE)
    result = p.communicate()[0]
    if p.wait() != 0:
        print('Failed to get Resource PC Protocol documentation:\n'
                           + result)
        sys.exit(1)

    do_update(site, 'Resource PC Protocol', result, options.publish)

    # ##################################################################################
    #
    # Process Validation Checks
    #

    print('Updating Process Validation Checks documentation')
    p = subprocess.Popen([automatec_exe, '/getvalidationdocs', '/user', 'admin'
                         , 'admin'], stdout=subprocess.PIPE)
    result = p.communicate()[0]
    if p.wait() != 0:
        print('Failed to get Process Validation Checks documentation:\n'
                           + result)
        sys.exit(1)

    do_update(site, 'Process Validation Improvements', result, options.publish)

    # ##################################################################################
    #
    # User Roles and Permissions
    #

    print('Updating User Roles and Permissions documentation')
    outfile = os.path.join(tempfile.gettempdir(), 'updatewiki.out.txt')
    p = subprocess.Popen([automatec_exe, '/rolereport', outfile, '/user', 'admin'
                         , 'admin'], stdout=subprocess.PIPE)
    result = p.communicate()[0]
    if p.wait() != 0:
        print('Failed to get Process Validation Checks documentation:\n'
                           + result)
        sys.exit(1)
    f = open(outfile, 'r')
    result = f.read()
    f.close()
    os.remove(outfile)

    do_update(site, 'User Roles and Permissions', result, options.publish)

    # ##################################################################################
    #
    # Index of all this stuff...
    #
    print('Updating index of auto-generated documentation')
    result = ''
    for page in updatedpages:
        result += '*[[' + page + ']]\n'
    do_update(site, 'Auto-Generated Documentation', result, options.publish)

    msg = None
    for pagename in reallyupdatedpages:
        if msg == None:
            msg = pagename
        else:
            msg += ', ' + pagename
    if msg != None:
        # Runtest.py recognises this output...
        print('Wiki documentation updated:' + msg)

    # Finished...
    print('Complete')
    sys.exit(0)
except Exception, e:

    print('Exception - {0}'.format(traceback.format_exc()))
    sys.exit(1)

