﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ClipboardHistory.View;
using ClipboardHistory.ViewModel;

namespace ClipboardHistory
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow window = new MainWindow();
            MainViewModel viewModel = new MainViewModel(window);
            window.DataContext = viewModel;
            window.Show();
        }
    }
}
