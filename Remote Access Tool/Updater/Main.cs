﻿using EagleMonitor.Controls;
using Leaf.xNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using static Updater.Utils;

/* 
|| AUTHOR Arsium ||
|| github : https://github.com/arsium       ||
*/

namespace Updater
{
    public partial class Main : FormPattern
    {
        public Main()
        {
            InitializeComponent();
        }

        private WebClient updaterWebClient;
        private string newVersion;
        private void Main_Load(object sender, EventArgs e)
        {
            using (HttpRequest httpRequest = new HttpRequest())
            {
                httpRequest.IgnoreProtocolErrors = true;
                httpRequest.UserAgent = Http.ChromeUserAgent();
                httpRequest.ConnectTimeout = 30000;
                string request = httpRequest.Get("https://api.github.com/repos/arsium/EagleMonitorRAT/releases").ToString();

                List<GitHubAPI> json = JsonConvert.DeserializeObject<List<GitHubAPI>>(request);

                int[] githubVersion = Utils.VersionSplitter(json[0].tag_name);

                int[] localVersion = Utils.VersionSplitter(CoreAssembly.Version.ToString());

                if (githubVersion[0] == localVersion[0]
                    && githubVersion[1] == localVersion[1]
                    && githubVersion[2] == localVersion[2]
                    && githubVersion[3] == localVersion[3]
                    )
                {
                    MessageBox.Show("You're updated !");
                    Environment.Exit(0);
                }
                else if (githubVersion[0] >= localVersion[0]
                    && githubVersion[1] >= localVersion[1]
                    && githubVersion[2] >= localVersion[2]
                    && githubVersion[3] >= localVersion[3]
                    )
                {
                    newVersion = json[0].tag_name;
                    MessageBox.Show($"A new update is available ! {json[0].tag_name}");
                }
                else
                {
                    MessageBox.Show($"You use an unknown version !");
                    Environment.Exit(0);
                }
            }
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            updaterWebClient = new WebClient();
            updaterWebClient.DownloadProgressChanged += updaterWebClient_DownloadProgressChanged;
            string url = $"https://github.com/arsium/EagleMonitorRAT/releases/download/{newVersion}/Eagle.Monitor.RAT.Reborn.Installer.exe";
            updaterWebClient.DownloadFileAsync(new Uri(url), System.IO.Path.GetTempPath() + "\\Eagle.Monitor.RAT.Reborn.Installer.exe", false);
        }

        private void updaterWebClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e) 
        {
            guna2ProgressBar1.Value = e.ProgressPercentage;
            label1.Text = $"Progress: {e.ProgressPercentage} % {e.BytesReceived} / {e.TotalBytesToReceive}";

            if (guna2ProgressBar1.Value == 100)
            { Process.Start(System.IO.Path.GetTempPath() + "\\Eagle.Monitor.RAT.Reborn.Installer.exe"); Environment.Exit(0); }
        }
    }
}
