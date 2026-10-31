using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AmongUsAchievementUnlocker.Gui;

public sealed class MainForm : Form
{
    private readonly DataGridView achievements = new();
    private readonly TextBox search = new();
    private readonly Label summary = new();
    private readonly Label status = new();
    private readonly Button refresh = new();
    private readonly Button unlock = new();
    private readonly TextBox details = new();
    private readonly ComboBox filter = new();
    private readonly ProgressBar progress = new();
    private readonly Button diagnostics = new();
    private readonly List<(string Id, bool Unlocked)> items = new();
    private readonly string cliPath = Path.Combine(AppContext.BaseDirectory, "cli", "AmongUsAchievementUnlocker.exe");
    private bool busy;

    public MainForm()
    {
        Text = "Among Us Achievement Manager";
        Width = 1080;
        Height = 760;
        MinimumSize = new Size(700, 520);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(20, 27, 40);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        var header = new Label
        {
            Text = "AMONG US  |  ACHIEVEMENT MANAGER",
            Dock = DockStyle.Top,
            Height = 58,
            Padding = new Padding(20, 16, 0, 0),
            Font = new Font("Segoe UI Semibold", 15, FontStyle.Bold),
            ForeColor = Color.FromArgb(127, 208, 255)
        };
        Controls.Add(header);

        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(16, 6, 6, 4), WrapContents = false };
        refresh.Text = "Refresh";
        refresh.Width = 115;
        refresh.Height = 34;
        refresh.BackColor = Color.FromArgb(42, 110, 171);
        refresh.ForeColor = Color.White;
        refresh.FlatStyle = FlatStyle.Flat;
        refresh.FlatAppearance.BorderSize = 0;
        refresh.Click += async (_, _) => await RefreshAsync();
        unlock.Text = "Unlock remaining";
        unlock.Width = 168;
        unlock.Height = 34;
        unlock.BackColor = Color.FromArgb(42, 110, 171);
        unlock.ForeColor = Color.White;
        unlock.FlatStyle = FlatStyle.Flat;
        unlock.FlatAppearance.BorderSize = 0;
        unlock.Click += async (_, _) => await UnlockAsync();
        search.PlaceholderText = "Filter achievements...";
        search.Width = 255;
        search.Height = 34;
        search.TextChanged += (_, _) => Render();
        bar.Controls.Add(refresh);
        bar.Controls.Add(unlock);
        bar.Controls.Add(search);
        filter.DropDownStyle = ComboBoxStyle.DropDownList;
        filter.Width = 130;
        filter.Items.AddRange(new object[] { "All", "Unlocked", "Locked" });
        filter.SelectedIndex = 0;
        filter.SelectedIndexChanged += (_, _) => Render();
        bar.Controls.Add(filter);
        diagnostics.Text = "Show diagnostics";
        diagnostics.AutoSize = true;
        diagnostics.Height = 34;
        diagnostics.Click += (_, _) =>
        {
            details.Visible = !details.Visible;
            diagnostics.Text = details.Visible ? "Hide diagnostics" : "Show diagnostics";
        };
        bar.Controls.Add(diagnostics);
        Controls.Add(bar);

        summary.Dock = DockStyle.Top;
        summary.Height = 36;
        summary.Padding = new Padding(21, 6, 0, 0);
        summary.Text = "Not connected";
        Controls.Add(summary);
        progress.Dock = DockStyle.Top;
        progress.Height = 8;
        progress.Style = ProgressBarStyle.Continuous;
        progress.Maximum = 100;
        Controls.Add(progress);

        details.Multiline = true;
        details.ReadOnly = true;
        details.ScrollBars = ScrollBars.Vertical;
        details.Dock = DockStyle.Bottom;
        details.Height = 150;
        details.Visible = false;
        details.BackColor = Color.FromArgb(13, 19, 30);
        details.ForeColor = Color.Gainsboro;
        details.Font = new Font("Consolas", 9);
        Controls.Add(details);

        status.Dock = DockStyle.Bottom;
        status.Height = 32;
        status.Padding = new Padding(18, 7, 0, 0);
        status.Text = "Ready";
        Controls.Add(status);

        achievements.Dock = DockStyle.Fill;
        achievements.BackgroundColor = Color.FromArgb(25, 34, 48);
        achievements.GridColor = Color.FromArgb(55, 67, 82);
        achievements.EnableHeadersVisualStyles = false;
        achievements.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 49, 66);
        achievements.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        achievements.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 49, 66);
        achievements.DefaultCellStyle.BackColor = Color.FromArgb(27, 37, 51);
        achievements.DefaultCellStyle.ForeColor = Color.FromArgb(230, 239, 248);
        achievements.DefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 113, 180);
        achievements.DefaultCellStyle.SelectionForeColor = Color.White;
        achievements.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(33, 45, 60);
        achievements.RowTemplate.Height = 32;
        achievements.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        achievements.BorderStyle = BorderStyle.None;
        achievements.AutoGenerateColumns = false;
        achievements.AllowUserToAddRows = false;
        achievements.ReadOnly = true;
        achievements.RowHeadersVisible = false;
        achievements.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        achievements.MultiSelect = false;
        achievements.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        achievements.ColumnHeadersHeight = 36;
        achievements.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Achievement", FillWeight = 44 });
        achievements.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Steam ID", FillWeight = 38 });
        achievements.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", FillWeight = 18 });
        Controls.Add(achievements);
        // Docked controls are laid out in reverse z-order; keep the table behind the header and toolbars.
        achievements.SendToBack();

        Shown += async (_, _) => await RefreshAsync();
    }

    private void SetBusy(bool value)
    {
        busy = value;
        refresh.Enabled = !value;
        unlock.Enabled = !value && items.Any(x => !x.Unlocked);
        search.Enabled = !value;
        status.Text = value ? "Communicating with Steam..." : "Ready";
    }

    private void Render()
    {
        string filter = search.Text.Trim();
        achievements.Rows.Clear();
        var mode = this.filter.SelectedItem?.ToString() ?? "All";
        foreach (var entry in items.Where(x =>
            (mode == "All" || (mode == "Unlocked" && x.Unlocked) || (mode == "Locked" && !x.Unlocked)) &&
            (x.Id.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
             FriendlyName(x.Id).Contains(filter, StringComparison.OrdinalIgnoreCase))))
            achievements.Rows.Add(FriendlyName(entry.Id), entry.Id, entry.Unlocked ? "Unlocked" : "Locked");
        int unlockedCount = items.Count(x => x.Unlocked);
        summary.Text = $"Unlocked: {unlockedCount} / {items.Count}  |  Remaining: {items.Count - unlockedCount}";
        unlock.Enabled = !busy && items.Any(x => !x.Unlocked);
        progress.Value = items.Count == 0 ? 0 : (int)Math.Round(100.0 * unlockedCount / items.Count);
    }

    private static string FriendlyName(string id)
    {
        string name = id.Replace("hns_", "Hide and Seek: ")
                        .Replace('_', ' ');
        if (name.Length == 0) return id;
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private async Task RefreshAsync()
    {
        if (busy) return;
        SetBusy(true);
        try
        {
            var (exitCode, output) = await ExecuteCliAsync("--list");
            details.Text = output;
            if (exitCode != 0)
            {
                status.Text = $"Steam read failed (exit {exitCode}). See diagnostics.";
                MessageBox.Show(this, output, "Steam connection error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var matches = Regex.Matches(output, @"^\s*\[\s*(UNLOCKED|LOCKED)\s*\]\s+([^\r\n]+)\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (matches.Count == 0) throw new InvalidOperationException("No achievement records were found in CLI output.");
            items.Clear();
            foreach (Match match in matches)
                items.Add((match.Groups[2].Value.Trim(), match.Groups[1].Value.Equals("UNLOCKED", StringComparison.OrdinalIgnoreCase)));
            Render();
            status.Text = "Steam achievements loaded.";
        }
        catch (Exception ex)
        {
            status.Text = "Refresh failed.";
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { SetBusy(false); }
    }

    private async Task UnlockAsync()
    {
        if (busy || !items.Any(x => !x.Unlocked)) return;
        int count = items.Count(x => !x.Unlocked);
        var response = MessageBox.Show(this,
            $"Request Steam to unlock {count} remaining achievement(s)?\n\nThis changes your Steam account's achievement record and may not be reversible.",
            "Confirm achievement changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (response != DialogResult.Yes) return;

        SetBusy(true);
        try
        {
            var (exitCode, output) = await ExecuteCliAsync("--unlock", "UNLOCK");
            details.Text = output;
            if (exitCode != 0)
                MessageBox.Show(this, output, "Steam rejected or did not confirm the changes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show(this, "Request completed. Refreshing achievements from Steam to verify.", "Steam response", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Unlock failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { SetBusy(false); }
        await RefreshAsync();
    }

    private async Task<(int Code, string Output)> ExecuteCliAsync(string argument, string? input = null)
    {
        if (!File.Exists(cliPath))
            throw new FileNotFoundException($"CLI executable missing: {cliPath}. Publish the CLI into the 'cli' subfolder.");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = cliPath,
                Arguments = argument,
                WorkingDirectory = Path.GetDirectoryName(cliPath)!,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        if (input is not null)
            await process.StandardInput.WriteLineAsync(input);
        process.StandardInput.Close();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            throw new TimeoutException("Steam operation exceeded 90 seconds. Recheck achievements before retrying.");
        }
        string output = await outputTask;
        string error = await errorTask;
        return (process.ExitCode, output + (string.IsNullOrWhiteSpace(error) ? "" : Environment.NewLine + error));
    }
}
