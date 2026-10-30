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
    private readonly List<(string Id, bool Unlocked)> items = new();
    private readonly string cliPath = Path.Combine(AppContext.BaseDirectory, "cli", "AmongUsAchievementUnlocker.exe");
    private bool busy;

    public MainForm()
    {
        Text = "Among Us Achievement Manager";
        Width = 960;
        Height = 730;
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
        refresh.Click += async (_, _) => await RefreshAsync();
        unlock.Text = "Unlock remaining";
        unlock.Width = 168;
        unlock.Height = 34;
        unlock.Click += async (_, _) => await UnlockAsync();
        search.PlaceholderText = "Filter achievements...";
        search.Width = 255;
        search.Height = 34;
        search.TextChanged += (_, _) => Render();
        bar.Controls.Add(refresh);
        bar.Controls.Add(unlock);
        bar.Controls.Add(search);
        Controls.Add(bar);

        summary.Dock = DockStyle.Top;
        summary.Height = 36;
        summary.Padding = new Padding(21, 6, 0, 0);
        summary.Text = "Not connected";
        Controls.Add(summary);

        details.Multiline = true;
        details.ReadOnly = true;
        details.ScrollBars = ScrollBars.Vertical;
        details.Dock = DockStyle.Bottom;
        details.Height = 150;
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
        achievements.BorderStyle = BorderStyle.None;
        achievements.AutoGenerateColumns = false;
        achievements.AllowUserToAddRows = false;
        achievements.ReadOnly = true;
        achievements.RowHeadersVisible = false;
        achievements.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        achievements.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        achievements.ColumnHeadersHeight = 36;
        achievements.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Achievement ID", DataPropertyName = "Id", FillWeight = 78 });
        achievements.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "State", FillWeight = 22 });
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
        foreach (var entry in items.Where(x => x.Id.Contains(filter, StringComparison.OrdinalIgnoreCase)))
            achievements.Rows.Add(entry.Id, entry.Unlocked ? "Unlocked" : "Locked");
        int unlockedCount = items.Count(x => x.Unlocked);
        summary.Text = $"Unlocked: {unlockedCount} / {items.Count}  |  Remaining: {items.Count - unlockedCount}";
        unlock.Enabled = !busy && items.Any(x => !x.Unlocked);
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
