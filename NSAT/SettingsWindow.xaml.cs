using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace NSAT
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.Title = "Settings";

        }
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
           
          
            
            if (Directory.Exists(tbWorkspace.Text))
            {
                GlobalSettings.WorkspaceFolder = tbWorkspace.Text;
            }
            if (File.Exists(tbBlastnExec.Text))
            {
                GlobalSettings.BlastnExecutablePath = tbBlastnExec.Text;
            }
            if (Project.projectOpen == true)
            {
                Project.SaveProject();
            }
            GlobalSettings.saveGlobals();
                this.Close();
        }
      

      
        private void btnBrowseWorkspace_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "My Title";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = GlobalSettings.WorkspaceFolder;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = GlobalSettings.WorkspaceFolder;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbWorkspace.Text = System.IO.Path.GetFullPath(dlg.FileName);
            }
        }

        private void btnBrowseBlastn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Executable files|*.exe";
            ofd.FilterIndex = 1;
            if (ofd.ShowDialog() == true)
            {
                tbBlastnExec.Text = System.IO.Path.GetFullPath(ofd.FileName);
            }
            

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

