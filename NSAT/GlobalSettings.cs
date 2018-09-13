using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace NSAT
{
    class GlobalSettings
    {
        
        public static string WorkspaceFolder;
        public static string BlastnExecutablePath;
        public static bool loadGlobals()
        {//Loads global settings on the application launch, if there are none present in the registry creates registry key with default values and a workspace folder
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            if (key.OpenSubKey("NSAT") == null)
            {
                key.CreateSubKey("NSAT");
            }
            key = key.OpenSubKey("NSAT", true);
            if (key.OpenSubKey("Global") == null)
            {
                key.CreateSubKey("Global");
            }
            key = key.OpenSubKey("Global", true);

            WorkspaceFolder = (string)key.GetValue("workspace");
            if (WorkspaceFolder == null)
            {
                WorkspaceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NSAT");
            }
            if (!Directory.Exists(WorkspaceFolder))
            {
                string newWorkspace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NSAT");
                MessageBox.Show("Directory " + WorkspaceFolder + " is not valid, using the default " + newWorkspace + " instead.");
                WorkspaceFolder = newWorkspace;
            }
            key.SetValue("workspace", WorkspaceFolder);
            BlastnExecutablePath = (string)key.GetValue("blastn");
            if (BlastnExecutablePath == null)
            {
                MessageBox.Show("Fill the path to blastn.exe in Tools->Settings");
            }
            return true;
        }
        public static bool saveGlobals()
        {//saves (changes) global settings
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key = key.OpenSubKey("NSAT", true);
            key = key.OpenSubKey("Global", true);
            key.SetValue("workspace", WorkspaceFolder);
            key.SetValue("blastn", BlastnExecutablePath);
            return true;

        }
    }
}
