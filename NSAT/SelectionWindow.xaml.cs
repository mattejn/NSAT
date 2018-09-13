using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace NSAT
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    /// 
    public partial class SelectionWindow : Window

    {

        private List<Project.ItemsToComputeANITETRA> uniqueFastaPairs = new List<Project.ItemsToComputeANITETRA>();
        private List<Project.ItemsSelectedToCompute> tetra = new List<Project.ItemsSelectedToCompute>();
        private List<Project.ItemsSelectedToCompute> anib = new List<Project.ItemsSelectedToCompute>();
        internal static SelectionWindow main;
        internal string Status
        {
            set { lblStatusANIT.Content = value; }
        }



        private void GeneratePairs()
        {

            for (int i = 0; i < Project.fastas.Count -1; i++)
            {
                for (int k = i+1; k < Project.fastas.Count; k++)
                {
                    uniqueFastaPairs.Add(new Project.ItemsToComputeANITETRA()
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(Project.fastas[i]) + "vs" +
                        System.IO.Path.GetFileNameWithoutExtension(Project.fastas[k]),
                        FullPath = System.IO.Path.GetFullPath(Project.fastas[i]),
                        FullPathB = System.IO.Path.GetFullPath(Project.fastas[k]),
                        ANIB = true,
                        TETRA = false,

                    });
                }
            }


        }
       
        public SelectionWindow()
        {
           GeneratePairs();
            InitializeComponent();
            this.Title = "ANIb and TETRA calculation";
            main = this;
            lbSelection.ItemsSource = uniqueFastaPairs;
            btnCancel.IsEnabled = true;
            btnCompute.IsEnabled = true;
            lbSelection.IsEnabled = true;
        }

        private void btnCompute_Click(object sender, RoutedEventArgs e)
        {


            btnCancel.IsEnabled = false;
            btnCompute.IsEnabled = false;
            lbSelection.IsEnabled = false;
            Status = "Calculation in progress, please wait";
            foreach (var pair in uniqueFastaPairs)
            {

                if (pair.ANIB == true)
                {


                    anib.Add(new Project.ItemsSelectedToCompute()
                    {
                        FullPath = pair.FullPath,
                        FullPathB = pair.FullPathB,
                        Title = pair.Title

                    });
                }
                if (pair.TETRA == true)
                {
                    tetra.Add(new Project.ItemsSelectedToCompute()
                    {
                        FullPath = pair.FullPath,
                        FullPathB = pair.FullPathB,
                        Title = pair.Title


                    }

                    );
                }
            }
            int tetrasToComp = tetra.Count;
            int anibToComp = anib.Count;
            int errorsANI = 0;
            bool successTETRA = false;
            if (tetrasToComp > 0)
            {
                successTETRA = TETRA.computeTetra(tetra);
                if (successTETRA == true)
                {
                    string inf="Tetranucleotide calculation successful. No errors.";
                    MessageBox.Show(inf, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Status = inf;
                    MainWindow.main.Status = inf;
                    MainWindow.main.EnableMatrixC();
                }
            
                else 
                {
                    string error = "Tetranucleotide calculation failed for one or more pairs.";
                    MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    lblStatusANIT.Content = error;
                    MainWindow.main.lblStatus.Content = error;
                    Project.ErrorLog.Add(error);
                }
            }
            if (anibToComp > 0)
            { string dir = Project.folderFullPath + "\\temp\\ani";
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
                errorsANI = ANI.computeANIb(anib);

                if (errorsANI == 0)
                {
                    string inf = "ANIb calculation successful. No errors.";
                    MessageBox.Show(inf, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Status  = inf;
                    MainWindow.main.Status = inf;
                    MainWindow.main.EnableMatrixC();
                }
                else if (errorsANI == 1)
                {

                    string inf = " All pairs have already been processed and their ANIb results are available.";
                    MessageBox.Show(inf, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Status = inf;
                    MainWindow.main.Status  = inf;
                    MainWindow.main.EnableMatrixC();
                }
                else if (errorsANI == 2)
                {

                    string inf = "ANIb calculation finished with errors.Missing files and respective pairs have been removed from the project.For more informations check logs.";
                    MessageBox.Show(inf, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Status  = inf;
                    MainWindow.main.Status  = inf;
                    MainWindow.main.EnableMatrixC();
                }
                else if (errorsANI == 3)
                {
                    string error = "ANIb calculation failed for all selected pairs. Missing files and respective pairs have been removed from the project. For more informations check logs.";
                    MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Status  = error;
                    MainWindow.main.Status  = error;
                    Project.ErrorLog.Add(error);
                }
            }

            this.Close();
           
    }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var pair in uniqueFastaPairs)
            {
                pair.ANIB = true;
            }
            lbSelection.ItemsSource = null;
            lbSelection.ItemsSource = uniqueFastaPairs;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var pair in uniqueFastaPairs)
            {
                pair.ANIB = false;
            }
            lbSelection.ItemsSource = null;
            lbSelection.ItemsSource = uniqueFastaPairs;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var pair in uniqueFastaPairs)
            {
                if (pair.ANIB)
                {
                    pair.ANIB = false;
                }
                else { pair.ANIB = true; }
            }
            lbSelection.ItemsSource = null;
            lbSelection.ItemsSource = uniqueFastaPairs;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            foreach (var pair in uniqueFastaPairs)
            {
                pair.TETRA = true;
            }
            lbSelection.ItemsSource = null;
            lbSelection.ItemsSource = uniqueFastaPairs;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            foreach (var pair in uniqueFastaPairs)
            {
                pair.TETRA = false;
            }
            lbSelection.ItemsSource = null;
            lbSelection.ItemsSource = uniqueFastaPairs;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            foreach (var pair in uniqueFastaPairs)
            {
                if (pair.TETRA)
                {
                    pair.TETRA = false;
                }
                else { pair.TETRA = true; }
            }
            lbSelection.ItemsSource = null;
            lbSelection.ItemsSource = uniqueFastaPairs;
        }
        private void ANIbSelect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in lbSelection.SelectedItems)
            {
                string bla = item.ToString();
            }
            lbSelection.ItemsSource = null;
            lbSelection.ItemsSource = uniqueFastaPairs;
        }
        private void TETRASelect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var pair in uniqueFastaPairs)
            {
                if (pair.TETRA)
                {
                    pair.TETRA = false;
                }
                else { pair.TETRA = true; }
            }
            lbSelection.ItemsSource = null;
            lbSelection.ItemsSource = uniqueFastaPairs;
        }
        private void ATSelect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var pair in uniqueFastaPairs)
            {
                if (pair.TETRA)
                {
                    pair.TETRA = false;
                }
                else { pair.TETRA = true; }
            }
            lbSelection.ItemsSource = null;
            lbSelection.ItemsSource = uniqueFastaPairs;
        }
    }
}