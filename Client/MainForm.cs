using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace TransictionScript
{
    public partial class TransictionScriptClient : Form
    {
        public TransictionScriptClient()
        {
            InitializeLogs();
            InitalizeConfiguration();
            InitializeComponent();

            string status = PingFunction();
        }

        private string PingFunction()
        {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(config.functionUrlbase + "/api/ping?code=" + config.pingFunctionSecret);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                // List data response.
                HttpResponseMessage response = client.GetAsync(client.BaseAddress).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                if (response.IsSuccessStatusCode)
                {
                    labelOnline.Text = "Online Status: Online";
                    return "Online";
                }
                else
                {
                    labelOnline.Text = "Online Status: Offline";
                    return "Offline";
                }

        }

        private void StartButton_Click(object sender, EventArgs e)
        {

            string status = PingFunction();
            if (status == "Offline")
            {
                MessageBox.Show("Services seems to be offline right now","Services Offline",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            progressBar1.Visible = true;
            progressBar1.Value = 10;


            StartButton.Enabled = false;

            foreach (var process in Process.GetProcessesByName("Teams"))
            {
                WriteLog("Killing Teams process", "INFO");
                process.Kill();
            }

            foreach (var process in Process.GetProcessesByName("Outlook"))
            {
                WriteLog("Killing Outlook process", "INFO");
                process.Kill();
            }

            foreach (var process in Process.GetProcessesByName("WINWORD"))
            {
                WriteLog("Killing WINWORD process", "INFO");
                process.Kill();
            }


            foreach (var process in Process.GetProcessesByName("Excel"))
            {
                WriteLog("Killing Excel process", "INFO");
                process.Kill();
            }

            foreach (var process in Process.GetProcessesByName("PowerPoint"))
            {
                WriteLog("Killing PowerPoint process", "INFO");
                process.Kill();
            }

            foreach (var process in Process.GetProcessesByName("OneNote"))
            {
                WriteLog("Killing OneNote process", "INFO");
                process.Kill();
            }





            progressBar1.Value = 20;

            DeleteKey("Software\\Microsoft\\OneDrive");
            DeleteKey("Software\\Policies\\OneDrive");
            DeleteKey("Software\\Microsoft\\Office\\16.0\\Common\\Identity");
            DeleteKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Desktop\\NameSpace");
            DeleteKey("Software\\SyncEngines\\Providers\\OneDrive");
            DeleteKey("Software\\Classes\\CLSID\\{04271989-C4D2-8D26-E559-3BE1753BC3ED}");
            DeleteKey("Software\\Classes\\WOW6432Node\\CLSID\\{04271989-C4D2-8D26-E559-3BE1753BC3ED}");
            DeleteKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Desktop\\NameSpace\\{04271989-C4D2-8D26-E559-3BE1753BC3ED}");
            DeleteKey("Software\\Microsoft\\Office\\Teams\\LoggedInOnce");
            DeleteKey("Software\\Microsoft\\Office\\Teams\\HomeUserUpn");
            DeleteKey("Software\\Microsoft\\Office\\Teams\\DeadEnd");
            DeleteKey("Software\\Microsoft\\Office\\Outlook\\Addins\\TeamsAddin.FastConnect");

            progressBar1.Value = 30;
            AddOutlookKeys();
            progressBar1.Value = 40;
            AddTeamsKeys();
            progressBar1.Value = 80;
            MoveOneDriveFiles();

            progressBar1.Value = 100;
            StartButton.Visible = false;

            
            MainMessage.Visible = false;
            FinishLabel.Visible = true;
            FinishBtn.Visible = true;
            FinishBtn.Enabled = true;

        }

       

        private void MoveOneDriveFiles()
        {

            string Targettenantname = "Iveco Group";

            string ProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string OneDriveTargetPath = ProfilePath + "\\OneDrive - " + Targettenantname;

            System.IO.Directory.CreateDirectory(OneDriveTargetPath);
            WriteLog("Created target OneDrive folder", "INFO");
            var oneDrivesourcePath = Environment.GetEnvironmentVariable("OneDriveCommercial");


            if (oneDrivesourcePath != null)
            {

                try
                {
                    WriteLog("Starting OneDrive content move", "INFO");
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            Arguments = "Move-Item '" + oneDrivesourcePath + "\\*' '" + OneDriveTargetPath + "' -Force -ErrorAction SilentlyContinue",
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    WriteLog("Finished OneDrive content move", "INFO");
                }
                catch (Exception e)
                {
                    WriteLog("Error moving files to new OneDrive path", "ERROR");
                }
                
            }


        }

        public string SessionLogPath;

        public class Configuration
        {
            public string functionUrlbase { get; set; }
            public string pingFunctionSecret { get; set; }
        }
        public Configuration config=new Configuration();

        private void InitalizeConfiguration()
        {
            if (File.Exists("config.json"))
            {
                config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
            } else
            {
                System.Windows.Forms.Application.Exit();
            }
        }


        private void InitializeLogs()
        {
            string folderName = @".\Logs";
            // If directory does not exist, create it
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            SessionLogPath = folderName + "\\ExecutionLog." + DateTime.Now.ToString("yyyyMMddHHmm") + ".log";

            using (StreamWriter sw = File.AppendText(SessionLogPath))
            {
                sw.WriteLine("TimeStamp;Severity;Message");
            }

            WriteLog("Transiction Script started", "INFO");
        }


        private void WriteLog(string message,string severity) 
        {
            string path = SessionLogPath;
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyyMMddHHMMss") + ";" + severity +";" + message);
            }
        }
        


        private void DeleteKey (string keyf)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(keyf);
                WriteLog("Deleted key " + keyf, "INFO");
            } catch (Exception e)
            {
                WriteLog("Failed to delete key " + keyf , "WARNING");
            }
        }


        private void AddOutlookKeys()
        {
            try
            {

                Microsoft.Win32.RegistryKey Teamsmykey;
                Teamsmykey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Office\\Teams");
                Teamsmykey.SetValue("SkipUpnPrefill", "1");
                Teamsmykey.Close();


                Microsoft.Win32.RegistryKey OutlookKeys;
                OutlookKeys = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Office\\16.0\\Outlook\\Profiles\\TSClientProfile");
                OutlookKeys.Close();


                Microsoft.Win32.RegistryKey OutlookKeys2;
                OutlookKeys2 = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Office\\16.0\\Outlook");
                OutlookKeys2.SetValue("DefaultProfile", "TSClientProfile");
                OutlookKeys2.Close();


                WriteLog("Create required keys for Outlook", "INFO");
            }
            catch (Exception e)
            {
                WriteLog("Failed to add Outlook keys", "ERROR");
            }
        }



        private void AddTeamsKeys()
        {
            try
            {

                Microsoft.Win32.RegistryKey Teamsmykey;
                Teamsmykey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Office\\Teams");
                Teamsmykey.SetValue("SkipUpnPrefill", "1");
                Teamsmykey.Close();

                WriteLog("Create required keys for Teams", "INFO");
            }
            catch (Exception e)
            {
                WriteLog("Failed to add Teams", "ERROR");
            }
        }

        private void FinishBtn_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
    }
}
