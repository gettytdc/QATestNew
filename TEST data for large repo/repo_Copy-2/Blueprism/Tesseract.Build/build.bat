@echo off

git submodule update --init --recursive
cp -r -f ..\Tesseract_v4\tessdata ..\bin\Tesseract
cp -f sw.cpp ..\Tesseract_v4

pushd ..\Tesseract_v4
sw -platform x86 -static build

cp -f .sw/*/*tesseract-4.0.0.exe ..\bin\Tesseract\tesseract-4.0.0.exe
popd
