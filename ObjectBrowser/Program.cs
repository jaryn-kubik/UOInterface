using System;
using System.Net;
using System.Windows;
using UOInterface;

namespace ObjectBrowser
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            MainWindow window = new MainWindow();

            Client.ServerIP = (uint)IPAddress.Parse("127.0.0.1").Address;
            Client.ServerPort = 2593;
            Client.PatchEncryption = true;
            Client.Start("C:\\UO\\Test\\client.exe");
            window.Title = "ObjectBrowser - Client " + Client.Version;

            new Application().Run(window);
        }
    }
}