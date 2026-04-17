using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EDGBAPRO_Rom_Manager.Controls;
using EDGBAPRO_Rom_Manager.Models;
using EDGBAPRO_Rom_Manager.Services;

namespace EDGBAPRO_Rom_Manager
{
    public class MainForm : Form
    {
        private readonly Button scanButton;
        private readonly Button importButton;
        private readonly DataGridView gameGrid;
        private readonly StatusStrip statusStrip;
        private readonly ToolStripStatusLabel statusLabel;
        private readonly ToolStripControlHost progressHost;
        private readonly DarkRedProgressBar progressBar;
        private readonly MenuStrip menuStrip;

        private readonly List<RomEntry> currentEntries = new();
        private string? currentRootFolder;

        public MainForm()
        {
            Text = "EDGBAPRO Rom Manager";
            Width = 980;
            Height = 650;
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(24, 24, 24);
            ForeColor = Color.Gainsboro;

            menuStrip = BuildMenu();
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);

            scanButton = MakeButton("Scan", 12, 36);
            scanButton.Click += async (s, e) => await ScanButton_Click();

            importButton = MakeButton("Import Save", 140, 36);
            importButton.Click += ImportButton_Click;

            gameGrid = BuildGrid();

            statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.White,
                SizingGrip = true
            };

            progressBar = new DarkRedProgressBar();
            progressHost = new ToolStripControlHost(progressBar)
            {
                Margin = new Padding(2, 3, 2, 3),
                AutoSize = false,
                Width = 180,
                Height = 18
            };

            statusLabel = new ToolStripStatusLabel("Ready")
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            statusStrip.Items.Add(progressHost);
            statusStrip.Items.Add(statusLabel);

            Controls.Add(scanButton);
            Controls.Add(importButton);
            Controls.Add(gameGrid);
            Controls.Add(statusStrip);

            WindowsThemeHelper.ApplyDarkTitleBar(this);
            WindowsThemeHelper.ApplyDarkExplorerTheme(gameGrid);
            WindowsThemeHelper.ApplyDarkExplorerTheme(statusStrip);
        }

        private Button MakeButton(string text, int left, int top)
        {
            var btn = new Button
            {
                Text = text,
                Left = left,
                Top = top,
                Width = 120,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            return btn;
        }

        private DataGridView BuildGrid()
        {
            var grid = new DataGridView
            {
                Left = 12,
                Top = 80,
                Width = ClientSize.Width - 24,
                Height = ClientSize.Height - 140,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackgroundColor = Color.FromArgb(18, 18, 18),
                GridColor = Color.FromArgb(45, 45, 45),
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(34, 34, 34);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(34, 34, 34);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            grid.DefaultCellStyle.BackColor = Color.FromArgb(18, 18, 18);
            grid.DefaultCellStyle.ForeColor = Color.Gainsboro;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(139, 0, 0);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;

            grid.RowsDefaultCellStyle.BackColor = Color.FromArgb(18, 18, 18);
            grid.RowsDefaultCellStyle.ForeColor = Color.Gainsboro;
            grid.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(139, 0, 0);
            grid.RowsDefaultCellStyle.SelectionForeColor = Color.White;

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RomName",
                HeaderText = "ROM Name",
                FillWeight = 78
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SaveType",
                HeaderText = "Save Type",
                FillWeight = 22
            });

            grid.CellFormatting += GameGrid_CellFormatting;
            return grid;
        }

        private void GameGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1)
                return;

            bool selected = gameGrid.Rows[e.RowIndex].Selected;
            if (selected)
                return;

            string text = Convert.ToString(e.Value) ?? "";
            e.CellStyle.ForeColor = text.ToUpperInvariant() switch
            {
                "SRAM" => Color.FromArgb(90, 220, 120),
                "EEPROM" => Color.FromArgb(90, 170, 255),
                "FLASH" => Color.FromArgb(255, 170, 70),
                _ => Color.Gainsboro
            };
        }

        private MenuStrip BuildMenu()
        {
            var strip = new MenuStrip
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            var help = new ToolStripMenuItem("Help");
            help.Click += (s, e) => ShowInfoWindow("Help",
                "This build uses ROM-content save detection like the working Python version.\n\n" +
                "It scans .gba files, searches ROM bytes for EEPROM_V, SRAM_V, SRAM_F_V, FLASH_V, FLASH512_V, and FLASH1M_V.\n" +
                "If no signature is found, it defaults to SRAM, matching the Python build.\n\n" +
                "Import Save writes the selected game's save as bram.srm / bram.eep / bram.fla based on the detected save type.");

            var about = new ToolStripMenuItem("About");
            about.Click += (s, e) => ShowInfoWindow("About",
                "EDGBAPRO Rom Manager\n\n" +
                "Native .NET 6 WinForms build.\n" +
                "Uses ROM-content save detection matching the original Python version.\n" +
                "UI list is rendered with DataGridView to avoid owner-draw ghosting.");

            strip.Items.Add(help);
            strip.Items.Add(about);
            return strip;
        }

        private void ShowInfoWindow(string title, string text)
        {
            var form = new Form
            {
                Text = title,
                Width = 700,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.Gainsboro
            };

            var box = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(24, 24, 24),
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10),
                Text = text
            };

            form.Controls.Add(box);
            form.ShowDialog(this);
        }

        private void UpdateProgressStatus(int percent, string text)
        {
            progressBar.Value = Math.Max(0, Math.Min(100, percent));
            statusLabel.Text = text;
        }

        private async System.Threading.Tasks.Task ScanButton_Click()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select the SD card or root folder to scan"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                return;

            currentRootFolder = dialog.SelectedPath;
            currentEntries.Clear();
            gameGrid.Rows.Clear();
            UpdateProgressStatus(0, "Preparing scan...");

            scanButton.Enabled = false;
            importButton.Enabled = false;

            try
            {
                var progress = new Progress<ScanProgressInfo>(info => UpdateProgressStatus(info.Percent, $"{info.Percent}% - {info.Activity}"));
                var entries = await RomScanner.ScanAsync(currentRootFolder, progress);
                currentEntries.Clear();
                currentEntries.AddRange(entries);
                RefreshGrid();
                UpdateProgressStatus(100, $"100% - Scan complete. {currentEntries.Count} ROM(s) found.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateProgressStatus(0, "Scan failed.");
            }
            finally
            {
                scanButton.Enabled = true;
                importButton.Enabled = true;
            }
        }

        private void RefreshGrid()
        {
            gameGrid.Rows.Clear();
            foreach (var entry in currentEntries)
            {
                int row = gameGrid.Rows.Add(entry.RomName, entry.SaveType);
                gameGrid.Rows[row].Tag = entry;
            }
        }

        private void ImportButton_Click(object? sender, EventArgs e)
        {
            if (gameGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "Select a game first.", "Import Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (gameGrid.SelectedRows[0].Tag is not RomEntry entry)
            {
                MessageBox.Show(this, "Selected item is invalid.", "Import Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = "Select a save file to import",
                Filter = "Save files|*.sav;*.srm;*.eep;*.fla;*.bin;*.*|All files|*.*"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
                return;

            try
            {
                SaveImporter.ImportSave(entry, dialog.FileName);
                UpdateProgressStatus(progressBar.Value, $"Imported save to {entry.SaveFileName} for {entry.RomName}");
                MessageBox.Show(this,
                    $"Imported save for {entry.RomName}\n\nDestination:\n{Path.Combine(entry.GameDataFolder, entry.SaveFileName)}",
                    "Import Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
