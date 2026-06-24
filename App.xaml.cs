using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SQLitePCL;

namespace SultanStore
{

    public partial class App : Application
    {
        public App()
        {
            Batteries.Init(); // 🔥 ключевая строка
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new LoginWindow().Show();
        }
    }
}