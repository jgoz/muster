@echo off

for /F %%M in ('where 7za') do set ZIP_EXE=%%M
where /Q 7za
if errorlevel 1 (goto zipnotfound) else (goto start)

:zipnotfound
echo '7za.exe' is not available on the system path
goto done

:start
set NUGET_EXE=bin\nuget-bin\nuget.exe
set NUGET_BOOTSTRAPPER_EXE=bin\nuget-bin\nuget-bootstrap.exe

if exist %NUGET_EXE% goto package

%NUGET_BOOTSTRAPPER_EXE%
move %NUGET_BOOTSTRAPPER_EXE% %NUGET_EXE%
move %NUGET_BOOTSTRAPPER_EXE%.old %NUGET_BOOTSTRAPPER_EXE%

:package
set /p VERSION=Enter version (e.g. 1.0): 
set /p BUILD=Enter a build (e.g. 11234.2): 
set /p MATURITY=Enter maturity (e.g. Alpha, Beta, RC, Release, etc.): 

echo using System.Reflection; > "src/VersionAssemblyInfo.cs"
echo. >> "src/VersionAssemblyInfo.cs"
echo [assembly: AssemblyVersion("%VERSION%.0.0")] >> "src/VersionAssemblyInfo.cs"
echo [assembly: AssemblyFileVersion("%VERSION%.%BUILD%")] >> "src/VersionAssemblyInfo.cs"
echo //// [assembly: AssemblyInformationalVersion("%VERSION%.%BUILD% %MATURITY%")] >> "src/VersionAssemblyInfo.cs"

if exist package ( rmdir /s /q package )
mkdir package

call build.cmd
cd publish-v4.0
%ZIP_EXE% a -mx9 -r -y "..\package\Muster-%VERSION%.%BUILD%-net40.zip" *.*
cd ..

%NUGET_EXE% Pack muster.nuspec -Version %VERSION%.%BUILD% -OutputDirectory package

rmdir /s /q publish-v4.0

:done
if errorlevel 1 pause else exit