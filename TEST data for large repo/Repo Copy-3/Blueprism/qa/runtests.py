#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import os
import sys
import glob
import re
import subprocess
import time
import requests
import datetime
try:
    import winreg
    import win32evtlog
    from win32com.shell import shellcon, shell
except:
    pass
import shutil
import traceback
import json
from subprocess import Popen
from optparse import OptionParser
from socket import gethostname
from bs4 import UnicodeDammit
import semantic_version

# The QA directory must be the current directory!
qaroot = os.path.abspath(os.path.dirname(__file__))
os.chdir(qaroot)

# Now we can import a couple of things from relative paths
sys.path.append(os.path.join(sys.path[0], '..'))
import qacommon

_psutil = None   # Will get imported later if needed

# Determine if the supplied path appears to be a network path. This is only
# used for hacking around various Microsoft things that refuse to function
# from a "network drive", which in this case is simply a VBox share in a
# virtual development machine. We always use H: for these, but encapsulate
# it here in case it needs changing.
def isnetworkpath(path):
    return re.search(r"H:", path) is not None
# If the above is the case, various hacks will copy files to this location
# and use them there instead.
def getqatemp():
    qatemp = r"C:\QATemp"
    if not os.path.exists(qatemp):
        os.mkdir(qatemp)
    return qatemp


def check_process_started(process, output, proc):
    # This is a simple RC check now, as non-English text can't be matched.
    # The process name, however, should be in the output if it was started.
    if process.returncode > 0 or proc not in output:
        return False
    else:
        return True


# Get the HTTP GET commands required to authenticate with the Resource PC

def GetHTTPAuth():
    if not using_sso:
        return 'user%20name%20admin&password%20admin'
    p = subprocess.Popen([automatec_exe, '/getauthtoken'] + loginargs,
                         stdout=subprocess.PIPE)
    retval = UnicodeDammit(p.communicate()[0]).unicode_markup
    if p.wait() != 0:
        raise Exception('Failed to get token')
    token = retval.splitlines()
    return 'internalauth%20' + token[0]


def ResourcePC_GET(req, timeout=60, allowfail=False, port=8181):
    """Do a GET request to the local Resource PC

    :param req: the request
    :param timeout: timeout in seconds
    :param allowfail: True to not raise an exception if the response is not
       a 200
    :param port: port for resource PC
    :returns: (statuscode, responsetext)
    """
    url = "http://localhost:{port}{req}".format(port=str(port), req=req)
    try:
        f = requests.get(url)
        response = UnicodeDammit(f.text).unicode_markup
        status = f.status_code
        return (status, response)
    except Exception as e:
        print(e)
        return (500, str(e))


def GetSessionParams(sessionid):
    _, responseText = ResourcePC_GET('/' + GetHTTPAuth() + '&getparams%20' + sessionid)
    responseLines = responseText.splitlines()
    linenum = len(responseLines) - 1
    regex = re.compile(r"PARAMS:([.*])")
    match = regex.findall(responseLines[linenum])
    if len(match) > 0:
        return match[0]
    return responseLines[linenum]


def DeleteAllPendingSessions():
    _, ResponseText = ResourcePC_GET('/' + GetHTTPAuth() + '&status')
    ResponseLines = ResponseText.splitlines()
    regex = re.compile(r" - PENDING ([\w\d\-]*)")
    for i in range(0, len(ResponseLines) - 1):
        match = regex.findall(ResponseLines[i])
        if len(match) > 0:
            sessionid = match[0]
            print('Deleting', sessionid)
            ResourcePC_GET('/' + GetHTTPAuth() + '&delete%20' + sessionid)


def get_session_status(output):
    status_regex = re.compile(r".*(Running|Completed|Failed|Pending|Stopped|Terminated|Debugging).*")
    session_status = None
    for line in output.splitlines():
        status_match = status_regex.findall(line)
        if len(status_match) > 0:
            session_status = status_match[0]
    if session_status is None:
        print("Could not determine status from {0}".format(output))
    return session_status


def get_session_id(output):
    session_regex = re.compile(r"([0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12})")
    sessionid = None
    for line in output.splitlines():
        session_match = session_regex.findall(line)
        if len(session_match) > 0:
            sessionid = session_match[0]
    if sessionid is None:
        print("Could not determine session ID from {0}".format(output))
    return sessionid


def get_session_resource(output):
    resource_regex = re.compile(r"Actual resource:([\w\d\-]*)")
    resource = None
    for line in output.splitlines():
        resource_match = resource_regex.findall(line)
        if len(resource_match) > 0:
            resource = resource_match[0]
    if resource is None:
        print("Could not determine resource from {0}".format(output))
    return resource


respc = None


# Perform the run mode tests.
def dorunmodetests(parms):

    RunmodeProcesses = []
    CompatibilityMatrix = []

    if qacommon.is_ge_win7():
        RunmodeProcesses = [r"TestBackgroundVBOObjects",
                            r"TestForeGroundVBOObjects", r"TestExclusiveVBOObjects"
            , r"TestCallExclusiveProcess",
                            r"TestCallBackgroundProcess"]
        CompatibilityMatrix = [[True, True, False, False, True], [True, False,
                                                                  False, False, False], [False, False, False, False,
                                                                                         False], [False, False, False, False, True], [True,
                                                                                                                                      True, False, True, True]]
    else:

        # Processes of different runmodes. We will attempt to create a pair of sessions
        # in each combination, checking the result
        RunmodeProcesses = [
            r"TestBackgroundVBOObjects",
            r"TestForeGroundVBOObjects",
            r"TestExclusiveVBOObjects",
            r"TestBackgroundCOMObjects",
            r"TestForegroundCOMObjects",
            r"TestExclusiveCOMObjects",
            r"TestCallExclusiveProcess",
            r"TestCallBackgroundProcess",
        ]
        # Indicates whether the runmode of process i above is compatible with the
        # runmode of process j
        CompatibilityMatrix = [
            [
                True,
                True,
                False,
                True,
                True,
                False,
                False,
                True,
            ],
            [
                True,
                False,
                False,
                True,
                False,
                False,
                False,
                False,
            ],
            [
                False,
                False,
                False,
                False,
                False,
                False,
                False,
                False,
            ],
            [
                True,
                True,
                False,
                True,
                True,
                False,
                False,
                True,
            ],
            [
                True,
                False,
                False,
                True,
                False,
                False,
                False,
                False,
            ],
            [
                False,
                False,
                False,
                False,
                False,
                False,
                False,
                False,
            ],
            [
                False,
                False,
                False,
                False,
                False,
                False,
                False,
                True,
            ],
            [
                True,
                True,
                False,
                True,
                True,
                False,
                True,
                True,
            ],
        ]

    for proc in RunmodeProcesses:
        if subprocess.call([automatec_exe, '/publish', proc] + loginargs) > 0:
            return (False, 'Failed to publish process')

    # Check for any existing pending sessions (there should be none in a new database)
    # and abort if there are...
    _, ResponseText = ResourcePC_GET('/' + GetHTTPAuth() + '&status')
    ResponseLines = ResponseText.splitlines()
    for i in range(0, len(ResponseLines) - 1):
        if re.search(r"PENDING", ResponseLines[i]):
            return (False,
                    'Pending sessions exist. Cannot continue with runmode tests.'
                    )

    report = ''

    # Run the actual tests...
    try:
        for i in range(0, len(RunmodeProcesses) - 1):
            for j in range(0, len(RunmodeProcesses) - 1):
                FirstSessionID = ''
                SecondSessionID = ''

                # create session for first process, and check success.
                # This should always be successful
                status, Reply = ResourcePC_GET('/' + GetHTTPAuth() + '&create%20name%20'
                                               + RunmodeProcesses[i], allowfail=True)
                if status != 200:
                    report += 'HTTP Error - status is {0}'.format(status)
                    return (False, report)
                else:
                    ExpectedReply = 'SESSION CREATED'
                    SessionLocation = Reply.find(ExpectedReply)
                    if SessionLocation != -1:
                        report += \
                            'Successfully created session for first process ' \
                            + RunmodeProcesses[i] + '\n'
                        FirstSessionID = Reply[SessionLocation + 18:]
                        report += 'First Session ID ' + FirstSessionID + '\n'
                    else:
                        report += \
                            'Failed to create session for first process ' \
                            + RunmodeProcesses[i]
                        return (False, report)

                # create session for second process, and check success.
                # Success here will depend on the runmode of the two processes in question
                time.sleep(0.5)
                status, Reply = ResourcePC_GET('/' + GetHTTPAuth() + '&create%20name%20'
                                               + RunmodeProcesses[j], allowfail=True)
                if status != 200:
                    report += 'HTTP Error - status is {0}'.format(status)
                    return (False, report)
                else:
                    ExpectedReply = 'UNAVAILABLE'
                    if CompatibilityMatrix[i][j] == True:
                        ExpectedReply = 'SESSION CREATED'
                    SessionLocation = Reply.find(ExpectedReply)
                    if SessionLocation != -1:
                        report += "Achieved expected reply - '" \
                                  + ExpectedReply \
                                  + ' - when creating session for second process - ' \
                                  + RunmodeProcesses[j] + '\n'
                        SecondSessionID = Reply[SessionLocation + 18:]
                    else:
                        report += \
                            'Unexpected result when creating session for second process ' \
                            + RunmodeProcesses[j] + '\n'
                        report += 'Reply was ' + Reply + ' but we expected ' \
                                  + ExpectedReply
                        return (False, report)

                # delete both sessions i and j
                time.sleep(0.5)
                _, Reply = ResourcePC_GET('/' + GetHTTPAuth() + '&delete%20'
                                          + FirstSessionID)
                if Reply.find('SESSION DELETED') == -1:
                    report += 'Failed to delete session ' + FirstSessionID \
                              + ' - reply was - ' + Reply + '\n'
                    return (False, report)
                else:
                    report += 'Deleted first session - ' + FirstSessionID

                time.sleep(0.5)
                if SecondSessionID != '':
                    _, Reply = ResourcePC_GET('/' + GetHTTPAuth() + '&delete%20'
                                              + SecondSessionID)
                    if Reply.find('SESSION DELETED') == -1:
                        report += 'Failed to delete session ' \
                                  + SecondSessionID + ' - reply was - ' + Reply
                        return (False, report)
                    else:
                        report += 'Deleted second session - ' \
                                  + SecondSessionID + '\n'
    finally:
        # Delete any remaining sessions that are pending, to minimise impact on subsequent tests
        DeleteAllPendingSessions()

    for proc in RunmodeProcesses:
        if subprocess.call([automatec_exe, '/unpublish', proc] + loginargs) \
                > 0:
            return (False, 'Failed to unpublish process')

    return (True, report)


######################################################################################
# End Test of bug 3427................................................................ #
######################################################################################

poolresourcepcs = []  # Subprocess handle to active resource pc processes
poolresourcecontroller = None   # When set up, this says which is the controller, 0-n

def startpoolresourcepcs(preseed=False):
    global poolresourcepcs, poolresourcecontroller
    print('Creating pool.')
    args = [automatec_exe, '/poolcreate', '/pool', 'TestPool1'] + loginargs
    if subprocess.call(args) != 0:
        print('Failed to create pool')
    for i in range(5):
        print('Starting pool resource PC #' + str(i))
        args = [automate_exe, '/resourcepc', '/public', '/port', str(8182 + i)] + loginargs_nonlocale
        poolresourcepcs.append(subprocess.Popen(args))

    # If we're just pre-seeding event sources, this hangs forever, because no DB exists yet.
    if not preseed:
        print('Waiting for all resources to be online...')
        started = time.time()
        for i in range(5):
            while True:
                try:
                    _, responsetext = ResourcePC_GET('/user%20name%20admin&password%20admin&pool', timeout=10, port=8182 + i, allowfail=True)
                    if responsetext.find('Not in a pool') > 0:
                        break
                    else:
                        print('...hmm:' + responsetext)
                except:
                    pass
                if time.time() - started > 60:
                    return (False, 'Took too long for resources to come online')
                time.sleep(5)
        print('All are online')

    for i in range(5):
        print('Adding pool member ' + str(i) + ' to pool')
        res = gethostname().upper() + ':' + str(8182 + i)
        args = [
                   automatec_exe,
                   '/pooladd',
                   '/resource',
                   res,
                   '/pool',
                   'TestPool1',
               ] + loginargs
        if subprocess.call(args) != 0:
            return (False, 'Failed to add pool member ' + res)

    if not preseed:
        print('Waiting for all resources to be online and in the pool...')
        controller = -1
        started = time.time()
        for i in range(5):
            while True:
                try:
                    _, responsetext = ResourcePC_GET('/user%20name%20admin&password%20admin&pool', timeout=10, port=8182 + i, allowfail=True)
                    if responsetext.find('Member of') > 0:
                        print('#' + str(i) + ' is a member')
                        break
                    if responsetext.find('Controller of') > 0:
                        if controller != -1:
                            print('#' + str(i) \
                                  + ' also claims to be the controller')
                            return (False,
                                    'More than one resource said it was the controller!'
                                    )
                        print('#' + str(i) + ' is the controller')
                        controller = i
                        break
                except:
                    pass
                if time.time() - started > 180:
                    return (False, 'Took too long for resources to be in the pool')
                time.sleep(1)
        print('All are online - #' + str(controller) + ' is the controller')
        poolresourcecontroller = controller

    return (True, 'OK')


def stoppoolresourcepcs():
    global poolresourcepcs
    # This should work even if startpoolresourcepcs did not complete successfully
    for pc in poolresourcepcs:
        pc.kill()
    poolresourcepcs = []
    print('Deleting pool.')
    args = [automatec_exe, '/pooldelete', '/pool', 'TestPool1'] + loginargs
    if subprocess.call(args) != 0:
        print('Failed to delete pool')


def runpoolprocesses():
    global poolresourcecontroller
    print('Running 6 processes - should be one on each resource, plus one extra!')
    sessionids = []
    # Startup parameters, as we will set them and as we expect to get them back when we
    # check later!!
    startparams = \
        "<inputs><input name='MustBe1' type='number' value='1'/></inputs>"
    startparamsret = \
        '<inputs><input name="MustBe1" type="number" value="1" /></inputs>'
    for i in range(6):
        args = [
                   automatec_exe,
                   '/run',
                   'PoolTest',
                   '/startp',
                   startparams,
                   '/resource',
                   'TestPool1',
               ] + loginargs
        p = subprocess.Popen(args, stdout=subprocess.PIPE)
        output = UnicodeDammit(p.communicate()[0]).unicode_markup
        if p.wait() != 0:
            return (False, 'Failed to start process - ' + output)
        sessionid = get_session_id(output)
        actualres = get_session_resource(output)
        if not sessionid:
            return (False, 'Failed to get session id - output:\n' + output)
        if not actualres:
            return (False, 'Failed to get actual resource - output:\n' + output)
        sessionids.append(sessionid)
        print('...started session ' + sessionid + ', running on ' + actualres)

    time.sleep(2)
    print('Telling all processes to stop')
    for s in sessionids:
        # We *must* talk to the controller here, see bug #6275
        status, resptxt = ResourcePC_GET('/' + GetHTTPAuth() + '&setvar%20' + s + '%20%5BStop%5D%20Flag%20%22True%22',
                                         port = 8182 + poolresourcecontroller, allowfail=True)
        if status != 200:
            return (False, 'Failed to set Stop variable on ' + s + ' - HTTP Error - status is {0}'.format(status))
        if resptxt.splitlines()[-1] != "SET":
            return (False, 'Failed to set Stop variable on ' + s + ' - response was:"' + resptxt + '"')


    print('Waiting for all processes to complete')
    while len(sessionids) > 0:
        time.sleep(2)
        args = [automatec_exe, '/status', sessionids[0]] + loginargs
        p = subprocess.Popen(args, stdout=subprocess.PIPE)
        output = UnicodeDammit(p.communicate()[0]).unicode_markup
        if p.wait() != 0:
            return (False, 'Failed to get status of session ' + sessionids[0])
        if output.find('Status:Completed') != -1:
            print('...' + sessionids[0] + ' has completed')
            # Check the startup parameters are as we set them. Tests bug #4611.
            paramsused = GetSessionParams(sessionids[0])
            if paramsused != startparamsret:
                return (False,
                        'Startup parameters used did not match those set - '
                        + paramsused)
            sessionids = sessionids[1:]
        elif output.find('Status:Failed') != -1:
            return (False, 'Process failed')
        elif output.find('Status:Running') == -1:
            return (False, 'Unexpected process status - ' + output)

    return (True, 'Processes completed')


def dopooltest(parms):
    (result, msg) = startpoolresourcepcs()
    if not result:
        return (False, msg)

    # Publish the process:
    if subprocess.call([automatec_exe, '/publish', 'PoolTest'] + loginargs) \
            > 0:
        return (False, 'Failed to publish process')

    # Run processes...
    (result, msg) = runpoolprocesses()

    if subprocess.call([automatec_exe, '/unpublish', 'PoolTest'] + loginargs) \
            > 0:
        return (False, 'Failed to unpublish process')

    stoppoolresourcepcs()
    if result == False:
        return (result, msg)
    return (True, 'Pool test succeeded')


def dohttptests(parms):
    args = ['code/HTTP/bin/debug/HTTP.exe']
    if using_sso:
        args.append(automatec_exe)
    p = subprocess.Popen(args, stdout=subprocess.PIPE)
    report = UnicodeDammit(p.communicate()[0]).unicode_markup
    if p.wait():
        return (False, 'HTTP test failed - \n' + report)
    else:
        return (True, 'HTTP test succeeded - \n' + report)


def dobusobjwstest(parms):
    if bp_features["independent_ws_publishing"]:
        pubopt = '/publishws'
        unpubopt = '/unpublishws'
    else:
        pubopt = '/publish'
        unpubopt = '/unpublish'

    if subprocess.call([automatec_exe, pubopt, 'Calculator'] + loginargs) \
            > 0:
        return (False, 'Failed to publish Calculator business object')
    report = "Calling web service with credentials {0}\n".format(loginup)
    p = subprocess.Popen(['code/BusObjWSTest/bin/release/BusObjWSTest.exe',
                          'Calculator'] + loginup,
                         stdout=subprocess.PIPE)
    report += UnicodeDammit(p.communicate()[0]).unicode_markup + '\n'
    if p.wait() == 0:
        retval = (False, report + 'BusObjWS test failed')
    else:
        retval = (True, report + 'BusObjWS test succeeded')
    if subprocess.call([automatec_exe, unpubopt, 'Calculator']
                       + loginargs) > 0:
        return (False, 'Failed to unpublish Calculator business object')
    return retval


def dobusobjrenamedwstest(parms):
    if bp_features["independent_ws_publishing"]:
        pubopt = '/publishws'
        unpubopt = '/unpublishws'
    else:
        pubopt = '/publish'
        unpubopt = '/unpublish'

    if subprocess.call([automatec_exe, pubopt, 'Calculator', 'RenamedCalculator'] + loginargs) \
            > 0:
        return (False, 'Failed to publish renamed Calculator business object')
    report = "Calling web service with credentials {0}\n".format(loginup)
    p = subprocess.Popen(['code/BusObjWSTest/bin/release/BusObjWSTest.exe',
                          'RenamedCalculator'] + loginup,
                         stdout=subprocess.PIPE)
    report += UnicodeDammit(p.communicate()[0]).unicode_markup + '\n'
    if p.wait() == 0:
        retval = (False, report + 'BusObjRenamedWS test failed')
    else:
        retval = (True, report + 'BusObjRenamedWS test succeeded')
    if subprocess.call([automatec_exe, unpubopt, 'Calculator']
                       + loginargs) > 0:
        return (False, 'Failed to unpublish renamed Calculator business object')

    # Re-publish as 'Calculator' in case it is published later by another test
    # (otherwise the service name 'RenamedCalculator' will be used by default)
    if subprocess.call([automatec_exe, pubopt, 'Calculator', 'Calculator'] + loginargs) \
            > 0:
        return (False, 'Failed to re-publish Calculator business object')
    if subprocess.call([automatec_exe, unpubopt, 'Calculator']
                       + loginargs) > 0:
        return (False, 'Failed to re-unpublish Calculator business object')
    return retval


def dowsuserdetect(parms):
    if bp_features["independent_ws_publishing"]:
        pubopt = '/publishws'
        unpubopt = '/unpublishws'
    else:
        pubopt = '/publish'
        unpubopt = '/unpublish'
    if subprocess.call([automatec_exe, pubopt, 'UserTest']
                       + loginargs) > 0:
        return (False,
                'Failed to publish UserTest process')
    p = subprocess.Popen(['code/BusObjWSTest/bin/release/BusObjWSTest.exe',
                          'UserTest'] + loginup, stdout=subprocess.PIPE)
    output = UnicodeDammit(p.communicate()[0]).unicode_markup
    report = output + "\n"
    if p.wait() != 0:
        retval = (False, report)
    else:
        found = False
        for line in output.splitlines():
            if re.search('Detected user:', line):
                du = line.split(':')[1]
                if '@' in du:
                    du = du.split('@')[0]
                if du == loginup[0]:
                    report += "Correct username detected\n"
                    retval = (True, report)
                else:
                    report += "Incorrect username detected - expected {0}\n".format(loginup[0])
                    retval = (False, report)
                found = True
                break
        if not found:
            report += "Didn't find username\n"
            retval = (False, report)

    if subprocess.call([automatec_exe, unpubopt, 'UserTest'
                        ] + loginargs) > 0:
        return (False,
                'Failed to unpublish UserTest process object')
    return retval


def dowsiotest(parms):
    if bp_features["independent_ws_publishing"]:
        pubopt = '/publishws'
        unpubopt = '/unpublishws'
    else:
        pubopt = '/publish'
        unpubopt = '/unpublish'
    if subprocess.call([automatec_exe, pubopt, 'WebServiceInputsOutputs']
                       + loginargs) > 0:
        return (False,
                'Failed to publish WebServiceInputsOutputs business object')
    p = subprocess.Popen(['code/BusObjWSTest/bin/release/BusObjWSTest.exe',
                          'InputsOutputs'] + loginup, stdout=subprocess.PIPE)
    report = UnicodeDammit(p.communicate()[0]).unicode_markup + '\n'
    if p.wait() == 0:
        retval = (False, report)
    else:
        retval = (True, report)
    if subprocess.call([automatec_exe, unpubopt, 'WebServiceInputsOutputs'
                        ] + loginargs) > 0:
        return (False,
                'Failed to unpublish WebServiceInputsOutputs business object')
    return retval


def dobusobjwstestauto(parms):
    if bp_features["independent_ws_publishing"]:
        pubopt = '/publishws'
        unpubopt = '/unpublishws'
    else:
        pubopt = '/publish'
        unpubopt = '/unpublish'
    if subprocess.call([automatec_exe, pubopt, 'WebServiceInputsOutputs']
                       + loginargs) > 0:
        return (False,
                'Failed to publish WebServiceInputsOutputs business object')
    p = subprocess.Popen(['code/BusObjWSTest/bin/release/BusObjWSTest.exe',
                          'Auto'] + loginup, stdout=subprocess.PIPE)
    report = UnicodeDammit(p.communicate()[0]).unicode_markup + '\n'
    if p.wait() == 0:
        retval = (False, report)
    else:
        retval = (True, report)

    # Stop the auto-created session so we leave the Resource PC in a clean
    # state at the end of the test.
    _, responsetext = ResourcePC_GET('/status')
    stopped = 0
    regex = re.compile(r" - (\w*) (\w*)")
    for line in responsetext.splitlines():
        match = regex.findall(line)
        if len(match) > 0:
            st = match[0]
            if st == 'IDLE':
                sessionid = match[1]
                ResourcePC_GET('/' + GetHTTPAuth() + '&stop%20' + sessionid)
                stopped += 1
    if stopped != 1:
        return (False, "Mismatch when looking for auto-created sessions to stop")

    if subprocess.call([automatec_exe, unpubopt, 'WebServiceInputsOutputs'
                        ] + loginargs) > 0:
        return (False,
                'Failed to unpublish WebServiceInputsOutputs business object')
    return retval


def test_dointass(parms):
    retval = test_runprocess(parms)
    # Kill off the Internet Explorer instance, because the test doesn't.
    # This is because the test ensures that the launch button in integration
    # assistant works so it can't really call a terminate action.
    for proc in _psutil.process_iter():
        try:
            if proc.name().find('iexplore') != -1:
                proc.kill()
        except:
            pass
    return retval

def dowsioautomatetest(parms):
    if bp_features["independent_ws_publishing"]:
        pubopt = '/publishws'
        unpubopt = '/unpublishws'
    else:
        pubopt = '/publish'
        unpubopt = '/unpublish'
    if subprocess.call([automatec_exe, pubopt, 'WebServiceInputsOutputs']
                       + loginargs) > 0:
        return (False,
                'Failed to publish WebServiceInputsOutputs business object')
    if subprocess.call([
                           automatec_exe,
                           '/regwebservice',
                           'WebServiceInputsOutputsService',
                           'http://localhost:8181/ws/webserviceinputsoutputs?WSDL',
                           '/wsauth',
                           'admin',
                           'admin',
                           '/timeout',
                           '15000',
                       ] + loginargs) > 0:
        return (False,
                'Failed to register WebServiceInputsOutputsService web service'
                )
    retval = test_runprocess(parms)
    if subprocess.call([automatec_exe, '/unregwebservice',
                        'WebServiceInputsOutputsService'] + loginargs) > 0:
        return (False,
                'Failed to unregister WebServiceInputsOutputsService web service'
                )
    if subprocess.call([automatec_exe, unpubopt, 'WebServiceInputsOutputs'
                        ] + loginargs) > 0:
        return (False,
                'Failed to unpublish WebServiceInputsOutputs business object')
    return retval

def dowsioautomatetest42(parms):
    if bp_features["independent_ws_publishing"]:
        pubopt = '/publishws'
        unpubopt = '/unpublishws'
    else:
        pubopt = '/publish'
        unpubopt = '/unpublish'
    if subprocess.call([automatec_exe, pubopt, 'WebServiceInputsOutputs42']
                       + loginargs) > 0:
        return (False,
                'Failed to publish WebServiceInputsOutputs42 business object')
    if subprocess.call([
                           automatec_exe,
                           '/regwebservice',
                           'WebServiceInputsOutputs42Service',
                           'http://localhost:8181/ws/webserviceinputsoutputs42?WSDL',
                           '/wsauth',
                           'admin',
                           'admin',
                           '/timeout',
                           '15000',
                       ] + loginargs) > 0:
        return (False,
                'Failed to register WebServiceInputsOutputs42Service web service'
                )
    retval = test_runprocess(parms)
    if subprocess.call([automatec_exe, '/unregwebservice',
                        'WebServiceInputsOutputs42Service'] + loginargs) > 0:
        return (False,
                'Failed to unregister WebServiceInputsOutputs42Service web service'
                )
    if subprocess.call([automatec_exe, unpubopt, 'WebServiceInputsOutputs42'
                        ] + loginargs) > 0:
        return (False,
                'Failed to unpublish WebServiceInputsOutputs42 business object')
    return retval


def dowsindependenttest(parms):

    p = subprocess.Popen([automatec_exe, '/regwebservice', 'QAWebService',
                          'http://localhost:8888/QAWebService.asmx?WSDL',
                          '/timeout', '30000']
                         + loginargs, stdout=subprocess.PIPE)
    output = UnicodeDammit(p.communicate()[0]).unicode_markup
    if p.wait() != 0:
        return (False,
                'Failed to register QA Sample Web Service web service - '
                + output)
    retval = test_runprocess(parms)
    if subprocess.call([automatec_exe, '/unregwebservice', 'QAWebService']
                       + loginargs) > 0:
        return (False, 'Failed to unregister QA Sample Web Service web service'
                )
    return retval


def dowsfidelitymockuptest(parms):
    if subprocess.call([automatec_exe, '/regwebservice', 'InstructionService_1'
                           , 'http://localhost:8888/FidelityMockup.asmx?WSDL']
                       + loginargs) > 0:
        return (False,
                'Failed to register Fidelity Mockup Web Service web service')
    retval = test_runprocess(parms)
    if subprocess.call([automatec_exe, '/unregwebservice',
                        'InstructionService_1'] + loginargs) > 0:
        return (False,
                'Failed to unregister Fidelity Mockup Web Service web service')
    return retval


def dowsincludetest(parms):
    if subprocess.call([automatec_exe, '/regwebservice', 'StockQuoteService',
                        'http://localhost:8089/StockQuoteWS/stockquoteservice.wsdl'
                        ] + loginargs) > 0:
        return (False, 'Failed to register StockQuote web service')
    if subprocess.call([automatec_exe, '/unregwebservice', 'StockQuoteService'
                        ] + loginargs) > 0:
        return (False, 'Failed to unregister StockQuote web service')
    return (True, 'Registered (and unregistered) service without errors')


def dowsincludetest2(parms):
    if subprocess.call([automatec_exe, '/regwebservice', 'StockQuoteService2',
                        'http://localhost:8089/StockQuoteWS2/stockquoteservice.wsdl'
                        ] + loginargs) > 0:
        return (False, 'Failed to register StockQuote2 web service')
    if subprocess.call([automatec_exe, '/unregwebservice', 'StockQuoteService2'
                        ] + loginargs) > 0:
        return (False, 'Failed to unregister StockQuote2 web service')
    return (True, 'Registered (and unregistered) service without errors')


def doqueuecreatetest(parms):
    if subprocess.call([
                           automatec_exe,
                           '/createqueue',
                           'Account Number',
                           'True',
                           '5',
                           '/queuename',
                           'Simple Queue',
                       ] + loginargs) > 0:
        return (False, 'Failed to create queue: Simple Queue')

    if subprocess.call([
                           automatec_exe,
                           '/deletequeue',
                           '/queuename',
                           'Simple Queue',
                       ] + loginargs) > 0:
        return (False, 'Failed to delete queue: Simple Queue')

    if subprocess.call([
                           automatec_exe,
                           '/createqueue',
                           '',
                           'True',
                           '1',
                           '/queuename',
                           'Keyless',
                       ] + loginargs) > 0:
        return (False, 'Failed to create queue: Keyless')

    if subprocess.call([
                           automatec_exe,
                           '/deletequeue',
                           '/queuename',
                           'Keyless',
                       ] + loginargs) > 0:
        return (False, 'Failed to delete queue: Keyless')

    if subprocess.call([
                           automatec_exe,
                           '/createqueue',
                           'MyKey',
                           'False',
                           '1',
                           '/queuename',
                           'Not Active',
                       ] + loginargs) > 0:
        return (False, 'Failed to create queue: Not Active')

    if subprocess.call([
                           automatec_exe,
                           '/deletequeue',
                           '/queuename',
                           'Not Active',
                       ] + loginargs) > 0:
        return (False, 'Failed to delete queue: Not Active')

    return (True, 'Queues created')


def dowsdltest(parms):

    if bp_features["independent_ws_publishing"]:
        pubopt = '/publishws'
        unpubopt = '/unpublishws'
    else:
        pubopt = '/publish'
        unpubopt = '/unpublish'
    try:
        if subprocess.call([automatec_exe, pubopt, 'Calculator']
                           + loginargs) > 0:
            return (False, 'Failed to publish Calculator business object')

        report = 'Retrieving WSDL...'
        _, responsetext = ResourcePC_GET('/ws/Calculator?wsdl', timeout=10)
        if responsetext.find('/wsdl:definitions') == -1:
            report += 'Response was incorrect:\n'
            report += responsetext
            return (False, report)

        report += 'Retrieving WSDL again...'
        _, responsetext = ResourcePC_GET('/ws/Calculator?wsdl', timeout=10)
        if responsetext.find('/wsdl:definitions') == -1:
            report += 'Response was incorrect'
            report += responsetext
            return (False, report)

        if subprocess.call([automatec_exe, unpubopt, 'Calculator']
                           + loginargs) > 0:
            report += 'Failed to unpublish Calculator business object\n'
            return (False, report)

        report += 'Complete\n'
        return (True, report)
    except Exception as e:
        report += 'Exception - ' + str(e) + '\n'
        return (False, report)


# parms[0]=True for the attaching version False for the normal launching one
def test_hookedevents(parms):

    report = ''

    if parms[0]:
        proc = 'Hooked Event Test (Attach)'
        # If we're attaching, launch the target app now. The process will terminate it for us.
        Popen(['code/Coop2BZR/Coop2BZR.exe'])
    else:
        proc = 'Hooked Event Test'

    # Publish the process:
    if subprocess.call([automatec_exe, '/publish', proc] + loginargs) > 0:
        report += 'Failed to publish process ' + proc + '\n'
        return (False, report)
    # Publish the Hooked Event Presser business object...
    if subprocess.call([automatec_exe, '/publish', 'Hooked Event Presser']
                       + loginargs) > 0:
        report += 'Failed to publish Hooked Event Presser\n'
        return (False, report)

    try:

        p = subprocess.Popen([automatec_exe, '/run', proc] + loginargs,
                             stdout=subprocess.PIPE)
        retval = UnicodeDammit(p.communicate()[0]).unicode_markup
        report += retval + '\n'
        if not check_process_started(p, retval, proc):
            report += '(was trying to start process:' + proc + ')\n'
            return (False, report)
        sessionid = get_session_id(retval)
        if sessionid is None:
            report += 'Unable to get session ID\n'
            return (False, report)

        # The main process should now be running. Wait a while, then use our
        # 'Hooked Event Presser' object to press the cancel button in the
        # target application, which should cause the main process to end
        # successfully when it detects that.
        time.sleep(20)
        _, replytext = ResourcePC_GET('/' + GetHTTPAuth()
                                      + '&create%20name%20Hooked%20Event%20Presser')
        SessionLocation = replytext.find('SESSION CREATED')
        if SessionLocation != -1:
            sessionid2 = replytext[SessionLocation + 18:SessionLocation + 18 + 35]
        else:
            report += 'Failed to create session for object:\n' + replytext
            return (False, report)

        # Wait for it to be idle, which is when it has finished initialising
        loopcount = 0
        while True:
            _, replytext = ResourcePC_GET('/' + GetHTTPAuth() + '&outputs%20'
                                          + sessionid2)
            if replytext.find('SESSION IDLE') != -1:
                break
            loopcount += 1
            if loopcount > 60:
                report += \
                    'Waited too long for session to be idle after creation\n'
                return (False, report)
            time.sleep(1)

        report += "Starting Do The Pressing action - session ID '" \
                  + sessionid2 + "'\n"
        _, replytext = ResourcePC_GET('/' + GetHTTPAuth() + '&action%20' + sessionid2
                                      + '%20Do%20The%20Pressing')
        if replytext.find('STARTED') == -1:
            report += 'Failed to start action:\n' + replytext
            return (False, report)

        # Wait for it to complete and get the outputs
        report += 'Getting outputs from Do The Pressing action\n'
        loopcount = 0
        while True:
            _, replytext = ResourcePC_GET('/' + GetHTTPAuth() + '&outputs%20' + sessionid2)
            if replytext.find('OUTPUTS:') != -1:
                outputsresult = True
                break
            loopcount += 1
            if loopcount > 60:
                report += 'Waited too long for outputs from action\n'
                report += 'Last response: ' + replytext
                outputsresult = False
                break
            time.sleep(1)

        time.sleep(2)
        _, replytext = ResourcePC_GET('/' + GetHTTPAuth() + '&stop%20' + sessionid2)
        if replytext.find('STOPPED') != -1:
            report += 'Failed to stop business object:\n' + replytext
            return (False, report)

        if not outputsresult:
            return (False, report)

        loopcount = 0
        while True:
            p = subprocess.Popen([automatec_exe, '/status', sessionid]
                                 + loginargs, stdout=subprocess.PIPE)
            retval = UnicodeDammit(p.communicate()[0]).unicode_markup
            report += "AutomateC status was {retval}".format(retval=retval)
            status = get_session_status(retval)
            if status is None:
                report += 'Failed to get session status\n' + retval
                return (False, report)
            if status == 'Failed':
                report += 'Process failed\n'
                # Show log for failed processes
                p = subprocess.Popen([automatec_exe, '/getlog', sessionid]
                                     + loginargs, stdout=subprocess.PIPE)
                retval = UnicodeDammit(p.communicate()[0]).unicode_markup
                report += retval + '\n'
                return (False, report)
            if status == 'Completed':
                report += 'Process completed successfully\n'
                return (True, report)
            loopcount += 1
            if loopcount > 20:
                report += \
                    'Waiting too long for final process completion status\n'
                return (False, report)
            time.sleep(4)
    finally:

        # Unpublish the process and objects
        if subprocess.call([automatec_exe, '/unpublish', proc] + loginargs) \
                > 0:
            report += 'Failed to unpublish process ' + proc + '\n'
            return (False, report)
        if subprocess.call([automatec_exe, '/unpublish', 'Hooked Event Presser'
                            ] + loginargs) > 0:
            report += 'Failed to unpublish Hooked Event Presser\n'
            return (False, report)


def dohtmlfailtest(parms):

    report = ''

    # Publish the processes:
    for proc in ['HTMLFail', 'HTMLFail Parent', 'HTMLFail Closer']:
        if subprocess.call([automatec_exe, '/publish', proc] + loginargs) > 0:
            report += 'Failed to publish process\n'
            return (False, report)

    # Run main process...
    proc = 'HTMLFail Parent'
    p = subprocess.Popen([automatec_exe, '/run', proc] + loginargs,
                         stdout=subprocess.PIPE)
    retval = UnicodeDammit(p.communicate()[0]).unicode_markup
    report += retval + '\n'
    if not check_process_started(p, retval, proc):
        report += '(was trying to start process:' + proc + ')\n'
        return (False, report)
    sessionid = get_session_id(retval)
    if sessionid is None:
        report += 'Unable to get session ID\n'
        return (False, report)
    while True:
        p = subprocess.Popen([automatec_exe, '/status', sessionid]
                             + loginargs, stdout=subprocess.PIPE)
        retval = UnicodeDammit(p.communicate()[0]).unicode_markup
        status = get_session_status(retval)
        if status is None:
            report += 'Failed to get session status\n' + retval
            return (False, report)
        if status == 'Completed' or status == 'Failed':
            break
        time.sleep(2)
    # Get the log
    p = subprocess.Popen([automatec_exe, '/getlog', sessionid] + loginargs,
                         stdout=subprocess.PIPE)
    proclog = UnicodeDammit(p.communicate()[0]).unicode_markup
    report += proclog + '\n'
    if status == 'Completed':
        report += 'Process should have failed!\n'
        return (False, report)
    if proclog.find('Should') != -1:
        report += 'Process should not have reached that point!\n'
        return (False, report)

    # Run closer process...
    proc = 'HTMLFail Closer'
    p = subprocess.Popen([automatec_exe, '/run', proc] + loginargs,
                         stdout=subprocess.PIPE)
    retval = UnicodeDammit(p.communicate()[0]).unicode_markup
    report += retval + '\n'
    if not check_process_started(p, retval, proc):
        report += '(was trying to start process:' + proc + ')\n'
        return (False, report)
    sessionid = get_session_id(retval)
    if sessionid is None:
        report += 'Unable to get session ID\n'
        return (False, report)
    while True:
        p = subprocess.Popen([automatec_exe, '/status', sessionid]
                             + loginargs, stdout=subprocess.PIPE)
        retval = UnicodeDammit(p.communicate()[0]).unicode_markup
        status = get_session_status(retval)
        if status is None:
            report += 'Failed to get session status\n' + retval
            return (False, report)
        if status == 'Failed':
            report += 'Process failed\n'
            # Show log for failed processes
            p = subprocess.Popen([automatec_exe, '/getlog', sessionid]
                                 + loginargs, stdout=subprocess.PIPE)
            retval = UnicodeDammit(p.communicate()[0]).unicode_markup
            report += retval + '\n'
            return (False, report)
        if status == 'Completed':
            report += 'Process completed successfully\n'
            return (True, report)
        time.sleep(2)


def createappmanconfigfile(enablelog):
    appmanconfigdir = shell.SHGetFolderPath(0, shellcon.CSIDL_APPDATA, 0, 0)
    appmanconfigfilename = 'appman_config.xml'
    appmanconfigpath = os.path.join(appmanconfigdir, appmanconfigfilename)
    print(("Creating appman config file at {0}".format(appmanconfigpath)))

    # Write our own config file
    f = open(appmanconfigpath, 'w')
    f.write('<?xml version="1.0"?>\n')
    f.write('<config>')
    f.write('  <userfontdirectory>' + qaroot
            + '\\code\\fontrec\\fonts\\</userfontdirectory>')
    if enablelog:
        f.write('<logfile>' + qaroot + '\\appmanlog.txt</logfile>')
        f.write("""
  <logtimings>true</logtimings>
  <logwin32>true</logwin32>
  <logwait>true</logwait>
  <logfontrec>true</logfontrec>
  <loghook>true</loghook>
  <agentdiags>true</agentdiags>
""")
    f.write('</config>')
    f.close()


def doamivalidation(parms):
    p = subprocess.Popen([uiscript_exe, '--amivalidate'],
                         stdout=subprocess.PIPE)
    report = UnicodeDammit(p.communicate()[0]).unicode_markup
    return (p.returncode == 0, report)


def doofflinefonttest(parms):
    # run each test through the UIScript Facility
    report = 'Beginning file based unit tests\n'
    extensions = ('.png', '.bmp')
    testroot = qaroot + r"\Code\FontRec\Tests"
    for testdir in [d for d in os.listdir(testroot)
                    if os.path.isdir(os.path.join(testroot, d))]:
        fontname = testdir  # os.path.dirname(testdir)
        report += 'Testing Font ' + fontname + '\n'
        testdirpath = os.path.join(testroot, testdir)
        for testfile in [f for f in os.listdir(testdirpath)
                         if os.path.isfile(os.path.join(testdirpath, f))
                            and f.endswith(extensions)]:
            imagefilepath = os.path.join(testdirpath, testfile)
            report += ' - Testing Font Sample "' + testdir + '\\' + testfile \
                      + '"\n'
            textfilename = testfile
            for ext in extensions:
                textfilename = textfilename.replace(ext, '.txt')
            referencefilepath = os.path.join(testdirpath, textfilename)
            if verifyfontsample(fontname, imagefilepath, referencefilepath):
                report += '   -- OK\n'
            else:
                report += '   -- FAILED\n'
                return (False, report)
    return (True, report)


def verifyfontsample(fontname, filepath, referencefilepath):
    p = subprocess.Popen([uiscript_exe, '--query',
                          'VerifyFontRecSample ReferenceFile="'
                          + referencefilepath.replace("\\", "\\\\") +
                          '" ImageFile="' + filepath.replace("\\", "\\\\")
                          + '" Font="' + fontname + '" MultiLine=True'],
                         stdout=subprocess.PIPE)
    retval = UnicodeDammit(p.communicate()[0]).unicode_markup
    if retval.rstrip('\n').rstrip('\r').endswith('OK'):
        return True
    else:
        print(retval)
        return False


def dolibcatpcomtest(parms):
    #    subprocess.Popen([r"cmd.exe", '/c', qaroot + r"\code\pcom\a.ws"])
    #    time.sleep(10)
    result, report = test_runprocess(parms)
    # Kill pcom...
    for proc in _psutil.process_iter():
        # It seems that, only on XP, access is denied when trying to get the
        # name of certain processes. So if getting the name fails, we just
        # assume it's not pcom, and skip over...
        ispcom = False
        try:
            if proc.name() == 'pcsws.exe' or proc.name() == 'pcscm.exe':
                ispcom = True
        except:
            pass
        if ispcom:
            proc.kill()
            gone, alive = _psutil.wait_procs([proc], 90)
            if len(alive) > 0:
                report += "\nIBM pcom ({0}) did not terminate.\n".format(proc.name())

    return (result, report)

def dolibcatrmdtest(parms):
    p = subprocess.Popen("java -cp RMDEmulator.jar;commons-net-1.4.1.jar com.lsdg.emulator.EmulatorFrame rmdPort=4554 dmPort=4555 connectPort=4321 debug=0 qhhost=localhost host=localhost",
                         cwd=qaroot + r"\Code\RMD", creationflags=subprocess.CREATE_NEW_CONSOLE)
    time.sleep(10)
    retval = test_runprocess(parms)
    p.kill()
    return retval

def doexporttest(parms):
    report = ''
    p = subprocess.Popen([automatec_exe, '/listprocesses']
                         + loginargs, stdout=subprocess.PIPE)
    procs = UnicodeDammit(p.communicate()[0]).unicode_markup.splitlines()
    if p.wait() != 0:
        report += 'Failed to get process list\n'
        return (False, report)

    exportdir = os.path.join(qaroot, 'Exports')
    if os.path.exists(exportdir):
        shutil.rmtree(exportdir)
    os.mkdir(exportdir)
    for proc in procs:
        p = subprocess.Popen([automatec_exe, '/export', proc]
                             + loginargs, stdout=subprocess.PIPE,
                             cwd=exportdir)
        UnicodeDammit(p.communicate()[0]).unicode_markup.splitlines()
        if p.wait() != 0:
            report += 'Failed to export ' + proc + '\n'
            return (False, report)

    report += 'Exported ' + str(len(procs)) + ' processes.\n'

    return (True,report)


def doauditlogtest(parms):
    try:
        report = ''

        p = subprocess.Popen([automatec_exe, '/getauditlog', '/age', '2d']
                             + loginargs, stdout=subprocess.PIPE)
        logsbefore = UnicodeDammit(p.communicate()[0]).unicode_markup.splitlines()
        if p.wait() != 0:
            report += 'Failed to get audit logs before\n'
            return (False, report)

        # Publish the 'Do Nothing' processes:
        proc = 'Do Nothing'
        if subprocess.call([automatec_exe, '/publish', proc] + loginargs) > 0:
            report += 'Failed to publish process\n'
            return (False, report)

        p = subprocess.Popen([automatec_exe, '/getauditlog', '/age', '2d']
                             + loginargs, stdout=subprocess.PIPE)
        logsafter = UnicodeDammit(p.communicate()[0]).unicode_markup.splitlines()
        if p.wait() != 0:
            report += 'Failed to get audit logs before\n'
            return (False, report)

        diff = len(logsafter) - len(logsbefore)
        if diff < 2:
            report += 'Not enough audit log differences\n'
            return (False, report)

        logsafter = logsafter[-diff:]
        foundpublish = False
        foundlogin = False
        report += 'Audit difference\n'
        for line in logsafter:
            report += '  ' + line + '\n'
            if using_sso:
                expecteduser = None
            else:
                expecteduser = 'admin'
            if ((not expecteduser or
                 line.find("user '" + expecteduser + "'") != -1)
                    and line.find("'Published'") != -1 and line.find("'Do Nothing'"
                                                                     ) != -1):
                foundpublish = True
            if ((not expecteduser or
                 line.find("User '" + expecteduser + "'") != -1)
                    and line.find('logged in') != -1 and line.find("'"
                                                                   + gethostname().upper() + "'") != -1):
                foundlogin = True

        if not foundpublish:
            report += """
Couldn't find audit of publishing
"""
            return (False, report)

        if not foundlogin:
            report += """
Couldn't find audit of login
"""
            return (False, report)

        return (True, report)
    finally:

        # Unpublish the process
        if subprocess.call([automatec_exe, '/unpublish', 'Do Nothing']
                           + loginargs) > 0:
            report += "Failed to unpublish process 'Do Nothing'\n"
            return (False, report)


def dostoppingprocesstest(parms):
    try:
        report = ''

        # Publish the 'Do Nothing' processes:
        proc = 'Do Nothing'
        if subprocess.call([automatec_exe, '/publish', proc] + loginargs) > 0:
            report += 'Failed to publish process\n'
            return (False, report)

        # Run the process
        p = subprocess.Popen([automatec_exe, '/run', proc] + loginargs,
                             stdout=subprocess.PIPE)
        retval = UnicodeDammit(p.communicate()[0]).unicode_markup
        report += retval + '\n'
        if not check_process_started(p, retval, proc):
            report += '(was trying to start process:' + proc + ')\n'
            return (False, report)
        sessionid = get_session_id(retval)
        if sessionid is None:
            report += 'Unable to get session ID\n'
            return (False, report)
        report += 'Session ID is ' + sessionid + '\n'

        # Wait for the process to be established and then stop it running
        time.sleep(10)
        _, replytext = ResourcePC_GET('/' + GetHTTPAuth() + '&stop%20' + sessionid)
        if replytext.find('STOPPING') == -1:
            report += 'Failed to stop process:\n' + replytext
            return (False, report)

        # Now wait and verify that it stops
        loopcount = 0
        stillrunning = True
        while stillrunning:
            time.sleep(2)
            loopcount += 1
            if loopcount > 30:
                report += 'Waiting too long for process to stop\n'
                return (False, report)
            _, replytext = ResourcePC_GET('/' + GetHTTPAuth() + '&status%20' + sessionid)
            print(replytext)
            lines = replytext.splitlines()
            for line in lines:
                if line.find(sessionid) != -1 and line.find('STOPPED') != -1:
                    stillrunning = False

        # Now test the same in the UI. We could put this test in the
        # standard list, but there would be the complication of having
        # to publish the 'Do Nothing' test somehow.
        return test_runprocess(['Control Room UI Tests'])
    finally:

        # If the "Control Room UI Tests" fails at the wrong point, it can leave
        # a pending session lying around and mess up later tests, so make sure
        # there isn't one! See bug #4847
        DeleteAllPendingSessions()

        # Unpublish the process
        if subprocess.call([automatec_exe, '/unpublish', 'Do Nothing']
                           + loginargs) > 0:
            report += "Failed to unpublish process 'Do Nothing'\n"
            return (False, report)


def restart_resourcepc():
    global respc
    if respc:
        print('Stopping resource PC...')
        respc.kill()
        time.sleep(10)
        print('Re-starting resource PC...')
        args = [automate_exe, '/resourcepc', '/public'] + loginargs_nonlocale
        respc = Popen(args + loginargs)
        time.sleep(10)
        status, response = ResourcePC_GET("/status", timeout=120, allowfail=True)
        if status != 200:
            raise Exception("Failed to restart resource PC - " + response)
        return respc
    else:
        return None


def test_failzombie(parms):
    try:
        report = ''

        # Publish the 'Do Nothing' processes:
        proc = 'Do Nothing'
        if subprocess.call([automatec_exe, '/publish', proc] + loginargs) > 0:
            report += 'Failed to publish process\n'
            return (False, report)

        # Run the process
        p = subprocess.Popen([automatec_exe, '/run', proc] + loginargs,
                             stdout=subprocess.PIPE)
        retval = UnicodeDammit(p.communicate()[0]).unicode_markup
        report += retval + '\n'
        if not check_process_started(p, retval, proc):
            report += '(was trying to start process:' + proc + ')\n'
            return (False, report)
        sessionid = get_session_id(retval)
        if sessionid is None:
            report += 'Unable to get session ID\n'
            return (False, report)
        report += 'Running as session ' + sessionid + '\n'

        # Wait for the process to be established and then kill the resource pc
        time.sleep(10)
        restart_resourcepc()

        # Now wait and verify that it shows as failed
        loopcount = 0
        while True:
            time.sleep(5)
            loopcount += 1
            if loopcount > 10:
                report += 'Waiting too long for process to fail\n'
                return (False, report)
            p = subprocess.Popen([automatec_exe, '/status', sessionid]
                                 + loginargs, stdout=subprocess.PIPE)
            retval = UnicodeDammit(p.communicate()[0]).unicode_markup
            report += retval + '\n'
            lines = retval.splitlines()
            regex = re.compile(r"Status: Failed")
            for line in lines:
                match = regex.match(line)
                if match is not None:
                    report += 'The process was correctly marked as failed'
                    return (True, report)
    finally:

        # Unpublish the process
        if subprocess.call([automatec_exe, '/unpublish', 'Do Nothing']
                           + loginargs) > 0:
            report += "Failed to unpublish process 'Do Nothing'\n"
            return (False, report)


extraresourcepcs = []


def startextraresourcepcs(preseed=False):
    global extraresourcepcs
    extraresourcepcs = []
    resprocmap, result = startspecificresourcepcs(list(range(8182, 8191)), preseed)
    extraresourcepcs.extend(list(resprocmap.values()))
    if not result:
        return (False, 'Failed to start resource pcs')
    return (True, 'OK')

def stopextraresourcepcs():
    global extraresourcepcs
    # This should work even if startextraresourcepcs did not complete successfully
    for pc in extraresourcepcs:
        pc.kill()

# Starts resource pcs on the given ports, returning (pcs, result) where pcs is a
# dictionary of the subprocess mapped against the port number, and result is the
# ultimate result, True = success, False = failed. Note that in the event of failure
# some or all PCs still may have been started, so the pcs data is still valid for
# the purposes of cleaning them up.
def startspecificresourcepcs(portnumbers, preseed=False):
    resprocmap = {}
    for port in portnumbers:
        args = [automate_exe, '/resourcepc', '/public', '/port', str(port)] + loginargs_nonlocale
        resprocmap[port] = subprocess.Popen(args)

    if not preseed:
        print('Waiting for all resources to be online...')
        started = time.time()
        for port in list(resprocmap.keys()):
            while True:
                try:
                    _, responsetext = ResourcePC_GET('/pool', port=port, timeout=10)
                    if re.search(r"Not in a pool", responsetext) is not None:
                        break
                    else:
                        print('...hmm:' + responsetext)
                except:
                    pass
                if time.time() - started > 360:
                    print('Took too long for resources to come online')
                    return (resprocmap, False)
                time.sleep(1)
        print('All are online')
    return (resprocmap, True)


def test_bpschedulerui(parms):

    try:
        report = ''

        # Publish the 'Do Nothing' processes:
        donothingproc = 'Do Nothing'
        if subprocess.call([automatec_exe, '/publish', donothingproc]
                           + loginargs) > 0:
            report += 'Failed to publish process\n'
            return (False, report)

        # Run the process that creates the task...
        (passed, report) = test_runprocess(['Scheduler GUI'])
        if not passed:
            return (False, report)

        # Now need to wait and see if the task (process "Do Nothing")
        # completes. It should get started in approximately 2 minutes!
        started = False
        finished = False
        timeoutmins = 5
        timeout = 60 * timeoutmins / 15  # <timeoutmins> minutes
        # regex for checking that the schedule is complete.
        # (?m) indicates that ^ and $ should match on SOL/EOL and not (only) SOF/EOF
        regex = re.compile(r"(?m)^- New Schedule.*\d?\d:\d\d:\d\d \|\s*$")
        while not finished:
            time.sleep(15)
            p = subprocess.Popen([automatec_exe, '/viewschedreport',
                                  'Recent Activity'] + loginargs,
                                 stdout=subprocess.PIPE)
            retval = UnicodeDammit(p.communicate()[0]).unicode_markup
            # Check if it's completed...
            if regex.search(retval):
                report += 'Schedule report indicates schedule is finished\n'
                report += retval + '\n'
                finished = True
            elif not started and retval.find('- New Schedule') != -1:
                report += 'Found entry in schedule report\n'
                report += retval + '\n'
                started = True
            timeout = timeout - 1
            if timeout <= 0:
                report += retval + '\n'
                report += 'Schedule did not complete after %d minutes\n' \
                          % (timeoutmins, )
                return (False, report)
    finally:

        # Unpublish the process
        if subprocess.call([automatec_exe, '/unpublish', donothingproc]
                           + loginargs) > 0:
            report += "Failed to unpublish process 'Do Nothing'\n"
            return (False, report)

    return (True, report)


def test_bpschedulermany(parms):

    donothingproc = 'Do Nothing'
    try:

        # Publish the 'Do Nothing' processes:
        if subprocess.call([automatec_exe, '/publish', donothingproc]
                           + loginargs) > 0:
            report = 'Failed to publish process\n'
            return (False, report)

        result, report = startextraresourcepcs()
        if not result:
            return (False, report)

        # Run the process that creates the task...
        (passed, report) = test_runprocess(['Scheduler GUI Many'])
        if not passed:
            return (False, report)

        # Now need to wait and see if the task (process "Do Nothing")
        # completes. It should get started in approximately 2 minutes!
        started = False
        finished = False
        timeoutmins = 5
        timeout = 60 * timeoutmins / 15  # <timeoutmins> minutes
        # regex for checking that the schedule is complete. Looking for something like this:
        #   -- Wed 18 May 2011 --
        #   - New Schedule                             | 22:18:45 | 22:18:45 | 22:20:58 |
        #   - * New Schedule - New Task                | -        | 22:18:45 | 22:20:58 |
        #   - = 13060117-1aee-4a70-ab8c-5be6d37332a9   | -        | 22:18:50 | 22:20:57 |
        #   - = 26188b99-b9e1-4c6f-ad19-fd3eff5e6dcf   | -        | 22:18:50 | 22:20:57 |
        #   - = b81f1090-4e13-4dce-8123-661940227500   | -        | 22:18:50 | 22:20:57 |
        #   etc...
        # (?m) indicates that ^ and $ should match on SOL/EOL and not (only) SOF/EOF
        #
        # There's a second regex for the case where a scheduled task started before
        # midnight and ended on the next day, which looks like this:
        # - = 2daf87b0-0542-4f26-a9f7-6f6c7cc04df9   | -        | 23:59:01 | 00:01:02 [2012-09-01] |
        #
        regex = re.compile(r"(?m)^- = .*\d\d:\d\d:\d\d \|\s*$")
        regex2 = re.compile(r"(?m)^- = .*\d\d:\d\d:\d\d \[\d\d\d\d-\d\d-\d\d\] \|\s*$")
        while not finished:
            time.sleep(15)
            p = subprocess.Popen([automatec_exe, '/viewschedreport',
                                  'Recent Activity'] + loginargs,
                                 stdout=subprocess.PIPE)
            retval = UnicodeDammit(p.communicate()[0]).unicode_markup
            # Check if it's completed...
            foundsofar = len(regex.findall(retval))
            foundsofar += len(regex2.findall(retval))
            if foundsofar >= 3:
                report += \
                    'Schedule report indicates all sessions are finished\n'
                report += retval + '\n'
                finished = True
            elif not started and retval.find('- Schedule Of Many') != -1:
                report += 'Found entry in schedule report\n'
                report += retval + '\n'
                started = True
            timeout = timeout - 1
            if timeout <= 0:
                report += retval + '\n'
                report += 'Schedule(s) did not complete after %d minutes\n' \
                          % timeoutmins
                report += 'Completed schedules found: %d\n' % foundsofar
                return (False, report)
    finally:

        # Unpublish the process
        if subprocess.call([automatec_exe, '/unpublish', donothingproc]
                           + loginargs) > 0:
            report += "Failed to unpublish process 'Do Nothing'\n"
            return (False, report)
        stopextraresourcepcs()

    return (True, report)

# Tests the 'Fail Fast' aspect of the scheduler by scheduling
# two processes - one which succeeds, one which fails.
# The expectation is that the successful process will complete
# fully despite the other session failing.
def test_bpschedulerfailfast(parms):

    resprocmap, result = startspecificresourcepcs([8182])
    if not result:
        for proc in list(resprocmap.values()):
            proc.kill()
        return (False, 'Failed to start resource PCs')
    procstopublish = ['Do Nothing', 'Doomed Process']
    try:
        # Publish the required processes:
        for procname in procstopublish:
            if subprocess.call([automatec_exe, '/publish', procname]
                               + loginargs) > 0:
                report = 'Failed to publish process\n'
                return (False, report)

        # Run the process that creates the task...
        (passed, report) = test_runprocess(['Setup Scheduler Fail Fast Test'])
        if not passed:
            return (False, report)

        # Now need to wait and see if the task (process "Do Nothing")
        # completes. It should get started in approximately 2 minutes!
        started = False
        finished = False
        timeoutmins = 5
        timeout = 60 * timeoutmins / 15  # <timeoutmins> minutes
        # regex for checking that the schedule is complete. Looking for something like this:
        #   -- Wed 18 May 2011 --
        #   - Mixed Success Schedule                   | 22:18:45 | 22:18:45 | 22:20:58 |
        #   - * Mixed Success Task                     | -        | 22:18:45 | 22:20:58 |
        #   - = 13060117-1aee-4a70-ab8c-5be6d37332a9   | -        | 22:18:50 | 22:20:57 |
        #   - = 26188b99-b9e1-4c6f-ad19-fd3eff5e6dcf   | -        | 22:18:50 | 22:20:57 | Failed for some reason
        #   etc...
        # (?m) indicates that ^ and $ should match on SOL/EOL and not (only) SOF/EOF
        regex = re.compile(r"(?m)^- Mixed Success Schedule .*\d\d:\d\d:\d\d \|\s*$")
        rxSessSuccess = re.compile(r"(?m)^- = [-a-f0-9]{36}   \| -        \| \d\d:\d\d:\d\d \| \d\d:\d\d:\d\d \|\s*$")
        rxSessFail = re.compile(r"(?m)^- = [-a-f0-9]{36}   \| -        \| \d\d:\d\d:\d\d \| \d\d:\d\d:\d\d \| (\S.*)$")

        while not finished:
            time.sleep(15)
            p = subprocess.Popen([automatec_exe, '/viewschedreport',
                                  'Recent Activity'] + loginargs,
                                 stdout=subprocess.PIPE)
            retval = UnicodeDammit(p.communicate()[0]).unicode_markup
            # Check if it's completed...
            m = regex.search(retval)
            if m:
                report += 'Schedule report indicates schedule is finished\n'
                report += retval + '\n'
                numsucceeded = len(rxSessSuccess.findall(retval, m.start()))
                numfailed = len(rxSessFail.findall(retval, m.start()))
                if (numsucceeded != numfailed):
                    report += "Found %d succeeded; %d failed - expected equal numbers\n" \
                              % (numsucceeded, numfailed)
                    return(False, report)
                finished = True
            elif not started and retval.find('- New Schedule') != -1:
                report += 'Found entry in schedule report\n'
                report += retval + '\n'
                started = True

            timeout = timeout - 1
            if timeout <= 0:
                report += retval + '\n'
                report += 'Schedule did not complete after %d minutes\n' \
                          % (timeoutmins, )
                return (False, report)
    finally:

        # Unpublish the process
        # Publish the 'Do Nothing' processes:
        for procname in procstopublish:
            if subprocess.call([automatec_exe, '/unpublish', procname]
                               + loginargs) > 0:
                report = "Failed to unpublish process '", procname, "'"
                return (False, report)
        for proc in list(resprocmap.values()):
            proc.kill()

    return (True, report)


def test_bpexportschedulelog(parms):

    #Start resource PC
    resprocmap, result = startspecificresourcepcs([8182])
    if not result:
        for proc in list(resprocmap.values()):
            proc.kill()
        return (False, 'Failed to start resource PCs')

    try:

        # Run the process that runs a schedule, and exports the schedule report...
        (passed, report) = test_runprocess(['Export Scheduler Logs Test'])
        if not passed:
            return (False, report)

    finally:

        #Stop resource PC
        for proc in list(resprocmap.values()):
            proc.kill()

    return (True, report)

def killapp(pname, proc):
    if re.search(r"pcscm", pname) is not None:
        import win32com.client
        mgr =  win32com.client.Dispatch("PCOMM.autECLConnMgr")
        lst =  win32com.client.Dispatch("PCOMM.autECLConnList")
        lst.Refresh()
        for n in range(0, lst.Count):
            sess = lst(n+1)
            print("...shut down PCOM session {0} with COM".format(sess.Name))
            mgr.StopConnection(sess.Handle)

    proc.kill()

def killrogueapps():
    """Kill any rogue apps still running after a test

    This checks only for specific applications - currently IE or PCOM.

    Some time is allowed for the applications to terminate of their own
    accord - the Process/Object may have told them to exit, but they
    haven't yet.

    :returns: A list of process names that were killed. If there are any, the
        test should be failed if it hasn't already, and True if killing was
        impossible and the tests should be aborted.
    """

    count = 4
    killed = []
    while True:
        found = []

        for proc in _psutil.process_iter():
            try:
                pname = proc.name().lower()
            except:
                pname = None
            if pname and (
                    re.search(r"iexplore", pname) is not None or
                    re.search(r"pcscm", pname) is not None
            ):
                if count == 0:
                    try:
                        killapp(pname, proc)
                        killed.append(pname)
                    except:
                        pass
                found.append(pname)

        if len(found) > 0:
            count -= 1
            if count < -4:
                return (killed, True)
            print("...waiting for rogue applications ({0}) to exit".format(','.join(killed)))
            time.sleep(10)
        else:
            return (killed, False)



# Wait until resource pc is free - see bug #3795. This is called after
# each test. If the resource PC is still not free after waiting an
# appropriate amount of time, False is returned to signify a problem,
# but the Resource PC is freed up so the next test can run.
#
# If it isn't possible to free up the resource pc, an Exception is raised.
# Subsequent tests cannot be run reliably, so the whole run should be
# aborted.
#
# Also checks that nothing is hogging the CPU, and causes the whole run to
# be aborted in that case too.
#
# Returns (freeok, report, unfixable) where freeok is True if the Resource
# PC was free of its own accord, or False if it required poking, and report
# is report text. If the Resource PC is still not free on return, 'unfixable'
# will be True, meaning there is no point running any further tests.
def waitforresourcepcfree():

    maxwait = 5
    report = ''
    time.sleep(5)
    while maxwait > -5:
        _, responsetext = ResourcePC_GET('/status')
        totalrunning = False
        counts = {'RUNNING': 0, 'PENDING': 0, 'IDLE': 0}
        regex = re.compile(r" - (\w*)( (\w*))?")
        for line in responsetext.splitlines():
            match = regex.split(line)
            if len(match) > 1:
                st = match[1]
                if st in counts:
                    counts[st] += 1
                else:
                    counts[st] = 1
                if maxwait < 0 and st in ['IDLE', 'PENDING', 'RUNNING']:
                    # Match group 3 - because group 2 has the leading space
                    roguesessionid = match[3]
                    if roguesessionid is not None:
                        if st == 'PENDING':
                            cmd = "delete"
                        else:
                            cmd = "stop"
                        report += 'Trying to ' + cmd + ' rogue session ' + roguesessionid + '\n'
                        _, replytext = ResourcePC_GET('/' + GetHTTPAuth() + '&' + cmd + '%20' + roguesessionid)
                        report += replytext
            elif re.search(r"Total running: ", line):
                totalrunning = True
        if not totalrunning:
            raise Exception("Unable to determine Resource PC status - response:\n" + responsetext)
        if counts['RUNNING'] + counts['PENDING'] + counts['IDLE'] == 0:
            if maxwait < 0:
                return (False, "Resource PC required intervention to free it", False)
            else:
                return (True, "Resource PC is free", False)
        maxwait -= 1
        time.sleep(10)

    report += 'Waiting for resource PC - busy reponse was\n:' \
              + responsetext + '\n'
    report += "Resource PC could not be freed up for next test\n"
    return (False, report, True)


# Publish a process, run it, and unpublish it afterwards.
# Returns (success, report, sessionid).
def publishandrun(proc):

    report = ''
    sessionid = None

    # Publish the process:
    p = subprocess.Popen([automatec_exe, '/publish', proc] + loginargs,
                         stdout=subprocess.PIPE)
    output = UnicodeDammit(p.communicate()[0]).unicode_markup
    if p.returncode != 0:
        report += 'Failed to publish process - {0}\n'.format(output)
        return (False, report, sessionid)

    p = subprocess.Popen([automatec_exe, '/run', proc] + loginargs,
                         stdout=subprocess.PIPE)
    retval = UnicodeDammit(p.communicate()[0]).unicode_markup
    report += retval + '\n'
    if not check_process_started(p, retval, proc):
        report += '(was trying to start process:' + proc + ')\n'
        return (False, report, sessionid)
    sessionid = get_session_id(retval)
    if sessionid is None:
        report += 'Unable to get session ID\n'
        return (False, report, sessionid)

    # We'll use a progressive delay in between checks to see if the process has
    # completed. On one hand, we don't want to over-use resources by repeatedly
    # checking, but on the other we don't want to waste time by not seeing that
    # a quick-running process has completed until 10 seconds later. Ideally the
    # product would supply a better method of doing this than polling by
    # launching an EXE file.
    delaytime = 1
    elapsed = 0

    while True:
        time.sleep(delaytime)
        elapsed += delaytime
        if delaytime < 10:
            delaytime += 1
        p = subprocess.Popen([automatec_exe, '/status', sessionid]
                             + loginargs, stdout=subprocess.PIPE)
        retval = UnicodeDammit(p.communicate()[0]).unicode_markup
        status = get_session_status(retval)
        if status is None:
            report += 'Failed to get session status\n' + retval
            return (False, report, sessionid)
        if status == 'Failed':
            report += 'Process failed\n'
            # Show log for failed processes
            p = subprocess.Popen([automatec_exe, '/getlog', sessionid]
                                 + loginargs, stdout=subprocess.PIPE)
            retval = UnicodeDammit(p.communicate()[0]).unicode_markup
            report += retval + '\n'
            retval = (False, report, sessionid)
            break
        if status == 'Completed':
            report += 'Process completed successfully\n'
            retval = (True, report, sessionid)
            break
        if status == 'Pending':
            report += "Session status should not be Pending, it said it was running!!\n"
            return (False, report, sessionid)

        if elapsed > 60*45:
            report += "Process was still running after 45 minutes\n"
            retval = (False, report, sessionid)
            break

    # Unpublish the process:
    if subprocess.call([automatec_exe, '/unpublish', proc] + loginargs) > 0:
        report += 'Failed to unpublish process\n'
        return (False, report, sessionid)

    return retval


# Get all output paramters from the given session.
# Returns (success, report, outputs). Currently outputs is in XML format.
# TODO: it would be nice if we unpacked the outputs into a dictionary here.
def getoutputs(sessionid):

    report = ''

    _, replytext = ResourcePC_GET('/' + GetHTTPAuth() + '&outputs%20'
                                  + sessionid)
    op = replytext.find('OUTPUTS:')
    if op == -1:
        return (False, 'Failed to get outputs from ' + replytext, None)
    op = replytext[op + 8:]
    return (True, report, op)


def test_runprocess(parms):

    report = ''
    proc = parms[0]

    result, rep, _ = publishandrun(proc)
    report += rep
    return (result, report)


def test_functions(parms):

    report = ''

    result, rep, sessionid = publishandrun('Functions')
    report += rep
    if not result:
        return (False, report)

    result, rep, outputs = getoutputs(sessionid)
    report += rep
    if not result:
        return (False, report)

    report += 'Output parameters were: ' + outputs
    if outputs.find(sessionid) != -1:
        return (True, report)
    else:
        return (False, report)


def ConfigureInternetExplorer():
    # Disable IE security feature that prevents javascript being run in locally-hosted web page
    print('Disabling Internet Explorer restrictions on locally hosted active content ...')
    try:
        Key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,
                              r"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_LOCALMACHINE_LOCKDOWN"
                              , 0, winreg.KEY_WRITE)
        winreg.SetValueEx(Key, 'iexplore.exe', 0, winreg.REG_DWORD, 0)
        print('OK')
    except:
        print('FAILED', sys.exc_info())
        return False

    # Disable 'did you notice information bar' prompt
    print('Disabling Internet Explorer Information Bar first-use prompt ...')
    try:
        Key = winreg.OpenKey(winreg.HKEY_CURRENT_USER,
                              'Software\\Microsoft\\Internet Explorer', 0,
                              winreg.KEY_ALL_ACCESS)
        SubKey = winreg.CreateKey(Key, 'InformationBar')
        winreg.SetValueEx(SubKey, 'FirstTime', 0, winreg.REG_DWORD, 0)
        print('OK')
    except:
        print('FAILED', sys.exc_info())
        return False

    print('IE successfully configured')
    return True


def ConfigureAttachmate():
    # Associating Attachmate Extra! session 'A' with desired session file\n
    # (see libcatatt test) ...
    print("Associating Attachmate Extra! session 'A' with desired session file\n (see libcatatt test) ...")
    try:
        Key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, 'Software', 0,
                              winreg.KEY_WRITE)
        Key = winreg.CreateKey(Key, 'Attachmate')
        Key = winreg.CreateKey(Key, 'EXTRA!')
        Key = winreg.CreateKey(Key, 'WorkStationUser')
        Key = winreg.CreateKey(Key, 'ConfiguredSessions')
        winreg.SetValueEx(Key, '2', 0, winreg.REG_SZ,
                           r"C:\BLUEPRISM\QA\PROCESSES\ATTACHMATE\SHEFFIELD LIBRARY.EDP,A,25,80,18696205,0,0"
                           )
        winreg.SetValueEx(Key, '3', 0, winreg.REG_SZ,
                           r"C:\BLUEPRISM\QA\CODE\ATTACHMATE EXTRA\GETCURSORPOS.EDP,B,25,80,18696205,0,0"
                           )
        print('OK')
        return True
    except:
        print('FAILED', sys.exc_info())
        return False


def ConfigureTeemtalk():
    # The license key is: 3-BG2NU-Z27SD-KU923-HHZ5J
    # However, we're not using this currently because the version we have doesn't have
    # the API we need to support!
    return True


def ConfigurePcom():
    # TODO: somehow set the session up!!
    return True


# Start the web server (1 of 2)
def StartCassini():
    docroot = os.path.join(qaroot, r"Code\HTML")
    if isnetworkpath(qaroot):
        qatemp = getqatemp()
        qatemp = os.path.join(qatemp, 'cassini-docs')
        if os.path.exists(qatemp):
            shutil.rmtree(qatemp)
        shutil.copytree(docroot, qatemp)
        docroot = qatemp
    return  Popen([qaroot + r"\Cassini\CassiniWebServer.exe", docroot,
                   '8089'])

# Start what is sometimes known as "the other web server"..
def StartCassini2():
    docroot = os.path.join(qaroot, r"Code\QA Sample WS Test\QA Sample WS Test")
    if isnetworkpath(qaroot):
        qatemp = getqatemp()
        qatemp = os.path.join(qatemp, 'cassini2-docs')
        if os.path.exists(qatemp):
            shutil.rmtree(qatemp)
        shutil.copytree(docroot, qatemp)
        docroot = qatemp
    return Popen([os.path.join(qaroot,
                               r"Code\WebServer\WebDev.WebServer.EXE"),
                  '/port:8888', '/path:' + docroot],
                 cwd=os.path.join(qaroot, r"Code\WebServer"))

def EnsureCassiniConfigured():
    print('Setting up web server...')
    gacutil = Popen([qaroot + r"\Cassini\gacutil.exe", '/l'],
                    stdout=subprocess.PIPE)
    retval = UnicodeDammit(gacutil.communicate()[0]).unicode_markup
    if retval.find('Cassini') == -1:
        print('Registering Cassini.dll in GAC')
        if subprocess.call([qaroot + r"\Cassini\gacutil.exe", '/i', qaroot
                                                                    + r"\Cassini\Cassini.dll"]) > 0:
            print('Failed to register cassini.dll in GAC')
            return False
        if subprocess.call([qaroot + r"\Cassini\gacutil.exe", '/i', qaroot
                                                                    + r"\Code\WebServer\WebDev.WebHost.dll"]) > 0:
            print('Failed to register cassini.dll in GAC')
            return False
    print('OK')
    return True


def RegisterFarpoint():
    print('Registering Farpoint control')
    if subprocess.call(['Regsvr32.exe', '/s',
                        r"C:\BluePrism\QA\Code\Farpoint\Ss32x25.ocx"]) != 0:
        print('Failed to register Farpoint control')
        return False
    print('OK')
    return True


def RegisterVB6Runtime():
    # On windows Vista we have to run the VB apps before enabling UAC. Otherwise they fail to run
    # afterwards with messages such as "Component 'comctl32.ocx' or one of its dependencies not correctly registered"
    # See bug 4073
    print('Setting up VB6 ...')
    if qacommon.has_uac():
        print('Launching VB Apps. They will be closed automatically in a few seconds ...')
        VB6AppList = [r"code\VB6 App\VB6AutomatedTestApp.exe",
                      r"code\VB6 App 2\VB6AutomatedTestApp.exe",
                      r"Code\VB6 App 3\VB6AutomatedTestApp.exe"]
        for App in VB6AppList:
            P = Popen([App])
            time.sleep(5)
            subprocess.Popen('taskkill /PID %i' % P.pid)
    print('Done setting up VB6')
    return True


def do_archiving_test():
    """Do the archiving test

    This is a special test that runs after all the others.
    """
    AppendReport(qacommon.sep_test())
    log = ""
    try:
        # Clear out our temporary archive directory, then archive all the logs generated by
        # the test runs, and finally restore them again.
        # Use unicode for the archive directories in order to ensure
        # that dates in alternate locales still work correctly
        archivedir = os.path.join(os.getcwd(), 'Archives')
        if os.path.exists(archivedir):
            shutil.rmtree(archivedir)
        os.mkdir(archivedir)
        if subprocess.call([automatec_exe, '/setarchivepath', archivedir]
                           + loginargs):
            raise Exception('Failed to set archive path')

        def _get_session_counts():
            reportfile = 'archive_report.txt'
            p = subprocess.Popen([automatec_exe, '/report', reportfile] + loginargs,
                                 stdout=subprocess.PIPE)
            output = UnicodeDammit(p.communicate()[0]).unicode_markup
            if p.wait() != 0:
                raise Exception('Failed to generate report : ' + output)
            with open(reportfile, 'r') as f:
                report = f.read()
            sessions = None
            logs = None
            session_count_regex = re.compile(r"Session Count:(\d*)")
            session_log_regex = re.compile("Session Log Count:(\d*)")
            for line in report.splitlines():
                session_count_match = session_count_regex.findall(line)
                session_log_match = session_log_regex.findall(line)
                if len(session_count_match[0] > 0):
                    sessions = int(session_count_match)
                elif len(session_log_match[0] > 0):
                    logs = int(session_log_match[0])
            if sessions is None or logs is None:
                raise Exception('Failed to read session/logs count')
            return (sessions, logs)

        sessions_before, logs_before = _get_session_counts()
        log += ("Before archiving, {0} sessions, {1} logs\n"
                .format(sessions_before, logs_before))

        p = subprocess.Popen([automatec_exe, '/archive'] + loginargs,
                             stdout=subprocess.PIPE)
        output = UnicodeDammit(p.communicate()[0]).unicode_markup
        if p.wait() != 0:
            raise Exception('Failed to archive logs : ' + output)

        def _count_archives():
            return len([os.path.join(dp, f) for dp, dn, fn in os.walk(archivedir) for f in fn])

        numsessions = _count_archives()
        log += "Archived logs from {0} sessions\n".format(numsessions)

        sessions_after, logs_after = _get_session_counts()
        log += ("After archiving, {0} sessions, {1} logs\n"
                .format(sessions_after, logs_after))

        p = subprocess.Popen([automatec_exe, '/restorearchive'] + loginargs,
                             stdout=subprocess.PIPE)
        output = UnicodeDammit(p.communicate()[0]).unicode_markup
        if p.wait() != 0:
            raise Exception('Failed to restore logs : ' + output)

        numsessions = _count_archives()
        if numsessions == 0:
            log += ("0 archive sessions left on disk after restore, as expected\n")
        else:
            raise Exception('Log restore problem - {0} sessions still left on disk'.format(numsessions))

        sessions_restored, logs_restored = _get_session_counts()
        log += ("After restore, {0} sessions, {1} logs\n"
                .format(sessions_restored, logs_restored))

        if sessions_restored != sessions_before:
            raise Exception('Archive restore problem - {0} sessions before should match {1} sessions restored'
                            .format(sessions_before,sessions_restored))
        if logs_restored != logs_before:
            raise Exception('Archive restore problem - {0} logs before should match {1} logs restored'
                            .format(logs_before,logs_restored))

        passed = True

    except Exception as e:
        log += "{0}\n".format(e)
        passed = False

    AppendReport('Test: archiving')
    AppendReport('Result: ' + (('PASS' if passed else 'FAIL')))
    AppendReport('Duration: {0}'.format('?'))
    AppendReport(log + '\n')
    return passed


# Various globals that are available to test handlers etc, that are populated
# during startup to define various characteristcs of the environment.
automate_exe = None
automatec_exe = None
uiscript_exe = None
loginargs = None
loginargs_nonlocale = None
loginup = None
using_sso = None
bp_features = None

respc = None

procreport = u""
def AppendReport(s):
    global procreport
    try:
        procreport += s + "\n"
    except:
        s = "<<<SOME INVALID DATA>>>\n"
        procreport += s
    print(s)


def main():
    global automate_exe, automatec_exe, uiscript_exe, loginargs, loginargs_nonlocale, using_sso
    global bp_features, loginup
    global procreport
    global respc
    global _psutil

    # Parse command line...
    parser = OptionParser()
    parser.add_option(
        '--usedbcon', action='store_true', default=False,
        help='Use the existing already configured database connection and '
             'database. This overrides all the other database connection '
             'and setup options, and is used when Server.py/Client.py have '
             'already configured everything.'
    )
    parser.add_option(
        '--usesso', action='store_true', default=False,
        help='Use with --usedbcon if single sign-on (AD) is to be used.'
    )
    parser.add_option(
        '-u', '--dbuser', default='sa',
        help='Set database user (default sa)'
    )
    parser.add_option(
        '-p', '--dbpassword', default='password',
        help='Set database password'
    )
    parser.add_option(
        '-w', '--dbwinauth', action='store_true', default=False,
        help='Use Windows Authentication for database connection'
    )
    parser.add_option(
        '-s', '--server', default='localhost\\SQLExpress',
        help="Set server to use. In the form host:port, it sets a Blue Prism Server, and if the host is 'localhost' a local BP Server will be configured and started. Just host refers to a SQL server."
    )
    parser.add_option(
        '--nosso', action='store_true', default=False,
        help="Use to force SSO off. Only necessary when using a remote BP server, "
             "which normally makes runtests.py assume SSO is in use."
    )
    parser.add_option(
        '-b', '--dbname', default='testdb001',
        help='Set database name (default testdb001)')
    parser.add_option(
        '-i', '--inst', dest='automate_inst', action='store_true', default=False,
        help='Run against installed version of Automate (c:\\Program Files\\Blue Prism\\Blue Prism Automate) - otherwise dev version'
    )
    parser.add_option(
        '--debugger', action='store_true', default=False,
        help="Run the main Resource PC via the Visual Studio debugger"
    )
    parser.add_option(
        '--automate_projpath', default=None,
        help="Specify path to automate project. If not using --inst, it's assumed to live at ../../automate unless you use this option to say different."
    )
    parser.add_option(
        '-o', '--importonly', action='store_true', default=False,
        help="Import processes and set up the database only - do not actually "
             "run anything. This is useful for setting up a development "
             "environment, and is also used during automated QA as part of a "
             "two-phase setup, where a run with --importonly (on only one VM) "
             "is followed by runs on one or more VMs using --existingvb. "
             "Thus, the combination of --importonly followed by --existingdb "
             "in separate runs should be the equivalent of a single run with "
             "neither specified."
    )
    parser.add_option(
        '-e', '--existingdb', action='store_true', default=False,
        help="Use existing database and processes. Connection options must "
             "still be specified. The database should be in a valid state "
             "for running the requested tests - for example, by having "
             "previously run with --importonly."
    )
    parser.add_option(
        '--prepversions', default=None,
        help="Normally used in conjunction with --importonly in a two-phase "
             "run, this allows the Blue Prism versions to prepare for to be "
             "specified. Normally, the installed Blue Prism verseion is "
             "detected, and according to that tests can be excluded, and "
             "therefore setup needed only for those tests is skipped. But if "
             "the intention is to later run additional Blue Prism versions "
             "that will result it the setup lacking some things. This option "
             "allows that to be rememdied by providing advance knowledge of "
             "which Blue Prism versions we intend to test against."
    )
    parser.add_option(
        '--testrun', default=1,
        help="When doing multiple runs of the tests against the same database "
             "(which could only happen with --existingdb) this should be used "
             "to specify which run (1-n) the current one is."
    )
    parser.add_option(
        '-q', '--quickonly', action='store_true', default=False,
        help='Quick (and able to run in background) tests only.'
    )
    parser.add_option(
        '-t', '--test', default=None,
        help='Specify specific tests to run (instead of running all), using a comma-separated list of test IDs.'
    )
    parser.add_option(
        '-l', '--loop', action='store_true', default=False,
        help='Keep running tests in a loop until forcibly terminated.'
    )
    parser.add_option(
        '-f', '--force', action='store_true', default=False,
        help='Force tests to run even if system configuration/versioning would normally skip them. Good for local testing.'
    )
    parser.add_option(
        '--until-fail', dest='untilfail', action='store_true', default=False,
        help='Use (with or without --loop) to stop at the first test failure'
    )
    parser.add_option(
        '-N', '--nhs', action='store_true', default=False,
        help='Use an NHS Edition license instead of a normal one'
    )
    parser.add_option(
        '-C', '--configure', action='store_true', default=False,
        help='Runs setup/configuration. This should be called before running any tests for the first time. Must be used alone.'
    )
    parser.add_option(
        '-a', '--appmanlog', action='store_true', default=False,
        help='Run with application manager logging enabled.'
    )
    parser.add_option(
        '--wslog', action='store_true', default=False,
        help='Force web service logging on Resource PC'
    )
    parser.add_option(
        '', '--webservers', action='store_true', default=False,
        help='Just fire up the web servers and exit'
    )
    parser.add_option(
        '-I', '--insecure', action='store_true', default=False,
        help='When using a Blue Prism Server connection, do so in insecure mode'
    )
    parser.add_option(
        '--outputhost', default=None,
        help="Set a host to send output to via TCP, instead of stdout. The "
             "--outputport option must also be specified."
    )
    parser.add_option(
        '--outputport', type='int', default=None,
        help='Set the port to use with --outputhost.'
    )
    parser.add_option(
        '--bpusername', default='admin',
        help="Set the username to use for an existing Blue Prism environment."
    )
    parser.add_option(
        '--bppassword', default='admin',
        help="Set the password to use for an existing Blue Prism environment."
    )
    parser.add_option(
        '--testrail', action='store_true', default=False,
        help='Flag to indicate a testrail run, in which case tests are read from the roles file.'
    )
    (options, args) = parser.parse_args()

    qacommon.redirect_output(options.outputhost, options.outputport)

    loginup = [options.bpusername, options.bppassword]
    # Determine the login arguments that will be required extensively later for
    # all command-line operations.
    if options.usesso:
        loginargs = ['/sso']
    else:
        loginargs = ['/user'] + loginup

    # Hack this onto the back of loginargs to forcibly translate everything back to English.
    loginargs_nonlocale = loginargs.copy()
    loginargs.extend(['/locale', 'en-GB'])

    # Get paths of Automate stuff, according to environment and options...
    if options.automate_inst:
        # Paths for using an installed version...
        automate_projpath = None
        automate_binpath = r"C:\Program Files\Blue Prism Limited\Blue Prism Automate"
        vbo_path = os.path.join(automate_binpath, 'VBO')
    else:
        # Paths for using a development version...
        automate_projpath = options.automate_projpath or os.path.abspath(os.path.join(qaroot, r"..\..\automate"))
        automate_binpath = os.path.join(automate_projpath, "bin")
        vbo_path = os.path.join(automate_projpath, "VBO")
    automate_exe = os.path.join(automate_binpath, 'Automate.exe')
    automatec_exe = os.path.join(automate_binpath, 'AutomateC.exe')
    uiscript_exe = os.path.join(automate_binpath, 'UIScript.exe')
    bpserver_service_exe = os.path.join(automate_binpath, 'BPServerService.exe')

    if not (os.path.exists(automate_exe) and os.path.exists(automatec_exe)
            and os.path.exists(vbo_path)):
        print('One or more required files/directories do not exist. Please check:')
        print('  ' + automate_exe)
        print('  ' + automatec_exe)
        print('  ' + vbo_path)
        return 1

    if options.importonly and options.existingdb:
        print("The --importonly and --existingdb options cannot be used together.")
        return 1

    if options.webservers:
        print('Starting the two test web servers.')
        print('You will need to shut these down manually, especially if you want to ')
        print(' run the tests afterwards.')
        webserver = StartCassini()
        otherwebserver = StartCassini2()
        return 0

    if options.configure:
        ConfigureInternetExplorer()
        # Note, not checking the error code on the above. It never has, and it fails
        # on Windows 2000 for whatever reason.
        if not ConfigureAttachmate():
            return 1
        if not ConfigureTeemtalk():
            return 1
        if not ConfigurePcom():
            return 1
        if not EnsureCassiniConfigured():
            return 1
        if not RegisterVB6Runtime():
            return 1
        if not RegisterFarpoint():
            return 1
        # On a platform with UAC, pre-run the BP server to create event sources.
        if qacommon.has_uac():
            if not qacommon.manage_service('Blue Prism Server', 'start') or not startpoolresourcepcs(preseed=True) or not startextraresourcepcs(preseed=True):
                return 1
            # This seemed to hang without cleanup, and the box wouldn't reboot.
            stoppoolresourcepcs()
            stopextraresourcepcs()
        print('Configuration complete')
        return 0

    # Make sure we're running from the directory where the script is
    # because we use relative paths for everything. (This is a double check,
    # since we've already forced it right at the top!)
    if not os.path.exists('testdefs.json'):
        print('You need to run runtests.py from the correct directory. Expected to find testdefs.json.')
        return 1

    try:
        import psutil
        _psutil = psutil
    except:
        print('Need psutil, but it seems to not be installed')
        return 1

    # Load test definitions
    with open('testdefs.json', 'r') as f:
        testdefs = json.load(f)

    # Load roles file, if needed
    if options.testrail:
        with open(r"C:\Client.roles", 'r') as f:
            roles = json.load(f)
        options.test = ','.join(roles['tests']['tests'])

    requestedtests = qacommon.get_requested_tests(testdefs, options.test, options.quickonly)

    # Get the version of Automate we're running...
    # We end up with 'versiontext' containing, e.g. "5.0.6.24" or "4.2.52.0"
    # (note - always 4 parts)
    p = subprocess.Popen([automatec_exe, '/help'], stdout=subprocess.PIPE)
    automatechelp = UnicodeDammit(p.communicate()[0]).unicode_markup
    versiontext = UnicodeDammit(automatechelp).unicode_markup
    versiontextmatch = re.search(r"(\d+\.\d+\.\d+(?:\.\d+)?)", versiontext).groups(1)[0]
    bpversion = semantic_version.Version.coerce(versiontextmatch)

    print(u"Testing version {version}".format(version=str(bpversion)))

    # Do any detection of product features here. The intention is to make this
    # script gracefully operate with whatever version (within reason) of Blue
    # Prism it is given.
    bp_features = {}
    bp_features["independent_ws_publishing"] = '/publishws' in versiontext
    bp_features["wcf"] = '/connectionmode' in versiontext
    bp_features["set_command_timeout"] = bpversion > semantic_version.Version('4.2.2')

    print(u"Detected BP features:")
    print(bp_features)


    # For Windows 7, bump up the SQL command timeout because it just seems
    # to be *really* slow under some circumstances... (bug #6343)
    if qacommon.is_win7() and bp_features["set_command_timeout"]:
        print("Increasing SQL command timeout")
        if subprocess.call([automatec_exe, '/setcommandtimeout', '600']):
            print('Failed to set SQL command timeout')
            return 1


    using_bpserver = False
    using_sso = False
    using_localbpserver = False
    using_remotebpserver = False
    bpserverport = '9999'
    # dbserver is used for an env-var for the SQL Server VBO tests - ensure that it's defined for later
    dbserver = ''

    if options.usedbcon:
        using_sso = options.usesso
    else:
        print('Setting up database connection...')
        index = options.server.find(':')
        if index == -1:
            using_bpserver = False
            using_sso = False
        else:
            using_bpserver = True
            bpserverport = options.server[index + 1:]
            bpserverhost = options.server[:index]
            if bpserverhost == 'localhost':
                using_sso = False
                using_localbpserver = True
            else:
                if options.nosso:
                    using_sso = False
                else:
                    using_sso = True
                using_remotebpserver = True

        if not using_remotebpserver:
            # Hardwired SQL Server when using local bpserver, because we've re-used the parameter to
            # specify both the BP Server and the SQL Server!! This is not good!
            if using_localbpserver:
                dbserver = 'localhost\\SQLExpress'
            else:
                dbserver = options.server
            if options.dbwinauth:
                conopts = ['/setdbname', options.dbname, '/setdbserver', dbserver]
            else:
                conopts = [
                    '/setdbname',
                    options.dbname,
                    '/setdbserver',
                    dbserver,
                    '/setdbusername',
                    options.dbuser,
                    '/setdbpassword',
                    options.dbpassword,
                ]
            if using_localbpserver:
                conopts += ['/dbconname', 'server']
            if subprocess.call([automate_exe] + conopts) > 0:
                print('Failed to set up database connection')
                return 1

    ieversion = qacommon.get_explorer_version()

    # Determine what tests we will actually run...
    tests_to_run = []
    tests_to_prep = []
    for test in requestedtests:
        exclude_reason = qacommon.check_exclude_test(test,
                                                     using_sso, options.nhs, bpversion, options.usedbcon,
                                                     options.testrun, ieversion)
        if exclude_reason:
            print(('Test ' + test['id'] + ' is ' + exclude_reason))
            if options.force:
                dothistest = True
                print("...but doing it anyway because you said so")
            else:
                dothistest = False
        else:
            dothistest = True
        prepthistest = dothistest
        if not prepthistest and options.prepversions is not None:
            for pv in options.prepversions.split('>'):
                if (qacommon.check_exclude_test(test,
                                                using_sso, options.nhs, pv, options.usedbcon,
                                                options.testrun, ieversion) is None):
                    print(("..but prepping anyway, for {0}".format(pv)))
                    prepthistest = True
                    break
        if dothistest:
            tests_to_run.append(test)
        if prepthistest:
            tests_to_prep.append(test)

    # See if a ResourcePC is required for the tests we're running
    respcrequired = False
    for test in tests_to_prep:
        if test['respc']:
            respcrequired = True
            break

    # If we're running the development version, we need to make sure that the Windows
    # Service from the development build is registered.
    if respcrequired and not options.automate_inst:
        print('Registering development version of Windows Service')
        subprocess.call(['sc', 'delete', 'Blue Prism Server'])
        subprocess.call(['sc', 'stop', 'Blue Prism Server'])
        servicepath = os.path.join(automate_binpath, 'BPServerService.exe')
        # Nasty hack to allow running the service from H: (which is a network
        # drive - just copy it (and the rest of the files, for dependencies!)
        # to C:\QATemp
        if isnetworkpath(servicepath):
            qatemp = getqatemp()
            qatemp = os.path.join(qatemp, 'bpserver')
            if not os.path.exists(qatemp):
                os.mkdir(qatemp)
            for f in glob.glob(os.path.join(automate_binpath, '*.exe')):
                shutil.copy(f, qatemp)
            for f in glob.glob(os.path.join(automate_binpath, '*.dll')):
                shutil.copy(f, qatemp)
            servicepath = os.path.join(qatemp, 'BPServerService.exe')
        print('...now registering ' + servicepath)
        subprocess.call(['sc', 'create', 'Blue Prism Server', 'binPath=',
                         servicepath])
        print('Finished registering the service')

    if not options.usedbcon and respcrequired and not (using_remotebpserver or options.existingdb):
        if options.bpusername != 'admin' or options.bppassword != 'admin':
            print("Using a custom username/password can't work here")
            return 1
        print('Ensure SQL Server is running...')
        qacommon.manage_service('SQL Server (SQLExpress)', 'start')
        print('Creating database...')
        if subprocess.call([automatec_exe, '/createdb', options.dbpassword]):
            print('Failed to create database')
            sys.exit(1)
        print('Replacing database...')
        p = subprocess.Popen([automatec_exe, '/replacedb', options.dbpassword],
                             stdout=subprocess.PIPE)
        output = UnicodeDammit(p.communicate()[0]).unicode_markup
        if p.wait() != 0:
            print('Failed to replace database')
            print(output)
            return 1
        print('Creating database again...')
        if subprocess.call([automatec_exe, '/createdb', options.dbpassword]):
            print('Failed to create database again')
            return 1

        print('Unexpiring Automate admin user...')
        if subprocess.call([automatec_exe, '/unexpire'] + loginargs) > 0:
            print('Failed to unexpire. Trying again...')
            if subprocess.call([automatec_exe, '/unexpire'] + loginargs) > 0:
                print('Failed to unexpire, giving up.')
                return 1

        print('Setting license details...')
        if options.nhs:
            licensefile = 'BluePrismER.lic'
        else:
            licensefile = 'BluePrismQA.lic'
        if not os.path.exists(licensefile) and automate_projpath:
            licensefile = os.path.join(automate_projpath, 'qa', licensefile)
        print('...license file at ' + os.path.abspath(licensefile))
        with open(licensefile, 'r') as f:
            licensedata = f.read()
        if licensedata.startswith('@'):
            _, licensee, key = licensedata.split('@')
            p = subprocess.Popen([automatec_exe,"/license",
                                  licensee, key] + loginargs, stdout=subprocess.PIPE)
        else:
            p = subprocess.Popen([automatec_exe, '/license', licensefile]
                                 + loginargs, stdout=subprocess.PIPE)
        output = UnicodeDammit(p.communicate()[0]).unicode_markup
        if p.wait() != 0:
            print('Failed to set license')
            print(output)
            return 1

         # print('Toggling enforce permissions off')
         # args = [automatec_exe, "/enforcecontrollinguserpermissions", "false"]
         # args.extend(loginargs_nonlocale)
         # p = subprocess.Popen(args, stdout=subprocess.PIPE)
         # output = UnicodeDammit(p.communicate()[0]).unicode_markup
         # print(output)

    if not options.usedbcon and using_bpserver:
        args = ['/setbpserver', bpserverhost, bpserverport]
        if options.insecure:
            args += ['/bpserversecure', 'False']
        if subprocess.call([automate_exe] + args) > 0:
            print('Failed to set up BP Server connection')
            return 1

    # Start the server service if required. (Specifically not required if we're using a
    # Blue Prism Server as our connection, and not for the NHS edition). Note that we
    # need it even if we're not actually using a Blue Prism Server Connection, to run
    # schedules.
    if not options.usedbcon and not using_remotebpserver and not options.nhs and (using_localbpserver
                                                                                  or not options.importonly) and respcrequired:
        print('Starting Blue Prism Server service')
        if using_localbpserver:
            dbconname = 'server'
        else:
            dbconname = options.dbname
        if subprocess.call([automatec_exe, '/serverconfig', 'Default', dbconname,
                            bpserverport]):
            print('Failed to configure server settings')
            return 1
        returncode, output = qacommon.manage_service('Blue Prism Server', 'start')
        print(output)
        if returncode > 0:
            # Return code being nonzero means that something went wrong, so return False.
            print('Failed to start server service')
            return 1
        serverservicestarted = True
    else:
        serverservicestarted = False

    if respcrequired and not options.existingdb:
        # Set an environment variable for the root directory of the QA stuff...
        def setbpenv(name, datatype, value, desc):
            p = subprocess.Popen([automatec_exe,
                                  '/setev', name, datatype, value, desc] + loginargs,
                                 stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
            output = UnicodeDammit(p.communicate()[0]).unicode_markup
            if p.returncode != 0:
                print("Failed to set environment variable: {0}".format(output))
                raise Exception("Failed to set environment variable: " + output)
        setbpenv('QA Root', 'text', qaroot, 'Root directory of QA files')
        setbpenv('BP Version', 'text', str(bpversion), 'Blue Prism version being tested')
        setbpenv('Automate EXE', 'text', automate_exe, 'Path to Automate.exe')
        setbpenv('AutomateC EXE', 'text', automatec_exe, 'Path to AutomateC.exe')
        setbpenv('BPServerService EXE', 'text', bpserver_service_exe, 'Path to BPServerService.exe')
        setbpenv('Login Args', 'text', ' '.join(loginargs).lstrip('/'), 'Login arguments for automate without leading slash/c')
        if not options.usedbcon:
            # These can only be set if we know the database details.
            setbpenv('DB Name', 'text', options.dbname, 'The name of the database connected to')
            setbpenv('DB Server', 'text', dbserver, 'The database server')
            if options.dbwinauth:
                sqluser = ''
                sqlpassword = ''
            else:
                sqluser = options.dbuser
                sqlpassword = options.dbpassword
            setbpenv('DB User', 'text', sqluser, 'The database username')
            setbpenv('DB Password', 'text', sqlpassword, 'The database password')

    # Register required COM business objects in the database
    # (not on Windows 7, because CommonAutomation is not installed)
    if respcrequired and not qacommon.is_ge_win7() and not options.existingdb:
        print('Registering required business objects...')
        if subprocess.call([automatec_exe, '/regobject',
                            'CommonAutomation.clsExcel']):
            print('Failed to register CommonAutomation.clsExcel business object')
            return 1
        if subprocess.call([automatec_exe, '/regobject', 'CommonAutomation.clsWord'
                            ]):
            print('Failed to register CommonAutomation.clsWord business object')
            return 1
        if subprocess.call([automatec_exe, '/regobject',
                            'CommonAutomation.clsTestForeground']):
            print('Failed to register CommonAutomation.clsTestForeground business object')
            return 1
        if subprocess.call([automatec_exe, '/regobject',
                            'CommonAutomation.clsTestExclusive']):
            print('Failed to register CommonAutomation.clsTestExclusive business object')
            return 1

    # Set up the web servers and what have you required by the tests
    webserver = None
    otherwebserver = None
    traceplayer = None
    if not (options.importonly or options.quickonly):
        # Set of tests which require the cassini web server
        testsrequiringwebserver = set(test['id'] for test in tests_to_run if 'needscassini' in test)

        # Set of tests which require the 'other Cassini' - this is the ASP.NET
        # development web server, and is used to run a set of .NET-based Web
        # Services, as defined in Code\QA Sample WS Test
        testsrequiringotherwebserver = set(test['id'] for test in tests_to_run if 'needsothercassini' in test)
        # Set of tests which require the traceplayer
        testsrequiringtraceplayer = set(test['id'] for test in tests_to_run if 'needstraceplayer' in test)

        # Extract the requested test ids into a list
        requestedtestids = [test['id'] for test in tests_to_prep]

        # First find out which web servers we need for these tests
        # Then test that set against the tests requiring each type of webserver
        needcassini = len(testsrequiringwebserver.intersection(requestedtestids)) > 0
        needothercassini = len(testsrequiringotherwebserver.intersection(requestedtestids)) > 0

        if needcassini:
            if not EnsureCassiniConfigured():
                sys.exit(1)
            print('Starting web server...')
            webserver = StartCassini()
            # What we should really do is wait until we can see Cassini is listening. And
            # we could be getting other stuff set up and check that before actually starting,
            # rather than doing it here....
            time.sleep(10)

        # Launch 'the other Cassini'
        if needothercassini:
            if not EnsureCassiniConfigured():
                sys.exit(1)
            print('Starting other web server...')
            otherwebserver = StartCassini2()
            # What we should really do is wait until we can see Cassini is listening. And
            # we could be getting other stuff set up and check that before actually starting,
            # rather than doing it here....
            time.sleep(10)

        # Launch the traceplayer (but only if it's absolutely necessary, because it's very irritating
        # Also, something should get rid of this again when the script completes.
        needtraceplayer = \
            len(testsrequiringtraceplayer.intersection(requestedtestids)) > 0
        if needtraceplayer:
            print('Starting traceplayer...')
            traceplayer = Popen([r"cmd.exe", '/c', r"start /min " + qaroot
                                 + r"\processes\v3demo\run_1stmet.bat"])
            print('Started traceplayer')

    # Create application manager configuration file.
    if not options.importonly:
        createappmanconfigfile(options.appmanlog)

    # Import fonts.
    if respcrequired and not options.existingdb:
        print('Importing fonts...')
        args = [automatec_exe, '/fontimport', os.path.join(qaroot,
                                                           'Code/FontRec/DBFonts/*.xml')] + loginargs
        p = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        output = UnicodeDammit(p.communicate()[0]).unicode_markup
        if p.wait() != 0:
            print(('Failed import fonts : ' + output))
            return 1

    # (Don't) run the font path checking app.
    if False and respcrequired and not options.importonly:
        print('Checking font path...')
        shutil.copy(os.path.join(qaroot,'Code/FontPathChecker/bin/Release/FontPathChecker.exe'),automate_binpath)
        p = Popen([os.path.join(automate_binpath,'FontPathChecker.exe')],stdout=subprocess.PIPE)
        print((UnicodeDammit(p.communicate()[0]).unicode_markup))

    # Start the resource pc if required.
    respc = None
    if respcrequired and not options.importonly:
        print('Starting resource PC...')
        args = [automate_exe, '/resourcepc', '/public'] + loginargs_nonlocale
        if options.debugger:
            args = ["vsjitdebugger.exe"] + args
        respc = Popen(args, cwd=automate_binpath)
        if options.wslog:
            print("Enabling web service logging")
            if subprocess.call([automatec_exe, '/wslog', 'on'] + loginargs) != 0:
                print("...failed")
                return 1
    else:
        # We don't wait here for the Resource PC to be ready, since we may be going to do
        # unrelated time-consuming stuff next, but we will wait later before we actually
        # start using it.
        respc = None

    requiredimports = []
    for test in tests_to_prep:
        for f in test['processes']:
            if not f in requiredimports:
                requiredimports.append(f)

    if not options.existingdb:
        print('Importing processes ...')
        for filename in requiredimports:

            index = filename.find('>')
            if index == -1:
                procfile = filename
                forceid = None
            else:
                procfile = filename[:index]
                forceid = filename[index + 1:]

            optional = False
            if procfile.startswith('?'):
                optional = True
                procfile = procfile[1:]
            if procfile.startswith('$VBO$'):
                procfile = procfile.replace('$VBO$', vbo_path)
            elif procfile.startswith('$AUTOMATE$'):
                pfold = procfile
                if not automate_projpath:
                    automateprocs = 'Processes\\_automate'
                else:
                    automateprocs = os.path.join(automate_projpath, 'qa', 'Processes')
                procfile = procfile.replace('$AUTOMATE$', automateprocs)
                if not os.path.exists(procfile):
                    procfile = pfold.replace('$AUTOMATE$', 'Processes')
                    if not os.path.exists(procfile):
                        if optional:
                            procfile = None
                        else:
                            print('Process "{0}" does not exist in either qacontrol or automate.'.format(procfile))
                            return 1
            else:
                procfile = 'Processes\\' + procfile
            if procfile:
                print('     ... ' + procfile)
                extension = os.path.splitext(procfile)[1]
                importcmd = "/importrelease" if (extension == ".bprelease" or extension == ".bpskill") else "/import"
                args = [automatec_exe, importcmd, procfile] + loginargs
                if forceid:
                    args.extend(['/forceid', forceid])
                p = subprocess.Popen(args, stdout=subprocess.PIPE)
                print(UnicodeDammit(p.communicate()[0]).unicode_markup)
                if p.wait() > 0:
                    print('Failed to import process')
                    return 1

    if options.importonly:
        # Runtest.py expects to see this message...
        print('Import complete')
        return 0

    # Wait for the Resource PC to be ready...
    if respcrequired:
        while True:
            try:
                status, _ = ResourcePC_GET('/' + GetHTTPAuth() + '&status')
                if status == 200:
                    break
            except Exception as e:
                time.sleep(0.25)

    numpassed = 0
    numamber  = 0
    numfailed = 0

    tests_complete = False
    testcount = len(tests_to_run)
    testnumber = 0
    try:
        print('Running tests...')

        # Load global event log mask
        with open('eventlog.json', 'r') as f:
            global_eventlog = json.load(f)

        while True:

            for test in tests_to_run:
                testnumber += 1
                amber = False
                percentcomplete = int((float(testnumber) / float(testcount)) * 100)
                AppendReport(qacommon.sep_test())
                AppendReport('Test: ' + test['id'])
                AppendReport("({0} of {1}, {2}% complete)".format(testnumber, testcount, percentcomplete))
                startTime = datetime.datetime.utcnow()

                # Run the test...
                try:
                    fn = getattr(sys.modules[__name__], test['function'])
                    (passed, report) = fn(test.get('params', []))
                except Exception:
                    passed = False
                    report = "Failed due to exception - %s\n" % (traceback.format_exc())

                if not passed:
                    # That test failed, quickly grab a screenshot.
                    print('SCREENSHOT PLEASE')
                # The resource PC should be free after each test is finished.
                # The following will wait/check, and if it has to intervene to
                # free it, it will tell us so we can fail the test. If it can't
                # free it up, it will throw an Exception so we abort the whole
                # test run.
                # The only circumstance we don't check this in is when *none* of
                # the tests we're running need a resource PC, in which case there
                # won't be one.
                unfixable = False
                if respcrequired:
                    freeok, freerep, unfixable = waitforresourcepcfree()
                    if not freeok:
                        report += freerep
                        if passed:
                            passed = False
                            report += "Failed because processes were left running\n"

                killed, abort = killrogueapps()
                if len(killed) > 0:
                    if passed:
                        passed = False
                        report += "Failed because rogue applications were left running: {0}\n".format(','.join(killed))
                if abort:
                    unfixable = True
                    report += "Unable to continue because a rogue app is still running\n"

                endTime = datetime.datetime.utcnow()

                # Check the event log for things that shouldn't be there.
                # We only do this for 4.2.32 onwards, because prior to that
                # nearly everything gets failed due to bogus messages.
                if qacommon.comparebpversions(str(bpversion), "4.2.32") >= 0:
                    if not win32evtlog:
                        raise Exception("Missing pywin32 - this is required, please install it")
                    evh = win32evtlog.OpenEventLog("", "Blue Prism")
                    flags = win32evtlog.EVENTLOG_SEQUENTIAL_READ|win32evtlog.EVENTLOG_FORWARDS_READ
                    expected = test.get('eventlog', [])
                    # Append the global ignore mask to each test
                    expected.extend(global_eventlog)
                    for ex in expected:
                        ex['count'] = 0
                    unexpected = []
                    keepReadingEventLog = True
                    while keepReadingEventLog:
                        events = win32evtlog.ReadEventLog(evh, flags,0)
                        if not events:
                            break
                        for event in events:
                            if event.EventType == 1:
                                evtype = 'ERROR'
                            elif event.EventType == 2:
                                evtype = 'WARNING'
                            elif event.EventType == 4:
                                evtype = 'INFO'
                            elif event.EventType == 8:
                                evtype = 'SUCCESS'
                            elif event.EventType == 16:
                                evtype = 'FAILURE'
                            else:
                                evtype = 'UNKNOWN'
                            if evtype != 'INFO':
                                eventTime = datetime.datetime.utcfromtimestamp(event.TimeGenerated.timestamp())
                                if eventTime > startTime:
                                    try:
                                        msg = "{0}: {1}\n".format(evtype, ('//'.join(event.StringInserts)))
                                    except UnicodeError as e:
                                        msg = "Unicode error parsing event log, {0}".format(str(e))
                                        break
                                    found = False
                                    for ex in expected:
                                        if re.search(ex['match'], msg):
                                            ex['count'] += 1
                                            found = True
                                    if not found:
                                        if len(unexpected) > 0:
                                            unexpected.append("additional unexpected event logs which will not be reported.")
                                            keepReadingEventLog = False
                                            break
                                        unexpected.append(msg)

                    for ex in expected:
                        if ex['count'] < ex['min'] or ex['count'] > ex['max']:
                            if passed:
                                intro = 'Marked amber because'
                            else:
                                intro = 'Also, '
                            report += "{4} '{0}' found in event log {1} times (expected between {2} and {3})\n".format(
                                ex['match'], ex['count'], ex['min'], ex['max'], intro)
                            amber = True
                    for uex in unexpected:
                        if passed:
                            intro = 'Marked amber due to '
                        else:
                            intro = 'Also found '
                        report += "{1} unexpected event log {0}\n".format(uex, intro)
                        amber = True

                if passed:
                    if amber:
                        numamber += 1
                        narrative = "AMBER"
                    else:
                        numpassed += 1
                        narrative = "GREEN"
                else:
                    numfailed += 1
                    narrative = "RED"

                AppendReport('Result: {0}'.format(narrative))

                # Just endTime - startTime, but we want to lose the microseconds.
                # We could use timedelta.total_seconds but some QA setups have
                # python 2.6 which doesn't support it
                td = endTime - startTime
                total_seconds = (td.microseconds + (td.seconds + td.days * 24 * 3600) * 10**6) / 10**6
                duration = int(total_seconds)

                AppendReport('Started: {0}'.format(startTime.strftime("%H:%M:%SZ")))
                AppendReport('Duration: {0}'.format(duration))
                AppendReport('TestRail: {test_id} {test_result} {test_duration}'.format(test_id=test['id'], test_result=narrative, test_duration=duration))
                AppendReport(report + '\n')

                if unfixable:
                    raise Exception("Aborting tests due to unfixable problem")

            if options.untilfail and numfailed > 0:
                break
            # Unless we're in loop mode, we've finished running the tests...
            if not options.loop:
                break

        # End marker for log parse in Runtest.py
        print((qacommon.sep_end()))

        tests_complete = True

    except Exception:

        # End marker for log parse in Runtest.py
        print((qacommon.sep_end()))

        AppendReport('*************EXCEPTION************************')
        AppendReport('Details:%s' % traceback.format_exc())

    #
    # Generate output
    #
    summary = 'Summary: {red} {amber} {green}'.format(red=numfailed, amber=numamber, green=numfailed)
    headers = 'PC:{0}\n'.format(gethostname())
    headers += 'Date:{0}'.format(time.strftime("%Y-%m-%d %H:%M:%SZ", time.gmtime()))
    headers += 'Red:{0}\n'.format(numfailed)
    headers += 'Amber:{0}\n'.format(numamber)
    headers += 'Green:{0}\n'.format(numpassed)
    headers += 'IE Version:{0}\n'.format(ieversion)
    headers += 'Automate Version: {0}\n'.format(bpversion)
    procreport += summary + '\n\n' + headers + '\n\n' + procreport

    print('Updating report...')
    logfile = 'test.log'
    try:
        f = open(logfile, 'w', encoding='UTF-8')
        f.write(procreport)
        f.close()
    except Exception as e:
        print('Failed to write log - {0}'.format(str(e)))

    if tests_complete:
        print(headers + '\n')
        print(summary + '\n')
        print('Tests Complete. See the file ' + logfile + ' for a summary.')
    else:
        print("Tests Aborted.")

    # Kill the resource PC
    if respc:
        respc.kill()

    # Stop the server service if we started it
    if serverservicestarted:
        qacommon.manage_service('Blue Prism Server', 'stop')

    # Kill the web servers
    if webserver:
        webserver.kill()
    if otherwebserver:
        otherwebserver.kill()

    if traceplayer:
        # Cannot Kill the traceplayer because we get an access denied exception
        # traceplayer.kill()
        traceplayer = None

    # Runtest.py expects to see this message...
    print('Runtests.py finished')

    return 0


if __name__ == '__main__':
    try:
        retval = main()
    except Exception:
        print("Failed in runtests.py: " + traceback.format_exc())
        retval = 1
    sys.exit(retval)
