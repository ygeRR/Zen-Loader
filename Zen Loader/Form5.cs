using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;
using Core;

namespace Zen_Loader
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }
        string site = "https://yigiterzin.myds.me:5003";
        private yigiterzin core = new yigiterzin();

        // Form5 :) <3
        private void Form5_Load(object sender, EventArgs e)
        {
            txtLog.Text = "Uygulama başlatılıyor.";
            core.ApplicationName = "ZenLoader v5.3";
            this.Icon = Zen_Loader.Properties.Resources.ico;

        }
        clsHost hostClass = new clsHost();

        #region HASHES
        private string HASHv52 = "5CB0E2A9D7FD0A4030EF216D58DDA73D";
        private string HASHv53 = "3F25074824214901742B3011BCEF2A83";
        private string HASHv58 = "4238dca875e677a449ef8667dd26c0ab";
        #endregion

        #region VISUALIZE
#endregion

        private void Form5_Shown(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
            LogUpdate("Uygulama başlatıldı.");
            Inject();
        }

        

      
        #region FUNCTIONS

        private void Inject()
        {
            {
                try
                {
                    // Find all exe files in the application directory
                    var exeFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe");

                    if (exeFiles.Length == 0)
                    {
                        core.MessageboxWarn("Herhangi bir .exe dosyası bulunamadı.");
                        LogUpdate("Uyumlu dosya bulunamadı.");
                        return;
                    }

                    // Map known hashes to version labels
                    var known = new[]
                    {
            new { Hash = HASHv52, Version = "v5.2" },
            new { Hash = HASHv53, Version = "v5.3" },
            new { Hash = HASHv58, Version = "v5.8" }
        };

                    string selectedFile = null;
                    string matchedVersion = null;
                    string matchedHash = null;

                    // Check each exe for a matching hash
                    foreach (var file in exeFiles)
                    {
                        string fileHash;
                        try
                        {
                            fileHash = core.GenerateMD5(file);
                        }
                        catch
                        {
                            // If hash generation fails for a file, skip it
                            continue;
                        }

                        foreach (var k in known)
                        {
                            if (string.Equals(fileHash, k.Hash, StringComparison.OrdinalIgnoreCase))
                            {
                                selectedFile = file;
                                matchedVersion = k.Version;
                                matchedHash = k.Hash;
                                break;
                            }
                        }

                        if (selectedFile != null)
                            break;
                    }

                    if (selectedFile == null)
                    {
                        // No compatible exe found - provide feedback and copy hashes to clipboard for debugging
                        var firstFile = exeFiles[0];
                        string firstHash = core.GenerateMD5(firstFile);
                        core.MessageboxWarn("Dosya doğrulama tamamlanamadı." + Environment.NewLine +
                                            "Uyumlu Zen Macro uygulaması bulunamadı.");
                        LogUpdate("Uyumlu dosya bulunamadı. Uygulama durduruldu.");
                        return;
                    }

                    // Found a compatible exe
                    core.MessageboxInformation("Dosya doğrulama tamamlandı. Zen Macro sürümü " + matchedVersion + " ve hash değeri: " + matchedHash);
                    LogUpdate("ZenLoader " + matchedVersion);

                    LogUpdate("Host yedekleniyor: host.yedek");
                    hostClass.backupHost();
                    LogUpdate("Host yedeklendi.");
                    LogUpdate("Host düzenlemeri yapılıyor.");
                    hostClass.writeHost();
                    LogUpdate("Host düzenlemeri tamamlandı. İyi oyunlar.");

                    DialogResult dialogResult = MessageBox.Show("ZenAutobot başlatılsın mı?", "Anti-ZenAutobot", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        core.ProcessStart(selectedFile);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }
        }

        // Improved logging: thread-safe, persisted to disk, keeps an in-memory rolling buffer,
        // supports log levels and export/clear functionality.
        private const int MaxLogLines = 2000;
        private readonly System.Collections.Generic.List<string> _logLines = new System.Collections.Generic.List<string>();
        private readonly string _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZenLoader", "ZenLoader.log");

        private enum LogLevel
        {
            Info,
            Warning,
            Error,
            Debug
        }

        private void EnsureLogDirectory()
        {
            try
            {
                var dir = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch
            {
                // Swallow exceptions to avoid breaking the UI on logging failures.
            }
        }

        private void WriteLineToFile(string line)
        {
            try
            {
                EnsureLogDirectory();
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
            catch
            {
                // Ignore disk write errors (best-effort logging).
            }
        }

        private void UpdateTextBoxFromLines()
        {
            // Replace or truncate textbox content from in-memory buffer.
            txtLog.Text = string.Join(Environment.NewLine, _logLines);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void LogUpdate(string message, LogLevel level = LogLevel.Info)
        {
            if (message == null) message = string.Empty;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var line = $"{timestamp} [{level}] {message}";

            lock (_logLines)
            {
                _logLines.Add(line);
                if (_logLines.Count > MaxLogLines)
                {
                    // remove oldest lines to keep buffer bounded
                    var removeCount = _logLines.Count - MaxLogLines;
                    _logLines.RemoveRange(0, removeCount);
                }
            }

            // Persist asynchronously to avoid blocking UI thread (fire-and-forget)
            try
            {
                System.Threading.ThreadPool.QueueUserWorkItem(_ => WriteLineToFile(line));
            }
            catch
            {
                // ignore
            }

            // Update UI in a thread-safe manner
            if (txtLog.InvokeRequired)
            {
                try
                {
                    txtLog.BeginInvoke(new Action(UpdateTextBoxFromLines));
                }
                catch
                {
                    // if invocation fails, ignore to keep app stable
                }
            }
            else
            {
                UpdateTextBoxFromLines();
            }
        }

        // Optional helpers to provide logs to the user:

        private void ExportLogs(string destinationFilePath)
        {
            try
            {
                lock (_logLines)
                {
                    EnsureLogDirectory();
                    File.WriteAllText(destinationFilePath, string.Join(Environment.NewLine, _logLines));
                }
                core.MessageboxInformation("Loglar kaydedildi: " + destinationFilePath);
            }
            catch (Exception ex)
            {
                core.MessageboxWarn("Log dışa aktarılırken hata oluştu: " + ex.Message);
            }
        }

        private void ClearLogs(bool clearFileToo = false)
        {
            lock (_logLines)
            {
                _logLines.Clear();
            }
            if (clearFileToo)
            {
                try
                {
                    if (File.Exists(_logFilePath))
                        File.Delete(_logFilePath);
                }
                catch
                {
                    // ignore
                }
            }

            if (txtLog.InvokeRequired)
                txtLog.BeginInvoke(new Action(UpdateTextBoxFromLines));
            else
                UpdateTextBoxFromLines();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("https://yigiterzin.com");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
@"Bu uygulama ZenAutobot'un arkaplanda çalıştırdığı AutoitX3.exe dosyasını indirmemesi için host dosyanıza aşağıdaki adresleri ekler.
Uygulamanın Zen Macro ile aynı klasör içerisinde olması hash değerlerinin kontrolü için gereklidir.
yigiterzin @ 2025

Engellenen adresler: 
george-82.webselfsite.net
52.210.211.82
tenahuzemeno.blogspot.com
gargoilerdisar.blogspot.com
resources.blogblog.com
jimmy - 03.webselfsite.net

Desteklenen makro sürümleri: 5.2, 5.3, 5.8");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogUpdate("Host dosyası eski haline getiriliyor.");
            try
            {
                hostClass.reloadHost();
            }
            catch (Exception ex)
            {
                
                
            }

        }

        #endregion

        private void txtLog_Click(object sender, EventArgs e)
        {
            ExportLogs(Path.Combine((System.Reflection.Assembly.GetExecutingAssembly().Location), "ZenLoader_Logs.txt"));
        }
    }
}
