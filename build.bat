@echo off
REM Build script for esapi_nnunet_submit_image_and_labels worker
REM This script builds the project using MSBuild

setlocal enabledelayedexpansion

REM Set project directory
set "PROJECT_DIR=%~dp0"
set "PROJECT_FILE=%PROJECT_DIR%esapi_nnunet_submit_image_and_labels.csproj"

REM Default to Debug configuration if not specified
REM Normalize configuration name (case-insensitive)
if "%1"=="" (
    set "CONFIG=Debug"
) else (
    set "CONFIG=%1"
    REM Normalize to proper case
    if /i "%CONFIG%"=="debug" set "CONFIG=Debug"
    if /i "%CONFIG%"=="release" set "CONFIG=Release"
)

REM Default to x64 platform
if "%2"=="" (
    set "PLATFORM=x64"
) else (
    set "PLATFORM=%2"
)

echo ========================================
echo Building esapi_nnunet_submit_image_and_labels
echo Configuration: %CONFIG%
echo Platform: %PLATFORM%
echo ========================================
echo.

REM Try to find MSBuild (prioritize newer versions that can handle modern project formats)
set "MSBUILD_PATH="

REM Check for Visual Studio 2026 (version 19) - prioritize this first
if exist "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
REM Check for Visual Studio 2022 (common locations) - prioritize these for better project format support
REM Also check for Build Tools
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
REM Check for Visual Studio 2019
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
REM Check for Visual Studio 2017
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
REM Fallback to standalone MSBuild
) else if exist "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
REM Last resort: .NET Framework MSBuild (WARNING: This version doesn't support C# 6.0+ features)
) else if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
    set "OLD_MSBUILD=1"
) else if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" (
    set "MSBUILD_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
    set "OLD_MSBUILD=1"
)

if "%MSBUILD_PATH%"=="" (
    echo ERROR: MSBuild not found!
    echo Please install Visual Studio or .NET Framework SDK
    echo Or specify MSBuild path manually in this script
    exit /b 1
)

if defined OLD_MSBUILD (
    echo.
    echo ========================================
    echo ERROR: Old MSBuild detected!
    echo ========================================
    echo.
    echo The .NET Framework 4.0 MSBuild doesn't support C# 6.0+ features like:
    echo   - String interpolation
    echo   - Expression-bodied members
    echo   - Null-conditional operators
    echo.
    echo This project requires Visual Studio 2017 or later MSBuild.
    echo.
    echo Solutions:
    echo   1. Install Visual Studio 2017/2019/2022 (any edition)
    echo   2. Install Visual Studio Build Tools
    echo   3. Use Developer Command Prompt for Visual Studio
    echo   4. Manually set MSBUILD_PATH in this script to point to a newer MSBuild
    echo.
    echo Download Visual Studio Build Tools:
    echo   https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022
    echo.
    exit /b 1
)

echo Using MSBuild: %MSBUILD_PATH%
echo.

REM Build project dependencies first (esapi will build variandb as a dependency)
echo Building project dependencies...
echo.

REM Build esapi dependency first (which will build variandb)
echo Building esapi...
set "ESAPI_PROJ=%PROJECT_DIR%..\esapi\esapi.csproj"
"%MSBUILD_PATH%" "%ESAPI_PROJ%" /p:Configuration=%CONFIG% /p:Platform=%PLATFORM% /t:Build /v:minimal
if errorlevel 1 (
    echo ERROR: esapi build failed!
    exit /b 1
)

echo.
echo Building main project (esapi_nnunet_submit_image_and_labels)...
echo.

REM Clean obj folder to remove stale NuGet package references
if exist "%PROJECT_DIR%obj" (
    echo Cleaning obj folder to remove stale NuGet references...
    rmdir /s /q "%PROJECT_DIR%obj" 2>nul
)

REM Build the main project
REM Skip NuGet package restore since we're using local DLLs
"%MSBUILD_PATH%" "%PROJECT_FILE%" /p:Configuration=%CONFIG% /p:Platform=%PLATFORM% /p:RestorePackages=false /p:SkipRestorePackages=true /p:DisableImplicitNuGetFallbackFolder=true /p:ResolveNuGetPackages=false /p:BuildProjectReferences=false /t:Build /v:minimal
if errorlevel 1 (
    echo.
    echo ========================================
    echo Build failed!
    echo ========================================
    exit /b 1
) else (
    echo.
    echo ========================================
    echo Build succeeded!
    if /i "%CONFIG%"=="Debug" (
        echo Output: %PROJECT_DIR%bin\debug\esapi_nnunet_submit_image_and_labels.exe
    ) else (
        echo Output: %PROJECT_DIR%bin\release\esapi_nnunet_submit_image_and_labels.exe
    )
    echo ========================================
    exit /b 0
)

