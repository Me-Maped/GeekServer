﻿using Geek.Server.App.Common;
using Geek.Server.Core.Actors.Impl;
using Geek.Server.Core.Utils;
using NLog;
using NLog.Fluent;
using System.Diagnostics;
using System.Text;

namespace Geek.Server.App
{
    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static volatile bool ExitCalled = false;
        private static volatile Task GameLoopTask = null;
        private static volatile Task ShutDownTask = null;

        static async Task Main(string[] args)
        {
            try
            {
                var configName = args.Length > 0 ? args[0] : "app_config.json";
                Console.WriteLine("配置名字：" + configName);

                AppExitHandler.Init(HandleExit);
                GameLoopTask = AppStartUp.Enter(configName);
                await GameLoopTask;
                if (ShutDownTask != null)
                    await ShutDownTask;
            }
            catch (Exception e)
            {
                string error;
                if (Settings.AppRunning)
                {
                    error = $"服务器运行时异常 e:{e}";
                    Console.WriteLine(error);
                    ExceptionMonitor.Report(ExceptionType.UnhandledException, $"{e}").Wait(TimeSpan.FromSeconds(10));
                }
                else
                {
                    error = $"启动服务器失败 e:{e}";
                    Console.WriteLine(error);
                    ExceptionMonitor.Report(ExceptionType.UnhandledException, $"{e}").Wait(TimeSpan.FromSeconds(10));
                }
                File.WriteAllText("server_error.txt", $"{error}", Encoding.UTF8);
            }
        }

        private static void HandleExit()
        {
            if (ExitCalled)
                return;
            ExitCalled = true;
            Log.Info($"监听到退出程序消息");
            ShutDownTask = Task.Run(() =>
            {
                Settings.AppRunning = false;
                GameLoopTask?.Wait();
                LogManager.Shutdown();
                Console.WriteLine($"退出程序");
                Process.GetCurrentProcess().Kill();
            });
            ShutDownTask.Wait();
        }
    }
}