@echo off
cd %CD:BluePrism.Build=%
powershell "& '.\BluePrism.Build\build - api.ps1'"