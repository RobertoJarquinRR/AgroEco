using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows;

namespace AgroEco.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);

            webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

            string webRootFolder = Path.Combine(AppContext.BaseDirectory, "Frontend");

            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.AgroEco",
                webRootFolder,
                CoreWebView2HostResourceAccessKind.Allow
            );

#if DEBUG
            webView.Source = new Uri("http://localhost:5173/");
#else
            webView.Source = new Uri("http://app.AgroEco/index.html");
#endif



        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json = e.WebMessageAsJson;
            
        }
    }
}