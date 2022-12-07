@echo off
set PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319;%PATH%
csc %~dp0\*.cs
pause
