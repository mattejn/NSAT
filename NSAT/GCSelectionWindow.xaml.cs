using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace NSAT
{
    /// <summary>
    /// Interaction logic for GCSelectionWindow.xaml
    /// </summary>
    public partial class GCSelectionWindow : Window
    {
        private List<Project.ItemsToComputeGC> fastasGC = new List<Project.ItemsToComputeGC>();
        internal static GCSelectionWindow main;
        internal string Status
        {
            set { lblStatusGC.Content = value; }
        }
        private void GC()
        {
            int success = 0;
            int errors = 0;
            if (fastasGC.Count >= 1)
            {
                foreach (var fasta in fastasGC)
            {

                if (fasta.GC == true)
                {
                        Status  = "Computing GC content " + Path.GetFileNameWithoutExtension(fasta.FullPath);

                    if (GCC.computeGCcontent(fasta.FullPath) == true)
                    { success++; }
                    else
                    { 
                        string error="File" + System.IO.Path.GetFileNameWithoutExtension(fasta.FullPath) + "no longer exists (it has been deleted, moved or renamed) - removing the file from the project.";
                        Project.ErrorLog.Add(error);
                        lblStatusGC.Content = error;

                        Project.fastas.Remove(fasta.FullPath);
                
                        errors++;
                    }
                }

            }
            }
            else { string error = "Atleast one valid fasta file must be loaded to compute GC content.";
                MessageBox.Show(error, "Warning", MessageBoxButton.OK,MessageBoxImage.Warning);
                Status =  error;
            }

            if (errors != 0 && success > 0)
            {
                
                string inf = "GC content finished but some files were missing or not accessible, these files were removed from the project.";
                MessageBox.Show(inf, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Status =  inf;
                MainWindow.main.Status = inf;
                MainWindow.main.EnableMatrixC();
            }
            else if (errors == 0 && success > 0)
            {
               string inf="Computation finished successfully";
                MessageBox.Show(inf, "Sucess", MessageBoxButton.OK, MessageBoxImage.Information);
                Status = inf;
                MainWindow.main.Status = inf;
                MainWindow.main.EnableMatrixC();
            }
            else
            {
               

                string error = "GC conent computation failed for all pairs. Missing files and respective pairs have been removed from the project. For more informations check logs.";
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Status =  error;
                MainWindow.main.Status = error;
                Project.ErrorLog.Add(error);
            }
        }
        public GCSelectionWindow()
        {
            foreach (string fasta in Project.fastas)
            {
                if (File.Exists(System.IO.Path.GetFullPath(fasta)))
                {
                    fastasGC.Add(new Project.ItemsToComputeGC()
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(fasta),
                        FullPath = fasta,
                        GC = false
                    });
                }
                else
                {
                    string error="File" + System.IO.Path.GetFileNameWithoutExtension(fasta) + "no longer exists (It has been renamed, deleted or moved) - deleting the file from the project";
                    Status = error;
                    Project.ErrorLog.Add(error);
                    Project.fastas.Remove(fasta);
                }


            }
            InitializeComponent();
            this.Title = "GC percentage and sequence information";
            main = this;
            btnComputeGC.IsEnabled = true;
            btnCancel.IsEnabled = true;
            lbSelectionGC.IsEnabled = true;
            lbSelectionGC.ItemsSource = fastasGC;
        }

        private void btnComputeGC_Click(object sender, RoutedEventArgs e)
        {
            GC();
            btnComputeGC.IsEnabled = false;
            btnCancel.IsEnabled = false;
            lbSelectionGC.IsEnabled = false;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
