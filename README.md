# 🖥️ Monitor de Sistema - CPU Temperature Monitor

Um aplicativo de monitoramento de sistema em tempo real para Windows que exibe informações da CPU, memória e clock na bandeja do sistema.

## 📋 Funcionalidades

### Monitoramento em Tempo Real
- **Temperatura da CPU**: Monitora a temperatura do processador em tempo real
- **Uso de Memória**: Exibe uso atual e total da RAM do sistema
- **Clock da CPU**: Mostra o clock máximo atingido pelo processador
- **Atualização Automática**: Informações atualizadas a cada 2 segundos

### Sistema de Alertas
- **⚠️ Alerta de Temperatura Alta**: Notificação automática quando a temperatura da CPU excede 80°C
- **🔔 Notificação do Windows**: Balloon tip com aviso visual e sonoro
- **📝 Log de Eventos**: Registro automático de todos os alertas de temperatura
- **⏰ Controle de Cooldown**: Alertas limitados a cada 5 minutos para evitar spam

### Interface Intuitiva
- **Ícone na Bandeja**: Exibição compacta das informações no system tray
- **Indicação Visual**: Cores diferentes do ícone baseadas na temperatura:
  - 🟢 Verde (Normal): Temperatura até 70°C
  - 🟡 Amarelo (Atenção): Temperatura entre 70°C e 80°C
  - 🔴 Vermelho (Crítico): Temperatura acima de 80°C
- **Menu de Contexto**: Acesso rápido a logs e configurações

## 🚀 Como Usar

### Instalação Rápida (Recomendado)
1. **Execute o Instalador**:
   ```bash
   # Duplo clique no arquivo ou execute:
   Instalar.bat
   ```
   
2. **Ou use o PowerShell diretamente**:
   ```powershell
   # Instalar na inicialização do Windows
   .\Install-StartupMonitor.ps1 -Install
   
   # Verificar status
   .\Install-StartupMonitor.ps1 -Status
   
   # Desinstalar
   .\Install-StartupMonitor.ps1 -Uninstall
   ```

### Instalação Manual
1. **Compilar o projeto**:
   ```bash
   dotnet publish -c Release
   ```

2. **Executar o aplicativo**:
   ```bash
   # Execução única
   dotnet run
   
   # Ou executar o arquivo publicado
   .\bin\Release\net9.0-windows\publish\CpuTempMonitor.exe
   ```

### 🔄 Configuração para Inicialização Automática

#### Método 1: Script Automatizado (Recomendado)
Execute `Instalar.bat` como administrador. O script irá:
- Compilar o aplicativo
- Criar um atalho na pasta de inicialização do Windows
- Configurar para execução em segundo plano
- Oferecer opção para iniciar imediatamente

#### Método 2: Manual
1. Compile o projeto: `dotnet publish -c Release`
2. Copie o atalho do executável para:
   ```
   %APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\
   ```
3. Configure o atalho para executar minimizado

#### Verificação da Instalação
```powershell
# Verificar se está configurado para inicialização
.\Install-StartupMonitor.ps1 -Status

# Ou verifique manualmente a pasta:
explorer "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\"
```

### Interface do Sistema
1. **Ícone na Bandeja**: Após executar, o aplicativo aparecerá na bandeja do sistema
2. **Tooltip**: Passe o mouse sobre o ícone para ver as informações atuais
3. **Menu de Contexto**: Clique com o botão direito no ícone para acessar:
   - **Ver Log**: Abre o log completo do sistema
   - **Ver Alertas de Temperatura**: Abre o log específico de alertas
   - **Sair**: Fecha o aplicativo

### Interpretando as Informações
```
Formato do Tooltip: "CPU: 45.2°C | RAM: 8.1GB/16.0GB (51%) | Clock: 3200MHz"
```

- **CPU**: Temperatura atual do processador
- **RAM**: Memória usada/total (porcentagem de uso)
- **Clock**: Frequência máxima atual do processador

## 📊 Sistema de Logs

### Log Principal (`SystemMonitor.log`)
Localização: `%TEMP%\SystemMonitor.log`

Registra:
- Inicialização do aplicativo
- Hardware detectado
- Sensores encontrados
- Leituras de temperatura, memória e clock
- Erros e exceções

### Log de Alertas (`HighTemperatureAlerts.log`)
Localização: `%TEMP%\HighTemperatureAlerts.log`

Registra:
- Alertas de temperatura crítica (>80°C)
- Normalização da temperatura
- Timestamps de todos os eventos

## ⚙️ Configuração e Instalação

### Pré-requisitos
- Windows 10/11
- .NET 9.0 Runtime
- Permissões de administrador (para acesso aos sensores de hardware)

### Dependências
- `LibreHardwareMonitorLib v0.9.4`: Para acesso aos sensores de hardware
- `System.Windows.Forms`: Para interface gráfica e notificações

### Estrutura do Projeto
```
CpuTempMonitor/
├── Program.cs                    # Código principal do aplicativo
├── CpuTempMonitor.csproj        # Configuração do projeto .NET
├── CpuTempMonitor.sln           # Solução do Visual Studio
├── app.manifest                 # Manifesto para permissões administrativas
├── README.md                    # Esta documentação
├── Install-StartupMonitor.ps1   # Script de instalação PowerShell
├── Instalar.bat                 # Script de instalação simplificado
└── bin/Release/net9.0-windows/publish/  # Executável compilado
    └── CpuTempMonitor.exe       # Aplicativo principal
```

### 📁 Arquivos de Instalação

#### `Instalar.bat`
Script de instalação simplificado. Duplo clique para executar.
- Interface amigável para usuários
- Executa automaticamente o script PowerShell
- Não requer conhecimento técnico

#### `Install-StartupMonitor.ps1`
Script PowerShell avançado com opções completas:
```powershell
# Instalar
.\Install-StartupMonitor.ps1 -Install

# Desinstalar  
.\Install-StartupMonitor.ps1 -Uninstall

# Verificar status
.\Install-StartupMonitor.ps1 -Status

# Menu interativo (sem parâmetros)
.\Install-StartupMonitor.ps1
```

**Funcionalidades do Script:**
- ✅ Compilação automática do projeto
- ✅ Criação de atalho na inicialização
- ✅ Verificação de dependências
- ✅ Gerenciamento de processos
- ✅ Status detalhado da instalação
- ✅ Abertura de logs
- ✅ Desinstalação completa

## 🔧 Arquitetura Técnica

### Classes Principais

#### `SystemInfo`
Estrutura de dados que armazena as informações do sistema:
```csharp
public class SystemInfo
{
    public float? Temperature { get; set; }      // Temperatura da CPU em °C
    public float? MemoryUsed { get; set; }       // Memória usada em GB
    public float? MemoryTotal { get; set; }      // Memória total em GB
    public float? MaxClock { get; set; }         // Clock máximo em MHz
    public float? MemoryUsagePercent { get; set; } // Percentual de uso da RAM
}
```

#### `SystemMonitor`
Classe principal responsável pelo monitoramento:
- Gerencia conexão com LibreHardwareMonitor
- Coleta dados dos sensores de hardware
- Implementa fallback para Win32 API quando necessário
- Implementa `IDisposable` para limpeza de recursos

### Detecção de Sensores
O aplicativo utiliza uma estratégia de prioridade para detecção de sensores de temperatura:

1. **Core Average**: Média de todos os cores (preferencial)
2. **Package**: Temperatura do pacote do processador
3. **Tctl**: Temperatura de controle (AMD)
4. **Tdie**: Temperatura do die (AMD)
5. **Core**: Qualquer sensor de core individual
6. **Fallback**: Primeiro sensor disponível

### Sistema de Alertas
```csharp
private static void ShowHighTemperatureAlert(float temperature, NotifyIcon trayIcon)
{
    // Controle de cooldown de 5 minutos
    // Notificação balloon tip
    // Som de sistema
    // Log automático
}
```

## 🛠️ Troubleshooting

### Problemas Comuns

#### Temperatura Mostra Zero ou N/A
**Possíveis Causas:**
- Falta de permissões administrativas
- Processador não suportado
- Drivers de chipset desatualizados
- Sistema virtualizado

**Soluções:**
1. Execute como administrador
2. Atualize drivers do chipset
3. Verifique compatibilidade do processador

#### Aplicativo Não Inicia
**Possíveis Causas:**
- .NET Runtime não instalado
- Dependências ausentes

**Soluções:**
1. Instale .NET 9.0 Runtime
2. Execute `dotnet restore` para restaurar dependências

#### Alertas Não Aparecem
**Verificações:**
1. Notificações do Windows habilitadas
2. Temperatura realmente acima de 80°C
3. Não está em período de cooldown (5 minutos)

## 📈 Melhorias Futuras

### Funcionalidades Planejadas
- [ ] Configuração de limites de temperatura personalizáveis
- [ ] Monitoramento de GPU
- [ ] Gráficos de histórico de temperatura
- [ ] Exportação de relatórios
- [ ] Interface gráfica completa (opcional)
- [ ] Suporte a múltiplos processadores
- [ ] Integração com serviços de nuvem para monitoramento remoto

### Otimizações Técnicas
- [ ] Redução do uso de memória
- [ ] Otimização da frequência de atualização
- [ ] Cache inteligente de sensores
- [ ] Compressão de logs antigos

## 📞 Suporte

### Como Instalar e Configurar

#### Instalação Automática (Mais Fácil)
1. **Baixe o projeto** para uma pasta no seu computador
2. **Execute como administrador**: `Instalar.bat`
3. **Siga as instruções** na tela
4. **Pronto!** O monitor estará na bandeja do sistema

#### Instalação Manual (Avançado)
1. **Instale o .NET 9.0** se não tiver
2. **Abra PowerShell como administrador** na pasta do projeto
3. **Execute**: `dotnet publish -c Release`
4. **Execute**: `.\Install-StartupMonitor.ps1 -Install`

#### Comandos Úteis
```powershell
# Verificar se está funcionando
.\Install-StartupMonitor.ps1 -Status

# Ver logs do sistema
Get-Content "$env:TEMP\SystemMonitor.log" -Tail 20

# Ver alertas de temperatura
Get-Content "$env:TEMP\HighTemperatureAlerts.log"

# Parar o aplicativo
Get-Process CpuTempMonitor | Stop-Process

# Remover da inicialização
.\Install-StartupMonitor.ps1 -Uninstall
```

### Logs para Diagnóstico
Em caso de problemas, colete os seguintes arquivos:
- `%TEMP%\SystemMonitor.log`
- `%TEMP%\HighTemperatureAlerts.log`

### Informações do Sistema
Para suporte, inclua:
- Modelo do processador
- Versão do Windows
- Versão do .NET instalada
- Modelo da placa-mãe (se conhecido)

## 📄 Licença

Este projeto é de código aberto. Sinta-se livre para modificar e distribuir conforme necessário.

---

---

## 🎯 Resumo Final

### ✅ O que foi implementado:

1. **📊 Monitoramento Completo**:
   - Temperatura da CPU em tempo real
   - Uso de memória RAM (usado/total/%)
   - Clock máximo do processador
   - Atualização a cada 2 segundos

2. **🚨 Sistema de Alertas**:
   - Notificação automática para temperatura > 80°C
   - Som de alerta do sistema
   - Log detalhado de todos os eventos
   - Controle de cooldown (5 minutos entre alertas)

3. **🔧 Instalação Automática**:
   - Script `Instalar.bat` para usuários finais
   - Script `Install-StartupMonitor.ps1` para usuários avançados
   - Configuração automática na inicialização do Windows
   - Execução em segundo plano (bandeja do sistema)

4. **📝 Logs Detalhados**:
   - `SystemMonitor.log`: Log completo do sistema
   - `HighTemperatureAlerts.log`: Log específico de alertas
   - Timestamps precisos e informações técnicas

### 🚀 Como usar agora:

1. **Execute**: `Instalar.bat` (como administrador)
2. **Confirme**: a instalação na inicialização
3. **Verifique**: o ícone na bandeja do sistema
4. **Monitore**: as informações passando o mouse sobre o ícone

### 🎨 Interface Visual:
- 🟢 **Verde**: Temperatura normal (≤70°C)
- 🟡 **Amarelo**: Temperatura alta (70°C-80°C) 
- 🔴 **Vermelho**: Temperatura crítica (>80°C)

**⚠️ Aviso Important**: Este aplicativo requer permissões administrativas para acessar sensores de hardware. Use apenas em sistemas confiáveis.