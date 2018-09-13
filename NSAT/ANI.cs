using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NSAT
{
    class ANI
    {
        private static /*int*/ void ChopQuery(string PathQ)
        {//remove headers and chop query to 1020nt fragments
         //save the fragments into a temp folder with the query name
            int fragNo = 0;//relevant for temp files naming from 1 to n - file for each chunk
            string dir = Project.folderFullPath + "\\temp\\ani\\" + Path.GetFileNameWithoutExtension(PathQ);
            if (Directory.Exists(dir))
            {
                //When the chopping function is called this directory really should not exist, but just in case...
                //fragNo = Directory.GetFiles(dir).Length; //its pointless to do the chopping again
            }
            else
            {
                StreamReader reader = new StreamReader(PathQ);
            string line = reader.ReadLine();
            string seq = String.Empty;
            while (line != null)
            {
                if (!line.StartsWith(">"))
                {
                    seq = String.Concat(seq, line);
                }
                line = reader.ReadLine();
            }
            var sb = new StringBuilder(seq.Length);
            int length = 0; //raw seq length
                foreach (char k in seq)
                {
                    if (k != '\n' && k != '\r' && k != '\t' && k != ' ')

                        sb.Append(k);

                    length++;
                }
            seq = sb.ToString();
            //Should be the most efficient way to remove whitespaces/tabs and newlines from the string

            int chunk = 1020;//chunk size
            string fragment;//current chunk is stored in this
           
            

                Directory.CreateDirectory(dir);
                int i = 0;
                while ( i < length)
                {
                    fragNo++;
                    if (i + chunk < length)
                    {
                        fragment = ">" + Path.GetFileNameWithoutExtension(PathQ) + "_" + fragNo + Environment.NewLine + seq.Substring(i,chunk);//add header with chunk number
                        File.WriteAllText(dir + "//" + fragNo + ".fsa", fragment); //write the current 1020 fragment into the temp folde corresponding to the query file name
                        
                    }
                    else
                    {
                        fragment = ">" + Path.GetFileNameWithoutExtension(PathQ) + "_" + fragNo + Environment.NewLine + seq.Substring(i, (length - 1)-i);
                        File.WriteAllText(dir + "//" + fragNo + ".fsa", fragment);//write the last fragment, that is most likely smaller than 1020 bases
                        
                    }
                    i = i + chunk;
                }
               
            }
            //return fragNo;
        }


     private static void InterlockedAddDouble(ref double refLocation, double add)
        {//custom method for thread safe double addition
            double startVal = refLocation;
            
            while(true)
            {
                double current = startVal;
                double newVal = startVal + add;
                startVal = Interlocked.CompareExchange(ref refLocation, newVal, current);
                if (startVal == current) break;
            }

        }
   



        
        private static bool ANIb(string PathA, string PathB)
        {

            if (File.Exists(PathA))
            {
                if (File.Exists(PathB))
                {
                    int ChunkNo = 0;
                    double ani = 0;//sum of alignment scores from all chunks
                    string dir = Project.folderFullPath + "\\temp\\ani\\" + Path.GetFileNameWithoutExtension(PathB);
                    List<string> files=Directory.GetFiles(dir, "*.fsa", SearchOption.TopDirectoryOnly).ToList();
                    Parallel.ForEach(files, (file) => //multihtreaded blasting much performance such wow
                    //foreach (var file in files)
                    // Start the new process.
                    {
                        Process p = new Process();
                        // Redire   ct the output stream from shell to SO
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.FileName = GlobalSettings.BlastnExecutablePath;
                        p.StartInfo.Arguments =
                        " -outfmt \"6 length qlen nident pident mismatch gapopen\" " + //custom format separated by tab with only the results relevant for this use, not that not all fields are currently used
                        "-subject " + Path.GetFullPath(PathA) + " " +
                        "-query " + Path.GetFullPath(file) + " " +
                        "-penalty -1 " +
                        "-gapopen 5 " +
                        "-gapextend 2 " +
                        "-xdrop_gap_final 150 " +//parameters specified in methodology
                        "-evalue 1e-15 " + //background noise - expected number of purely coincidental matches
                        "-max_target_seqs 1 " + //we want just the top (best) alignment
                        " " + "-dust no"; //de dust the sequence (remove low complexity regions)


                        //Calling the BLAST algorithm
                        Console.Write("\"" + p.StartInfo.FileName + p.StartInfo.Arguments + "\n");
                        p.Start();
                        StreamReader reader = p.StandardOutput;//read the output
                        string output = reader.ReadToEnd();
                        p.WaitForExit();

                        if (p.ExitCode != 0) //exit code !=0 is a BLAST error
                        {
                            string error = "\"" + p.StartInfo.FileName + "\" " + p.StartInfo.Arguments + "\n" + p.StandardError.ReadToEnd();
                            MainWindow.main.Status = error;
                            Project.ErrorLog.Add(error);

                        }

                        else
                        {//parse output



                            string[] cells = output.Split('\t');
                            if (cells.Length == 6)
                            {
                                double TotalIdentity = 0;
                                double coverage = 0;
                                double AlignLength;
                                double identity;
                                double matched;
                                AlignLength = Double.Parse(cells[0]);
                                matched = Double.Parse(cells[2]);
                                identity = Double.Parse(cells[3], System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);
                                double FragLentgth;
                                FragLentgth = Double.Parse(cells[1]);
                                if (AlignLength >= 1020) coverage = 1;
                                else coverage = AlignLength / FragLentgth;
                                if (coverage >= 0.700000)
                                {
                                    TotalIdentity = (((identity / FragLentgth) * coverage)*10);
                                    if (TotalIdentity > 0.300000)
                                    {
                                        //ani = ani + matched / FragLentgth;
                                        //ChunkNo++;
                                        InterlockedAddDouble(ref ani, matched / FragLentgth);
                                        Interlocked.Increment(ref ChunkNo);
                                    }



                                }
                            }







                        }
                        p.Close();
                        //}
                    });
                    ani = ani / ChunkNo; //calculate anib from chunk scores and write it into a file
                    StreamWriter writer = new StreamWriter(Project.folderFullPath + "\\ANI\\" + Path.GetFileNameWithoutExtension(PathA) + "_vs_" + Path.GetFileNameWithoutExtension(PathB) + ".ani");
                    writer.Write(ani.ToString("F6"));
                    writer.Flush();
                    writer.Close();
                    return true;
                }
                else
                {
                    Project.fastas.Remove(PathB);
                    string error = "File " + PathB + "no logner exists and has been removed.";
                    MainWindow.main.Status = error;
                    Project.ErrorLog.Add(error);
                    return false;
                }
            }
            else if (!File.Exists(PathB) && !File.Exists(PathA)) {
                Project.fastas.Remove(PathA); Project.fastas.Remove(PathB);
                string error = "Files " + PathA + " and " + PathB + "no logner exist and have been removed.";
                MainWindow.main.Status = error;
                Project.ErrorLog.Add(error);
                return false; }
            else {
                Project.fastas.Remove(PathA);
                string error = "File " + PathA + "no logner exists and has been removed.";
                MainWindow.main.Status = error;
                //Project.ErrorLog.Add(error);
                return false; }
        }



        public static int computeANIb(List<Project.ItemsSelectedToCompute> pairsToCompute)
        {//Takes paths of specified pair of fastas, starts BLAST process with arguments specified in options, parses its output (external function),
         //Saves resulting score in a file named accordingly
         //If there already is file with results of given fasta file computation is skipped
            int errors = 0;
            int current = 0;
            int pairsToComp = pairsToCompute.Count;
            List<string> uniqueFiles = new List<string>();
            foreach (var pair in pairsToCompute) //create list of unique files, that will be used
            {
                if (!uniqueFiles.Contains(pair.FullPath)) uniqueFiles.Add(pair.FullPath);
                if (!uniqueFiles.Contains(pair.FullPathB)) uniqueFiles.Add(pair.FullPathB);

            }
            Parallel.ForEach(uniqueFiles, (file) => //multithreaded chopping - performance
            {
                ChopQuery(file);
            });

            foreach (var pair in pairsToCompute)
            {current++;
                if (File.Exists(pair.FullPath))
                {
                    if (File.Exists(pair.FullPathB))
                    {
                        if (!File.Exists(Project.folderFullPath + "\\ANI\\" + Path.GetFileNameWithoutExtension(pair.FullPath) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPathB) + ".ani"))
                        {
                            SelectionWindow.main.lblStatusANIT.Content = "ANIb computation in progress, now processing" + current + "/" + pairsToComp + " " + Path.GetFileNameWithoutExtension(pair.FullPath) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPathB);


                            if (ANIb(pair.FullPath, pair.FullPathB))
                            {
                                SelectionWindow.main.lblStatusANIT.Content = "ANIb computation in progress "+ current + "/" + pairsToComp + " " + Path.GetFileNameWithoutExtension(pair.FullPath) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPathB)+" success";
                            }
                            else { errors++;
                                SelectionWindow.main.lblStatusANIT.Content = "ANIb computation in progress " + current + "/" + pairsToComp + " " + Path.GetFileNameWithoutExtension(pair.FullPath) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPathB) + " failed";

                            }
                        }

                        if (!File.Exists(Project.folderFullPath + "\\ANI\\" + Path.GetFileNameWithoutExtension(pair.FullPathB) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPath) + ".ani"))
                        {
                            SelectionWindow.main.lblStatusANIT.Content = "ANIb computation in progress, now processing " + current + "/" + pairsToComp + " " + Path.GetFileNameWithoutExtension(pair.FullPathB) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPath);

                            if (ANIb(pair.FullPathB, pair.FullPath))
                            {
                                SelectionWindow.main.lblStatusANIT.Content = "ANIb computation in progress " + current + "/" + pairsToComp + " " + Path.GetFileNameWithoutExtension(pair.FullPathB) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPath) + "success";


                            }
                            else { errors++;
                                SelectionWindow.main.lblStatusANIT.Content = "ANIb computation in progress " + current + "/" + pairsToComp + " " + Path.GetFileNameWithoutExtension(pair.FullPathB) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPath) + " failed";
                            }

                        }
                    }
                    else { errors++; Project.fastas.Remove(pair.FullPathB); }
                }
                else

                {
                    errors++;
                    Project.fastas.Remove(pair.FullPath);
                    if (!File.Exists(pair.FullPathB)) { Project.fastas.Remove(pair.FullPathB); }
                }

            }
            
            if (pairsToComp == 0) { return 1; }//all selected pairs already computed
            else if (errors / pairsToComp >= 1) { return 3; } //errors in all pairs
            else if (errors / pairsToComp > 0) { return 2; }//errors in some pairs, but some computed successfuly
            else { return 0; }//no errors        
        }


        
            
            
          
                
         
        
        public static bool GenerateCSVAni(string file)
        {
            int computed = Directory.GetFiles(Project.folderFullPath + "\\ANI\\", "*.ani", SearchOption.TopDirectoryOnly).Length;
            if (computed != 0)//make sure there are some results
            {
                StreamWriter writer = new StreamWriter(file);
                writer.Write("name,"); // cell 1:1
                string[] aniPairs = new string[computed];
                aniPairs = Directory.GetFiles(Project.folderFullPath + "\\ANI\\", "*.ani", SearchOption.TopDirectoryOnly); //array of full filenames
                List<string> aniNames = new List<string>();
                foreach (string pair in aniPairs)
                {
                    aniNames.Add(Path.GetFileNameWithoutExtension(pair)); //list of filenames without the extensions
                }
                List<string> aniNamesUnique = new List<string>();
                foreach (string pair in aniPairs)
                {
                    string[] temp = new string[2];
                    temp = Path.GetFileNameWithoutExtension(pair).Split(new string[] { "_vs_" }, StringSplitOptions.None);
                    if (!aniNamesUnique.Contains(temp[0])) aniNamesUnique.Add(temp[0]);
                    if (!aniNamesUnique.Contains(temp[0])) aniNamesUnique.Add(temp[1]);
                }//filenames split again to individual fasta names, only unique names matter for the table consturction
                foreach (string name in aniNamesUnique)
                {
                    writer.Write(name + ",");//write first row (fasta names)
                }
                bool success = false;
                foreach (string a in aniNamesUnique)
                {
                    writer.Write(Environment.NewLine + a + ",");//new row with another name
                    foreach (string b in aniNamesUnique)

                    {
                        if (a == b)
                        {
                            writer.Write("X,"); //this value could also be 100 - comparing the sequence with itself (diagonal)
                        }
                        else
                        {
                            if (File.Exists(Project.folderFullPath + "\\ANI\\" + a + "_vs_" + b + ".ani"))
                            {


                                StreamReader reader = new StreamReader(Project.folderFullPath + "\\ANI\\" + a + "_vs_" + b + ".ani");
                                double GCC;
                                if (Double.TryParse(reader.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out GCC))
                                {
                                    writer.Write(GCC.ToString() + ",");
                                    reader.Close();
                                    success = true;
                                }
                                else
                                {
                                    writer.Write("err,");//error parsing
                                }
                            }

                            else
                            {
                                writer.Write("nc,");//not computed
                            }


                        }
                    }
                }
                writer.Close();
                if (success == true) //atleast one valid value in the table
                { return true; }
                else
                {
                    File.Delete(file); //There is no valid result in the matrix - delete it.
                    return false;
                }
            }



            else { return false; }
        }
        
            public static bool GenerateCSVAniAvg(string file)
            {
            int computed = Directory.GetFiles(Project.folderFullPath + "\\ANI\\", "*.ani", SearchOption.TopDirectoryOnly).Length;
            if (computed != 0)//make sure there are some results
            {
                StreamWriter writer = new StreamWriter(file);
                writer.Write("name,"); // cell 1:1
                string[] aniPairs = new string[computed];
                aniPairs = Directory.GetFiles(Project.folderFullPath + "\\ANI\\", "*.ani", SearchOption.TopDirectoryOnly); //array of full filenames
                List<string> aniNames = new List<string>();
                foreach (string pair in aniPairs)
                {
                    aniNames.Add(Path.GetFileNameWithoutExtension(pair)); //list of filenames without the extensions
                }
                List<string> aniNamesUnique = new List<string>();
                foreach (string pair in aniPairs)
                {
                    string[] temp = new string[2];
                    temp = Path.GetFileNameWithoutExtension(pair).Split(new string[] { "_vs_" }, StringSplitOptions.None);
                    if (!aniNamesUnique.Contains(temp[0])) aniNamesUnique.Add(temp[0]);
                    if (!aniNamesUnique.Contains(temp[1])) aniNamesUnique.Add(temp[1]);
                }//filenames split again to individual fasta names, only unique names matter for the table consturction
                foreach (string name in aniNamesUnique)
                {
                    writer.Write(name + ",");//write first row (fasta names)
                }
                bool success = false;
                foreach (string a in aniNamesUnique)
                {
                    writer.Write(Environment.NewLine + a + ",");//new row with another name
                    foreach (string b in aniNamesUnique)
                    {
                        if (a == b)
                        {
                            writer.Write("X,"); //A versus A = 100% identity and its pointless to compute or display its result, represented by x
                        }
                        else
                        {
                            if (File.Exists(Project.folderFullPath + "\\ANI\\" + a + "_vs_" + b + ".ani") && File.Exists(Project.folderFullPath + "\\ANI\\" + b + "_vs_" + a + ".ani"))
                            {


                                StreamReader reader1 = new StreamReader(Project.folderFullPath + "\\ANI\\" + a + "_vs_" + b + ".ani");
                                StreamReader reader2 = new StreamReader(Project.folderFullPath + "\\ANI\\" + b + "_vs_" + a + ".ani");
                                double GCC1;
                                double GCC2;
                                double GCC;
                                if (Double.TryParse(reader1.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out GCC1) && Double.TryParse(reader2.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out GCC2))//verifies valid file structure and content
                                {
                                    GCC = (GCC1 + GCC2) / 2;
                                    writer.Write(GCC.ToString() + ",");
                                    reader1.Close();
                                    reader2.Close();
                                    success = true;
                                }
                                else
                                {
                                    writer.Write("err,");//error parsing
                                }
                            }

                            else
                            {
                                writer.Write("nc,");//not computed


                            }
                        }
                    }
                }
                writer.Close();
                if (success == true) //atleast one valid value in the table
                { return true; }
                else
                {
                    File.Delete(file); //There is no valid result in the matrix - delete it.
                    return false;
                }
            }



            else { return false; }





            }



        }
    }

