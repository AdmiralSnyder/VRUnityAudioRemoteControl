using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NugetLib;

namespace NugetDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Packages Packages;
        private void Button1_Click(object sender, EventArgs e)
        {
            var downloader = new Downloader() { BaseFolder = @"C:\nuget signalr"};
            Packages = Packages ?? new Packages();
            Packages.GetDependencies(downloader, new PackageDependency() { ID = textBox1.Text, Version = textBox2.Text });
        }
    }
}
