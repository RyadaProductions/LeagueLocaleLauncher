﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static LeagueLocaleLauncher.Translation;

namespace LeagueLocaleLauncher
{
    public partial class LeagueLocaleLauncher : Form
    {
        private readonly Config _loadedConfig;
        
        private const string _registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Riot Games, Inc\League of Legends";
        private const string _registryName = "Location";

        public LeagueLocaleLauncher()
        {
            InitializeComponent();

            _loadedConfig = Config.Load();
            
            // TODO: Clean this up
            if (!File.Exists(_loadedConfig.LeagueClientPath))
            {
                // League install path is invalid, set new path.
                // Check registry for league install location
                _loadedConfig.LeagueBasePath = Registry.GetValue(_registryKey, _registryName, _loadedConfig.LeagueClientPath) as string;

                if (!File.Exists(_loadedConfig.LeagueClientPath))
                {
                    // League registry location is still invalid, manually setting path
                    using (var folderBrowserDialog = new FolderBrowserDialog())
                    {
                        var result = folderBrowserDialog.ShowDialog();

                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                        {
                            var baseDirectory = folderBrowserDialog.SelectedPath;
                            var files = Directory.GetFiles(baseDirectory);
                            if (files.Select(x => Path.GetFileName(x)).Contains(_loadedConfig.LeagueClientExecutable))
                            {
                                _loadedConfig.LeagueBasePath = baseDirectory;
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
          
            CultureInfo.CurrentCulture = new CultureInfo(_loadedConfig.ToolCulture);
            CultureInfo.CurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            RegionLabel.Text = Translate(REGION);
            foreach (var region in Enum.GetNames(typeof(Region)))
                RegionComboBox.Items.Add(new ComboBoxItem(region));
            RegionComboBox.SelectedIndex = (int)_loadedConfig.Region;

            LanguageLabel.Text = Translate(LANGUAGE);
            foreach (var language in Enum.GetNames(typeof(Language)))
                LanguageComboBox.Items.Add(new ComboBoxItem(language));
            LanguageComboBox.SelectedIndex = (int)_loadedConfig.Language;

            new ToolTip().SetToolTip(MinimizeButton, Translate(MINIMIZE_TT));
            new ToolTip().SetToolTip(CloseButton, Translate(CLOSE_TT));
            new ToolTip().SetToolTip(RegionLabel, Translate(REGION_TT));
            new ToolTip().SetToolTip(RegionComboBox, Translate(REGION_TT));
            new ToolTip().SetToolTip(LanguageLabel, Translate(LANGUAGE_TT));
            new ToolTip().SetToolTip(LanguageComboBox, Translate(LANGUAGE_TT));
            new ToolTip().SetToolTip(LaunchButton, Translate(LAUNCH_TT));
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
            WindowState = FormWindowState.Minimized;
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
                _loadedConfig.Region = region;
                _loadedConfig.Save();
            }
        }

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var languageString = ((ComboBoxItem)LanguageComboBox.SelectedItem).Value;
            var language = (Language)Enum.Parse(typeof(Language), languageString);
            _loadedConfig.Language = language;
            _loadedConfig.Save();
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            Hide();

            LeagueClientSettings.SetRegion(_loadedConfig, _loadedConfig.Region.ToString());
            
            foreach (var leagueProcessName in _loadedConfig.LeagueProcessNames)
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.ToLower().Contains(leagueProcessName.ToLower()))
                        process.Kill();

            var league = new Process();
            league.StartInfo.FileName = _loadedConfig.LeagueClientPath;
            league.StartInfo.Arguments = $" --locale={Enumerations.Languages[_loadedConfig.Language]}";
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
