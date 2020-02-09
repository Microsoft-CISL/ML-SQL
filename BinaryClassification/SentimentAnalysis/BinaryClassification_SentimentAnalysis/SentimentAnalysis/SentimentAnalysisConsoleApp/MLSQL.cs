using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ML.Trainers.FastTree;

namespace CreditCardFraudDetection.Trainer
{
    public class MLSQL
    {
        public MLSQL()
        {
        }



        public static string ExtractWord()
        {
            return default;
        }









        public static void GenerateNumbersTable(string path)
        {
            string numbers = "CREATE TABLE numbers (\n  n INT PRIMARY KEY); \n";
            numbers += "INSERT INTO numbers VALUES ";

            for (int i = 1; i < 10000; i++)
            {
                numbers += "(" + i + "),";
            }

            numbers = numbers.Substring(0, numbers.Length - 1);

            numbers += ";";


            WriteSQL(numbers, path);


        }


        public static string generateInsertIntoHeader(string tablename, string header)
        {
            string query = "INSERT INTO " + tablename + "  ( " + header + " ) VALUES ";

            return query;
        }


        public static string generateHeader(List<string> headers, string separator)
        {
            String line = "";
            foreach (string header in headers)
            {
                line += header + separator;
            }
            line = line.Substring(0, line.Length - separator.Length);

            return line;

        }


        public static string generateLineCSV(string data, string prediction, string separator)
        {
            String line = "";

            line += data;
            line += separator;
            line += prediction;


            return line;
        }


        public static string generateINSERTINTOLine(string data, string prediction, string separator)
        {
            String line = "(";

            line += data;
            line += separator;
            line += prediction;


            line += ")";

            return line;
        }

        public static void WriteSQL(string query, string outputPath)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputPath, false))
            {
                file.WriteLine(query);
            }
        }


        public static string EscapeStringSQL(string text)
        {
            string a = "'" + Regex.Replace(text, @"\p{Cs}", "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'") + "'";
            return a;
        }

        public static string EscapeStringQueryWithStartAndEnding(string text)
        {
            string a = "'␂" + Regex.Replace(text, @"\p{Cs}", "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'") + "␃'";
            return a;
        }

        public static string GenerateSQLProcededure(string query, string databaseName, string outputDatabaseName, string procedureName, string idParams, int mode, bool includeWhereClause)
        {


            string init = "delimiter //\nset names utf8;\ndrop procedure if exists " + databaseName + "." + procedureName + ";\n";


            string middle = "CREATE procedure " + databaseName + "." + procedureName + "(num INT, chuncksize INT)\nwholeblock:BEGIN\nSET @id = 1;\nWHILE @id <= num DO\n";

            string outputMode = "";


            string q = " select * from (" + query + " ) AS F;";

            switch (mode)
            {
                case 0:


                    break;
                //File output
                case 1:


                    break;
                //Output
                case 2:

                    outputMode = "";
                    q = " select count(1) from (" + query + " ) AS F;";
                    break;
                //No output
                case 3:
                    outputMode = "insert into " + outputDatabaseName + " \n";
                    //Database
                    break;
            }





            string whereClause = "\n where Id >= @id  and Id < ( @id + chuncksize ); \n";

            if (!includeWhereClause)
            {
                whereClause = "";
            }

            string end = "\nSET @id = @id + chuncksize;\n  END WHILE;\nEND//";


            return init + middle + outputMode + q + whereClause + end;

        }




        public static void JoinTrainAndTest(String path1, String path2, String outputFile, bool header)
        {
            string[] lines1 = System.IO.File.ReadAllLines(path1);
            string[] lines2 = System.IO.File.ReadAllLines(path2);

            int i = 0;

            if (header)
            {
                i = 1;
            }


            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFile, false))
            {

                for (int j = i; j < lines1.Length; j++)
                {
                    file.WriteLine(lines1[j]);
                }


                for (int j = i; j < lines2.Length; j++)
                {
                    file.WriteLine(lines2[j]);
                }

            }

            Console.WriteLine("End of the join process.");



        }





        public static int insertUniqueId(String inputFilePath, String outputFilePath, bool header, string separator, int start_id)
        {
            string[] lines = System.IO.File.ReadAllLines(inputFilePath);

            List<string> outputLines = new List<string>();

            int index = 0;
            if (header)
            {
                index = 1;
                outputLines.Add(lines[0] + separator + "Id");
            }


            for (int i = index; i < lines.Length; i++)
            {
                outputLines.Add(lines[i] + "" + separator + "" + start_id);
                start_id += 1;
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFilePath, false))
            {

                for (int j = 0; j < outputLines.Count; j++)
                {
                    file.WriteLine(outputLines.ElementAt(j));
                }

            }

            Console.WriteLine("Start ID: " + start_id);

            return start_id;

        }


        public static void createDataSample(String inputPathDataset, String outputFolderPath, int[] chunckSizes, bool header)
        {

            //List<string> lines = new List<string>();

            // read all lines

            string[] lines = System.IO.File.ReadAllLines(inputPathDataset);


            Console.WriteLine("Number of lines: " + lines.Length);

            int i = 0;
            String headerString = "";
            if (header)
            {
                i = 1;
                headerString = lines.ElementAt(0);
            }

            foreach (int chunkSize in chunckSizes)
            {
                for (int k = i; k < lines.Length; k = k + chunkSize)
                {

                    int index2 = k + chunkSize;

                    if (index2 > lines.Length)
                    {
                        index2 = lines.Length - 1;
                    }
                    string outputPath = outputFolderPath + "" + chunkSize + "/";
                    string filename = "sample_" + k + "_" + index2 + ".csv";
                    //Console.WriteLine("outputPath: " + outputPath);

                    bool exists = System.IO.Directory.Exists(outputPath);

                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(outputPath);
                    }
                    else
                    {
                    }
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputPath + filename, false))
                    {
                        if (header)
                        {
                            file.WriteLine(headerString);
                        }
                        for (int j = k; j < index2; j++)
                        {

                            file.WriteLine(lines[j]);

                        }


                    }




                }
            }

        }




        public static string printPaths(int node, RegressionTree tree, double treeWeight, string[] featureColumnNames)
        {
            int[] path = new int[1000];

            ImmutableArray<double> leafValues = (System.Collections.Immutable.ImmutableArray<double>)tree.LeafValues;

            ImmutableArray<int> leftChild = (System.Collections.Immutable.ImmutableArray<int>)tree.LeftChild;
            ImmutableArray<int> rightChild = (System.Collections.Immutable.ImmutableArray<int>)tree.RightChild;

            ImmutableArray<int> numericalSplitFeatureIndexes = (System.Collections.Immutable.ImmutableArray<int>)tree.NumericalSplitFeatureIndexes;

            ImmutableArray<float> numericalSplitThreshold = (System.Collections.Immutable.ImmutableArray<float>)tree.NumericalSplitThresholds;
            ImmutableArray<bool> categoricalSplit = (System.Collections.Immutable.ImmutableArray<bool>)tree.CategoricalSplitFlags;



            List<int[]> paths = new List<int[]>();

            printPathsRecur(node, path, 0, tree, paths);

            Console.WriteLine("Stored path: ");
            foreach (int[] p in paths)
            {
                for (int w = 0; w < p.Length; w++)
                {
                    Console.Write(p[w] + " ");
                }
                Console.WriteLine(" ");
            }






            bool debug = false;
            String sql = "CASE ";
            foreach (int[] p in paths)
            {
                sql += "WHEN ";
                for (int w = 0; w < p.Length - 1; w++)
                {
                    int index = p[w];
                    int index_2 = p[w + 1];

                    if (debug)

                    {
                        Console.WriteLine("\n----------------------------------------------");
                        Console.WriteLine("Index: " + index);
                        Console.WriteLine("Index2 : " + index_2);
                    }

                    string feature = featureColumnNames[numericalSplitFeatureIndexes[index]];
                    float numericalsplit = numericalSplitThreshold[index];

                    if (debug)
                    {
                        Console.WriteLine("Feature " + feature);
                        Console.WriteLine("numverical split : " + numericalsplit);
                    }

                    bool left = false;
                    if (leftChild[index].Equals(index_2))
                    {
                        left = true;

                    }
                    else if (rightChild[index].Equals(index_2))
                    {
                        left = false;
                    }
                    else
                    {
                        throw new Exception("Error");
                    }


                    if (debug)
                    {
                        Console.WriteLine("LEFT CONDITION: " + left);
                        Console.WriteLine("LEFT : " + leftChild[index]);
                        Console.WriteLine("RIGHT : " + rightChild[index]);
                    }


                    string oper = " < ";

                    if (!left)
                    {
                        oper = " > ";
                    }

                    string featureString = feature;

                    sql += featureString + oper + numericalsplit;




                    if (index_2 < 0)
                    {
                        sql += " then " + leafValues[~index_2] + " * " + treeWeight + " ";
                    }
                    else
                    {
                        sql += " and ";
                    }

                }
                Console.WriteLine("SQL:  " + sql);

            }

            return sql;



        }

        public static string GenerateSQLNormalizeByVarianceColumns(string[] columns, string[] selectParams, float[] avgs, float[] stds, string suffix, string tableName)
        {

            string query = "select ";
            for (int i = 0; i < columns.Length; i++)
            {
                String c = columns[i];
                query += "(" + c + " - " + avgs[i] + ")* 1.0 * (" + stds[i] + ")  as " + c + suffix + " ,";
            }

            for (int i = 0; i < selectParams.Length; i++)
            {
                query += selectParams[i] + ",";
            }

            query = query.Substring(0, query.Length - 1);

            Console.WriteLine("QUERY: " + query + "\n\n");

            query += " from " + tableName;

            Console.WriteLine("QUERY: " + query + "\n\n");

            return query;
        }



        public static string GenerateSQLMultiplyByLinearCombination(string[] columns, string[] selectParams, float[] weights, string suffix, string tableName)
        {
            string query = "select ";
            for (int i = 0; i < columns.Length; i++)
            {
                String c = columns[i];
                query += "(" + c + " * " + weights[i] + " )  as " + c + suffix + " ,";
            }
            for (int i = 0; i < selectParams.Length; i++)
            {
                query += selectParams[i] + ",";
            }

            query = query.Substring(0, query.Length - 1);

            Console.WriteLine("QUERY: " + query + "\n\n");

            query += " from " + tableName;

            return query;
        }





        public static string GenerateSQLRegressionTree(string[] columns, string[] selectParams, string suffix, string tableName, List<double> treeWeights, List<RegressionTree> trees)
        {

            string query = "select ";



            for (int i = 0; i < trees.Count(); i++)
            {

                double treeWeight = treeWeights[i];
                RegressionTree tree = trees.ElementAt(i);




                String sqlCase = printPaths(0, tree, treeWeight, columns);
                sqlCase += "END AS tree_" + i + ",";

                query += sqlCase;


            }




            //query = query.Substring(0, query.Length - 1);

            for (int i = 0; i < selectParams.Length; i++)
            {
                query += selectParams[i] + ",";
            }




            query = query.Substring(0, query.Length - 1);



            query += " from " + tableName;




            string s = " select (";

            for (int i = 0; i < trees.Count(); i++)
            {
                s += "tree_" + i + "+";
            }
            s = s.Substring(0, s.Length - 1);
            s += ") AS PredictedScore,";


            for (int i = 0; i < selectParams.Length; i++)
            {
                s += selectParams[i] + ",";
            }


            s = s.Substring(0, s.Length - 1);


            string externalQuery = s + " from (" + query + " ) AS F";






            return externalQuery;
        }

        public static void printPathsRecur(int node, int[] path, int pathLen, RegressionTree tree, List<int[]> paths)
        {


            /* append this node to the path array */
            path[pathLen] = node;
            pathLen++;

            /* it's a leaf, so print the path that led to here  */
            if (node < 0)
            {
                printArray(path, pathLen, paths);

            }
            else
            {
                /* otherwise try both subtrees */

                int left = tree.LeftChild[node];
                int right = tree.RightChild[node];


                printPathsRecur(left, path, pathLen, tree, paths);
                printPathsRecur(right, path, pathLen, tree, paths);
            }
        }


        public static void printArray(int[] ints, int len, List<int[]> paths)
        {
            int[] copy = new int[len];
            int i;
            for (i = 0; i < len; i++)
            {
                Console.Write(ints[i] + " ");
                copy[i] = ints[i];
            }
            Console.WriteLine("");
            paths.Add(copy);
        }



        public static string GenerateSQLFeatureText(string column, string[] selectParams, string idParam, string textParam, string suffix, string tableName, string weightTable, float bias)
        {


            string sb = "SELECT id, (SUM( F1.count * "+weightTable+".weight) +" + bias + ") AS weight FROM" +
         "(" +
         "SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from 	" +
         "(SELECT id, feature, count(*) * (1) as count FROM " +
         "(SELECT" +
         "  " +tableName+".Id, CONCAT(\"t.\",REPLACE(REPLACE(REPLACE(lower(SUBSTRING(REPLACE("+tableName+"."+textParam+",' ','␠'), numbers.n,3)),'␠','<␠>'),'␂','<␂>'),'␃','<␃>')) as feature" +
         "FROM" +
         "  numbers INNER JOIN "+tableName+" " +
         "  ON CHAR_LENGTH("+tableName+"."+textParam+")-3 >= numbers.n-1" +
         " WHERE "+tableName+".Id <= @id" +
         ") " +
         "AS F1 " +
         "group by id,feature) AS F1" +
         "LEFT JOIN " +
         "(SELECT id,SQRT(SUM(count)) as ww FROM" +
         "(SELECT id,feature, POW(count(*),2) as count FROM " +
         "(SELECT" +
         "  "+tableName+".Id, " +
         "  CONCAT(\"t.\",REPLACE(REPLACE(lower(SUBSTRING(REPLACE("+tableName+"."+textParam+",' ','␠'), numbers.n,3)),'␠','<␠>'),'␂','<␂>')) as feature" +
         "FROM" +
         "  numbers INNER JOIN "+ tableName +
         "  ON CHAR_LENGTH("+tableName+"."+textParam+")- 3 >= numbers.n-1" +
         " WHERE "+tableName+".Id <= @id" +
         ") " +
         "AS F1" +
         "group by id,feature) " +
         "AS F2" +
         "group by id) AS F2 " +
         "ON (F2.id = F1.id)" +
         "UNION ALL" +
         "SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from 	" +
         "(SELECT id, feature, count(*) * (1) as count FROM " +
         "(SELECT" +
         "  "+tableName+".Id," +
         "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( "+tableName+"."+textParam+" ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', numbers.n), ' ', -1) ) as feature" +
         "FROM" +
         "  numbers INNER JOIN "+tableName+"" +
         "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( "+tableName+"."+textParam+" ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )))" +
         "     -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( "+tableName+"."+textParam+" ,'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= numbers.n-1" +
         " WHERE "+tableName+".Id <= @id" +
         ")" +
         "AS F1" +
         "group by id,feature) AS F1" +
         "LEFT JOIN " +
         "(SELECT id, SQRT(SUM(count)) as ww FROM" +
         "(SELECT id,feature, POW(count(*),2) as count FROM " +
         "(SELECT" +
         "  "+tableName+".Id," +
         "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( "+tableName+"."+textParam+" ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', numbers.n), ' ', -1) ) as feature" +
         "FROM" +
         "  numbers INNER JOIN "+tableName+"" +
         "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( "+tableName+"."+textParam+" ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )))" +
         "     -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( "+tableName+"."+textParam+" ,'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= numbers.n-1" +
         " WHERE "+tableName+".Id <= @id" +
         ")" +
         "AS F1" +
         "group by id,feature )" +
         "AS F2" +
         "group by id) AS F2" +
         "ON (F2.id = F1.id) " +
         "#where F1.id < 1" +
         ")" +
         "as F1 INNER JOIN "+weightTable+" ON ("+weightTable+".label = F1.feature) group by id;";



            return sb;
        }


        public static string GenerateSQLFeatureTextWithChunkSize(string column, string[] selectParams, string idParam, string textParam, string suffix, string tableName, string weightTable, float bias)
        {


            string sb = "SELECT id, (SUM( F1.count * " + weightTable + ".weight) +" + bias + ") AS weight FROM\n" +
         "(" +
         "SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from 	\n" +
         "(SELECT id, feature, count(*) * (1) as count FROM \n" +
         "(SELECT" +
         "  " + tableName + ".Id, CONCAT(\"t.\",REPLACE(REPLACE(REPLACE(lower(SUBSTRING(REPLACE(" + tableName + "." + textParam + ",' ','␠'), numbers.n,3)),'␠','<␠>'),'␂','<␂>'),'␃','<␃>')) as feature\n" +
         "FROM" +
         "  numbers INNER JOIN " + tableName + " \n" +
         "  ON CHAR_LENGTH(" + tableName + "." + textParam + ")-3 >= numbers.n-1\n" +
         " WHERE " + tableName + ".Id >= @id" + " and " + tableName + "." + idParam + " < (@id + chuncksize )\n" +
         ") \n" +
         "AS F1 \n" +
         "group by id,feature) AS F1\n" +
         "LEFT JOIN \n" +
         "(SELECT id,SQRT(SUM(count)) as ww FROM\n" +
         "(SELECT id,feature, POW(count(*),2) as count FROM \n" +
         "(SELECT\n" +
         "  " + tableName + ".Id, \n" +
         "  CONCAT(\"t.\",REPLACE(REPLACE(lower(SUBSTRING(REPLACE(" + tableName + "." + textParam + ",' ','␠'), numbers.n,3)),'␠','<␠>'),'␂','<␂>')) as feature\n" +
         "FROM\n" +
         "  numbers INNER JOIN " + tableName + "\n" +
         "  ON CHAR_LENGTH(" + tableName + "." + textParam + ")- 3 >= numbers.n-1\n" +
         " WHERE " + tableName + ".Id >= @id" + " and " + tableName + "." + idParam + " < (@id + chuncksize )\n" +
         ") \n" +
         "AS F1\n" +
         "group by id,feature) \n" +
         "AS F2\n" +
         "group by id) AS F2 \n" +
         "ON (F2.id = F1.id)\n" +
         "UNION ALL\n" +
         "SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from 	\n" +
         "(SELECT id, feature, count(*) * (1) as count FROM \n" +
         "(SELECT\n" +
         "  " + tableName + ".Id,\n" +
         "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', numbers.n), ' ', -1) ) as feature\n" +
         "FROM\n" +
         "  numbers INNER JOIN " + tableName + "\n" +
         "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )))\n" +
         "     -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= numbers.n-1\n" +
         " WHERE " + tableName + ".Id >= @id" + " and " + tableName + "." + idParam + " < (@id + chuncksize )\n" +
         ")\n" +
         "AS F1\n" +
         "group by id,feature) AS F1\n" +
         "LEFT JOIN \n" +
         "(SELECT id, SQRT(SUM(count)) as ww FROM\n" +
         "(SELECT id,feature, POW(count(*),2) as count FROM \n" +
         "(SELECT\n" +
         "  " + tableName + ".Id," +
         "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', numbers.n), ' ', -1) ) as feature\n" +
         "FROM\n" +
         "  numbers INNER JOIN " + tableName + "\n" +
         "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )))\n" +
         "     -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= numbers.n-1\n" +
         " WHERE " + tableName + ".Id >= @id" + " and " + tableName + "." + idParam + " < (@id + chuncksize )\n" +
         ")\n" +
         "AS F1\n" +
         "group by id,feature )\n" +
         "AS F2\n" +
         "group by id) AS F2\n" +
         "ON (F2.id = F1.id) \n" +
         "#where F1.id < 1\n" +
         ")\n" +
         "as F1 INNER JOIN " + weightTable + " ON (" + weightTable + ".label = F1.feature) group by id\n";



            return sb;
        }





    }
}
