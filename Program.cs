using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using LibreHardwareMonitor.Hardware;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

public class SystemInfo
{
    public float? Temperature { get; set; }
    public float? MemoryUsed { get; set; }
    public float? MemoryTotal { get; set; }
    public float? MaxClock { get; set; }
    public float? MemoryUsagePercent { get; set; }
}

class SystemMonitor : IDisposable
{
    private readonly Computer _computer;
    private bool _disposed = false;

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    public SystemMonitor()
    {
        try
        {
            LogMessage("Inicializando LibreHardwareMonitor...");
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = false,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true, // Habilitar motherboard para AMD
                IsControllerEnabled = false,
                IsNetworkEnabled = false,
                IsStorageEnabled = false
            };
            _computer.Open();
            LogMessage("LibreHardwareMonitor inicializado com sucesso.");
        }
        catch (Exception ex)
        {
            LogMessage($"Erro ao inicializar LibreHardwareMonitor: {ex}");
            throw new InvalidOperationException($"Erro ao inicializar monitor de hardware: {ex.Message}", ex);
        }
    }

    public SystemInfo GetSystemInfo()
    {
        if (_disposed) return new SystemInfo();

        var info = new SystemInfo();

        try
        {
            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                LogMessage($"Hardware encontrado: {hardware.Name} - Tipo: {hardware.HardwareType}");

                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    // Temperatura da CPU
                    var temperatureSensors = hardware.Sensors
                        .Where(s => s.SensorType == SensorType.Temperature)
                        .ToList();

                    LogMessage($"Sensores de temperatura encontrados: {temperatureSensors.Count}");
                    foreach (var sensor in temperatureSensors)
                    {
                        LogMessage($"  - {sensor.Name}: {sensor.Value}°C");
                    }

                    // TESTE: Forçar temperatura para verificar se o sistema funciona
                    info.Temperature = 45.5f;
                    LogMessage($"TESTE: Temperatura forçada para: {info.Temperature}°C");

                    // Clock da CPU
                    var clockSensors = hardware.Sensors
                        .Where(s => s.SensorType == SensorType.Clock)
                        .ToList();

                    LogMessage($"Sensores de clock encontrados: {clockSensors.Count}");
                    foreach (var sensor in clockSensors)
                    {
                        LogMessage($"  - {sensor.Name}: {sensor.Value} MHz");
                    }

                    if (clockSensors.Any())
                    {
                        var maxClock = clockSensors
                            .Where(s => s.Value.HasValue)
                            .Select(s => s.Value!.Value)
                            .DefaultIfEmpty(0)
                            .Max();

                        if (maxClock > 0)
                        {
                            info.MaxClock = maxClock;
                            LogMessage($"Clock máximo: {maxClock} MHz");
                        }
                    }
                }
                else if (hardware.HardwareType == HardwareType.Memory)
                {
                    var memorySensors = hardware.Sensors
                        .Where(s => s.SensorType == SensorType.Data)
                        .ToList();

                    LogMessage($"Sensores de memória encontrados: {memorySensors.Count}");
                    foreach (var sensor in memorySensors)
                    {
                        LogMessage($"  - {sensor.Name}: {sensor.Value} GB");
                    }

                    var usedMemorySensor = memorySensors
                        .FirstOrDefault(s => s.Name.Contains("Used") || s.Name.Contains("Memory Used"));

                    var availableMemorySensor = memorySensors
                        .FirstOrDefault(s => s.Name.Contains("Available") || s.Name.Contains("Memory Available"));

                    if (usedMemorySensor?.Value.HasValue == true)
                    {
                        info.MemoryUsed = usedMemorySensor.Value.Value;
                        LogMessage($"Memória usada: {info.MemoryUsed} GB");
                    }

                    if (availableMemorySensor?.Value.HasValue == true)
                    {
                        var availableMemory = availableMemorySensor.Value.Value;
                        info.MemoryTotal = (info.MemoryUsed ?? 0) + availableMemory;
                        LogMessage($"Memória total: {info.MemoryTotal} GB");
                    }

                    // Calcular porcentagem de uso da memória
                    if (info.MemoryUsed.HasValue && info.MemoryTotal.HasValue && info.MemoryTotal.Value > 0)
                    {
                        info.MemoryUsagePercent = (info.MemoryUsed.Value / info.MemoryTotal.Value) * 100;
                        LogMessage($"Uso de memória: {info.MemoryUsagePercent:F1}%");
                    }
                }
            }

            // Se não conseguiu obter informações de memória via LibreHardwareMonitor, usa método alternativo
            if (!info.MemoryUsed.HasValue)
            {
                try
                {
                    // Usar Win32 API para obter informações de memória do sistema
                    var memStatus = new MEMORYSTATUSEX();
                    if (GlobalMemoryStatusEx(memStatus))
                    {
                        info.MemoryTotal = (float)(memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0));
                        var availableMemory = (float)(memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0));
                        info.MemoryUsed = info.MemoryTotal.Value - availableMemory;
                        info.MemoryUsagePercent = (info.MemoryUsed.Value / info.MemoryTotal.Value) * 100;
                        
                        LogMessage($"Memória obtida via Win32 API - Usada: {info.MemoryUsed:F2} GB, Total: {info.MemoryTotal:F2} GB");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Erro ao obter informações de memória via Win32 API: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Erro ao ler informações do sistema: {ex}");
        }

        return info;
    }

    private void LogMessage(string message)
    {
        try
        {
            var logPath = Path.Combine(Path.GetTempPath(), "SystemMonitor.log");
            File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
        catch
        {
            // Ignora erros de log
        }
    }

    private float? GetAMDTemperature(IHardware hardware, List<ISensor> temperatureSensors)
    {
        try
        {
            // Prioridade para sensores AMD
            var tctlSensor = temperatureSensors.FirstOrDefault(s => s.Name.Contains("Tctl"));
            var tdieSensor = temperatureSensors.FirstOrDefault(s => s.Name.Contains("Tdie"));
            var ccxSensor = temperatureSensors.FirstOrDefault(s => s.Name.Contains("CCX"));
            var coreSensor = temperatureSensors.FirstOrDefault(s => s.Name.Contains("Core") && !s.Name.Contains("Max"));

            // Testar sensores na ordem de prioridade
            if (tctlSensor?.Value.HasValue == true && tctlSensor.Value.Value > 0)
            {
                LogMessage($"AMD: Usando sensor Tctl = {tctlSensor.Value.Value}°C");
                return tctlSensor.Value.Value;
            }

            if (tdieSensor?.Value.HasValue == true && tdieSensor.Value.Value > 0)
            {
                LogMessage($"AMD: Usando sensor Tdie = {tdieSensor.Value.Value}°C");
                return tdieSensor.Value.Value;
            }

            if (ccxSensor?.Value.HasValue == true && ccxSensor.Value.Value > 0)
            {
                LogMessage($"AMD: Usando sensor CCX = {ccxSensor.Value.Value}°C");
                return ccxSensor.Value.Value;
            }

            if (coreSensor?.Value.HasValue == true && coreSensor.Value.Value > 0)
            {
                LogMessage($"AMD: Usando sensor Core = {coreSensor.Value.Value}°C");
                return coreSensor.Value.Value;
            }

            // Se chegou aqui, nenhum sensor AMD funcionou
            LogMessage("AMD: Nenhum sensor AMD específico forneceu temperatura válida");
            return null;
        }
        catch (Exception ex)
        {
            LogMessage($"Erro ao obter temperatura AMD: {ex.Message}");
            return null;
        }
    }

    private float? GetTemperatureViaWMI()
    {
        try
        {
            LogMessage("Tentando WMI para temperatura...");
            
            // Método básico sem WMI por enquanto
            LogMessage("WMI: Não implementado ainda");
            return null;
        }
        catch (Exception ex)
        {
            LogMessage($"Erro ao obter temperatura via WMI: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            LogMessage("Fechando LibreHardwareMonitor...");
            _computer?.Close();
            _disposed = true;
        }
    }
}

static class Program
{
    private static bool _highTempAlertShown = false;
    private static DateTime _lastHighTempAlert = DateTime.MinValue;
    private static readonly TimeSpan AlertCooldown = TimeSpan.FromMinutes(5); // Alerta a cada 5 minutos no máximo

    private static void ShowHighTemperatureAlert(float temperature, NotifyIcon trayIcon)
    {
        var now = DateTime.Now;
        
        // Verifica se já passou o tempo de cooldown desde o último alerta
        if (now - _lastHighTempAlert >= AlertCooldown)
        {
            // Log do evento
            LogHighTemperatureEvent(temperature);
            
            // Exibe notificação no Windows
            trayIcon.BalloonTipTitle = "⚠️ ALERTA DE TEMPERATURA ALTA";
            trayIcon.BalloonTipText = $"A temperatura da CPU está em {temperature:F1}°C!\nTemperatura crítica detectada.";
            trayIcon.BalloonTipIcon = ToolTipIcon.Warning;
            trayIcon.ShowBalloonTip(10000); // Mostra por 10 segundos
            
            // Atualiza o controle de cooldown
            _lastHighTempAlert = now;
            _highTempAlertShown = true;
            
            // Também pode reproduzir um som de sistema (opcional)
            System.Media.SystemSounds.Exclamation.Play();
        }
    }

    private static void LogHighTemperatureEvent(float temperature)
    {
        LogHighTemperatureEvent(temperature, "ALERTA: Temperatura crítica detectada");
    }

    private static void LogHighTemperatureEvent(float temperature, string message)
    {
        try
        {
            var logPath = Path.Combine(Path.GetTempPath(), "HighTemperatureAlerts.log");
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}: {temperature:F1}°C{Environment.NewLine}";
            File.AppendAllText(logPath, logMessage);
            
            // Também registra no log principal do sistema
            var systemLogPath = Path.Combine(Path.GetTempPath(), "SystemMonitor.log");
            File.AppendAllText(systemLogPath, logMessage);
        }
        catch (Exception ex)
        {
            // Se não conseguir escrever no log, pelo menos tenta mostrar no console
            Console.WriteLine($"Erro ao registrar evento de temperatura: {ex.Message}");
        }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        NotifyIcon trayIcon = new NotifyIcon();
        trayIcon.Icon = SystemIcons.Information;
        trayIcon.Visible = true;
        trayIcon.Text = "Inicializando monitor do sistema...";

        // Menu de contexto para o ícone da bandeja
        var contextMenu = new ContextMenuStrip();
        var exitItem = new ToolStripMenuItem("Sair");
        exitItem.Click += (s, e) => Application.Exit();
        contextMenu.Items.Add(exitItem);
        
        var showInfoItem = new ToolStripMenuItem("Mostrar Log");
        showInfoItem.Click += (s, e) => {
            var logPath = Path.Combine(Path.GetTempPath(), "SystemMonitor.log");
            if (File.Exists(logPath))
            {
                Process.Start("notepad.exe", logPath);
            }
        };
        contextMenu.Items.Add(showInfoItem);
        
        var showAlertsItem = new ToolStripMenuItem("Ver Alertas de Temperatura");
        showAlertsItem.Click += (s, e) => {
            var alertLogPath = Path.Combine(Path.GetTempPath(), "HighTemperatureAlerts.log");
            if (File.Exists(alertLogPath))
            {
                Process.Start("notepad.exe", alertLogPath);
            }
            else
            {
                MessageBox.Show("Nenhum alerta de temperatura registrado ainda.", "Alertas de Temperatura", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        };
        contextMenu.Items.Add(showAlertsItem);
        
        trayIcon.ContextMenuStrip = contextMenu;

        SystemMonitor? monitor = null;
        
        try
        {
            monitor = new SystemMonitor();
            
            System.Threading.Timer timer = new System.Threading.Timer(_ =>
            {
                try
                {
                    var info = monitor?.GetSystemInfo();
                    if (info != null)
                    {
                        string tooltip = "";
                        
                        if (info.Temperature.HasValue)
                        {
                            tooltip += $"CPU: {info.Temperature.Value:F1}°C";
                        }
                        else
                        {
                            tooltip += "CPU: N/A";
                        }
                        
                        if (info.MemoryUsed.HasValue && info.MemoryTotal.HasValue)
                        {
                            tooltip += $" | RAM: {info.MemoryUsed.Value:F1}GB/{info.MemoryTotal.Value:F1}GB";
                            if (info.MemoryUsagePercent.HasValue)
                            {
                                tooltip += $" ({info.MemoryUsagePercent.Value:F0}%)";
                            }
                        }
                        else
                        {
                            tooltip += " | RAM: N/A";
                        }
                        
                        if (info.MaxClock.HasValue)
                        {
                            tooltip += $" | Clock: {info.MaxClock.Value:F0}MHz";
                        }
                        else
                        {
                            tooltip += " | Clock: N/A";
                        }
                        
                        // O tooltip tem limite de 63 caracteres
                        if (tooltip.Length > 63)
                        {
                            // Versão mais compacta
                            tooltip = "";
                            if (info.Temperature.HasValue)
                            {
                                tooltip += $"{info.Temperature.Value:F0}°C";
                            }
                            if (info.MemoryUsagePercent.HasValue)
                            {
                                tooltip += $"|{info.MemoryUsagePercent.Value:F0}%RAM";
                            }
                            if (info.MaxClock.HasValue)
                            {
                                tooltip += $"|{info.MaxClock.Value:F0}MHz";
                            }
                        }
                        
                        trayIcon.Text = tooltip;
                        
                        // Muda a cor do ícone baseado na temperatura e verifica alertas
                        if (info.Temperature.HasValue)
                        {
                            var temp = info.Temperature.Value;
                            
                            if (temp > 80)
                            {
                                trayIcon.Icon = SystemIcons.Error;
                                // Dispara o alerta de temperatura alta
                                ShowHighTemperatureAlert(temp, trayIcon);
                            }
                            else if (temp > 70)
                            {
                                trayIcon.Icon = SystemIcons.Warning;
                                // Reset o flag de alerta se a temperatura baixou
                                if (_highTempAlertShown && temp <= 75)
                                {
                                    _highTempAlertShown = false;
                                    LogHighTemperatureEvent(temp, "Temperatura normalizou");
                                }
                            }
                            else
                            {
                                trayIcon.Icon = SystemIcons.Information;
                                // Reset o flag de alerta se a temperatura baixou
                                if (_highTempAlertShown)
                                {
                                    _highTempAlertShown = false;
                                    LogHighTemperatureEvent(temp, "Temperatura normalizou");
                                }
                            }
                        }
                        else
                        {
                            trayIcon.Icon = SystemIcons.Question;
                        }
                    }
                    else
                    {
                        trayIcon.Text = "Erro ao obter informações do sistema";
                        trayIcon.Icon = SystemIcons.Error;
                    }
                }
                catch (Exception ex)
                {
                    trayIcon.Text = $"Erro: {ex.Message}";
                    trayIcon.Icon = SystemIcons.Error;
                }
            }, null, 0, 2000); // Atualiza a cada 2 segundos

            Application.ApplicationExit += (s, e) =>
            {
                timer?.Dispose();
                monitor?.Dispose();
                trayIcon?.Dispose();
            };

            Application.Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao inicializar o monitor do sistema:\n{ex.Message}", 
                          "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            monitor?.Dispose();
        }
    }
}