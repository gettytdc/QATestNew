using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace BPJabInstaller
{
    class clsInstaller
    {
        private static Version version200 = new Version("2.0.0");

        ///--------------------------------------------------------------------------
        /// <summary>
        /// Event raised when there is status information.
        /// </summary>
        ///--------------------------------------------------------------------------
        public event StatusHandler Status;
        public delegate void StatusHandler(string msg);

        ///--------------------------------------------------------------------------
        /// <summary>
        /// Used internally to record a status message.
        /// </summary>
        /// <param name="msg">The message.</param>
        ///--------------------------------------------------------------------------
        private void SendStatus(string msg)
        {
            if (Status != null)
                Status(msg);
        }

        ///--------------------------------------------------------------------------
        /// <summary>
        /// Get a list of supported versions.
        /// </summary>
        /// <returns>A List of the versions.</returns>
        ///--------------------------------------------------------------------------
        public static List<Version> GetVersions()
        {
            List<Version> versions = new List<Version>();
            versions.Add(new Version("2.0.0"));
            versions.Add(new Version("2.0.2"));
            versions.Add(new Version("2.0.4"));
            return versions;
        }

        public bool Install(Version version, bool debug)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string instversion = fvi.FileVersion;
                SendStatus("JABInstaller version" + instversion.ToString());

                SendStatus("Removing existing installations, if any");
                if (!Uninstall(debug))
                {
                    SendStatus("Can't continue with installation until existing versions are uninstalled");
                    return false;
                }

                SendStatus("Starting installation of Java Access Bridge " + version);

                if (debug)
                {
                    SendStatus("Embedded resources:");
                    foreach (string name in this.GetType().Assembly.GetManifestResourceNames())
                        SendStatus("..." + name);
                }

                List<FileInfo> jabswitches = new List<FileInfo>();
                List<FileInfo> files = GetFileList(version, debug);
                foreach (FileInfo file in files)
                {
                    if (file.SourceRes == "jabswitch.exe")
                        jabswitches.Add(file);

                    ResToFile(file.SourceRes, file.DestPath);
                }

                if (jabswitches.Count>0)
                {
                    SendStatus("Enabling java access bridge");
                    foreach (FileInfo jabswitch in jabswitches)
                    {
                        try
                        {
                            ProcessStartInfo pi = new ProcessStartInfo(jabswitch.DestPath, "-enable");
                            pi.UseShellExecute = false;
                            Process.Start(pi);
                        }
                        catch { }
                    }
                }


                SendStatus("Installation complete.");
                return true;
            }
            catch (Exception ex)
            {
                if (debug)
                    SendStatus("Failed - " + ex.ToString());
                else
                    SendStatus("Failed - " + ex.Message);
                return false;
            }

        }

        public bool Uninstall(bool debug)
        {
            try
            {
                foreach (Version version in GetVersions())
                {
                    SendStatus("Doing uninstall for " + version);
                    List<FileInfo> files = GetFileList(version, debug);
                    foreach (FileInfo file in files)
                    {
                        if (File.Exists(file.DestPath))
                        {
                            SendStatus("...removing " + file.DestPath);
                            File.Delete(file.DestPath);
                        }
                    }
                }

                SendStatus("Uninstall complete.");
                return true;
            }
            catch (Exception ex)
            {
                if (debug)
                    SendStatus("Uninstall failed - " + ex.ToString());
                else
                    SendStatus("Uninstall failed - " + ex.Message);
                return false;
            }

        }

        private class FileInfo
        {
            public FileInfo(string srcname, string destdir)
            {
                SourceRes = srcname;
                DestPath = Path.Combine(destdir, srcname);
            }

            public FileInfo(Version version, string srcname, string destdir)
            {
                SourceRes = version.ToString() + "_" + srcname;
                DestPath = Path.Combine(destdir, srcname);
            }

            public string SourceRes;        // The resource name of the source file.
            public string DestPath;         // The destination it gets installed to.
        }
        private List<FileInfo> GetFileList(Version version, bool debug)
        {
            List<FileInfo> files = new List<FileInfo>();

            // Check if we're installing on a 32 or 64 bit OS. Since we're an 'any
            // architecture' .NET application, this will work...
            bool is64 = IntPtr.Size == 8;

            string windowshome = Environment.GetEnvironmentVariable("windir");
            if (debug)
                SendStatus("Windows directory is:" + windowshome);

            // Look for JREs. In the production version we will probably have to search
            // all local drives, or something like that!
            // We have to distinguish between 32 and 64 bit JREs, so we keep a separate
            // list of each.
            List<string> jre32 = new List<string>();
            List<string> jre64 = new List<string>();

            string[] jredirs = { "jre6", "jre7" };

            // Check for JRE directories in Program Files (x86). These must
            // always be 32 bit JREs, and we only check if we know we're on a
            // 64 bit machine.
            if (is64)
            {
                foreach (string jredir in jredirs)
                {
                    string javahome = System.Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                    if (javahome != null)
                    {
                        javahome = Path.Combine(javahome, "Java");
                        javahome = Path.Combine(javahome, jredir);
                        if (!Directory.Exists(javahome))
                            javahome = null;
                    }
                    if (javahome != null)
                        jre32.Add(javahome);
                }
            }

            // Check for JRE directories in Program Files. These will be 32 or 64
            // bit depending on the OS type.
            foreach (string jredir in jredirs)
            {
                string javahome = Environment.GetEnvironmentVariable("ProgramFiles");
                if (javahome != null)
                {
                    javahome = Path.Combine(javahome, "Java");
                    javahome = Path.Combine(javahome, jredir);
                    if (!Directory.Exists(javahome))
                        javahome = null;
                }
                if (javahome != null)
                {
                    if (is64)
                        jre64.Add(javahome);
                    else
                        jre32.Add(javahome);
                }
            }

            List<string> jreall = new List<string>();
            jreall.AddRange(jre32);
            jreall.AddRange(jre64);

            // If there are no JREs present, there's no point doing anything else...
            if (jreall.Count == 0)
            {
                SendStatus("No JREs found");
                return files;
            }

            // Report what JREs we found:
            foreach (string s in jre32)
                SendStatus("Found 32 bit JRE at:" + s);
            foreach (string s in jre64)
                SendStatus("Found 64 bit JRE at:" + s);

            // Common files...
            string destwow = Path.Combine(windowshome, "SYSWOW64");
            string destsys = Path.Combine(windowshome, "SYSTEM32");
            if (is64)
            {
                if (!Directory.Exists(destwow))
                    throw new ApplicationException("Missing " + destwow + " on 64 bit OS?");
                if (version == version200)
                {
                    files.Add(new FileInfo(version, "WindowsAccessBridge.dll", destwow));
                }
                else
                {
                    files.Add(new FileInfo(version, "WindowsAccessBridge-32.dll", destwow));
                    files.Add(new FileInfo(version, "WindowsAccessBridge-64.dll", destsys));
                }
            }
            else
            {
                files.Add(new FileInfo(version, "WindowsAccessBridge.dll", destsys));
            }

            foreach (string jre in jre32)
            {
                string bin = Path.Combine(jre, "bin");
                string lib = Path.Combine(jre, "lib");
                string libext = Path.Combine(lib, "ext");
                if (is64)
                {
                    if (version == version200)
                    {
                        files.Add(new FileInfo(version, "JavaAccessBridge.dll", bin));
                        files.Add(new FileInfo(version, "JAWTAccessBridge.dll", bin));
                        files.Add(new FileInfo(version, "access-bridge.jar", libext));
                    }
                    else
                    {
                        files.Add(new FileInfo(version, "JavaAccessBridge-32.dll", bin));
                        files.Add(new FileInfo(version, "JAWTAccessBridge-32.dll", bin));
                        files.Add(new FileInfo(version, "access-bridge-32.jar", libext));
                    }
                }
                else
                {
                    files.Add(new FileInfo(version, "JavaAccessBridge.dll", bin));
                    files.Add(new FileInfo(version, "JAWTAccessBridge.dll", bin));
                    files.Add(new FileInfo(version, "access-bridge.jar", libext));
                }
                files.Add(new FileInfo(version, "accessibility.properties", lib));
                string jaccess;
                if (version > version200)
                {
                    jaccess = "jaccess.jar";
                }
                else
                {
                    // TODO: perhaps (!) there is a better way of figuring out what
                    // JRE version we're looking at?
                    if (jre.IndexOf("2") != -1)
                        jaccess = "jaccess-1_2.jar";
                    else if (jre.IndexOf("3") != -1)
                        jaccess = "jaccess-1_3.jar";
                    else
                        jaccess = "jaccess-1_4.jar";
                }
                files.Add(new FileInfo(version, jaccess, libext));
            }
            foreach (string jre in jre64)
            {
                if (version == version200 || !is64)
                {
                    SendStatus("Skipping 64 bit JRE at " + jre);
                }
                else
                {
                    string bin = Path.Combine(jre, "bin");
                    files.Add(new FileInfo(version, "JavaAccessBridge-64.dll", bin));
                    files.Add(new FileInfo(version, "JAWTAccessBridge-64.dll", bin));
                    string lib = Path.Combine(jre, "lib");
                    files.Add(new FileInfo(version, "accessibility.properties", lib));
                    string libext = Path.Combine(lib, "ext");
                    files.Add(new FileInfo(version, "access-bridge-64.jar", libext));
                    files.Add(new FileInfo(version, "jaccess.jar", libext));
                }
            }


            if (version >= new Version("2.0.3"))
            {
                foreach (string jre in jreall)
                {
                    string bin = Path.Combine(jre, "bin");
                    files.Add(new FileInfo("jabswitch.exe", bin));
                }
            }

            return files;
        }


        ///--------------------------------------------------------------------------
        /// <summary>
        /// Write an embedded resource to a file.
        /// </summary>
        /// <param name="name">The name of the embedded resource (without the
        /// namespace).</param>
        /// <param name="destpath">The destination path and filename.</param>
        ///--------------------------------------------------------------------------
        private void ResToFile(string name, string destpath)
        {
            SendStatus("...installing " + destpath);
            Stream reader = (this.GetType().Assembly.GetManifestResourceStream("BPJabInstaller." + name));
            if (reader == null)
                throw new ApplicationException("Can't find embedded resource " + name);
            FileStream writer = new FileStream(destpath, FileMode.Create);
            // TODO: Buffer!!!
            for (int i = 0; i < reader.Length; i++)
                writer.WriteByte((byte)reader.ReadByte());
            reader.Close();
            writer.Close();
        }

    }
}
