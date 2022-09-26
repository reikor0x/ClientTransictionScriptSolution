using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TransictionScript
{
    public partial class TransictionScriptClient : Form
    {
        public TransictionScriptClient()
        {
            InitializeLogs();
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

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


            AddOutlookKeys();

            AddTeamsKeys();

            MoveOneDriveFiles();
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
                Directory.Move(oneDrivesourcePath, OneDriveTargetPath);
            }


        }

        public string SessionLogPath;
            
        
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


    }
}
