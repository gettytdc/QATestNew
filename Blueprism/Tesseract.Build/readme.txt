Updating to a new version of tesseract.
=======================================

If you haven't already run: submodule update --init --recursive

* Change to the Tesseract_v4 directory and pull the latest changes, then checkout the tag of the required release.
* Modify Tesseract.Build/sw.cpp set the version to the correct version. (In the addProject function)
* Modify Tesseract.Build/build.bat set the correct version to copy on the last line.
* Commit the script changes and the submodule commit-ish.

Building tesseract.
===================

* Install software network (https://software-network.org) and add it to your PATH.
* Change to the Tesseract.Build directory
* run build.bat
* Download the latest eng.traineddata from https://github.com/tesseract-ocr/tesseract/wiki/Data-Files and place in bin/Tesseract/tessdata/eng.traineddata
* Add the binary data changes under bin/Tesseract using git add -f
* Commit the binary data changes.


