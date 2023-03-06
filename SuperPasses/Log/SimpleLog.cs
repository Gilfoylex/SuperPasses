using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SuperPasses.Log;

public class SimpleLog
{
    private readonly BlockingCollection<string> _logCollection = new();
    private const int MaxSize = 10 * 1024 * 1024;
    private readonly string _logPath;

    public SimpleLog(string logPath)
    {
        _logPath = logPath;
        Write("-------Welcome To SuperPasses--------");
        Task.Run(Run);
    }

    public void Write(string log)
    {
        var method = new StackFrame(2).GetMethod();
        var className = method?.DeclaringType?.FullName;
        var func = method?.Name;
        var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss-fff");
        var s = $"{time}[{Environment.ProcessId}:{Environment.CurrentManagedThreadId} {className}::{func} {log}]";
        _logCollection.Add(s);
    }

    private void Run()
    {
        try
        {
            if (File.Exists(_logPath))
            {
                var info = new FileInfo(_logPath);
                if (info.Length > MaxSize)
                {
                    File.Delete(_logPath);
                    Thread.Sleep(100);
                    File.Create(_logPath);
                }
            }
            else
            {
                File.Create(_logPath);
            }

            while (_logCollection.TryTake(out var i, Timeout.Infinite))
            {
                File.AppendAllLines(_logPath, new []{i});
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}