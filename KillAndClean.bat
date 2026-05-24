@echo off
title Kill Build Locks
chcp 65001 >nul

echo ==========================================
echo   Killing build processes...
echo ==========================================

taskkill /F /IM CpmServer.exe    2>nul && echo [OK] CpmServer.exe    || echo [SKIP] CpmServer.exe not running
taskkill /F /IM dotnet.exe       2>nul && echo [OK] dotnet.exe       || echo [SKIP] dotnet.exe not running
taskkill /F /IM VBCSCompiler.exe 2>nul && echo [OK] VBCSCompiler.exe || echo [SKIP] VBCSCompiler.exe not running
taskkill /F /IM MSBuild.exe      2>nul && echo [OK] MSBuild.exe      || echo [SKIP] MSBuild.exe not running

echo.
echo ==========================================
echo   Cleaning bin/obj folders...
echo ==========================================

set "ROOT=C:\Users\adm\Qsync\AI\CPM_SrcCode"

if exist "%ROOT%\CpmServer\bin"             rd /s /q "%ROOT%\CpmServer\bin"             && echo [OK] CpmServer\bin
if exist "%ROOT%\CpmServer\obj"             rd /s /q "%ROOT%\CpmServer\obj"             && echo [OK] CpmServer\obj
if exist "%ROOT%\CpmServer.Core\bin"        rd /s /q "%ROOT%\CpmServer.Core\bin"        && echo [OK] CpmServer.Core\bin
if exist "%ROOT%\CpmServer.Core\obj"        rd /s /q "%ROOT%\CpmServer.Core\obj"        && echo [OK] CpmServer.Core\obj
if exist "%ROOT%\CpmServer.SharedKernel\bin" rd /s /q "%ROOT%\CpmServer.SharedKernel\bin" && echo [OK] CpmServer.SharedKernel\bin
if exist "%ROOT%\CpmServer.SharedKernel\obj" rd /s /q "%ROOT%\CpmServer.SharedKernel\obj" && echo [OK] CpmServer.SharedKernel\obj
if exist "%ROOT%\CpmServer.Modules.Approval\bin"     rd /s /q "%ROOT%\CpmServer.Modules.Approval\bin"     && echo [OK] Approval\bin
if exist "%ROOT%\CpmServer.Modules.Approval\obj"     rd /s /q "%ROOT%\CpmServer.Modules.Approval\obj"     && echo [OK] Approval\obj
if exist "%ROOT%\CpmServer.Modules.Quotation\bin"    rd /s /q "%ROOT%\CpmServer.Modules.Quotation\bin"    && echo [OK] Quotation\bin
if exist "%ROOT%\CpmServer.Modules.Quotation\obj"    rd /s /q "%ROOT%\CpmServer.Modules.Quotation\obj"    && echo [OK] Quotation\obj
if exist "%ROOT%\CpmServer.Modules.PM\bin"           rd /s /q "%ROOT%\CpmServer.Modules.PM\bin"           && echo [OK] PM\bin
if exist "%ROOT%\CpmServer.Modules.PM\obj"           rd /s /q "%ROOT%\CpmServer.Modules.PM\obj"           && echo [OK] PM\obj
if exist "%ROOT%\CpmServer.Modules.SequenceRule\bin" rd /s /q "%ROOT%\CpmServer.Modules.SequenceRule\bin" && echo [OK] SequenceRule\bin
if exist "%ROOT%\CpmServer.Modules.SequenceRule\obj" rd /s /q "%ROOT%\CpmServer.Modules.SequenceRule\obj" && echo [OK] SequenceRule\obj

echo.
echo ==========================================
echo   Done. You can now rebuild.
echo ==========================================
pause
