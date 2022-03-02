@echo off
echo Starting conductor
start /d"c:\conduct\bin" \conduct\bin\wscman.exe phoenix.prw
echo Press a key when conductor is ready!!!
pause
..\bin\uiscript <conductortest.txt
echo Now shut down conductor if you want to run this test again!!

