using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using System;
using Topshelf;

namespace DemoTopshelfWithGenericHost
{
    /// <summary>
    /// 啟動應用
    /// </summary>
    public class Startup
    {
        private static IHost Host { get; set; }
        private static string ServiceName { get; set; }

        /// <summary>
        /// 運行 Windows Service 服務
        /// </summary>
        /// <param name="host">泛型主機的實體</param>
        public static void RunWindowsServiceWithHost(IHost host)
        {
            Host = host;
            ServiceName = "DemoService";

            switch (ActivateTopshelf())
            {
                case TopshelfExitCode.Ok:
                    Console.WriteLine($"{ServiceName} status: Ok");
                    break;
                case TopshelfExitCode.ServiceAlreadyInstalled:
                    Console.WriteLine($"{ServiceName} status: ServiceAlreadyInstalled");
                    break;
                case TopshelfExitCode.ServiceAlreadyRunning:
                    Console.WriteLine($"{ServiceName} status: ServiceAlreadyRunning");
                    break;
                case TopshelfExitCode.ServiceNotInstalled:
                    Console.WriteLine($"{ServiceName} status: ServiceNotInstalled");
                    throw new Exception($"{ServiceName} status: ServiceNotInstalled");
                case TopshelfExitCode.ServiceNotRunning:
                    Console.WriteLine($"{ServiceName} status: ServiceNotRunning");
                    throw new Exception($"{ServiceName} status: ServiceNotRunning");
                case TopshelfExitCode.ServiceControlRequestFailed:
                    Console.WriteLine($"{ServiceName} status: ServiceControlRequestFailed");
                    throw new Exception($"{ServiceName} status: ServiceControlRequestFailed");
                case TopshelfExitCode.AbnormalExit:
                    Console.WriteLine($"{ServiceName} status: AbnormalExit");
                    throw new Exception($"{ServiceName} status: AbnormalExit");
                case TopshelfExitCode.SudoRequired:
                    Console.WriteLine($"{ServiceName} status: SudoRequired");
                    throw new Exception($"{ServiceName} status: SudoRequired");
                case TopshelfExitCode.NotRunningOnWindows:
                    Console.WriteLine($"{ServiceName} status: NotRunningOnWindows");
                    throw new Exception($"{ServiceName} status: NotRunningOnWindows");
                default:
                    Console.WriteLine($"{ServiceName} status: Unsupported status...");
                    break;
            }
        }

        /// <summary>
        /// 使用 Topshelf 建立並啟動 Windows Service
        /// </summary>
        /// <returns></returns>
        private static TopshelfExitCode ActivateTopshelf() =>
            HostFactory.Run(configurator =>
            {
                // 設定執行時所傳入的啟動參數
                var env = string.Empty;
                configurator.AddCommandLineDefinition(nameof(env), value => { env = value; });
                configurator.ApplyCommandLine();

                // 設定啟動的主要邏輯程式
                var app = Host.Services.GetRequiredService<App>();
                configurator.Service<App>(settings =>
                {
                    settings.ConstructUsing(() => app);
                    settings.WhenStarted(app => app.Start());
                    settings.BeforeStoppingService(service => { service.Stop(); });
                    settings.WhenStopped(app => { app.Stop(); });
                });

                // 設定啟動 Windows Service 的身分
                configurator.RunAsLocalSystem()
                    .StartAutomaticallyDelayed()
                    .EnableServiceRecovery(rc => rc.RestartService(5));

                // 設定服務名稱及描述
                configurator.SetServiceName($"{ServiceName}");
                configurator.SetDisplayName($"{ServiceName}");
                configurator.SetDescription($"{ServiceName}");

                // 設定發生例外時的處理方式
                configurator.OnException((exception) => { Console.WriteLine(exception.Message); });

                // 安裝之後將啟動時所需要的引數寫入 Windows 註冊表中，讓下次啟動時傳遞同樣的引數
                configurator.AfterInstall(installHostSettings =>
                {
                    using (var system = Registry.LocalMachine.OpenSubKey("System"))
                    using (var currentControlSet = system.OpenSubKey("CurrentControlSet"))
                    using (var services = currentControlSet.OpenSubKey("Services"))
                    using (var service = services.OpenSubKey(installHostSettings.ServiceName, true))
                    {
                        const string REG_KEY_IMAGE_PATH = "ImagePath";
                        var imagePath = service?.GetValue(REG_KEY_IMAGE_PATH);
                        service?.SetValue(REG_KEY_IMAGE_PATH, $"{imagePath} -env:{env}");
                    }
                });
            });
    }
}
