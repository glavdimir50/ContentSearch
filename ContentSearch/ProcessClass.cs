using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallPython
{
    public static class ProcessClass
    {
        public static string StartPython(string exePath,string filePath, string paras )
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = exePath; 
            start.Arguments = string.Format("\"{0}\" \"{1}\"", filePath, paras);
            start.UseShellExecute = false;
            start.CreateNoWindow = true; 
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string ex = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(ex))
                    {
                        return ex;
                    }

                    string result = reader.ReadToEnd();

                    return result;
                }
            }

        }
    }
}
