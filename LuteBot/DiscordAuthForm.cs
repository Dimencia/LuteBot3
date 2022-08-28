using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot
{
    public partial class DiscordAuthForm : Form
    {
        // https://discord.com/api/oauth2/authorize?response_type=token&client_id=886334900486160415&state=15773059ghq9183habn&scope=identify // Implicit auth link.  The one below is explicit and returns a code
        // Bardlord's API will have to redirect from his redirect, to somewhere that gives me info I can scrape
        private static readonly string AUTH_URL = @"https://discord.com/api/oauth2/authorize?client_id=886334900486160415&redirect_uri=http%3A%2F%2Flocalhost%3A9001&response_type=code&scope=identify";

        public DiscordAuthForm()
        {
            InitializeComponent();

            // Navigate browser to our auth URL
            
            //chromiumWebBrowser1.LoadUrlAsync(AUTH_URL);
            //chromiumWebBrowser1.LoadingStateChanged += ChromiumWebBrowser1_LoadingStateChanged;
            //chromiumWebBrowser1.LoadError += ChromiumWebBrowser1_LoadError;
        }

        //private void ChromiumWebBrowser1_LoadError(object sender, CefSharp.LoadErrorEventArgs e)
        //{
        //    // We should get this on the localhost load
        //    BeginInvoke((MethodInvoker)delegate { textBox1.Text = e.FailedUrl; Refresh(); }); // FailedUrl is correct and we can get the info out of it
        //}
        //
        //private void ChromiumWebBrowser1_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        //{
        //    if (!e.IsLoading)
        //    {
        //        if (chromiumWebBrowser1.Address.ToLower().StartsWith("http://localhost"))
        //        {
        //            // Read info from the url
        //            string url = chromiumWebBrowser1.Address;
        //        }
        //    }
        //    BeginInvoke((MethodInvoker)delegate { textBox1.Text = chromiumWebBrowser1.Address; Refresh(); });
        //}

    }
}
