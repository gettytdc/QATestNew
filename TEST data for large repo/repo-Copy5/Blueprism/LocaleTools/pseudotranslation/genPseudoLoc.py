#!/usr/bin/env python
# -*- coding: utf-8 -*-

# i18n helper script to scan a resx file and generate pseudo language items

# designed for python 3.x, tested on Win10

#foreach EN resx in automate: #folder in glob.glob('../automate') + glob.glob("src"):
#	newresx = createPsuedoResx(resx) #add resx xml prefix, delete existing pseudo first!
#	If (resx is "frm*.resx", "ctl*.resx" && ~ "frm*.Resources.resx") || 
#		folder is C:\Dev\automate\CharMatching\UI { #FontScannerSearchParamsForm.resx
#		textFieldOnly = true
#	}
#	Foreach dataElement in resx {
#		if !textFieldOnly (textFieldOnly &&datavalue = "*.Text" {){
#			newdataElement = pseudoize(dataElement)
#			write(newresx, newdataElement)
#		}
#	}
#}

import os
from imp import reload
import re
import json
import sys
import io
import glob
#import xmltodict
import jxmlease
import regex
import copy
import html
import random


reload(sys)

#locales = ['de-DE', 'fr-FR', 'ja-JP']
#locales = ['de-DE', 'ja-JP']
#locales = ['ja-JP']
locales = ['gsw-LI']
#locales = ['en-US']
#locales = ['de-DE']
rootdir = 'automate'
allowed_file_types = ['.resx']
excluded_paths = ['/dist/', '/node_modules/', 'Compilation']

# pseudioize resx data value
# FR: a-â, e-é, i-î, o-ô, u-ù, c-ç, A-Â, E-È, I-Î, O-Ô, U-Ù
# DE: a-ä, o-ö, u-ü, s-ß, A-Ä, O-Ö, U-Ü
prefix = dict([('de-DE', ('DE_', '_DE')), ('fr-FR', ('FR_', '_FR')), ('gsw-LI', ('ˁ', 'ˀ')), ('ja-JP', ('始', '終'))])
mapDE = dict([('a', 'ä'), ('o', 'ö'), ('u', 'ü'), ('s', 'ß'), ('A', 'Ä'), ('O', 'Ö'), ('U', 'Ü')])
mapFR = dict(
    [('a', 'â'), ('e', 'é'), ('i', 'î'), ('o', 'ô'), ('u', 'ù'), ('c', 'ç'), ('A', 'Â'), ('E', 'È'), ('I', 'Î'),
     ('O', 'Ô'), ('U', 'Ù')])
mapXX = dict(
    [('b', 'ḃ'),('c', 'ḉ'),('d', 'ḍ'),('f', 'ḟ'),('g', 'ḡ'),('i', 'ḭ'),('k', 'ḵ'),('m', 'ṁ'),('s', 'ṡ'),('v', 'ṽ'),('w', 'ẁ'),('x', 'ẋ'),('y', 'ẏ'),('z', 'ẑ')])
    # Todo excluding: ,('d', 'ḍ'),('y', 'ẏ') bzw. Date-Strings ("yy|mm|dd") (ẏẏ|ṁṁ|ḍḍ)
charMap = dict([('de-DE', mapDE), ('fr-FR', mapFR), ('gsw-LI', mapXX)])

def pseudoize(value, locale):
    print('pseudoize value:' + value)
    try:
        data = value
        if(not locale in prefix):
            #print('Skip psueodize')
            return data;

        if(excludeFromTranslations(data)):
            print('excludeFromTranslations')
            return '';
        if (len(html.unescape(data)) < 2 ):
            print('short string: ' + value)
            return data
        elif (dontPseudoize(data)):
            print('dontPseudoize')
            data = value
        elif (hasToStringParameters(data)):
            print('hasToStringParameters')
            data = pseudioizeToString(data, locale)
        elif (isFileFilter(data)):
            print('isFileFilter')
            return pseudioizeFileFilter(data, locale) # file filters can't have prefix/suffix
        else:	
            print('else - pseudoReplaceSubstring')
            data = pseudoReplaceSubstring(data, locale, 0, len(data))
        paddingArrayDE =  dict([(5, 3), (11, 2), (21, 0.60), (40, 0.40), (100, 0.20)])
        paddingArrayFR = dict([(5, 2.5), (11, 0.80), (21, 0.45), (40, 0.30), (100, 0.15)])
        paddingMap = dict([('de-DE', paddingArrayDE), ('fr-FR', paddingArrayFR)])
        padding = ''
        if (locale in paddingMap):
            prefixLen = len(prefix[locale][0]) + len(prefix[locale][1])
            for k, v in paddingMap[locale].items():
                #print('k, v: ' + str(k) + ', ' + str(v))
                if (len(data) < k or (len(data) > k and k == 100) and len(data) > 2):
                    print(data)
                    #print(data[:(round(len(data) * v) - prefixLen)])
                    #padding = data[:(round(len(data) * v) - prefixLen)]
                    #print('Ä' * (round(len(data) * v) - prefixLen))
                    padding = 'Ä' * (round(len(data) * v) - prefixLen)
                    break
        randomPart = ''
        #randomPart = random.choice('aAäÄbB')
        return randomPart + prefix[locale][0] + data + padding + prefix[locale][1]
    except:
        print('pseudoize ERROR!!')

#\.(AccessibleDescription|AccessibleName|BrowseWindowTitle|Caption|CustomFormat|GuidanceText|HeaderText|HintText|Text|Title|ToolTip|ToolTipText|Items[0-9]*)"
def isText(text):
    #print('text:' + text)
    if (text.endswith(('.AccessibleDescription', '.AccessibleName', '.BrowseWindowTitle', '.Caption', '.CustomFormat', '.GuidanceText', '.HeaderText', '.HintText', '.Text', '.Title', '.ToolTip', '.ToolTipText'))):
        return True
	# check if it ends with .Items, .Items1, ...
    pattern = '\.Items[0-9]{0,3}$'
    result = re.search(pattern, text)
    if (result):
        return True
    if ('.' in text):
        return False

    return True

	# replace substring
def pseudoReplaceSubstring(datax, locale, start, to):
    data = datax
    data1 = data[0:start]
    data2 = data[start:to]
    data3 = data [to:len(data)]
    if (locale in charMap):
        for k, v in charMap[locale].items():
            data2 = re.sub(k, v, data2)
    return data1 + data2 + data3

    # don't Pseudoize Strings that are toString parameters.   
def pseudioizeToString(datax, locale):
    data = datax
    inBraces = False
    afterColon = False
    start = 0
    for pos in range(0, len(data)):
        if (data[pos] == '{'):
            inBraces = True
        if (data[pos] == ':') and inBraces and (not afterColon):
            data = pseudoReplaceSubstring(data, locale, start, pos)
            afterColon = True
        if (data[pos] == '}'):
            inBraces = False
            if afterColon:
                start = pos
                afterColon = False
    data = pseudoReplaceSubstring(data, locale, start, len(data))
    return data

# Only pseudioize the descriptive parts, not the actual filter pattern.    
def pseudioizeFileFilter(datax, locale):
    data = datax
    inBraces = False
    inFilter = False
    start = 0
    for pos in range(0, len(data)):
        if ((data[pos] == '|') and (pos + 1 <= len(data)) and (data[pos+1] == '*')):
            data = pseudoReplaceSubstring(data, locale, start, pos)
            inFilter = True
        elif (data[pos] == '|'):
            if inFilter:
                start = pos
                inFilter = False
    return data
    
# checks if it has the pattern "...{...:g}" or similar.
def hasToStringParameters(data):
    pattern = '{[^}]*:[^}]*}'
    result = re.search(pattern, data)
    if (result):
        return True
    return False
    
def dontPseudoize(data):
    pattern = '{Action}'
    result = re.search(pattern, data)
    if (result):
        return True
    return False

def excludeFromTranslations(data):
    pattern = '{ORDINAL'
    result = re.search(pattern, data)
    if (result):
        return True
    return False
    
# checks if it has the pattern "|*" or similar.
def isFileFilter(data):
    pattern = '\|\*'
    result = re.search(pattern, data)
    if (result):
        return True
    return False

# check if supported type and contains markers that mean it should be processed further
def shouldFileBeProcessedAsResx(final_path):
    for excluded_path in excluded_paths:
        result = re.search(excluded_path, final_path)
        if (result):
            return False

    file_extension = os.path.splitext(final_path)[1]
    if (file_extension in allowed_file_types):
        pattern = '\.([a-z]{2,3}-[A-Z]{2})\.resx$' #ignore lang specific resx, eg, Resources.ja-JP.resx, clsAutoCheckingTreeView.ja-JP.resx
        result = re.search(pattern, final_path)
        if (result):
            print('shouldFileBeProcessed ***SKIP*** lang resx: ' + result.group(1))
            return False
        else:
            data = getXmlFromFile(final_path)#readInFile(final_path)
            return len(data) > 0
    else:
        return False


# recursively search every file in every likely location r'../automate'
def searchAllFilesForResx(dir=r'C:\Dev\automate', command='scan'):
    print('dir=' + dir)
    print('command=' + command)
    for folder in glob.glob(dir):
        #if (os.path.isdir(folder)):
        if (folder != r'../SFE-Packages' and os.path.isdir(folder)):
            print("Searching in: ", folder)
            for root, dirs, files in os.walk(folder):
                for file in files:
                    final_path = root + '\\' + file
                    print('file: ' + final_path)
                    if (not os.path.exists(final_path)):
                        print("Cannot find: ", final_path)
                    else:
                        if shouldFileBeProcessedAsResx(final_path):
                            data = getXmlFromFile(final_path)
                            #print(data)
                            xmlDataElements = []
                            try:
                                for dataelement in data['root']['data']:
                                    #print('dataelement: ' + dataelement)
                                    #print('name:' + dataelement.get_xml_attr('name'))
                                    #print('value:' + dataelement['value'])
                                    #<data name="ntfyDebug.Text" xml:space="preserve">
                                    #  <value>デバッグ</value>
                                    #</data>
                                    try:
                                        dataelement.get_xml_attr('xml:space')
                                    except:
                                        try:
                                            print('no attribute:')
                                            print('value: ' + dataelement['value'])
                                            continue
                                        except:
                                            #print('***SKIP*** Node is not xml:space attr')
                                            continue
                                    if (dataelement.get_xml_attr('xml:space') and dataelement.get_xml_attr('xml:space') == 'preserve'):
                                        name = dataelement.get_xml_attr('name')
                                        #print('name: ' + name)
                                        if (isText(name)):
                                            xmlDataElements.append(dataelement)
                                            print('name:' + name)
                                            print('value:' + dataelement['value'])
                                            #print(jxmlease.emit_xml(dataelement))
                                        else:
                                            #print('***SKIP*** name: ' + name)
                                            pass

                                #print('xmlDataElements: ')
                                #print(xmlDataElements)
                                createPseudoResx(final_path, xmlDataElements, command)#create even if NO data items
                            except:
                                print('XML format error - ***MISSING*** data element!!')

# load a file
def readInFile(filename, encoding = 'utf8'):
    if os.path.exists(filename):
        with io.open(filename, 'r', encoding=encoding) as f:
            data = f.read()
            return data
    return None

# load an xml file
def getXmlFromFile(filename, encoding = 'utf8'):
    try:
        if os.path.exists(filename):
            with io.open(filename, 'r', encoding=encoding) as f:
                return jxmlease.parse(f.read())
        return None
    except:
        print('Failed to read Xml from file ' + filename)

# delete file
def deleteFile(outputFile):
    print('Delete: ' + outputFile)
    try:
        os.remove(outputFile)
    except:
        print('Failed to delete file ' + outputFile)

# format Data element
def isCJK(value):
    print('isCJK')
    try:
        #matchObj = re.findall(u'[\u4E00-\u9FFF\u4E00-\u9FFF\uF900-\uFAFF]+', value)
        matchObj = regex.findall(r'[\p{IsHiragana}\p{IsKatakana}\p{IsHan}]+', value)
        if (len(matchObj) > 0):
            print('Element contains CJK: ' + matchObj)
        return len(matchObj) > 0
    except:
        print('Could not process string (probably contains invalid utf-8): ' + value)
        return False

# format Data element
def pseudoizeExistingElement(existingElement, dataElement, locale):
    existingValue = existingElement['value']
    newValue = dataElement['value']
    prefixLen = len(prefix[locale][0])
    print('existingValue: ' + existingValue)
    print('newValue: ' + newValue)
    pseudoPrefix = existingValue[:prefixLen]
    pseudoSuffix = existingValue[-prefixLen:]
    #print('existingValue[:prefixLen] ' + existingValue[:prefixLen])
    #print('prefix[locale][0]: ' + prefix[locale][0])
    #print('existingValue[-prefixLen:] ' + existingValue[-prefixLen:])
    #print('prefix[locale][0]: ' + prefix[locale][1])
    if (existingValue[:prefixLen] != prefix[locale][0] and existingValue[-prefixLen:] != prefix[locale][1]):#not yet pseudoized!
        print('locale: ' + locale)
        if ((locale != 'ja-JP') or (locale == 'ja-JP' and isCJK(existingValue) > 0)):
            print('existingValue: ' + existingValue)
            pseudoValue = pseudoize(existingValue, locale)
            print('pseudoizeExistingElement: ' + pseudoValue)
            existingElement['value'] = pseudoValue
            print('pseudoizeExistingElement: ' + existingElement['value'])
    return existingElement


# format Data element
def formatDataElement(xmlDataElements, existingXml, locale):
    print('xmlDataElements: ')
    print(xmlDataElements)

    xmlPseudoDataElements = []
    for dataElement in xmlDataElements:#xmlDataElements
        dataElement = dataElement.get_current_node()
        print('dataElement node: ')
        print(dataElement)
        elementExists = False
        name = dataElement.get_xml_attr('name')
        if (existingXml != None):#locale == 'ja-JP' and
            print('name: ' + name)
            for existingElement in existingXml['root']['data']:
                existingName = existingElement.get_xml_attr('name')
                print('Existing Name: ' + existingName)
                if (name == existingName and (name.startswith('DateTimeFormat_') == False)):
                    elementExists = True
                    #jxmlease.XMLDictNode.delete_xml_attr('name')
                    print('dataElement found in Existing XML ' + name)
                    newElement = pseudoizeExistingElement(existingElement, dataElement, locale)
                    if (newElement != existingElement):
                        print('Save newElement' + newElement)
                        existingXml['root']['data'] = newElement
                    break
        if (elementExists == False):
            print('Create new pseudoized element')
            datavalue = dataElement['value']
            print('**TEXT*** datavalue: ' + datavalue)
            
            if (name.startswith('DateTimeFormat_')):
                pseudoValue = datavalue
            else:
                pseudoValue = pseudoize(datavalue, locale)
                
            dataElement['value'] = pseudoValue
            if (len(pseudoValue) > 0):
                xmlPseudoDataElements.append(dataElement)
            print(jxmlease.emit_xml(dataElement))
            print("**TEXT*** dataElement['value']: " + dataElement['value'])

    if (xmlPseudoDataElements != None and len(xmlPseudoDataElements) > 0):
        print('len(xmlPseudoDataElements): ' + str(len(xmlPseudoDataElements)))
        data = jxmlease.emit_xml(xmlPseudoDataElements)
        print('XML newDataElements emit_xml: ')
        print(data)
        data = re.sub(r'^<\?xml version="1\.0" encoding="utf-8"\?>', '', data, flags=re.MULTILINE)
        data = data.strip('\n')
        data = re.sub(r'^(\s*)<data', r'\1  <data', data, flags=re.MULTILINE)#increase indent from by 2 spaces
        data = re.sub(r'^(\s*)</data', r'\1  </data', data, flags=re.MULTILINE)
        #data = re.sub(r'^  (\s*)<value>', r'\1<value>', data, flags=re.MULTILINE)#reduce indent from 4 to 2 spaces
        print('XML newDataElements emit_xml Add Indentation: ')
        print(data)
        return data

    print('formatDataElement return')
    return None

# write out xml to resx
def createPseudoResx(final_path, enXmlDataElements, command):
    for locale in locales:
        xmlDataElements = copy.deepcopy(enXmlDataElements)
        print('xmlDataElements: ')
        print(xmlDataElements)
        newresx = readInFile(r'C:\Dev\resxTemplate.resx')
        #jxmlresx = readInFile(r'C:\Dev\jxmleaseTemplate.resx')
        outputFile = final_path[:-4] + locale + '.resx'
        print('New resx: ' + outputFile)
        print('len(xmlDataElements): ' + str(len(xmlDataElements)))

        if (len(xmlDataElements) > 0):
            try:
                if (command == 'update'):
                    #print('Update!!')
                    existingXml = getXmlFromFile(outputFile)
                    newDataElements = formatDataElement(xmlDataElements, existingXml, locale)
                    #print('***WRITE*** file: ' + outputFile)
                    if (existingXml != None):
                        #print('existingXml:')
                        #print(existingXml)
                        emitXml = jxmlease.emit_xml(existingXml)
                        #print('XML existingXml emit_xml: ')
                        #print(emitXml)
                        emitXml = re.sub(r'^</root>', '', emitXml, flags=re.MULTILINE)
                        emitXml = emitXml.strip('\n')
                        emitXml = re.sub(r'^  (\s*)<', r'\1<', emitXml, flags=re.MULTILINE)#reduce indent from 4 to 2 spaces
                        emitXml = re.sub(r'^  (\s*)<value>', r'\1<value>', emitXml, flags=re.MULTILINE)#reduce indent from 4 to 2 spaces
                        #remove jxmlparse prefix which is different from the original xml
                        splitLines = emitXml.splitlines(True)
                        del splitLines[0:60]
                        outputFile = outputFile
                        print('***WRITE*** file: ' + outputFile)
                        #with io.open(outputFile, 'w', encoding='utf-8-sig') as f:
                        with io.open(outputFile, 'w', encoding='utf8') as f:
                            f.write(newresx)
                            f.write("\n")
                            f.write("".join(splitLines))
                    else:
                        #with io.open(outputFile, 'w', encoding='utf-8-sig') as f:
                        with io.open(outputFile, 'w', encoding='utf8') as f:
                            f.write(newresx)
                    #with io.open(outputFile, 'a', encoding='utf-8-sig') as f:
                    with io.open(outputFile, 'a', encoding='utf8') as f:
                        if (newDataElements != None):
                            f.write("\n")
                            f.write(newDataElements)
                        f.write("\n</root>")
                elif (command == 'delete' and locale != 'ja-JP'):
                    deleteFile(outputFile)
            except:
                print('createPseudoResx ERROR!!')

        else:
            print('No Data Elements!  Create default resx with prefix only.')
            if (command == 'update'):
                #if ((locale != 'ja-JP') or (locale == 'ja-JP' and not os.path.exists(final_path))):
                    print('***WRITE*** resx prefix to file: ' + outputFile)
                    #with io.open(outputFile, 'w', encoding='utf-8-sig') as f:
                    with io.open(outputFile, 'w', encoding='utf8') as f:
                        f.write(newresx)
                        f.write("\n</root>")
            elif (command == 'delete' and locale != 'ja-JP'):
                deleteFile(outputFile)

# usage when args missing...
command = "scan"
userFilename = r"C:\Dev\automate\AutomateUI\My Project"

if (len(sys.argv) == 2):
    print('argv1: ' + sys.argv[1] )
    command = "scan"
elif (len(sys.argv) == 3):
    print('argv1: ' + sys.argv[1] )
    print('argv2: ' + sys.argv[2] )
    command = sys.argv[2]

if (len(sys.argv) < 2):
    print("Usage: " + sys.argv[0] + " scanall [update|delete]")
    print("Usage: " + sys.argv[0] + " <folder> [update|delete]")
    #print("Usage: " + sys.argv[0] + " sym-TST")

elif sys.argv[1] == "sym-TST":
    #makeTestLocale()
    pass

elif sys.argv[1] == "scanall":
    print('perform scanall with ' + command)
    #searchAllFiles(fix=len(sys.argv) == 3 and sys.argv[2] == "update")
    searchAllFilesForResx(userFilename, command) #'C:\Dev\automate'
elif sys.argv[1] == "deleteall":
    print('perform deleteall with delete')
    searchAllFilesForResx(userFilename, 'delete')
else:
    userFilename = sys.argv[1]
    searchAllFilesForResx(userFilename, command)