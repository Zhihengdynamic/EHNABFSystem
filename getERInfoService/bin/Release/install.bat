REM The following directory is for .Net 4
SET DNET=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
SET PATH=%PATH%;%DNET%

ECHO Installing getERInfoService
ECHO ------------------------------------
InstallUtil /i .\getERInfoService.exe
ECHO ------------------------------------
Echo Done.
PAUSE
