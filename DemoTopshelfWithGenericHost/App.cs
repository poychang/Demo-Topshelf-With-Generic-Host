using Microsoft.Extensions.Options;
using System;
using DemoTopshelfWithGenericHost.Models;

namespace DemoTopshelfWithGenericHost
{
    /// <summary>
    /// 主要邏輯程式
    /// </summary>
    public class App
    {
        private readonly AppSettings _appSettings;

        public App(IOptions<AppSettings> config)
        {
            _appSettings = config.Value;
        }

        /// <summary>
        /// 執行主要邏輯程式
        /// </summary>
        public void Start()
        {
            Console.WriteLine($"{DateTime.Now} Hello World, {_appSettings.AppName}");
        }

        /// <summary>
        /// 停止主要邏輯程式後的動作
        /// </summary>
        public void Stop()
        {
            Console.WriteLine($"Bye, {_appSettings.AppName}");
        }
    }
}
