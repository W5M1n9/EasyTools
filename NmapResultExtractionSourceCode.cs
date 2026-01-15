using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using System.Globalization;

[assembly: AssemblyTitle("NmapResultExtraction")]
[assembly: AssemblyDescription("Nmap Scan Result Extractor")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("WuM1ng")]
[assembly: AssemblyProduct("NmapResultExtraction")]
[assembly: AssemblyCopyright("Copyright © WuM1ng 2026")]
[assembly: AssemblyTrademark("WuM1ng Tools")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace NmapResultExtraction
{
    public class MainForm : Form
    {
        private TabControl tabControl;
        
        private TextBox txtPasteContent;
        private TextBox txtPasteOutput;
        
        private TextBox txtFileInput;
        private TextBox txtFileOutput;
        
        private Button btnRunPaste;
        private Button btnRunFile;

        public MainForm()
        {
            this.Text = "NmapResultExtraction";
            this.Size = new Size(540, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            
            TabPage tabPaste = new TabPage("模式一：粘贴文本");
            InitializePasteTab(tabPaste);
            tabControl.TabPages.Add(tabPaste);

            TabPage tabFile = new TabPage("模式二：导入或拖拽文件");
            InitializeFileTab(tabFile);
            tabControl.TabPages.Add(tabFile);

            this.Controls.Add(tabControl);
        }

        private void InitializePasteTab(TabPage tab)
        {
            tab.Padding = new Padding(10);
            
            Label lblContent = new Label();
            lblContent.Text = "请在此处粘贴 Nmap 扫描结果：";
            lblContent.Location = new Point(15, 15);
            lblContent.AutoSize = true;
            tab.Controls.Add(lblContent);

            txtPasteContent = new TextBox();
            txtPasteContent.Multiline = true;
            txtPasteContent.ScrollBars = ScrollBars.Vertical;
            txtPasteContent.Location = new Point(15, 40);
            txtPasteContent.Size = new Size(490, 200);
            tab.Controls.Add(txtPasteContent);

            Label lblOut = new Label();
            lblOut.Text = "选择保存路径(留空默认当前目录)：";
            lblOut.Location = new Point(15, 255);
            lblOut.AutoSize = true;
            tab.Controls.Add(lblOut);

            txtPasteOutput = new TextBox();
            txtPasteOutput.Location = new Point(15, 275);
            txtPasteOutput.Size = new Size(400, 25);
            tab.Controls.Add(txtPasteOutput);

            Button btnBrowse = new Button();
            btnBrowse.Text = "浏览...";
            btnBrowse.Location = new Point(425, 273);
            btnBrowse.Size = new Size(80, 27);
            btnBrowse.Click += (s, e) => BrowseFolder(txtPasteOutput);
            tab.Controls.Add(btnBrowse);

            btnRunPaste = new Button();
            btnRunPaste.Text = "开始转换";
            btnRunPaste.Location = new Point(170, 310);
            btnRunPaste.Size = new Size(180, 45);
            btnRunPaste.BackColor = Color.SeaGreen;
            btnRunPaste.ForeColor = Color.White;
            btnRunPaste.Font = new Font("Arial", 11, FontStyle.Bold);
            btnRunPaste.Click += (s, e) => RunPasteProcess();
            tab.Controls.Add(btnRunPaste);
        }

        private void InitializeFileTab(TabPage tab)
        {
            int y = 30;

            Label lblIn = new Label();
            lblIn.Text = "选择 Nmap 扫描结果(txt) - 支持拖拽";
            lblIn.Location = new Point(20, y);
            lblIn.AutoSize = true;
            tab.Controls.Add(lblIn);
            y += 25;

            txtFileInput = new TextBox();
            txtFileInput.Location = new Point(20, y);
            txtFileInput.Size = new Size(380, 25);
            
            txtFileInput.AllowDrop = true;
            txtFileInput.DragEnter += (s, e) => 
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            };
            txtFileInput.DragDrop += (s, e) => 
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    if (files[0].EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        txtFileInput.Text = files[0];
                    }
                    else
                    {
                        MessageBox.Show("仅支持导入txt文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };
            tab.Controls.Add(txtFileInput);

            Button btnIn = new Button();
            btnIn.Text = "浏览...";
            btnIn.Location = new Point(410, y - 2);
            btnIn.Size = new Size(80, 27);
            btnIn.Click += (s, e) => {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Text Files|*.txt|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK) txtFileInput.Text = ofd.FileName;
            };
            tab.Controls.Add(btnIn);
            y += 60;

            Label lblOut = new Label();
            lblOut.Text = "选择保存路径(留空默认当前目录)";
            lblOut.Location = new Point(20, y);
            lblOut.AutoSize = true;
            tab.Controls.Add(lblOut);
            y += 25;

            txtFileOutput = new TextBox();
            txtFileOutput.Location = new Point(20, y);
            txtFileOutput.Size = new Size(380, 25);
            tab.Controls.Add(txtFileOutput);

            Button btnOut = new Button();
            btnOut.Text = "浏览...";
            btnOut.Location = new Point(410, y - 2);
            btnOut.Size = new Size(80, 27);
            btnOut.Click += (s, e) => BrowseFolder(txtFileOutput);
            tab.Controls.Add(btnOut);
            y += 80;

            btnRunFile = new Button();
            btnRunFile.Text = "开始转换";
            btnRunFile.Location = new Point(170, y);
            btnRunFile.Size = new Size(180, 45);
            btnRunFile.BackColor = Color.SteelBlue;
            btnRunFile.ForeColor = Color.White;
            btnRunFile.Font = new Font("Arial", 11, FontStyle.Bold);
            btnRunFile.Click += (s, e) => RunFileProcess();
            tab.Controls.Add(btnRunFile);
        }

        private void BrowseFolder(TextBox targetBox)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK) targetBox.Text = fbd.SelectedPath;
        }

        private void RunPasteProcess()
        {
            string content = txtPasteContent.Text;
            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("请先粘贴内容！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string outputDir = txtPasteOutput.Text.Trim();
            if (string.IsNullOrEmpty(outputDir)) outputDir = AppDomain.CurrentDomain.BaseDirectory;

            btnRunPaste.Enabled = false;
            btnRunPaste.Text = "处理中...";
            Application.DoEvents();

            try
            {
                string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                string result = Converter.Convert(lines, outputDir);
                MessageBox.Show("成功！表格已生成：\n" + result, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：" + ex.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRunPaste.Enabled = true;
                btnRunPaste.Text = "开始转换";
            }
        }

        private void RunFileProcess()
        {
            string inputFile = txtFileInput.Text.Trim();
            string outputDir = txtFileOutput.Text.Trim();

            if (!File.Exists(inputFile))
            {
                MessageBox.Show("输入文件不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(outputDir)) outputDir = AppDomain.CurrentDomain.BaseDirectory;

            btnRunFile.Enabled = false;
            btnRunFile.Text = "处理中...";
            Application.DoEvents();

            try
            {
                IEnumerable<string> lines = File.ReadLines(inputFile);
                string result = Converter.Convert(lines, outputDir);
                MessageBox.Show("成功！表格已生成：\n" + result, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：" + ex.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRunFile.Enabled = true;
                btnRunFile.Text = "开始转换";
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }

    public static class Converter
    {
        private static readonly Regex ipRegex = new Regex(@"Nmap scan report for ([\d\.]+)", RegexOptions.Compiled);
        private static readonly Regex portRegex = new Regex(@"^(\d+)/(tcp|udp)\s+(\w+)\s+(.+)$", RegexOptions.Compiled);
        private static readonly Regex timeRegex = new Regex(@"scan initiated (.*?) as:", RegexOptions.Compiled);

        public static string Convert(IEnumerable<string> lines, string outputDir)
        {
            string scanTime = null;
            List<string[]> data = new List<string[]>();
            string currentIp = null;
            
            int lineCount = 0;
            string rawTime = null;

            foreach (string line in lines)
            {
                lineCount++;
                string l = line.Trim();
                if (string.IsNullOrEmpty(l)) continue;

                bool isIpLine = l.StartsWith("Nmap scan report for");
                bool isPortLine = !isIpLine && currentIp != null && (l.Contains("/tcp") || l.Contains("/udp")) && l.Contains("open");

                if (!isIpLine && !isPortLine)
                {
                    if (scanTime == null && rawTime == null && lineCount < 100 && l.Contains("scan initiated"))
                    {
                        Match tm = timeRegex.Match(l);
                        if (tm.Success) rawTime = tm.Groups[1].Value.Trim();
                    }
                    continue;
                }

                if (isIpLine)
                {
                    Match im = ipRegex.Match(l);
                    if (im.Success)
                    {
                        currentIp = im.Groups[1].Value;
                    }
                    continue;
                }

                if (isPortLine)
                {
                    Match pm = portRegex.Match(l);
                    if (pm.Success && pm.Groups[3].Value == "open") 
                    {
                        data.Add(new string[] { currentIp, pm.Groups[1].Value, pm.Groups[2].Value, pm.Groups[4].Value });
                    }
                }
            }

            if (data.Count == 0) throw new Exception("未找到有效数据");

            string dateFormat = "yyyy.MM.dd_HH-mm-ss";
            if (rawTime != null)
            {
                string[] parseFormats = { "ddd MMM dd HH:mm:ss yyyy", "ddd MMM d HH:mm:ss yyyy" };
                DateTime dt;
                if (DateTime.TryParseExact(rawTime, parseFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
                {
                    scanTime = dt.ToString(dateFormat);
                }
                else
                {
                    scanTime = DateTime.Now.ToString(dateFormat);
                }
            }
            else
            {
                scanTime = DateTime.Now.ToString(dateFormat);
            }

            string fileName = Path.Combine(outputDir, string.Format("scan_result_{0}.xlsx", scanTime));

            Type excelType = Type.GetTypeFromProgID("Excel.Application");
            if (excelType == null) throw new Exception("本机未安装 Excel");

            dynamic excel = Activator.CreateInstance(excelType);
            dynamic workbook = null;
            
            try
            {
                excel.Visible = false;
                excel.DisplayAlerts = false;
                
                workbook = excel.Workbooks.Add();
                
                try { excel.ScreenUpdating = false; } catch {}
                try { excel.EnableEvents = false; } catch {}
                try { excel.Calculation = -4135; } catch {}

                dynamic ws = workbook.ActiveSheet;
                ws.Name = "ScanResults";

                ws.Cells[1, 1] = "IP";
                ws.Cells[1, 2] = "端口";
                ws.Cells[1, 3] = "协议";
                ws.Cells[1, 4] = "服务";

                object[,] arr = new object[data.Count, 4];
                for(int i=0; i<data.Count; i++)
                {
                    arr[i, 0] = data[i][0]; 
                    arr[i, 1] = int.Parse(data[i][1]); 
                    arr[i, 2] = data[i][2]; 
                    arr[i, 3] = data[i][3]; 
                }

                dynamic dataRange = ws.Range(ws.Cells[2, 1], ws.Cells[data.Count + 1, 4]);
                dataRange.Value = arr;

                dynamic fullRange = ws.Range(ws.Cells[1, 1], ws.Cells[data.Count + 1, 4]);
                
                fullRange.HorizontalAlignment = -4108; 
                fullRange.VerticalAlignment = -4108;   
                fullRange.Font.Name = "宋体";

                fullRange.Borders.LineStyle = 1;
                fullRange.Borders.Weight = 2;

                dynamic headerRange = ws.Range(ws.Cells[1, 1], ws.Cells[1, 4]);
                headerRange.Font.Bold = true;

                fullRange.Columns.AutoFit();

                try { excel.Calculation = -4105; } catch {}
                try { excel.ScreenUpdating = true; } catch {}

                workbook.SaveAs(fileName);
            }
            finally
            {
                try { excel.EnableEvents = true; } catch {}

                if (workbook != null) workbook.Close();
                excel.Quit();
                Marshal.ReleaseComObject(excel);
            }

            return fileName;
        }
    }
}
