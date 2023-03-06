using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using Renci.SshNet;
using SuperPasses.Helpers;
using SuperPasses.Log;
using SuperPasses.Models;

namespace SuperPasses.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly string _dataPath = "";
    private readonly SimpleLog _simpleLog;
    private SshClient? _sshClient;
    public MainWindowViewModel()
    {
        _dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _dataPath = Path.Join(_dataPath, "SuperPasses");
        if (!Directory.Exists(_dataPath))
            Directory.CreateDirectory(_dataPath);

        _simpleLog = new SimpleLog(Path.Combine(_dataPath, "log.txt"));
        LoadConfig();
    }
    private string _routerIp = "192.168.101.1";
    public string RouterIp
    {
        get => _routerIp;
        set => this.RaiseAndSetIfChanged(ref _routerIp, value);
    }

    private string _routerPort = "22";

    public string RouterPort
    {
        get => _routerPort;
        set => this.RaiseAndSetIfChanged(ref _routerPort, value);
    }

    private string _routerAccount = "root";

    public string RouterAccount
    {
        get => _routerAccount;
        set => this.RaiseAndSetIfChanged(ref _routerAccount, value);
    }

    private string _routerPassword = "";

    public string RouterPassword
    {
        get => _routerPassword;
        set => this.RaiseAndSetIfChanged(ref _routerPassword, value);
    }

    private string _sshStatus = "";

    public string SshStatus
    {
        get => _sshStatus;
        set => this.RaiseAndSetIfChanged(ref _sshStatus, value);
    }

    private string _interfaceName = "wan";

    public string InterfaceName
    {
        get => _interfaceName;
        set => this.RaiseAndSetIfChanged(ref _interfaceName, value);
    }

    private string _deviceName = "eth0.2";

    public string DeviceName
    {
        get => _deviceName;
        set => this.RaiseAndSetIfChanged(ref _deviceName, value);
    }

    private string _checkNetStatus = "";

    public string CheckNetStatus
    {
        get => _checkNetStatus;
        set => this.RaiseAndSetIfChanged(ref _checkNetStatus, value);
    }

    private string _schoolAccount = "";

    public string SchoolAccount
    {
        get => _schoolAccount;
        set => this.RaiseAndSetIfChanged(ref _schoolAccount, value);
    }

    private string _schoolPassword = "";
    public string SchoolPassword
    {
        get => _schoolPassword;
        set => this.RaiseAndSetIfChanged(ref _schoolPassword, value);
    }

    private string _loginStatus = "";

    public string LoginStatus
    {
        get => _loginStatus;
        set => this.RaiseAndSetIfChanged(ref _loginStatus, value);
    }

    private void LoadConfig()
    {
        try
        {
            var cfg = Path.Combine(_dataPath, "config.json");
            if (!File.Exists(cfg)) return;
            
            var jsonStr = "";
            using var r = new StreamReader(cfg);
            jsonStr = r.ReadToEnd();
            if (string.IsNullOrEmpty(jsonStr)) return;
                
            var ret = JsonSerializer.Deserialize<Models.Config>(jsonStr);
                
            if (ret == null) return;
            RouterIp = ret.RouterIp;
            RouterPort = ret.RouterPort;
            RouterAccount = ret.RouterAccount;
            RouterPassword = ret.RouterPassword;
            SchoolAccount = ret.SchoolAccount;
            SchoolPassword = ret.SchoolPassword;
            InterfaceName = ret.InterfaceName;
            DeviceName = ret.DeviceName;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Close()
    {
        SaveConfig();
        
        if (_sshClient is {IsConnected: true})
            _sshClient.Dispose();
    }

    public void SaveConfig()
    {
        try
        {
            var cfg = new Config
            {
                RouterIp = RouterIp, RouterPort = RouterPort,
                RouterAccount = RouterAccount, RouterPassword = RouterPassword,
                SchoolAccount = SchoolAccount, SchoolPassword = SchoolPassword,
                DeviceName = DeviceName, InterfaceName = InterfaceName
            };
            
            var cfgPath = Path.Combine(_dataPath, "config.json");
            var jsonStr = JsonSerializer.Serialize(cfg);
            using var sw = new StreamWriter(cfgPath);
            sw.Write(jsonStr);
        }
        catch (Exception e)
        {
            Log(e.Message);
            Console.WriteLine(e);
        }
    }

    private bool _sshEnable = true;

    public bool SSHEnable
    {
        get => _sshEnable;
        set => this.RaiseAndSetIfChanged(ref _sshEnable, value);
    }

    public async void ConnectSSH()
    {
        try
        {
            SSHEnable = false;
            SshStatus = "";
            await Task.Run(() =>
            {
                try
                {
                    _sshClient?.Dispose();

                    if (string.IsNullOrEmpty(RouterIp)
                        || string.IsNullOrEmpty(RouterPort)
                        || string.IsNullOrEmpty(RouterAccount)
                        || string.IsNullOrEmpty(RouterPassword))
                    {
                        SshStatus = "输入有效的ip地址、端口、账号及密码";
                        Log($"connect info error,RouterIp={RouterIp}, RouterPort={RouterPort}, RouterAccount={RouterAccount}, RouterPassword={RouterPassword}");
                        return;
                    }
                    
                    _sshClient = new SshClient(RouterIp, int.Parse(RouterPort), RouterAccount, RouterPassword);
                    _sshClient.Connect();

                    using var cmd = _sshClient.CreateCommand("pwd");
                    var ret = cmd.Execute();
                    SshStatus = ret.Replace("\n", "") + "   |  ";
                }
                catch (Exception e)
                {
                    SshStatus = e.Message;
                    Log(e.Message);
                    Console.WriteLine(e);
                }
                
                SshStatus += $"{RouterIp} connect success!";
                Log(SshStatus);
            });
        }
        catch (Exception e)
        {
            Log(e.Message);
            Console.WriteLine(e);
        }
        finally
        {
            SSHEnable = true;
        }
    }

    private void Log(string log)
    {
        _simpleLog.Write(log);
    }

    public void OpenRouterWebPage()
    {
        var url = @$"http://{RouterIp}";
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                Log("unknown OSPlatform");
            }
        }
    }

    private string _mac { get; set; } = "2C22812151004";
    private string _ip { get; set; } = "125.218.228.67";

    public void CheckRouterNetWork()
    {
        CheckNetStatus = "";
        _mac = "";
        _ip = "";
        Task.Run(() =>
        {
            try
            {
                if (_sshClient == null) return;
            
                if (!_sshClient.IsConnected)
                {
                    CheckNetStatus = "ssh connect error!";
                    return;
                }

                using var cmd1 = _sshClient.CreateCommand(BashHelper.GetMacAddress(DeviceName));
                var mac = cmd1.Execute();
                mac = mac.Replace("\n", "").Replace(":", "");
                CheckNetStatus += $"mac address is:{mac}";
                //ip address
                using var cmd2 = _sshClient.CreateCommand(BashHelper.GetIpAddress(InterfaceName));
                var ip = cmd2.Execute();
                ip = ip.Replace("\n", "").Replace(":", "");
                CheckNetStatus += $" | ip address is:{ip}";

                _mac = mac;
                _ip = ip;
            }
            catch (Exception e)
            {
                Log(e.Message);
                CheckNetStatus = e.Message;
                Console.WriteLine(e);
            }
        });
    }

    public void LoginSchoolNet()
    {
        LoginStatus = "";
        Task.Run(() =>
        {
            try
            {
                if (_sshClient == null) return;
            
                if (!_sshClient.IsConnected)
                {
                    LoginStatus = "ssh connect error!";
                    return;
                }
                var reqStr = new GDUFETemplate().GetCertificationRequestShort(SchoolAccount, SchoolPassword, _mac, _ip);
                reqStr = "curl " + reqStr + " --connect-timeout 5";

                // if (reqStr.Length > 500)
                // {
                //     var bashStr = @$"$({reqStr})";
                //     var l = BashHelper.GetOpenWrtBashFileStringList(bashStr);
                //     var path = Path.Combine(_dataPath, "command.sh");
                //     if (File.Exists(path))
                //     {
                //         File.Delete(path);
                //         Thread.Sleep(100);
                //     }
                //
                //     var f = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, false);
                //     f.Dispose();
                //     Thread.Sleep(100);
                //     File.AppendAllLines(path, l);
                // }

                Log($"命令长度 = {reqStr.Length}");
                Log($"request string is : {reqStr}");
                
                using var cmd = _sshClient.CreateCommand(reqStr);
                var ret = cmd.Execute();
                if (string.IsNullOrEmpty(ret))
                    ret = "stdout 没有输出内容！！";
                LoginStatus = $"Login Result is : {ret}";
                
                Log(LoginStatus);
            }
            catch (Exception e)
            {
                Log(e.Message);
                LoginStatus = e.Message;
                Console.WriteLine(e);
            }
        });
    }
}