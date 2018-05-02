@echo off
powershell -noprofile -executionPolicy RemoteSigned -file "%~dp0\Scripts\Package.ps1" -build -skipBuildExtras %*
