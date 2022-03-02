using System;
using System.Management;


namespace BluePrism.Datapipeline.Logstash
{
    public class JavaProcessHelper : IJavaProcessHelper
    {
        public int GetProcessIdWthStartupParamsContaining(string textToFind)
        {
                SelectQuery query = new SelectQuery("select ProcessId, CommandLine from Win32_Process where name = 'java.exe'");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    var results = searcher.Get();
                    foreach (var result in results)
                    {
                        var pid = Convert.ToInt32(result.GetPropertyValue("ProcessId"));
                        var cmdline = result.GetPropertyValue("CommandLine") as string;
                        if (cmdline != null && cmdline.ToLower().Contains(textToFind))
                        {
                            return pid;
                        }
                    }
                }

            return -1;
        }
    }
}
