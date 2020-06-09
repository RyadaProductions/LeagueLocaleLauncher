﻿using Microsoft.Win32;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LeagueLocaleLauncher
{
    public partial class LeagueLocaleLauncher : Form
    {
        private const string _registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Riot Games, Inc\League of Legends";
        private const string _registryName = "Location";

        public LeagueLocaleLauncher()
        {
            InitializeComponent();

            Config.Load();

            // TODO: Clean this up
            if (!File.Exists(Config.Loaded.LeagueClientPath))
            {
                // League install path is invalid, set new path.
                // Check registry for league install location
                if (Registry.GetValue(_registryKey, _registryName, Config.Loaded.LeagueClientPath) is string registryValue)
                {
                    Config.Loaded.LeagueBasePath = registryValue;
                }

                if (!File.Exists(Config.Loaded.LeagueClientPath))
                {
                    // League registry location is still invalid, manually setting path
                    using (var folderBrowserDialog = new FolderBrowserDialog())
                    {
                        var result = folderBrowserDialog.ShowDialog();

                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                        {
                            var baseDirectory = folderBrowserDialog.SelectedPath;
                            var files = Directory.GetFiles(baseDirectory);
                            if (files.Select(x => Path.GetFileName(x)).Contains(Config.Loaded.LeagueClientExecutable))
                            {
                                Config.Loaded.LeagueBasePath = baseDirectory;
                            }
                            else
                            {
                                MessageBox.Show("Closing application due to no valid location being selected.");
                                Close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Closing application due to no valid location being selected.");
                            Close();
                        }
                    }
                }


            }

            CultureInfo.CurrentCulture = new CultureInfo(Config.Loaded.ToolCulture);
            CultureInfo.CurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            RegionLabel.Text = Translation.Translate(Translation.REGION);
            foreach (var region in Enum.GetNames(typeof(Region)))
                RegionComboBox.Items.Add(new ComboBoxItem(region));
            RegionComboBox.SelectedIndex = (int)Config.Loaded.Region;

            LanguageLabel.Text = Translation.Translate(Translation.LANGUAGE);
            foreach (var language in Enum.GetNames(typeof(Language)))
                LanguageComboBox.Items.Add(new ComboBoxItem(language));
            LanguageComboBox.SelectedIndex = (int)Config.Loaded.Language;

            new ToolTip().SetToolTip(MinimizeButton, Translation.Translate(Translation.MINIMIZE_TT));
            new ToolTip().SetToolTip(CloseButton, Translation.Translate(Translation.CLOSE_TT));
            new ToolTip().SetToolTip(RegionLabel, Translation.Translate(Translation.REGION_TT));
            new ToolTip().SetToolTip(RegionComboBox, Translation.Translate(Translation.REGION_TT));
            new ToolTip().SetToolTip(LanguageLabel, Translation.Translate(Translation.LANGUAGE_TT));
            new ToolTip().SetToolTip(LanguageComboBox, Translation.Translate(Translation.LANGUAGE_TT));
            new ToolTip().SetToolTip(LaunchButton, Translation.Translate(Translation.LAUNCH_TT));
        }

        private void CloseButton_MouseEnter(object sender, EventArgs e)
        {
            CloseButton.BackgroundImage = Properties.Resources.close_button_hover;
            CloseButton.Refresh();
        }

        private void CloseButton_MouseLeave(object sender, EventArgs e)
        {
            CloseButton.BackgroundImage = Properties.Resources.close_button;
            CloseButton.Refresh();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MinimizeButton_MouseEnter(object sender, EventArgs e)
        {
            MinimizeButton.BackgroundImage = Properties.Resources.minimize_button_hover;
            MinimizeButton.Refresh();
        }

        private void MinimizeButton_MouseLeave(object sender, EventArgs e)
        {
            MinimizeButton.BackgroundImage = Properties.Resources.minimize_button;
            MinimizeButton.Refresh();
        }
        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void LaunchButton_MouseEnter(object sender, EventArgs e)
        {
            LaunchButton.BackgroundImage = Properties.Resources.launch_button_hover;
            LaunchButton.Refresh();
        }

        private void LaunchButton_MouseLeave(object sender, EventArgs e)
        {
            LaunchButton.BackgroundImage = Properties.Resources.launch_button;
            LaunchButton.Refresh();
        }


        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void TitleBarPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void LogoPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            TitleBarPanel_MouseDown(sender, e);
        }

        static int PreviousRegionComboBoxIndex = 0;
        private void RegionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = RegionComboBox.SelectedIndex;

            if (index == (int)global::LeagueLocaleLauncher.Region.KR || index == (int)global::LeagueLocaleLauncher.Region.PBE)
                RegionComboBox.SelectedIndex = PreviousRegionComboBoxIndex;
            else
            {
                PreviousRegionComboBoxIndex = RegionComboBox.SelectedIndex;
                var regionString = ((ComboBoxItem)RegionComboBox.SelectedItem).Value;
                var region = (Region)Enum.Parse(typeof(Region), regionString);
                Config.Loaded.Region = region;
                Config.Loaded.Save();
            }
        }

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var languageString = ((ComboBoxItem)LanguageComboBox.SelectedItem).Value;
            var language = (Language)Enum.Parse(typeof(Language), languageString);
            Config.Loaded.Language = language;
            Config.Loaded.Save();
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();

            LeagueClientSettings.SetRegion(Config.Loaded.Region.ToString());

            foreach (var leagueProcessName in Config.Loaded.LeagueProcessNames)
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.ToLower().Contains(leagueProcessName.ToLower()))
                        process.Kill();

            var league = new Process();
            league.StartInfo.FileName = Config.Loaded.LeagueClientPath;
            league.StartInfo.Arguments = $" --locale={Enumerations.Languages[Config.Loaded.Language]}";
            league.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            league.Start();
            Application.Exit();
        }

        private void RegionComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (e.Index >= 0)
            {
                if (e.Index == (int)global::LeagueLocaleLauncher.Region.KR || e.Index == (int)global::LeagueLocaleLauncher.Region.PBE)
                {
                    //e.Graphics.FillRectangle(System.Drawing.Brushes.LightSlateGray, e.Bounds);
                    e.Graphics.DrawString(comboBox.Items[e.Index].ToString(), comboBox.Font, System.Drawing.Brushes.IndianRed, e.Bounds);
                }
                else
                {
                    e.DrawBackground();

                    // Set the brush according to whether the item is selected or not
                    var brush = ((e.State & DrawItemState.Selected) > 0) ? System.Drawing.SystemBrushes.HighlightText : System.Drawing.Brushes.White;

                    e.Graphics.DrawString(comboBox.Items[e.Index].ToString(), comboBox.Font, brush, e.Bounds);
                    e.DrawFocusRectangle();
                }
            }
        }

        private void LanguageComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (e.Index >= 0)
            {
                e.DrawBackground();

                // Set the brush according to whether the item is selected or not
                var brush = ((e.State & DrawItemState.Selected) > 0) ? System.Drawing.SystemBrushes.HighlightText : System.Drawing.Brushes.White;

                e.Graphics.DrawString(comboBox.Items[e.Index].ToString(), comboBox.Font, brush, e.Bounds);
                e.DrawFocusRectangle();
            }
        }

        private void RegionComboBox_DropDownClosed(object sender, EventArgs e)
        {
            RegionLabel.Focus();
        }

        private void LanguageComboBox_DropDownClosed(object sender, EventArgs e)
        {
            LanguageLabel.Focus();
        }
    }
}
