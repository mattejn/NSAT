using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace NSAT
{
    public class GCC
    {
        public static bool computeGCcontent(string path)
        { //First this takes a path to given fasta file and checks if it exists, then it removes all fasta headers
          //Takes sequence as a string and its name, cleans it to leave only characters relevant to GC content computation, 
          //computes GC content and saves its value along with total sequence lenght and number of sequence fragments (fasta headers)
          // into a file with a corresponding name
          //if there is a problem with invalid input (which results in no headers or GC count being 0) returns false
          // It was originally split into removeheader and the rest, but this way it only has to iterate through all the lines and search for headers once, instead of twice
            if (File.Exists(path) && !File.Exists(Project.folderFullPath + "\\GC\\" + Path.GetFileNameWithoutExtension(path) + ".gc"))
            {
              
                try
                {
                    uint GCCount = 0;
                    uint ATCount = 0;
                    uint A = 0;
                    uint G = 0;
                    uint C = 0;
                    uint T = 0;
                    uint N = 0;
                    uint X = 0;
                    uint OverallCount = 0; //
                    uint FragCount = 0;

                    StreamReader reader = new StreamReader(path);
                        string line = reader.ReadLine();
                        while (line != null)
                        {
                            if (line.StartsWith(">"))
                            {
                            FragCount++;
                            }
                            else
                        {
                            line = line.ToUpper();
                            foreach (char i in line)
                            {

                                if (i == 'G')
                                {
                                    GCCount++;
                                    G++;
                                    OverallCount++;
                                }
                                else if (i == 'C')
                                {
                                    GCCount++;
                                    C++;
                                    OverallCount++;
                                }
                                else if (i == 'S')
                                {
                                    GCCount++;
                                    X++;
                                    OverallCount++;
                          
                                }
                                else if (i == 'A')
                                {
                                    ATCount++;
                                    A++;
                                    OverallCount++;
                                }
                                else if (i == 'T')
                                {
                                    ATCount++;
                                    T++;
                                    OverallCount++;
                                }
                                else if(i == 'W')
                                {
                                    X++;
                                    ATCount++;
                                    OverallCount++;
                                }
                                else if (i == 'N')
                                {
                                    N++;
                                    OverallCount++;
                                }
                                else if(Char.IsLetter(i))
                                {
                                    OverallCount++;
                                    X++;
                                }
                                
                            }
                        }
                            line = reader.ReadLine();
                        }
                        
                 


                  
                    if (GCCount != 0)
                    {

                        double total = GCCount + ATCount;
                        double GCpercentage = (GCCount / total)*100;

                        try
                        {
                            StreamWriter writer = new StreamWriter(Project.folderFullPath + "\\GC\\" + Path.GetFileNameWithoutExtension(path) + ".gc");
                            writer.WriteLine(GCpercentage.ToString("F6"));
                            writer.WriteLine(FragCount.ToString());
                            writer.WriteLine(OverallCount.ToString());
                            writer.WriteLine(A.ToString());
                            writer.WriteLine(T.ToString());
                            writer.WriteLine(G.ToString());
                            writer.WriteLine(C.ToString());
                            writer.WriteLine(N.ToString());
                            writer.WriteLine(X.ToString());


                            writer.Close();

                            return true;
                        }
                        catch
                        {
                            string error = "Problems writing into " + Project.folderFullPath + "\\GC\\" + " check permissions";
                            MainWindow.main.Status = error;
                            Project.ErrorLog.Add(error);
                            return false;
                        }
                    }
                    else { return false; }
                }
                catch
                {
                    string error = "Problems accessing " + Path.GetFileName(path) + " check permissions";
                    MainWindow.main.Status = error;
                    Project.ErrorLog.Add(error);
                    return false;
                }
            }
            else { return false; }
        }
        public static bool generateCSVGc(string file)
        {//generate csv file, where each row contains the name of fasta in question, its gc content, total sequence lenght (number of bases after concatenating fragments) and 
            int computed = Directory.GetFiles(Project.folderFullPath + "\\GC\\", "*.gc", SearchOption.TopDirectoryOnly).Length;
            if (computed > 0)
            {
                StreamWriter writer = new StreamWriter(file);
                writer.Write("name,GC_percentage,No_fragments,No_bases,A_bases,T_bases,G_bases,C_bases,N_bases, Other_ambiqous_sym");
                string[] gcFiles = new string[computed];
                gcFiles = Directory.GetFiles(Project.folderFullPath + "\\GC\\", "*.gc", SearchOption.TopDirectoryOnly);
                foreach (string fasta in gcFiles)
                {

                    string name = Path.GetFileNameWithoutExtension(fasta);
                    StreamReader reader = new StreamReader(Project.folderFullPath + "\\GC\\" + name + ".gc");
                    double GCper;
                    uint fragments;
                    uint lenght;
                    uint A = 0;
                    uint G = 0;
                    uint C = 0;
                    uint T = 0;
                    uint N = 0;
                    uint X = 0;
                    Double.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out GCper);
                    UInt32.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out fragments);
                    UInt32.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out lenght);
                    UInt32.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out A);
                    UInt32.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out T);
                    UInt32.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out G);
                    UInt32.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out C);
                    UInt32.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out N);
                    UInt32.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out X);

                    reader.Close();
                    writer.Write(Environment.NewLine + name + "," + GCper + "," + fragments + "," + lenght + ","+A + ","+T + ","+G+ ","+C + ","+N + ","+X);
                    

                }
                writer.Close();

                return true;
            }
            else { return false; }

        }
    }
}
