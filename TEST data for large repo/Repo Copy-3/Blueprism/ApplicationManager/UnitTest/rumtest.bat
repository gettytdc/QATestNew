@echo off
echo Running Rumba test
set op=%path%
set path="c:\Program Files\WallData\SYSTEM";%path%;
..\bin\uiscript <rumtest.txt
set path=%op%

