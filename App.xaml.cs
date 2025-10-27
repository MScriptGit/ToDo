using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace ToDo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
            //var window = new Window(new MainPage());
            //Application.Current.OpenWindow(window);

        }
    }
}