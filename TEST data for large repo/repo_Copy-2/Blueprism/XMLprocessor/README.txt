
XMLprocessor and LocaleTools have a chicken and egg type relationship.

XMLprocessor creates *_gsw-LI.resx files to embed in LocaleTools DLL, but loads and uses it as part of the processing.

The script 'UpdateAll.bat' is called to invoke the processor to update the .resx files.

How to add content to a new prefix or supplement an existing one:
-----------------------------------------------------------------

 Create a txt file with the new strings, for example here a new stored procedure exception in sperrors.txt

 Then from within the XMLProcessor folder run the command:

 bin/Release/XMLprocessor.exe textfile sperrors.txt error tile

 This uses the command "textfile" to tell XMLprocessor to read a text file (sperrors.txt) 
 and insert missing entries in to the "tile" resource file with the prefix of "error"

 In this example this would create new entries in tile_en-US.resx and pseudo localized in the other locale resource files.

 These would then typically be referenced from vb like:

 LTools.Get(errorMessage, "tile", clsOptions.CurrentLocale, "error")

 The above example is just showing how to add data for the "tile" resource file with the "error" prefix. 
 The same approach works for all the other resource files and prefixes as required.

 This is only needed where the strings cannot automatically be extracted from the database
 (as we do with parameter names etc)
