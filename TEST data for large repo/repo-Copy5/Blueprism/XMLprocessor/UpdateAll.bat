@echo off
rem This script is called to run the XML Processor to extract/reconcile the
rem .resx files in the XmlProcessor folder with the text resources in the
rem Blue Prism database and xml files.
rem 
rem The .resx files produced by the XML Processor are compiled into the LocaleTools
rem resource assembly.
rem 
rem By default the XML Processor tool sources the name of the Blue Prism database 
rem from the product config file C:\ProgramData\Blue Prism Limited\Automate V3\Automate.config,
rem but this can be overridden by the App.Config entry "DatabaseName".

set XMLPROCESS_FOLDER=%~dp0

rem echo "XMLPROCESS_FOLDER" %XMLPROCESS_FOLDER%

cd %XMLPROCESS_FOLDER%
start /b /w bin\Debug\XmlProcessor.exe all
