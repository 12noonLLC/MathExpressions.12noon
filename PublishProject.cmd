@echo off
rem
rem	Perform a clean restore and build.
rem	Run all unit tests.
rem	Create a NuGet library.
rem	Publish a standalone targets.
rem	Create an app bundle for the Microsoft Store.
rem

if NOT EXIST "MathExpressions.slnx" (
  echo Run this script from the solution folder.
  goto :EOF
)

echo.
echo Note: update the version in:
echo - MathExpressions.MAUI.csproj
echo - MathExpressions.Windows.csproj
echo.
echo Exit Visual Studio to avoid directory locks, such as PackageLayout, etc.
echo.

choice /c YN /n /m "Press N to quit, Y to continue: "
if errorlevel 2 (
	echo Quitting...
	exit /b 0
)

setlocal

:: Prompt for version number if not passed as an argument
if "%~1" == "" (
	echo.
	echo Usage: %~nx0 ^<version^>
	exit /b 1
)

set VERSION=%~1

set BuildOutputRoot=C:\VSIntermediate\MathExpressions.12noon

set MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64

::
:: RESTORE packages for Windows x64
::

dotnet clean
dotnet restore

::
:: BUILD
::
REM Note that the wapproj project must be disabled for
REM Release because only Visual Studio can build it.

dotnet build ^
	MathExpressions.slnx ^
	--configuration Release

if %ERRORLEVEL% neq 0 exit /b 1

::
:: TEST
::
:: Run unit tests using MS Testing Platform
REM Requirements:
REM 	Note that we need <UseMicrosoftTestingPlatform>true</UseMicrosoftTestingPlatform> in the csproj for MTP.
REM 	The SLNX file must be set to build the unit tests project. The project cannot include <Build Project="false" />.

REM Note: Cannot make "--solution MathExpressions.slnx" work.

dotnet test ^
	--project MathExpressions.UnitTests\MathExpressions.UnitTests.csproj ^
	--configuration Release

if %ERRORLEVEL% neq 0 exit /b 1

::
:: PACK the NuGet library
::

dotnet pack ^
	MathExpressions.Windows\MathExpressions.Windows.csproj ^
	-p:Version=%VERSION% ^
	--no-build ^
	--configuration Release ^
	--output "%BuildOutputRoot%\publish"

dotnet pack ^
	MathExpressions.MAUI\MathExpressions.MAUI.csproj ^
	-p:Version=%VERSION% ^
	--no-build ^
	--configuration Release ^
	--output "%BuildOutputRoot%\publish"

endlocal
