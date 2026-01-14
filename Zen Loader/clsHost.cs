using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zen_Loader
{
    public class clsHost
    {
        public string hostPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc");

        private string HostFile => Path.Combine(hostPath, "hosts");
        private string BackupFile => Path.Combine(hostPath, "hosts.ygeR");

        /// <summary>
        /// Ensure a hosts file exists at the destination. If not, attempt to write one from embedded resources
        /// (Properties.Resources.hosts or any manifest resource containing "hosts"). If none found, create a minimal hosts file.
        /// </summary>
        public void EnsureHostExists()
        {
            try
            {
                if (File.Exists(HostFile))
                    return;

                // Ensure directory exists (should normally exist on Windows)
                Directory.CreateDirectory(hostPath);

                // Try Properties.Resources.hosts (auto-generated strongly-typed resource class)
                try
                {
                    var resType = Type.GetType("Zen_Loader.Properties.Resources, " + Assembly.GetExecutingAssembly().GetName().Name);
                    if (resType != null)
                    {
                        var prop = resType.GetProperty("hosts", BindingFlags.Public | BindingFlags.Static);
                        if (prop != null)
                        {
                            var value = prop.GetValue(null) as string;
                            if (!string.IsNullOrEmpty(value))
                            {
                                File.WriteAllText(HostFile, value, Encoding.UTF8);
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    // ignore and try manifest resources
                }

                // Try embedded manifest resources that look like a hosts file
                var asm = Assembly.GetExecutingAssembly();
                var names = asm.GetManifestResourceNames();
                var match = names.FirstOrDefault(n =>
                    n.EndsWith(".hosts", StringComparison.OrdinalIgnoreCase) ||
                    n.EndsWith(".hosts.txt", StringComparison.OrdinalIgnoreCase) ||
                    n.IndexOf("hosts", StringComparison.OrdinalIgnoreCase) >= 0);

                if (match != null)
                {
                    using (var stream = asm.GetManifestResourceStream(match))
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        File.WriteAllText(HostFile, reader.ReadToEnd(), Encoding.UTF8);
                        return;
                    }
                }

                // Fallback: create a minimal hosts file
                File.WriteAllText(HostFile, "# hosts file created by Zen Loader" + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // Intentionally swallow exceptions here to avoid crashing the application.
                // Caller methods will handle cases where file operations still fail.
            }
        }

        public void backupHost()
        {
            try
            {
                EnsureHostExists();

                // Ensure previous backup removed, then create a new backup
                if (File.Exists(BackupFile))
                    File.Delete(BackupFile);

                if (File.Exists(HostFile))
                    File.Copy(HostFile, BackupFile);
            }
            catch
            {
                // swallow to avoid crashing; consider logging in real application
            }
        }

        public void reloadHost()
        {
            try
            {
                if (!File.Exists(BackupFile))
                    return; // nothing to reload

                if (File.Exists(HostFile))
                    File.Delete(HostFile);

                File.Move(BackupFile, HostFile);
            }
            catch
            {
                // swallow to avoid crashing; consider logging in real application
            }
        }

        public void writeHost()
        {
            try
            {
                EnsureHostExists();

                using (StreamWriter w = File.AppendText(HostFile))
                {
                    w.WriteLine("0.0.0.0 george-82.webselfsite.net");
                    w.WriteLine("0.0.0.0 52.210.211.82");
                    w.WriteLine("0.0.0.0 tenahuzemeno.blogspot.com");
                    w.WriteLine("0.0.0.0 gargoilerdisar.blogspot.com");
                    w.WriteLine("0.0.0.0 resources.blogblog.com");
                    w.WriteLine("0.0.0.0 jimmy - 03.webselfsite.net");
                }
            }
            catch
            {
                // swallow to avoid crashing; consider logging in real application
            }
        }
    }
}