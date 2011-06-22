@echo off

set FRAMEWORK_PATH=C:/WINDOWS/Microsoft.NET/Framework/v4.0.30319
set PATH=%PATH%;%FRAMEWORK_PATH%;

:target_config
set TARGET_CONFIG=Release
IF x==%1x goto framework_version
set TARGET_CONFIG=%1

:framework_version
set FRAMEWORK_VERSION=v4.0
if x==%2x goto build
set FRAMEWORK_VERSION=%2

:build
set publish=publish-%FRAMEWORK_VERSION%
if exist output ( rmdir /s /q output )
if exist %publish% ( rmdir /s /q %publish% )

mkdir output
mkdir "output\bin"
mkdir "output\lib"

echo === COMPILING ===
echo Compiling / Target: %FRAMEWORK_VERSION% / Config: %TARGET_CONFIG%
msbuild /nologo /verbosity:quiet src\Muster.sln /p:Configuration=%TARGET_CONFIG% /t:Clean
msbuild /nologo /verbosity:quiet src\Muster.sln /p:Configuration=%TARGET_CONFIG% /p:TargetFrameworkVersion=%FRAMEWORK_VERSION%

xcopy /Q /Y src\Muster\bin\%TARGET_CONFIG%\*.* output\lib

echo .xml >> exclude.txt
xcopy /Q /Y /EXCLUDE:exclude.txt src\Muster\bin\%TARGET_CONFIG%\*.* output\bin

echo .vshost.exe >> exclude.txt
echo .manifest >> exclude.txt
echo .config >> exclude.txt
xcopy /Q /Y /EXCLUDE:exclude.txt src\Muster.Runner\bin\%TARGET_CONFIG%\*.* output\bin

del exclude.txt

echo.
echo === FINALIZING ===
move output %publish%

echo.
echo === CLEANUP ===
echo Cleaning Build
msbuild /nologo /verbosity:quiet src/Muster.sln /p:Configuration=%TARGET_CONFIG% /t:Clean

echo.
echo === DONE ===

:done
if errorlevel 1 pause else exit