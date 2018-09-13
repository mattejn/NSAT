using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NSAT
{
    class TETRA
    {
        private static Dictionary<string, double> computeZscores(string path)
        {
            //first load the sequence, remove the headers and keep only G,C,A and T bases while concatenaning with the reverse complement
            try
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder sbC = new StringBuilder();
                StreamReader reader = new StreamReader(path);
                string line = reader.ReadLine();
                while (!String.IsNullOrEmpty(line))
                {
                    if (!line.StartsWith(">"))
                    {
                        line = line.ToUpper();
                        foreach (char c in line)
                        {
                            if (c == 'A')
                            {
                                sbC.Append('T');
                                sb.Append('A');
                            }
                            else if (c == 'T')
                            {
                                sbC.Append('A');
                                sb.Append('T');
                            }
                            else if (c == 'G')
                            {
                                sbC.Append('C');
                                sb.Append('G');
                            }
                            else if (c == 'C')
                            {
                                sbC.Append('G');
                                sb.Append('C');
                            }
                        }
                    }
                    line = reader.ReadLine();
                }
                string compl = sbC.ToString();
                char[] array = new char[compl.Length];
                int forward = 0;
                for (int i = compl.Length - 1; i >= 0; i--)
                {
                    array[forward++] = compl[i];
                }
                compl = new string(array);
                string seq = sb.ToString() + compl;
                char[] n4 = { 'A', 'C', 'G', 'T' };
                char[] tetra = new char[4];
                Dictionary<string, double> observed4 = new Dictionary<string, double>(); //fill the list with all 256 tetranucleotide combinations as keys 
                Dictionary<string, double> expected4 = new Dictionary<string, double>();
                Dictionary<string, double> zscores = new Dictionary<string, double>();
                foreach (char n1 in n4)
                {
                    tetra[0] = n1;

                    foreach (char ntwo in n4)
                    {
                        tetra[1] = ntwo;
                        foreach (char nthree in n4)
                        {
                            tetra[2] = nthree;
                            foreach (char nfour in n4)
                            {
                                tetra[3] = nfour;
                                observed4.Add(new string(tetra), 0.0);
                                expected4.Add(new string(tetra), 0.0);
                                zscores.Add(new string(tetra), 0.0);
                            }
                        }
                    }


                }
                char[] n3 = { 'A', 'C', 'G', 'T' };
                char[] tri = new char[3];
                Dictionary<string, double> expected3 = new Dictionary<string, double>();
                Dictionary<string, double> observed3 = new Dictionary<string, double>();
                //fill the list with all trinucleotide combinations as keys 
                foreach (char n1 in n3)
                {
                    tri[0] = n1;
                    foreach (char ntwo in n3)
                    {
                        tri[1] = ntwo;


                        foreach (char nthree in n3)
                        {
                            tri[2] = nthree;
                            observed3.Add(new string(tri), 0.0);
                            expected3.Add(new string(tri), 0.0);

                        }

                    }


                }
                char[] n2 = { 'A', 'C', 'G', 'T' };
                char[] di = new char[2];
                Dictionary<string, double> observed2 = new Dictionary<string, double>(); //fill the list with all dinucleotide combinations as keys 
                Dictionary<string, double> expected2 = new Dictionary<string, double>();
                foreach (char n1 in n2)
                {
                    di[0] = n1;
                    foreach (char ntwo in n2)
                    {
                        di[1] = ntwo;
                        observed2.Add(new string(di), 0.0);
                        expected2.Add(new string(di), 0.0);

                    }


                }
                int length = seq.Length;
                for (int i = 0; i < length - 3; i++)
                {//tetranucleotide observed frenquencies
                    observed4[seq.Substring(i, 4)] += 1;
                }
                for (int i = 0; i < length - 2; i++)
                {//trinucleotide observed frenquencies
                    observed3[seq.Substring(i, 3)] += 1;
                }
                for (int i = 0; i < length - 1; i++)
                {//dinucleotide observed frenquencies
                    observed2[seq.Substring(i, 2)] += 1;
                }
                foreach (string tet in observed4.Keys)
                {//Fill the expected tetranucleotide frenquencies
                    expected4[tet] = (observed3[tet.Substring(0, 3)] * observed3[tet.Substring(1, 3)]) / observed2[tet.Substring(1, 2)];
                }

                foreach (string tet in observed4.Keys)
                {//compute zscores for each tetranucleotide
                    zscores[tet] =(observed4[tet]- expected4[tet])/ 
                        Math.Abs(Math.Sqrt(expected4[tet] * ((observed2[tet.Substring(1, 2)] - observed3[tet.Substring(0, 3)]) * (observed2[tet.Substring(1, 2)] - observed3[tet.Substring(1, 3)])) / (observed2[tet.Substring(1, 2)] * observed2[tet.Substring(1, 2)])));
                }
                return zscores;
            }
            catch
            {
                return null;
            }
        }
        private static double ComputePearsons(Dictionary<string, double> zscoresA, Dictionary<string, double> zscoresB)
        {
            List<string> iterator = zscoresA.Keys.ToList();
            double sumA = 0;
            double sumB = 0;
            double sumAsq = 0;
            double sumBsq = 0;
            double sumAB = 0;
            foreach (string tet in iterator)
            {
                sumA += zscoresA[tet];
                sumB += zscoresB[tet];
                sumAsq += zscoresA[tet] * zscoresA[tet];
                sumBsq += zscoresB[tet] * zscoresB[tet];
                sumAB += zscoresA[tet] * zscoresB[tet];
            }
        
            double pearson = (sumAB - (sumA * sumB)) / (Math.Sqrt(Math.Abs(sumAsq - (sumA * sumA))) * Math.Sqrt(Math.Abs(sumBsq - (sumB * sumB))));
            return pearson;
        }
        public static bool computeTetra(List<Project.ItemsSelectedToCompute> FastaPairs)
        {//Compute Tetra score(Pearsons cc of z scores from seq a and seq b) using implemented functions
         //The way we store information is a dictionary with a given fastas filename as a key and dictionary of its zscores as a value
            try
            {
           
                List<string> uniqueFiles = new List<string>();
               
                foreach (var pair in FastaPairs)
                {
                    
                    if (!uniqueFiles.Contains(pair.FullPath)) uniqueFiles.Add(pair.FullPath);
                    if (!uniqueFiles.Contains(pair.FullPathB)) uniqueFiles.Add(pair.FullPathB);
                
                         

                    

                }
                ConcurrentDictionary<string, Dictionary<string, double>> Zscores = new ConcurrentDictionary<string, Dictionary<string, double>>();
                Parallel.ForEach(uniqueFiles, (file) =>
                {
                    Zscores.TryAdd(file, computeZscores(file));

                });

                Parallel.ForEach(FastaPairs, (pair) =>
                {
                    double tetra = ComputePearsons(Zscores[pair.FullPath], Zscores[pair.FullPathB]);
                    if (!File.Exists(Project.folderFullPath + "\\TETRA\\" + Path.GetFileNameWithoutExtension(pair.FullPath) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPathB) + ".tet"))
                    {
                        File.WriteAllText(Project.folderFullPath + "\\TETRA\\" + Path.GetFileNameWithoutExtension(pair.FullPath) + "_vs_" + Path.GetFileNameWithoutExtension(pair.FullPathB) + ".tet", tetra.ToString("F6"));
                    }
                });
                return true;
            }
            catch {
                return false; }
        }
        public static bool GenerateCSVTETRA(string file)
        {
            int computed = Directory.GetFiles(Project.folderFullPath + "\\TETRA\\", "*.tet", SearchOption.TopDirectoryOnly).Length;
            if (computed != 0)//make sure there are some results
            {
                StreamWriter writer = new StreamWriter(file);
                writer.Write("name,"); // cell 1:1
                string[] tetraPairs = new string[computed];
                tetraPairs = Directory.GetFiles(Project.folderFullPath + "\\TETRA\\", "*.tet", SearchOption.TopDirectoryOnly); //array of full filenames
                List<string> tetraNames = new List<string>();
                foreach (string pair in tetraPairs)
                {
                    tetraNames.Add(Path.GetFileNameWithoutExtension(pair)); //list of filenames without the extensions
                }
                List<string> tetraNamesUnique = new List<string>();
                foreach (string pair in tetraNames)
                {
                    string[] temp = new string[2];
                    temp = pair.Split(new string[] { "_vs_" }, StringSplitOptions.None);
                    if (!tetraNamesUnique.Contains(temp[0])) tetraNamesUnique.Add(temp[0]);
                    if (!tetraNamesUnique.Contains(temp[1])) tetraNamesUnique.Add(temp[1]);
                }//filenames split again to individual fasta names, only unique names matter for the table consturction
                foreach (string name in tetraNamesUnique)
                {
                    writer.Write(name + ",");//write first row (fasta names)
                }
                bool success = false;
                foreach (string a in tetraNamesUnique)
                {
                    writer.Write(Environment.NewLine + a + ",");//new row with another name
                    foreach (string b in tetraNamesUnique)

                    {
                        if (a == b)
                        {
                            writer.Write("X,"); //this value could also be 100 - comparing the sequence with itself (diagonal)

                        }
                        else
                        {
                            if (File.Exists(Project.folderFullPath + "\\TETRA\\" + a + "_vs_" + b + ".tet"))
                            {


                                StreamReader reader = new StreamReader(Project.folderFullPath + "\\TETRA\\" + a + "_vs_" + b + ".tet");
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
    }
}
