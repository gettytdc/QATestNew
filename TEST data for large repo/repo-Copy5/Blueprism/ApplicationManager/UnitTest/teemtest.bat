@echo off
echo Running Teemtalk test
set op=%path%
set path=c:\teemtalk\api;%path%;
..\bin\uiscript <teemtest.txt
set path=%op%

