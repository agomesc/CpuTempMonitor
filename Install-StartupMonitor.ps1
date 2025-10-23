# Script de Instalacao - Monitor de Sistema
# Este script configura o CpuTempMonitor para iniciar automaticamente com o Windows

param(
    [switch]$Install,
    [switch]$Uninstall,
    [switch]$Status
)

$AppName = "CpuTempMonitor"
$AppDisplayName = "Monitor de Sistema"
$CurrentPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$PublishPath = Join-Path $CurrentPath "bin\Release\net9.0-windows\publish"
$ExecutablePath = Join-Path $PublishPath "CpuTempMonitor.exe"
$StartupFolder = [Environment]::GetFolderPath("Startup")
$ShortcutPath = Join-Path $StartupFolder "$AppName.lnk"

# Funcao para verificar se esta executando como administrador
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Funcao para criar atalho na pasta de inicializacao
function Install-StartupShortcut {
    Write-Host "Instalando Monitor de Sistema na inicializacao do Windows..." -ForegroundColor Green
    
    # Verifica se o executavel existe
    if (-not (Test-Path $ExecutablePath)) {
        Write-Host "Erro: Executavel nao encontrado em $ExecutablePath" -ForegroundColor Red
        Write-Host "Execute primeiro: dotnet publish" -ForegroundColor Yellow
        return $false
    }
    
    try {
        # Cria objeto WScript.Shell para criar o atalho
        $WScriptShell = New-Object -ComObject WScript.Shell
        $Shortcut = $WScriptShell.CreateShortcut($ShortcutPath)
        $Shortcut.TargetPath = $ExecutablePath
        $Shortcut.WorkingDirectory = $PublishPath
        $Shortcut.Description = "Monitor de Sistema - Temperatura, Memoria e Clock"
        $Shortcut.WindowStyle = 7  # Minimizado
        $Shortcut.Save()
        
        Write-Host "Atalho criado com sucesso em: $ShortcutPath" -ForegroundColor Green
        Write-Host "O Monitor de Sistema agora iniciara automaticamente com o Windows" -ForegroundColor Green
        
        # Opcao para iniciar agora
        $response = Read-Host "Deseja iniciar o Monitor de Sistema agora? (s/n)"
        if ($response -eq 's' -or $response -eq 'S' -or $response -eq 'sim') {
            Start-Process $ExecutablePath -WorkingDirectory $PublishPath
            Write-Host "Monitor de Sistema iniciado!" -ForegroundColor Green
        }
        
        return $true
    }
    catch {
        Write-Host "Erro ao criar atalho: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Funcao para remover atalho da pasta de inicializacao
function Uninstall-StartupShortcut {
    Write-Host "Removendo Monitor de Sistema da inicializacao do Windows..." -ForegroundColor Yellow
    
    if (Test-Path $ShortcutPath) {
        try {
            Remove-Item $ShortcutPath -Force
            Write-Host "Atalho removido com sucesso!" -ForegroundColor Green
            Write-Host "O Monitor de Sistema nao iniciara mais automaticamente" -ForegroundColor Green
            
            # Opcao para parar processo atual
            $processes = Get-Process -Name "CpuTempMonitor" -ErrorAction SilentlyContinue
            if ($processes) {
                $response = Read-Host "Monitor de Sistema esta executando. Deseja fecha-lo agora? (s/n)"
                if ($response -eq 's' -or $response -eq 'S' -or $response -eq 'sim') {
                    $processes | Stop-Process -Force
                    Write-Host "Monitor de Sistema finalizado!" -ForegroundColor Green
                }
            }
            
            return $true
        }
        catch {
            Write-Host "Erro ao remover atalho: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
    else {
        Write-Host "Atalho nao encontrado. O aplicativo nao esta configurado para inicializacao automatica." -ForegroundColor Blue
        return $true
    }
}

# Funcao para verificar status da instalacao
function Get-InstallationStatus {
    Write-Host "=== Status do Monitor de Sistema ===" -ForegroundColor Cyan
    
    # Verifica se o executavel existe
    if (Test-Path $ExecutablePath) {
        Write-Host "Executavel encontrado: $ExecutablePath" -ForegroundColor Green
    }
    else {
        Write-Host "Executavel nao encontrado: $ExecutablePath" -ForegroundColor Red
        Write-Host "   Execute: dotnet publish" -ForegroundColor Yellow
    }
    
    # Verifica se esta na inicializacao
    if (Test-Path $ShortcutPath) {
        Write-Host "Configurado para inicializacao automatica" -ForegroundColor Green
        Write-Host "   Atalho: $ShortcutPath" -ForegroundColor Gray
    }
    else {
        Write-Host "Nao esta configurado para inicializacao automatica" -ForegroundColor Red
    }
    
    # Verifica se esta executando
    $processes = Get-Process -Name "CpuTempMonitor" -ErrorAction SilentlyContinue
    if ($processes) {
        Write-Host "Monitor de Sistema esta executando" -ForegroundColor Green
        Write-Host "   PIDs: $($processes.Id -join ', ')" -ForegroundColor Gray
    }
    else {
        Write-Host "Monitor de Sistema nao esta executando" -ForegroundColor Yellow
    }
    
    # Verifica logs
    $logPath = Join-Path $env:TEMP "SystemMonitor.log"
    if (Test-Path $logPath) {
        $logInfo = Get-Item $logPath
        Write-Host "Log principal: $logPath" -ForegroundColor Gray
        Write-Host "   Tamanho: $([math]::Round($logInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
        Write-Host "   Ultima modificacao: $($logInfo.LastWriteTime)" -ForegroundColor Gray
    }
    
    $alertLogPath = Join-Path $env:TEMP "HighTemperatureAlerts.log"
    if (Test-Path $alertLogPath) {
        $alertLogInfo = Get-Item $alertLogPath
        Write-Host "Log de alertas: $alertLogPath" -ForegroundColor Gray
        Write-Host "   Tamanho: $([math]::Round($alertLogInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    }
    else {
        Write-Host "Nenhum alerta de temperatura registrado ainda" -ForegroundColor Gray
    }
}

# Funcao principal
function Main {
    Write-Host "=== Instalador do Monitor de Sistema ===" -ForegroundColor Cyan
    Write-Host "Monitoramento de CPU, Memoria e Clock em tempo real" -ForegroundColor Gray
    Write-Host ""
    
    if ($Status) {
        Get-InstallationStatus
        return
    }
    
    if ($Install) {
        $success = Install-StartupShortcut
        if ($success) {
            Write-Host ""
            Write-Host "=== Instalacao Concluida ===" -ForegroundColor Green
            Write-Host "• O Monitor de Sistema iniciara automaticamente com o Windows" -ForegroundColor White
            Write-Host "• Aparecera na bandeja do sistema (system tray)" -ForegroundColor White
            Write-Host "• Clique com botao direito no icone para ver opcoes" -ForegroundColor White
            Write-Host "• Alertas automaticos para temperatura > 80C" -ForegroundColor White
        }
        return
    }
    
    if ($Uninstall) {
        Uninstall-StartupShortcut
        return
    }
    
    # Menu interativo se nenhum parametro foi fornecido
    do {
        Write-Host ""
        Write-Host "Escolha uma opcao:" -ForegroundColor Yellow
        Write-Host "1. Instalar na inicializacao do Windows"
        Write-Host "2. Remover da inicializacao do Windows"
        Write-Host "3. Verificar status"
        Write-Host "4. Compilar e publicar aplicativo"
        Write-Host "5. Abrir logs"
        Write-Host "0. Sair"
        Write-Host ""
        
        $choice = Read-Host "Digite sua escolha (0-5)"
        
        switch ($choice) {
            "1" {
                Install-StartupShortcut
            }
            "2" {
                Uninstall-StartupShortcut
            }
            "3" {
                Get-InstallationStatus
            }
            "4" {
                Write-Host "Compilando aplicativo..." -ForegroundColor Yellow
                Push-Location $CurrentPath
                try {
                    dotnet publish -c Release
                    Write-Host "Compilacao concluida!" -ForegroundColor Green
                }
                catch {
                    Write-Host "Erro na compilacao: $($_.Exception.Message)" -ForegroundColor Red
                }
                finally {
                    Pop-Location
                }
            }
            "5" {
                $logPath = Join-Path $env:TEMP "SystemMonitor.log"
                $alertLogPath = Join-Path $env:TEMP "HighTemperatureAlerts.log"
                
                if (Test-Path $logPath) {
                    Start-Process "notepad.exe" $logPath
                }
                
                if (Test-Path $alertLogPath) {
                    $response = Read-Host "Deseja abrir tambem o log de alertas? (s/n)"
                    if ($response -eq 's' -or $response -eq 'S') {
                        Start-Process "notepad.exe" $alertLogPath
                    }
                }
                
                if (-not (Test-Path $logPath)) {
                    Write-Host "Nenhum log encontrado ainda." -ForegroundColor Yellow
                }
            }
            "0" {
                Write-Host "Saindo..." -ForegroundColor Gray
                break
            }
            default {
                Write-Host "Opcao invalida!" -ForegroundColor Red
            }
        }
    } while ($choice -ne "0")
}

# Executa funcao principal
Main