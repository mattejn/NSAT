using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace NSAT
{
    class Project
    {//this class manages project strucutre and its files
        //there is no project=>save funcitonality in ui itself since the program saves automatically at any time, when it makes sesne.
        public static bool projectOpen = false;



        public static string folderFullPath;//full path to the project folder
        public static string projectName;//just the name
        public static List<string> fastas;//list of fasta files currently imported into the project (contains full paths)
        public static List<string> ErrorLog;//List of errors, only from current session, errors from previous sessions are accessible in the projects error log
        public class  ItemsToCompute
        {
            public string Title { get; set; }

            public string FullPath { get; set; }


        }
        public class ItemsSelectedToCompute : ItemsToCompute
        {
            public string FullPathB { get; set; }
        }
        public class ItemsToComputeGC : ItemsToCompute
        {
            public bool GC { get; set; }
        }
        public class ItemsToComputeANITETRA : ItemsSelectedToCompute
        {

            public bool ANIB { get; set; }
            public bool TETRA { get; set; }

        }
        public static bool NewProject(string name)

        {//create a new project with initializing all the Project variables anew and reading global variables from registry, if available
            //Also calls SaveProject() to save default configurations 
            if (projectOpen == true)
            {
                SaveProject();

               
                fastas.Clear();
            }
            fastas = new List<string>();
            ErrorLog = new List<string>();

            if (!Directory.Exists(GlobalSettings.WorkspaceFolder))
            {
                Directory.CreateDirectory(GlobalSettings.WorkspaceFolder);
            }
            if (Directory.Exists(GlobalSettings.WorkspaceFolder + "\\" + name))
            {

                MessageBox.Show("Project not created. A project with the name " + name + "already exists.");
            }
            Directory.CreateDirectory(GlobalSettings.WorkspaceFolder + "\\" + name);
            if (!Directory.Exists(GlobalSettings.WorkspaceFolder + "\\" + name))
            {
                string error = "Error creating project folder" + GlobalSettings.WorkspaceFolder + "\\" + name + ". Check permissions or try changing workspace folder in preferences";
                MessageBox.Show(error, "Error creating project", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
                try
                {
                    Directory.CreateDirectory(GlobalSettings.WorkspaceFolder + "\\" + name + "\\fastas");
                    Directory.CreateDirectory(GlobalSettings.WorkspaceFolder + "\\" + name + "\\ANI");
                    Directory.CreateDirectory(GlobalSettings.WorkspaceFolder + "\\" + name + "\\TETRA");
                    Directory.CreateDirectory(GlobalSettings.WorkspaceFolder + "\\" + name + "\\GC");
                }
                catch
                {
                    string error = "Problems with access to project folder " + GlobalSettings.WorkspaceFolder + "\\" + name + ". Check permissions";
                    MainWindow.main.lblStatus.Content = error;
                    ErrorLog.Add(error);
                    MessageBox.Show(error, "Error creating project", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            projectName = name;
            folderFullPath = GlobalSettings.WorkspaceFolder + "\\" + name;
            projectOpen = true;
            SaveProject();
            MainWindow.main.lblStatus.Content = "Project " + projectName + " created successfuly";

            return true;
        }
        public static void DeleteProject()//Delete the current project
        {
            bool error = false;
            if (projectOpen == false) MainWindow.main.Status = "Nothing to delete"; ; 
           
            
            try
            {
                Directory.Delete(folderFullPath, true);
                MainWindow.main.Status = "Project deleted successfuly";
            }
            catch { error = true; MainWindow.main.Status = "Project deletion failed, check permission"; }
            if (error == false) {
                folderFullPath = null;
                projectName = null;
                projectOpen = false;


                if (fastas != null)
                {
                    fastas.Clear();
                }
            }
        }
        public static bool OpenProject(string path)
        {//Opening a new project, if a project is already open, it will first save it, then open the new one.
            if (projectOpen == true)
            {
                SaveProject();
                
            }
            bool error = false;
            try
            {
                StreamReader reader = new StreamReader(path);
                int count = Int32.Parse(reader.ReadLine());

               List<string> np = new List<string>();
                ErrorLog = new List<string>();
             
                if (count != 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string line = reader.ReadLine();

                        if (File.Exists(line) && ValidateFile(line))
                        {
                            np.Add(line);
                        }
                        else
                        {
                            error = true;
                        }

                    }
                }

                folderFullPath = Path.GetDirectoryName(path);
                projectName = Path.GetFileNameWithoutExtension(path);
                projectOpen = true;
                reader.Close();
                
                if (fastas!= null)
                {
                    fastas.Clear();
                }
                    
             
                fastas = np;
                if (error) MainWindow.main.Status = "Project " + projectName + " was loaded, but some files were invalid, missing or inaccessible, those files were removed.";

                else MainWindow.main.Status = "Project " + projectName + " loaded successfuly";

                return true;
            }
            catch
            {
                MainWindow.main.Status = "Opening project " + projectName + " failed";

                return false;
            }
        }

        public static bool SaveProject()
        {//Saves project(paths to fasta files) and project-specific configuration to a file

            if (projectOpen)
            {
                string[] project = new string[1 + fastas.Count];

                project[0] = fastas.Count.ToString();
                int i = 1;
                foreach (string fasta in fastas)
                {
                    project[i] = fasta;
                    i++;
                }
                if (Directory.Exists(folderFullPath))
                {
                    if (File.Exists(folderFullPath + "\\" + projectName + ".nsat"))
                    {
                        try
                        {
                            File.Delete(folderFullPath + "\\" + projectName + ".nsat");
                        }
                        catch
                        {
                            string error = "Problems with access to " + folderFullPath + "\\" + projectName + ".nsat file. Check permissions.";
                            MainWindow.main.Status = error;
                            ErrorLog.Add(error);
                            MessageBox.Show(error, "Error saving project", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                }
                else
                {
                    string error = "Problems with access to " + folderFullPath + ". Which is the project folder for the current project. Check permissions.";
                    MainWindow.main.lblStatus.Content = error;
                    ErrorLog.Add(error);
                    MessageBox.Show(error, "Error saving project", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                try
                {
                    StreamWriter writer = new StreamWriter(folderFullPath + "\\" + projectName + ".nsat");

                    foreach (string s in project)
                    {
                        writer.WriteLine(s);
                    }
                    writer.Close();
                }
                catch
                {
                    string error = "Problems writing to " + folderFullPath + ". Which is the project folder for the current project. Check permissions.";
                    MainWindow.main.Status = error;
                    ErrorLog.Add(error);
                    MessageBox.Show(error, "Error saving project", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            if (File.Exists(folderFullPath + "//errorlog.log"))
            {
                try
                {
                    StreamWriter sw = File.AppendText(folderFullPath + "//errorlog.log");
                    foreach (string error in ErrorLog)
                    {
                        sw.WriteLine(error);

                    }
                    sw.Close();
                }

                catch
                {
                    string error = "Problems accessing " + folderFullPath + "//errorlog.log. Which is the projects error log. Check permissions.";
                    MainWindow.main.Status = error;
                    ErrorLog.Add(error);
                    MessageBox.Show(error, "Error saving project", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }

            else
            {
                try
                {
                    StreamWriter writer = new StreamWriter(folderFullPath + "//errorlog.log");
                    foreach (string error in ErrorLog)
                    {
                        writer.WriteLine(error);

                    }
                    writer.Close();
                }
                catch
                {
                    string error = "Problems writing to " + folderFullPath + ". Which is the project folder for the current project. Check permissions.";
                    MainWindow.main.Status = error;
                    ErrorLog.Add(error);
                    MessageBox.Show(error, "Error saving project", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;

        }
        public static bool importFastaFF(string name)
        {//Imports fasta files passed by openfiledialo, checks for duplicity and availability of files
         //add validation
            MainWindow.main.lblStatus.Content = "Importing and validating files, please wait.";
                if (File.Exists(Path.GetFullPath(name)))
                {
                    if (!fastas.Contains(Path.GetFullPath(name)))
                    {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                    {
                        MainWindow.main.lblStatus.Content = "Validating " + Path.GetFileNameWithoutExtension(name);
                    }));
                    if (ValidateFile(name))
                    {
                        fastas.Add(Path.GetFullPath(name));
                        return true;
                    }
                        
                        else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindow.main.lblStatus.Content = "File " + Path.GetFileName(name) + " is not valid. Skipping.";
                        });
                        return false;

                        }



                    }
                    else
                    {
                        MainWindow.main.lblStatus.Content = "File " + Path.GetFileName(name) + " has been already imported. Skipping.";
                    return false;
                    }
                }
                else
                {
                    if (!fastas.Contains(Path.GetFullPath(name)))
                    {
                        string error1 = "File" + Path.GetFileNameWithoutExtension(name) + "no longer exists (It has been renamed, deleted or moved) - deleting file from project";
                        MainWindow.main.lblStatus.Content = error1;
                        ErrorLog.Add(error1);
                    return false;
                    }
                    string error = "File" + Path.GetFileNameWithoutExtension(name) + "no longer exists (It has been renamed, deleted or moved) - skipping";
                    MainWindow.main.lblStatus.Content = error;
                    ErrorLog.Add(error);
                return false;
                }
            


       
        }
        public static bool ValidateFile(string path)
        {
            StreamReader reader = new StreamReader(path);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.main.lblStatus.Content = "Validating " + Path.GetFileNameWithoutExtension(path);
            });

            string line = reader.ReadLine();
            line = line.TrimStart();
            for (int i = 0; i < 10; i++)
            {
                if (line == "") line = reader.ReadLine();
                else if (line.StartsWith(">"))
                {
                    break;
                }
            }
            line = line.TrimStart();
            if (!line.StartsWith(">"))
            {
                return false;
            }
            line = reader.ReadLine();
            while (line != null)
            {
                line = line.TrimStart();
                if (line.StartsWith(">")) line = reader.ReadLine();
                else
                {
                    line = line.ToLower();
                    foreach (char i in line)
                    {


                        if (i == 'g' || i == 'c' || i == 's' || i == 't' || i == 'a' || i == 'w' || i == 'r' || i == 'y' || i == 'm' || i == 'k' || i == 'h' || i == 'b' || i == 'v' || i == 'd' || i == 'n' || i == '\r' || i == ' ' || i == '\t')
                        {
                            line = reader.ReadLine();
                        }
                        else return false;
                    }

                }
                line = reader.ReadLine();
            }
            return true;
        }
    }
}

