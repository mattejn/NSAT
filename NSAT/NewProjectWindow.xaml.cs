using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NSAT
{
    /// <summary>
    /// Interaction logic for NewProjectWindow.xaml
    /// </summary>
    public partial class NewProjectWindow : Window
    {
      
        public NewProjectWindow()
        {
            InitializeComponent();
            this.Activate();
            this.Title = "New project";
           
        }
        private void NewPrev(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "^[^ a - zA - Z0 - 9æøåÆØÅ_ -] + $");
        }

        private void NewProjectOk_Click(object sender, RoutedEventArgs e)
        {
            if (Project.NewProject(tbProjectName.Text) == true)
            {
                MainWindow.main.Status = "Project " + Project.projectName + " created successfuly.";
                MainWindow.main.DisableAllC();
                MainWindow.main.EnableImportC();
             
            }
            else
            {
                MainWindow.main.Status = "Project creation failed.";
            }
            

            this.Close();
        }

        private void NewProjectCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
