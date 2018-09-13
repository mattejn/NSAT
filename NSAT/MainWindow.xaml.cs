using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace NSAT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
            this.Title = "NSAT";
            GlobalSettings.loadGlobals();

            InitializeComponent();
            lblStatus.SetBinding(ContentProperty, new Binding("Status"));
            main = this;
            DisableAllC();
            main.Status = "Start by creating a new project or opening one.";
            


        }
        internal static MainWindow main;
        internal string Status
        {
            set { lblStatus.Content = value; }
        }

       
        internal void EnableImportC()
        {

            Import.IsEnabled = true;
            FromFiles.IsEnabled = true;
            DeleteProject.IsEnabled = true;

        }

        internal void DisableComputeC()
        {
            ComputeGC.IsEnabled = false;
            ComputeTA.IsEnabled = false;
        }
        internal void DisableMatrixC()
        {
            AniCSV.IsEnabled = false;
            AniCSVAvg.IsEnabled = false;
            GCtoCSV.IsEnabled = false;
            TETRAtoCSV.IsEnabled = false;
        }
        internal void EnableMatrixC()
        { if (Directory.GetFiles(Project.folderFullPath + "\\ANI\\", "*.ani", SearchOption.TopDirectoryOnly).Length != 0)
            {
                AniCSV.IsEnabled = true;
                AniCSVAvg.IsEnabled = true;
                ANIbToDendrogram.IsEnabled = true;
                ResetANIb.IsEnabled = true;
            }
            else {
                AniCSV.IsEnabled = false;
                AniCSVAvg.IsEnabled = false;
                ANIbToDendrogram.IsEnabled = false;
                ResetANIb.IsEnabled = false;
            }
            if (Directory.GetFiles(Project.folderFullPath + "\\GC\\", "*.gc", SearchOption.TopDirectoryOnly).Length != 0)
            {
                GCtoCSV.IsEnabled = true;
                ResetGC.IsEnabled = true;
            }
            else
            {
                GCtoCSV.IsEnabled = false;
                ResetGC.IsEnabled = false;
            }
            if (Directory.GetFiles(Project.folderFullPath + "\\TETRA\\", "*.tet", SearchOption.TopDirectoryOnly).Length != 0)
            {
                TETRAtoCSV.IsEnabled = true;
                ResetTETRA.IsEnabled = true;
            }
            else
            {
                TETRAtoCSV.IsEnabled =false;
                ResetTETRA.IsEnabled = false;
            }
        }
        internal void DisableAllC()
        {
            ComputeGC.IsEnabled = false;
            ComputeTA.IsEnabled = false;
            AniCSV.IsEnabled = false;
            AniCSVAvg.IsEnabled = false;
            GCtoCSV.IsEnabled = false;
            TETRAtoCSV.IsEnabled = false;
            FromFiles.IsEnabled = false;
            FromNCBI.IsEnabled = false;
            ANIbToDendrogram.IsEnabled = false;
            DeleteProject.IsEnabled = false;
            ResetANIb.IsEnabled = false;
            ResetGC.IsEnabled = false;
            ResetTETRA.IsEnabled = false;


        }
        
        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            NewProjectWindow NPW = new NewProjectWindow();
            NPW.Show();
            NPW.Owner = this;
            NPW.Show();
            lbFastas.Items.Clear();
            EnableImportC();
          

        }
        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove the " + Project.projectName + " project?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                Project.DeleteProject();
            }
            if (Project.projectName==null)
            {
                DisableAllC();
                lbFastas.Items.Clear();
            }
        }

            private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = GlobalSettings.WorkspaceFolder;
            ofd.Filter = "NSAT project|*.nsat";


            if (ofd.ShowDialog() == true && ofd.FileName != null)
            {
                lblStatus.Content = "Loading project and verifyig files, please wait";
                if (Project.OpenProject(ofd.FileName) == true)
                {
                    
                        EnableImportC();
                    if (Project.fastas.Count > 1)
                    {
                        EnableMatrixC();
                        EnableCompC();
                    }
                    

                    lbFastas.Items.Clear();
                    foreach (string fasta in Project.fastas)
                    {
                        lbFastas.Items.Add(System.IO.Path.GetFileNameWithoutExtension(fasta));
                    }
                    Project.SaveProject();
                    
                }
                else
                {
                    MessageBox.Show("Unable to open project.");
                }
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow SW = new SettingsWindow();
            SW.Show();
            SW.Owner = this;
            SW.Show();

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EnableCompC()
        {
            ComputeGC.IsEnabled = true;
            ComputeTA.IsEnabled = true;
        }
        private void FromFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd1 = new OpenFileDialog();
            ofd1.Multiselect = true;
            ofd1.Filter = "Text files|*.txt|FASTA files|*.fasta;*.fas;*.fsa";
            ofd1.FilterIndex = 2;
            if (ofd1.ShowDialog() == true)
            {
                lblStatus.Content = "Importing and validating files, please wait";
                foreach (var filename in ofd1.FileNames)
                {
                    if (Project.importFastaFF(filename))
                    {
                        lbFastas.Items.Add(System.IO.Path.GetFileNameWithoutExtension(filename));
                    }
                }
               
                    Project.SaveProject();
                    if (Project.fastas.Count > 1)
                    {
                        EnableCompC();
                    }
                Dispatcher.InvokeAsync(() =>
                {
                    Status = "Importing finished";
                });
                Dispatcher.InvokeAsync(() =>
                {
                    lblStatus.Content = "Importing finished";
                });
            }
            
            

        }

        

        private void ComputeTA_Click(object sender, RoutedEventArgs e)
        {
            SelectionWindow SlW = new SelectionWindow();
            SlW.Show();
            SlW.Owner = this;
            SlW.Show();
            Project.SaveProject();
        }

        private void ComputeGC_Click(object sender, RoutedEventArgs e)
        {
            GCSelectionWindow SlW = new GCSelectionWindow();
            SlW.Show();
            SlW.Owner = this;
            SlW.Show();
            Project.SaveProject();

        }

     

        private void GCtoCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Project.folderFullPath;
            saveFileDialog1.FileName = Project.projectName + "_GC_Content";
            saveFileDialog1.Filter = "table (.csv)|*.csv";
            if (saveFileDialog1.ShowDialog() == true)
            {
                if (GCC.generateCSVGc(saveFileDialog1.FileName)==true) MainWindow.main.lblStatus.Content = "Table generated to: " + System.IO.Path.GetFileName(saveFileDialog1.FileName);
                else {
                    MainWindow.main.lblStatus.Content = "Generating GC table failed (No results?).";
                    Project.ErrorLog.Add("Generating GC table failed (No results?).");
                }
            }
        }

        private void AniCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Project.folderFullPath;
            saveFileDialog1.FileName = Project.projectName + "_ANIb_Full";
            saveFileDialog1.Filter = "table (.csv)|*.csv";
            saveFileDialog1.FilterIndex = 2;
            if (saveFileDialog1.ShowDialog() == true)
            {
                if (ANI.GenerateCSVAni(saveFileDialog1.FileName) == true) MainWindow.main.lblStatus.Content = "Table generated to: " + System.IO.Path.GetFileName(saveFileDialog1.FileName);
                else { MainWindow.main.lblStatus.Content = "Failed (No results?)."; }
            }
        }

        private void AniCSVAvg_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Project.folderFullPath;
            saveFileDialog1.FileName = Project.projectName + "_ANIb_Averaged";
            saveFileDialog1.Filter = "table (.csv)|*.csv";
            saveFileDialog1.FilterIndex = 2;
            if (saveFileDialog1.ShowDialog() == true)
            {
                if (ANI.GenerateCSVAniAvg(saveFileDialog1.FileName) == true) MainWindow.main.lblStatus.Content = "Table generated to: " + System.IO.Path.GetFileName(saveFileDialog1.FileName);
                else { MainWindow.main.lblStatus.Content = "Failed (No results?)."; }
            }
        }

        private void TETRAtoCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Project.folderFullPath;
            saveFileDialog1.FileName = Project.projectName + "_TETRA";
            saveFileDialog1.Filter = "table (.csv)|*.csv";
            saveFileDialog1.FilterIndex = 2;
            if (saveFileDialog1.ShowDialog() == true)
            {
                if (TETRA.GenerateCSVTETRA(saveFileDialog1.FileName) == true) MainWindow.main.lblStatus.Content = "Table generated to: " + System.IO.Path.GetFileName(saveFileDialog1.FileName);
                else { MainWindow.main.lblStatus.Content = "Failed (No results?)."; }
            }
        }



        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (lbFastas.SelectedItems.Count !=0)
            { 
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove selected files from the project?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    
                    foreach (var item in lbFastas.SelectedItems)
                    {
                        string itemS = item.ToString();
                        try
                        {
                            
                            var todel = Project.fastas.Find(x => x.Contains(itemS));
                            Project.fastas.Remove(todel);
                        }
                        catch
                        {
                            string error = "Failed to delete " + itemS + ".";
                            MainWindow.main.lblStatus.Content = error;
                            Project.ErrorLog.Add(error);
                        }
                    }
                    lbFastas.Items.Clear();
                    foreach (string fasta in Project.fastas)
                    {
                        lbFastas.Items.Add(System.IO.Path.GetFileNameWithoutExtension(fasta));
                    }
                    Project.SaveProject();
                }
        } }

        private void ResetANIb_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete all results from ANIb computation in this project?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
                foreach (string file in Directory.GetFiles(Project.folderFullPath+"//ANI//", "*.ani").Where(item => item.EndsWith(".ani")))
            {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        string error = "Unable to delete file " + Path.GetFullPath(file) + ". Check permissions"; 
                    }
            }
            DisableMatrixC();
        }

        private void ResetGC_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete all results from GC computation in this project?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
                foreach (string file in Directory.GetFiles(Project.folderFullPath + "//GC//", "*.gc").Where(item => item.EndsWith(".gc")))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        string error = "Unable to delete file " + Path.GetFullPath(file) + ". Check permissions";
                    }
                }
            DisableMatrixC();
        }

        private void ResetTETRA_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete all results from TETRA computation in this project?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
                foreach (string file in Directory.GetFiles(Project.folderFullPath + "//TETRA//", "*.tetra").Where(item => item.EndsWith(".tetra")))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        string error = "Unable to delete file " + Path.GetFullPath(file) + ". Check permissions";
                    }
                }
            DisableMatrixC();
        }
    }
}
