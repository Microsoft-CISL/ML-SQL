using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QueryExecutor
{
    public class Utility
    {
        public Utility()
        {
        }


        public static string readQueryFromFile(string filename)
        {
            string query = File.ReadAllText(filename, Encoding.UTF8);
            return query;
        }


        public static void writeResultsOnCsvFile(List<RunResult> results, string filename)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(filename))
            {
                file.WriteLine(RunResult.getHeader(","));

                foreach ( RunResult res in results)
                {
                    // If the line doesn't contain the word 'Second', write the line to the file.
                    file.WriteLine(res.GetLineCsv(","));
                }

                file.Close();
            }


        }
    }
}
