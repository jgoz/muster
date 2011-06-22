@echo off

for /F %%M in ('where ILMerge') do set ILMERGE_EXE=%%M
if errorlevel 1 (goto ilmergenotfound) else (goto start)

:ilmergenotfound
echo 'ILMerge.exe' is not available on the system path
goto done

:start
set FRAMEWORK_PATH=C:/WINDOWS/Microsoft.NET/Framework/v4.0.30319
set PATH=%PATH%;%FRAMEWORK_PATH%;

:target_config
set TARGET_CONFIG=Release
IF x==%1x goto framework_version
set TARGET_CONFIG=%1

:framework_version
set FRAMEWORK_VERSION=v4.0
set ILMERGE_VERSION=v4,%FRAMEWORK_PATH%
if x==%2x goto build
set FRAMEWORK_VERSION=%2
set ILMERGE_VERSION=%3

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

echo.
echo === MERGING ===
echo Merging Primary Assembly
set FILES_TO_MERGE=
set FILES_TO_MERGE=%FILES_TO_MERGE% "src\Muster.Runner\bin\%TARGET_CONFIG%\musterin.exe"
set FILES_TO_MERGE=%FILES_TO_MERGE% "src\Muster\bin\%TARGET_CONFIG%\Muster.dll"
%ILMERGE_EXE% /keyfile:src\Muster\Muster.snk /targetplatform:%ILMERGE_VERSION% /out:output\bin\musterin.exe %FILES_TO_MERGE%

echo Building Standalone Assembly
msbuild /nologo /verbosity:quiet src\Muster\Muster.csproj /p:Configuration=%TARGET_CONFIG% /p:TargetFrameworkVersion=%FRAMEWORK_VERSION% /p:OutputPath=..\..\output\lib

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