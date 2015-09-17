﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using LMDb.Properties;
using MetroFramework;
using MetroFramework.Components;
using MetroFramework.Controls;
using MetroFramework.Forms;

namespace LMDb
{
    public partial class MainForm :MetroForm
    {
        private int _height { get; set; }
        private int _colCount { get; set; }

        public MainForm()
        {
            InitializeComponent();

            int i = -1;
            foreach (MetroThemeStyle e in Types.GetEnumList<MetroThemeStyle>())
            {
                if (e.ToString().ToLower() != "default")
                {
                    mcTheme.Items.Add(e);
                    i++;
                    if (Settings.Default.Theme.Equals(e))
                    {
                        mcTheme.SelectedIndex = i;
                    }
                }
            }

            i = -1;
            foreach (MetroColorStyle e in Types.GetEnumList<MetroColorStyle>())
            {
                if (e.ToString().ToLower() != "default")
                {
                    mcStyle.Items.Add(e);
                    i++;
                    if (Settings.Default.Style.Equals(e))
                    {
                        mcStyle.SelectedIndex = i;
                    }
                }
            }

            tlOverview.ColumnStyles.RemoveAt(0);
            tlOverview.ColumnCount = 0;
            Settings.Default.SearchableExtensions = Settings.Default.SearchableExtensions ?? new StringCollection();
            if (Settings.Default.SearchableExtensions.Count == 0)
            {
                Settings.Default.SearchableExtensions.AddRange(Types.GetEnumStrings<Types.Extensions>().ToArray());
                Settings.Default.Save();
            }

            scanningBackgroundWorker.RunWorkerAsync();
            resizeTimer.Start();
            
        }

        private void resizeTimer_Tick(object sender, EventArgs e)
        {
            if (_height != tlOverview.Height || _colCount != tlOverview.ColumnStyles.Count)
            {
                _colCount = tlOverview.ColumnStyles.Count;
                _height = tlOverview.Height;
                int posterCount = tlOverview.Controls.OfType<PosterCard>().Count();
                int width = (int)Math.Round((((tlOverview.Height - 10) * (0.675)) * posterCount) + (posterCount * 10));
                tlOverview.Width = width;
            }
        }

        private void scanningBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                vcHome.SelectTab(0);
            }));


            ProgressState progress = new ProgressState();
            progress.SetText(Resources.Loading_Library_CheckChanges);
            scanningBackgroundWorker.ReportProgress(progress.Value, progress);

            Thread.Sleep(1250);

            List<string> files = new List<string>();
            foreach (string file in FileSystem.GetFileTree(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), scanningBackgroundWorker, progress))
            {
                files.Add(file);
            }

            progress.IncrementValue(35).SetText(Resources.Loading_Library_Update_Database).EmptySubText();
            scanningBackgroundWorker.ReportProgress(progress.Value, progress);

            Thread.Sleep(1250);

            progress.IncrementValue(15).SetText(Resources.Loading_Library_Prepare_Homepage).EmptySubText();
            scanningBackgroundWorker.ReportProgress(progress.Value, progress);

            Thread.Sleep(1250);

            int i = 0;
            //files = files.GetRange(0, 24);
            float perc = 100 / (float)files.Count();
            int percPerFile = (int)Math.Floor(((float)files.Count() / 50F) * 100);
            foreach (string file in files)
            {
                progress.IncrementValue(percPerFile).SetSubText("(" + i + "/" + files.Count() + " - " + file + ")");
                scanningBackgroundWorker.ReportProgress(progress.Value, progress);
                PosterCard pc = new PosterCard();
                pc.Text = file;
                pc.Dock = DockStyle.Fill;
                this.Invoke(new Action(() =>
                {
                    tlOverview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, perc));
                    tlOverview.Controls.Add(pc, i, 0);
                }));
                i++;
            }

            progress.SetValue(100).SetText(Resources.Loading_Library_Complete).EmptySubText();
            scanningBackgroundWorker.ReportProgress(progress.Value, progress);

            Thread.Sleep(1250);

            this.Invoke(new Action(() =>
            {
                vcHome.SelectTab(1);
            }));

            progress.ResetValue().EmptyText().EmptySubText();
            scanningBackgroundWorker.ReportProgress(progress.Value, progress);
            
        }

        private void scanningBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ldHome.Value = e.ProgressPercentage;
            if (e.UserState is ProgressState)
            { 
                ProgressState progress = (ProgressState) e.UserState;
                ldHome.Text = progress.Text;
                ldHome.SubText = progress.SubText;
            }
        }

        private void NavigationClicked(object sender, EventArgs e)
        {
            MetroLink label = sender as MetroLink;
            if (label != null)
            {
                int val;
                if (label.Tag != null && int.TryParse(label.Tag.ToString(), out val))
                {
                    vcSettings.SelectTab(val);
                }
            }
        }

        private void mrTheme_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void mcTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            var theme = mcTheme.SelectedItem;
            if (theme is MetroThemeStyle)
            {
                MetroStyleManager.Default.Theme = (MetroThemeStyle) theme;
                Settings.Default.Theme = (MetroThemeStyle) theme;
                Settings.Default.Save();
            }
        }

        private void mcStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            var style = mcStyle.SelectedItem;
            if (style is MetroColorStyle)
            {
                MetroStyleManager.Default.Style = (MetroColorStyle) style;
                Settings.Default.Style = (MetroColorStyle) style;
                Settings.Default.Save();
            }
        }

        private void mbChangeImage_Click(object sender, EventArgs e)
        {

        }
    }
}
