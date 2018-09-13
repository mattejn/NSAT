using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NSAT
{
    class Clustering
    {
        private class UpgmaItem
        {
            public string SeqA { get; set; }

            public string SeqB { get; set; }

            public double distanceAvg { get; set; }


        }
        private class cluster
        {
            public string nameA { get; set; }
            public string nameB { get; set; }
            public cluster clusterA { get; set; }
            public cluster cluterB { get; set; }
            public double dist { get; set; }
        }
        public static bool Upgma()
        {
            try {
                string dir = Project.folderFullPath + "\\ANI\\";
                List<string> files = Directory.GetFiles(dir, "*.ani", SearchOption.TopDirectoryOnly).ToList();
                List<UpgmaItem> items = new List<UpgmaItem>();
                foreach (var file in files)
                {//WARNING BROKEN AFTER THE SPLIT SECOND PART IS NOT THE FULL PATH
                    string[] ab = Regex.Split(file, "vs");
                    if (ab.Length == 2)
                    {
                        if ((items.Exists(item => item.SeqA == ab[1] && item.SeqB == ab[0])) || (items.Exists(item => item.SeqA == ab[0] && item.SeqB == ab[1])))
                        {
                            double a;
                            double b;
                          
                            //Double.TryParse(File.ReadAllText(file), out a);
                            //StreamReader reader1 = new StreamReader(ab[1] + "vs" + ab[0]);
                            //Double.TryParse(reader1.ReadLine(), out b);
                            //items.Add(new UpgmaItem()
                            //{
                            //    SeqA = ab[0],
                            //    SeqB = ab[1],
                            //    distanceAvg = 100 - ((a + b) / 2)
                            //});
                            //reader.Close();
                        }

                    }
                }
                return true;

            }
            catch { return false; }
            }
        }
}
