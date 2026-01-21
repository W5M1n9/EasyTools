using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("PingIpChecker")]
[assembly: AssemblyDescription("Ping IP Checker Tool")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("WuM1ng")]
[assembly: AssemblyProduct("IpCheckerApp")]
[assembly: AssemblyCopyright("Copyright © WuM1ng 2026")]
[assembly: AssemblyTrademark("WuM1ng Tools")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace IpCheckerApp
{
    public class MainForm : Form
    {
        private RichTextBox inputBox;
        private RichTextBox successBox;
        private RichTextBox failBox;
        private Button checkButton;
        private Panel headerPanel;
        private Label titleLabel;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public MainForm()
        {
            this.Text = "PingIPChecker";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.DoubleBuffered = true;

            InitializeCustomUI();
        }

        private void InitializeCustomUI()
        {
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            headerPanel.BackColor = Color.FromArgb(44, 62, 80);

            titleLabel = new Label();
            titleLabel.Text = "PING IP CHECKER";
            titleLabel.ForeColor = Color.White;
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Fill;
            headerPanel.Controls.Add(titleLabel);

            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Padding = new Padding(10);
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));

            GroupBox inputGroup = new GroupBox();
            inputGroup.Text = "输入 IP 地址 (支持 IPv4 / IPv6)";
            inputGroup.Dock = DockStyle.Fill;
            inputGroup.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            inputBox = new RichTextBox();
            inputBox.Dock = DockStyle.Fill;
            inputBox.BorderStyle = BorderStyle.None;
            inputBox.Font = new Font("Consolas", 10);
            inputGroup.Controls.Add(inputBox);

            checkButton = new Button();
            checkButton.Text = "开始检测";
            checkButton.Dock = DockStyle.Fill;
            checkButton.BackColor = Color.FromArgb(39, 174, 96);
            checkButton.ForeColor = Color.White;
            checkButton.FlatStyle = FlatStyle.Flat;
            checkButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            checkButton.Cursor = Cursors.Hand;
            checkButton.Click += async (s, e) => await RunProcess();

            TableLayoutPanel resultLayout = new TableLayoutPanel();
            resultLayout.Dock = DockStyle.Fill;
            resultLayout.ColumnCount = 2;
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            GroupBox successGroup = new GroupBox();
            successGroup.Text = "Alive";
            successGroup.Dock = DockStyle.Fill;
            successGroup.ForeColor = Color.Green;

            successBox = new RichTextBox();
            successBox.Dock = DockStyle.Fill;
            successBox.BorderStyle = BorderStyle.None;
            successBox.Font = new Font("Consolas", 10);
            successBox.ReadOnly = true;
            successGroup.Controls.Add(successBox);

            GroupBox failGroup = new GroupBox();
            failGroup.Text = "Dead";
            failGroup.Dock = DockStyle.Fill;
            failGroup.ForeColor = Color.Red;

            failBox = new RichTextBox();
            failBox.Dock = DockStyle.Fill;
            failBox.BorderStyle = BorderStyle.None;
            failBox.Font = new Font("Consolas", 10);
            failBox.ReadOnly = true;
            failGroup.Controls.Add(failBox);

            resultLayout.Controls.Add(successGroup, 0, 0);
            resultLayout.Controls.Add(failGroup, 1, 0);

            mainLayout.Controls.Add(inputGroup, 0, 0);
            mainLayout.Controls.Add(checkButton, 0, 1);
            mainLayout.Controls.Add(resultLayout, 0, 2);

            this.Controls.Add(mainLayout);
            this.Controls.Add(headerPanel);
        }

        private async Task RunProcess()
        {
            checkButton.Enabled = false;
            checkButton.Text = "检测中...";
            checkButton.BackColor = Color.Gray;
            successBox.Clear();
            failBox.Clear();

            string rawText = inputBox.Text;
            var ipList = await Task.Run(() => ExtractIps(rawText));

            if (ipList.Count == 0)
            {
                MessageBox.Show("未找到有效 IP", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ResetButton();
                return;
            }

            using (SemaphoreSlim semaphore = new SemaphoreSlim(50))
            {
                var tasks = ipList.Select(async ip =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        using (var pinger = new Ping())
                        {
                            try
                            {
                                PingReply reply = await pinger.SendPingAsync(ip, 2000);
                                return new { Ip = ip, Success = reply.Status == IPStatus.Success, Message = reply.Status.ToString() };
                            }
                            catch (Exception ex)
                            {
                                string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                                return new { Ip = ip, Success = false, Message = errorMsg };
                            }
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                var results = await Task.WhenAll(tasks);

                var successList = results.Where(r => r.Success).Select(r => r.Ip).ToList();
                var failList = results.Where(r => !r.Success).Select(r => string.Format("{0} ({1})", r.Ip, r.Message)).ToList();

                if (successList.Count > 0)
                {
                    AppendToBox(successBox, string.Join("\n", successList), Color.DarkGreen);
                    AppendToBox(successBox, string.Format("\n\n[Total: {0}]", successList.Count), Color.Black);
                }

                if (failList.Count > 0)
                {
                    AppendToBox(failBox, string.Join("\n", failList), Color.DarkRed);
                    AppendToBox(failBox, string.Format("\n\n[Total: {0}]", failList.Count), Color.Black);
                }
            }

            ResetButton();
            MessageBox.Show("检测完成", "PingIpChecker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResetButton()
        {
            checkButton.Enabled = true;
            checkButton.Text = "开始检测";
            checkButton.BackColor = Color.FromArgb(39, 174, 96);
        }

        private List<string> ExtractIps(string text)
        {
            var results = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return results;

            string ipv4Pattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
            MatchCollection v4Matches = Regex.Matches(text, ipv4Pattern);
            foreach (Match match in v4Matches)
            {
                IPAddress tempIp;
                if (IPAddress.TryParse(match.Value, out tempIp))
                {
                    results.Add(match.Value);
                }
            }

            string ipv6Pattern = @"([0-9a-fA-F]{1,4}:){1,7}:?([0-9a-fA-F]{1,4}|:)?";
            MatchCollection v6Matches = Regex.Matches(text, ipv6Pattern);
            foreach (Match match in v6Matches)
            {
                string rawV6 = match.Value;
                char[] trimChars = new char[] { '[', ']', '(', ')', '"', '\'' };
                rawV6 = rawV6.Trim(trimChars);

                IPAddress tempIp;
                if (IPAddress.TryParse(rawV6, out tempIp))
                {
                     if (tempIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                     {
                         if(rawV6.Length > 2 || rawV6 == "::1")
                         {
                            results.Add(rawV6);
                         }
                     }
                }
            }

            return results.Distinct().ToList();
        }

        private void AppendToBox(RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}