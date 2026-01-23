using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: AssemblyTitle("FscanMultiProcessProcessing")]
[assembly: AssemblyDescription("Fscan Multi Process Processing Tool")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("WuM1ng")]
[assembly: AssemblyProduct("FscanMultiProcessProcessing")]
[assembly: AssemblyCopyright("Copyright © WuM1ng 2026")]
[assembly: AssemblyTrademark("WuM1ng Tools")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace FscanMultiProcessProcessing
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private TextBox txtTargetIp;
        private TextBox txtCommandArgs;
        private CheckBox chkFullPort;
        private CheckBox chkNoPoc;
        private CheckBox chkNoBrute;
        private Button btnStart;
        private RichTextBox rtbLog;
        private string fscanPath;
        private string workDir;

        private Color colorBg = Color.FromArgb(45, 45, 48);
        private Color colorPanel = Color.FromArgb(30, 30, 30);
        private Color colorText = Color.FromArgb(240, 240, 240);
        private Color colorAccent = Color.FromArgb(0, 122, 204);
        private Color colorInput = Color.FromArgb(60, 60, 60);

        public MainForm()
        {
            InitializeComponent();
            CheckEnvironment();
        }

        private void InitializeComponent()
        {
            this.Text = "FscanMultiProcessProcessing";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = colorBg;
            this.ForeColor = colorText;
            this.Font = new Font("Segoe UI", 10f);

            Label lblTitle = new Label();
            lblTitle.Text = "Fscan Multi-Process Controller";
            lblTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblTitle.ForeColor = colorAccent;
            lblTitle.Location = new Point(20, 15);
            lblTitle.AutoSize = true;

            Label lblIp = new Label();
            lblIp.Text = "Target Network / IP:";
            lblIp.Location = new Point(25, 60);
            lblIp.AutoSize = true;
            lblIp.ForeColor = Color.LightGray;

            txtTargetIp = CreateStyledTextBox();
            txtTargetIp.Location = new Point(25, 85);
            txtTargetIp.Size = new Size(350, 29);

            Label lblArgs = new Label();
            lblArgs.Text = "Command Arguments:";
            lblArgs.Location = new Point(25, 125);
            lblArgs.AutoSize = true;
            lblArgs.ForeColor = Color.LightGray;

            txtCommandArgs = CreateStyledTextBox();
            txtCommandArgs.Location = new Point(25, 150);
            txtCommandArgs.Size = new Size(630, 29);
            txtCommandArgs.Text = "-t 200";

            Panel pnlOptions = new Panel();
            pnlOptions.Location = new Point(25, 195);
            pnlOptions.Size = new Size(630, 50);
            pnlOptions.BackColor = colorPanel;

            chkFullPort = CreateStyledCheckBox("Full Ports (1-65535)");
            chkFullPort.Location = new Point(15, 12);
            
            chkNoPoc = CreateStyledCheckBox("No POC (-nopoc)");
            chkNoPoc.Location = new Point(220, 12);
            
            chkNoBrute = CreateStyledCheckBox("No Brute (-nobr)");
            chkNoBrute.Location = new Point(400, 12);

            pnlOptions.Controls.Add(chkFullPort);
            pnlOptions.Controls.Add(chkNoPoc);
            pnlOptions.Controls.Add(chkNoBrute);

            btnStart = new Button();
            btnStart.Text = "START SCAN";
            btnStart.Location = new Point(25, 260);
            btnStart.Size = new Size(630, 45);
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.BackColor = colorAccent;
            btnStart.ForeColor = Color.White;
            btnStart.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnStart.Cursor = Cursors.Hand;
            btnStart.Click += BtnStart_Click;

            Label lblLog = new Label();
            lblLog.Text = "Execution Log:";
            lblLog.Location = new Point(25, 320);
            lblLog.AutoSize = true;
            lblLog.ForeColor = Color.Gray;

            rtbLog = new RichTextBox();
            rtbLog.Location = new Point(25, 345);
            rtbLog.Size = new Size(630, 240);
            rtbLog.ReadOnly = true;
            rtbLog.BackColor = Color.Black;
            rtbLog.ForeColor = Color.Lime;
            rtbLog.Font = new Font("Consolas", 9.5f);
            rtbLog.BorderStyle = BorderStyle.None;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblIp);
            this.Controls.Add(txtTargetIp);
            this.Controls.Add(lblArgs);
            this.Controls.Add(txtCommandArgs);
            this.Controls.Add(pnlOptions);
            this.Controls.Add(btnStart);
            this.Controls.Add(lblLog);
            this.Controls.Add(rtbLog);

            chkFullPort.CheckedChanged += new EventHandler(chkFullPort_CheckedChanged);
            chkNoPoc.CheckedChanged += new EventHandler(chkNoPoc_CheckedChanged);
            chkNoBrute.CheckedChanged += new EventHandler(chkNoBrute_CheckedChanged);
        }

        private TextBox CreateStyledTextBox()
        {
            TextBox tb = new TextBox();
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = colorInput;
            tb.ForeColor = Color.White;
            tb.Font = new Font("Consolas", 10f);
            return tb;
        }

        private CheckBox CreateStyledCheckBox(string text)
        {
            CheckBox cb = new CheckBox();
            cb.Text = text;
            cb.AutoSize = true;
            cb.ForeColor = colorText;
            cb.Cursor = Cursors.Hand;
            return cb;
        }

        private void UpdateArgument(string flag, bool add)
        {
            string current = txtCommandArgs.Text;
            if (add)
            {
                if (!current.Contains(flag))
                {
                    txtCommandArgs.Text = (current + " " + flag).Trim();
                }
            }
            else
            {
                if (current.Contains(flag))
                {
                    txtCommandArgs.Text = current.Replace(flag, "").Trim();
                }
            }
        }

        private void chkFullPort_CheckedChanged(object sender, EventArgs e)
        {
            string current = txtCommandArgs.Text;
            string fullPortFlag = "-p 1-65535";

            if (chkFullPort.Checked)
            {
                current = Regex.Replace(current, @"-p\s+[^\s]+", "").Trim();
                txtCommandArgs.Text = (current + " " + fullPortFlag).Trim();
            }
            else
            {
                if (current.Contains(fullPortFlag))
                {
                    txtCommandArgs.Text = current.Replace(fullPortFlag, "").Trim();
                }
            }
        }

        private void chkNoPoc_CheckedChanged(object sender, EventArgs e)
        {
            UpdateArgument("-nopoc", chkNoPoc.Checked);
        }

        private void chkNoBrute_CheckedChanged(object sender, EventArgs e)
        {
            UpdateArgument("-nobr", chkNoBrute.Checked);
        }

        private void CheckEnvironment()
        {
            workDir = AppDomain.CurrentDomain.BaseDirectory;
            fscanPath = Path.Combine(workDir, "fscan", "fscan.exe");
            if (!File.Exists(fscanPath))
            {
                string msg = string.Format("File not found: {0}\nPlease ensure the 'fscan' folder exists in the executable directory.", fscanPath);
                MessageBox.Show(msg, "FscanMultiProcessProcessing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStart.Enabled = false;
                btnStart.BackColor = Color.Gray;
            }
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            string ip = txtTargetIp.Text.Trim();
            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Target IP cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string userArgs = txtCommandArgs.Text.Trim();
            if (CheckRestrictedArgs(userArgs))
            {
                MessageBox.Show("Please do not include '-h' or '-o' in the custom arguments box. These are handled automatically.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnStart.Enabled = false;
            btnStart.BackColor = Color.FromArgb(0, 100, 160); 
            rtbLog.Clear();
            Log(">>> FscanMultiProcessProcessing Started");
            Log(">>> Target: " + ip);
            Log(">>> Phase 1: Host Discovery (ICMP Only)...");

            try
            {
                List<string> liveHosts = await RunDiscovery(ip);
                ProcessLiveHosts(liveHosts, ip, userArgs);
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message);
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnStart.Enabled = true;
                btnStart.BackColor = colorAccent;
            }
        }

        private bool CheckRestrictedArgs(string args)
        {
            return args.Contains(" -h ") || args.StartsWith("-h ") || args.Contains(" -o ") || args.StartsWith("-o ");
        }

        private Task<List<string>> RunDiscovery(string ip)
        {
            return Task.Factory.StartNew(() =>
            {
                List<string> hosts = new List<string>();
                Process p = new Process();
                p.StartInfo.FileName = fscanPath;
                p.StartInfo.Arguments = string.Format("-h {0} -nobr -nopoc -nocolor -no", ip);
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = Path.GetDirectoryName(fscanPath);
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                DataReceivedEventHandler handler = (s, e) => 
                { 
                    if (!string.IsNullOrEmpty(e.Data)) 
                    {
                        string line = e.Data;
                        this.Invoke((MethodInvoker)(() => Log(line)));
                        
                        Match match = Regex.Match(line, @"Target\s+(?<ip>[0-9\.]+)\s+is\s+alive");
                        if (match.Success)
                        {
                            string host = match.Groups["ip"].Value.Trim();
                            if (!hosts.Contains(host))
                            {
                                hosts.Add(host);
                            }
                        }

                        if (line.Contains("Icmp alive hosts len is:"))
                        {
                            try 
                            { 
                                p.Kill(); 
                                this.Invoke((MethodInvoker)(() => Log(">>> Discovery limit reached. Halting fscan to skip port scan.")));
                            } 
                            catch {}
                        }
                    } 
                };

                p.OutputDataReceived += handler;
                p.ErrorDataReceived += handler; 

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                return hosts;
            });
        }

        private void ProcessLiveHosts(List<string> liveHosts, string originalIp, string userArgs)
        {
            int count = liveHosts.Count;
            Log(">>> Alive hosts found: " + count);

            string outputDir = PrepareOutputDirectory(originalIp);
            if (string.IsNullOrEmpty(outputDir)) return;

            if (count == 0)
            {
                Log(">>> No live hosts found. Scan stopped.");
                return;
            }

            if (count <= 10)
            {
                Log(">>> Count <= 10. Launching single process...");
                string safeName = GetSafeName(originalIp);
                string fileName = safeName + "_scan_result.txt";
                string fullPath = Path.Combine(outputDir, fileName);
                
                string finalCmd = string.Format("-h {0} {1} -nocolor -o \"{2}\"", originalIp, userArgs, fullPath);
                LaunchCmd(finalCmd);
            }
            else
            {
                DialogResult result = MessageBox.Show(string.Format("Found {0} live hosts. Do you want to split the scan process?", count), "Optimization Suggested", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.No)
                {
                    Log(">>> User cancelled split. Stopping.");
                    return;
                }

                List<List<string>> partitions = PartitionHosts(liveHosts);
                string safeName = GetSafeName(originalIp);

                Log(string.Format(">>> Splitting into {0} processes...", partitions.Count));

                for (int i = 0; i < partitions.Count; i++)
                {
                    string segmentIps = string.Join(",", partitions[i]);
                    string fileName = string.Format("{0}_scan_result_part{1}.txt", safeName, i + 1);
                    string fullPath = Path.Combine(outputDir, fileName);

                    string finalCmd = string.Format("-h {0} {1} -nocolor -o \"{2}\"", segmentIps, userArgs, fullPath);
                    LaunchCmd(finalCmd);
                }
            }
        }

        private string PrepareOutputDirectory(string ip)
        {
            try
            {
                string baseOutput = Path.Combine(workDir, "output");
                if (!Directory.Exists(baseOutput)) Directory.CreateDirectory(baseOutput);

                string safeName = GetSafeName(ip);
                string targetFolder = string.Format("{0}_scan_result", safeName);
                string fullPath = Path.Combine(baseOutput, targetFolder);

                if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                
                Log(">>> Output directory: " + fullPath);
                return fullPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create output directory: " + ex.Message);
                return null;
            }
        }

        private string GetSafeName(string ipInput)
        {
            string safeName = ipInput.Replace("/", "_").Replace("\\", "_").Replace(":", "_").Replace("*", "_");
            if (safeName.Contains("_24")) 
                safeName = safeName.Replace("_24", "");
            
            string[] segments = safeName.Split('.');
            if (segments.Length >= 3)
            {
                 safeName = string.Format("{0}.{1}.{2}", segments[0], segments[1], segments[2]);
            }
            return safeName;
        }

        private List<List<string>> PartitionHosts(List<string> hosts)
        {
            List<List<string>> parts = new List<List<string>>();
            
            int total = hosts.Count;
            int splitCount = 4;
            bool useChunkStrategy = false;

            int baseSize = total / splitCount;
            int remainder = total % splitCount;
            
            for (int i = 0; i < splitCount; i++)
            {
                int size = baseSize + (i < remainder ? 1 : 0);
                if (size > 10)
                {
                    useChunkStrategy = true;
                    break;
                }
            }

            if (!useChunkStrategy)
            {
                int currentIndex = 0;
                for (int i = 0; i < splitCount; i++)
                {
                    int size = baseSize + (i < remainder ? 1 : 0);
                    if (size > 0)
                    {
                        parts.Add(hosts.GetRange(currentIndex, size));
                        currentIndex += size;
                    }
                }
            }
            else
            {
                for (int i = 0; i < total; i += 10)
                {
                    parts.Add(hosts.GetRange(i, Math.Min(10, total - i)));
                }
            }

            return parts;
        }

        private void LaunchCmd(string arguments)
        {
            try
            {
                string scanDir = Path.GetDirectoryName(fscanPath);
                string cmdArgs = string.Format("/k cd /d \"{0}\" && fscan.exe {1}", scanDir, arguments);
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = cmdArgs,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                });
                
                Log(">>> Launched CMD Window");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to launch CMD: " + ex.Message);
            }
        }

        private void Log(string message)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action<string>(Log), message);
                return;
            }
            rtbLog.AppendText(message + Environment.NewLine);
            rtbLog.ScrollToCaret();
        }
    }

}
