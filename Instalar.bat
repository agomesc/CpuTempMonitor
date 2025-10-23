@echo off
echo ============================================
echo    Monitor de Sistema - Instalador
echo ============================================
echo.
echo Este script configurara o Monitor de Sistema
echo para iniciar automaticamente com o Windows.
echo.
pause

PowerShell -ExecutionPolicy Bypass -File "%~dp0Install-StartupMonitor.ps1"

echo.
echo Pressione qualquer tecla para sair...
pause >nul