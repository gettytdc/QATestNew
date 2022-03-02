@echo off
%cd%\lib\nuget.exe restore %cd%\..\LoginAgent\LoginAgent.sln
%cd%\lib\nuget.exe restore %cd%\..\BluePrism.sln