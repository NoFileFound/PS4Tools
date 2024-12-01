@echo off
setlocal

set /p file1="File A: "
set /p file2="File B: "

if not exist "%file1%" (
    echo File A does not exist.
    exit /b
)

if not exist "%file2%" (
    echo File B does not exist.
    exit /b
)

for /f "tokens=*" %%A in ('powershell -command "Get-FileHash -Path '%file1%' -Algorithm SHA256 | Select-Object -ExpandProperty Hash"') do set file1hash=%%A
echo %file1% - %file1hash%
for /f "tokens=*" %%B in ('powershell -command "Get-FileHash -Path '%file2%' -Algorithm SHA256 | Select-Object -ExpandProperty Hash"') do set file2hash=%%B
echo %file2% - %file2hash%

endlocal
pause