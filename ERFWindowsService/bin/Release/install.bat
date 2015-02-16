REM The following directory is for .Net 4
SET DNET=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
SET PATH=%PATH%;%DNET%

ECHO Installing ERFWindowsService.exe
ECHO ------------------------------------
InstallUtil /i .\ERFWindowsService.exe
ECHO ------------------------------------
Echo Done.
PAUSE
