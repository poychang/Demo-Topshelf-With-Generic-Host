using DemoTopshelfWithGenericHost.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace DemoTopshelfWithGenericHost
{
    public static class Program
    {
        /// <summary>
        /// 主程式進入點
        /// </summary>
        /// <remarks>
        /// 若有指定環境變數，執行時請使用以下方式傳入 -env:Debug
        /// </remarks>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            Startup.RunWindowsServiceWithHost(host);
        }

        /// <summary>
        /// 建立泛型主機，並處理相關設定值與 DI 服務
        /// </summary>
        /// <remarks>.NET Core 泛型主機 https://docs.microsoft.com/zh-tw/aspnet/core/fundamentals/host/generic-host </remarks>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration((config) =>
                {
                    Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", GetArgumentValue(args, "env"));
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<AppSettings>(hostContext.Configuration);
                    services.AddTransient<App>();
                });

        /// <summary>
        /// 取得啟動時所傳遞的指定參數值
        /// </summary>
        /// <param name="args">啟動時所傳遞的所有參數</param>
        /// <param name="key">指定的參數名稱</param>
        /// <returns>參數值</returns>
        private static string GetArgumentValue(string[] args, string key) =>
            args.Any()
                ? args.FirstOrDefault(arg => arg.StartsWith($"-{key}:"))?.Split(':')[1]
                : string.Empty;
    }
}
