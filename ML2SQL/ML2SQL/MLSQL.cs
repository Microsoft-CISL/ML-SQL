using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;

namespace ML2SQL
{
    public class MLSQL
    {

        public static string ExtractWord()
        {
            return default;
        }


        public static string GenerateSQLRegressionTree(string[] columns, string[] selectParams, string outputColumn, string suffix, string tableName, string whereClause, List<double> treeWeights, List<RegressionTree> trees, Dictionary<string, string> categoricalFeatureMap = null)
        {

            string query = "select ";



            for (int i = 0; i < trees.Count(); i++)
            {

                double treeWeight = treeWeights[i];
                Microsoft.ML.Trainers.FastTree.RegressionTree tree = trees.ElementAt(i);




                String sqlCase = printPaths(0, tree, treeWeight, columns, categoricalFeatureMap);
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
            s += ") AS " + outputColumn + ",";


            for (int i = 0; i < selectParams.Length; i++)
            {
                s += selectParams[i] + ",";
            }


            s = s.Substring(0, s.Length - 1);


            string externalQuery = s + " from (" + query + " " + whereClause + " ) AS F";






            return externalQuery;
        }


        /*public static string GenerateSQLRegressionTreeSplit(string[] columns, string[] selectParams, string outputColumn, string suffix, string tableName, string whereClause, List<double> treeWeights, List<RegressionTree> trees, Dictionary<string, string> categoricalFeatureMap = null, string preProcessingTable=null, string treeScoresTable=null)
        {

            string query = "create table " + treeScoresTable + " (TreeId varchar(255), TreeScore double, Id int);\n\n\n";


            Console.WriteLine("SPLIT INIT");
            for (int i = 0; i < trees.Count(); i++)
            {

                double treeWeight = treeWeights[i];
                Microsoft.ML.Trainers.FastTree.RegressionTree tree = trees.ElementAt(i);

                String sqlCase = printPaths(0, tree, treeWeight, columns, categoricalFeatureMap);
                sqlCase += "END AS tree_" + i;

                for (int i = 0; i < selectParams.Length; i++)
                {
                    sqlCase += "," + selectParams[i];
                }

                sqlCase += " from " + preProcessingTable + " " + whereClause;
                string internalQuery = "INSERT INTO " + treeScoresTable + " select 'tree_" + i + "', tree_" + i + " from (select" + " " + sqlCase + ");";

                query += "\n\n\n" + internalQuery;


            }

            externalQuery = "select SUM(TreeScore)";
            for (int i = 0; i < selectParams.Length; i++)
            {
                externalQuery += "," + selectParams[i];
            }

            externalQuery += " from " + treeScoresTable + " GROUP BY Id";

            List<string> queryList = new List<string>();
            for (int i = 0; i < Modes.Length; i++)
            {
                string QQ = GenerateSQLQueriesOnDifferentModes(externalQuery, database, outputDB, procedure_names_FastTree[i], "Id", Modes[i], false);
                string newQuery = query.Copy();
                newQuery + = QQ + "\n\n\n";
                queryList.Add(newQuery);
            }

            return queryList;
        }*/


        public static string GenerateSQLSumANDExpColumns(string[] columns, string[] selectParams, float bias, string outcomeColumn, string tableName)
        {
            string query = "select ";
            query += " EXP ( ";
            for (int i = 0; i < columns.Length; i++)
            {
                String c = columns[i];
                query += c + "+";
            }
            query += "" + bias;
            //query = query.Substring(0, query.Length - 1);
            query += ") as " + outcomeColumn + ","
                ;
            for (int i = 0; i < selectParams.Length; i++)
            {
                query += selectParams[i] + ",";
            }

            query = query.Substring(0, query.Length - 1);

            Console.WriteLine("QUERY: " + query + "\n\n");

            query += " from " + tableName;

            return query;
        }




        public static string GenerateSQLFastTreeTweedie(string[] columns, string[] selectParams, string outputColumn, string suffix, string tableName, string whereClause, List<double> treeWeights, List<RegressionTree> trees, Dictionary<string, string> categoricalFeatureMap = null)
        {

            string query = "select ";



            for (int i = 0; i < trees.Count(); i++)
            {

                double treeWeight = treeWeights[i];
                Microsoft.ML.Trainers.FastTree.RegressionTree tree = trees.ElementAt(i);




                String sqlCase = printPaths(0, tree, treeWeight, columns, categoricalFeatureMap);
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




            string s = " select EXP (";

            for (int i = 0; i < trees.Count(); i++)
            {
                s += "tree_" + i + "+";
            }
            s = s.Substring(0, s.Length - 1);
            s += ") AS " + outputColumn + ",";


            for (int i = 0; i < selectParams.Length; i++)
            {
                s += selectParams[i] + ",";
            }


            s = s.Substring(0, s.Length - 1);


            string externalQuery = s + " from (" + query + " " + whereClause + " ) AS F";






            return externalQuery;
        }

        public static string GenerateSQLSumColumns(string[] columns, string[] selectParams, float bias, string outcomeColumn, string tableName)
        {
            string query = "select ";
            query += " ( ";
            for (int i = 0; i < columns.Length; i++)
            {
                String c = columns[i];
                query += c + "+";
            }
            query += "" + bias;
            //query = query.Substring(0, query.Length - 1);
            query += ") as " + outcomeColumn + ","
                ;
            for (int i = 0; i < selectParams.Length; i++)
            {
                query += selectParams[i] + ",";
            }

            query = query.Substring(0, query.Length - 1);

            Console.WriteLine("QUERY: " + query + "\n\n");

            query += " from " + tableName;

            return query;
        }




        public static string generateNormalizeTable(string tablename, string[] columns, Dictionary<string, Tuple<float, float>> columnsNormalizations)
        {

            List<string> selectionParams = new List<string>();


            foreach (string column in columns)
            {

                if (columnsNormalizations.ContainsKey(column))
                {
                    Tuple<float, float> t = columnsNormalizations[column];
                    string sel = "( ( " + column + " - " + t.Item1 + " ) * " + t.Item2 + " ) as " + column;

                    selectionParams.Add(sel);
                }
                else
                {
                    selectionParams.Add(column);
                }

            }

            string select = "select ";
            for (int i = 0; i < selectionParams
                .Count - 1; i++)
            {
                select += selectionParams
                    [i] + ",";
            }
            select += selectionParams[selectionParams.Count - 1];


            string query = select + "  from  " + tablename;






            return query;
        }

        public static Dictionary<string, int> createValueMappingOneHotEnconding(IDataView d, string originalColumn, string encodedColumn, int offset, bool debug)
        {
            /*var keyEncodedColumn = d.GetColumn<float[]>(encodedColumn);

            var keyColumn = d.GetColumn<string>(originalColumn);

            //One Hot Encoding of single column 'Education', with key type output.



            int lenght = keyEncodedColumn.Count();
            var valueEnconding = new Dictionary<string, float[]>();
            for (int k = 0; k < lenght; k++)
            {
                if (!valueEnconding.ContainsKey(keyColumn.ElementAt(k)))
                {
                    valueEnconding.Add(keyColumn.ElementAt(k), keyEncodedColumn.ElementAt(k));
                }
            }

            foreach (float[] element in keyEncodedColumn)
            {
                if (debug)
                {
                    // Console.WriteLine("element: " + element.ToString() + " length: " + element.Length);
                }
            }


            var categoryValue = new Dictionary<string, int>();

            foreach (string key in valueEnconding.Keys)
            {
                int index = offset;
                string categoricalValue = key;
                float[] oneHotEnconding = valueEnconding[key];
                for (int x = 0; x < oneHotEnconding.Length; x++)
                {
                    if (oneHotEnconding[x] == 1)
                    {
                        index = index + x;
                    }
                }

                categoryValue.Add(key, index);
                if (debug)
                {
                    Console.WriteLine("Value: " + categoricalValue + " featurePosition: " + index);
                }
            }

            return categoryValue;*/

            var keyEncodedColumn = d.GetColumn<float[]>(encodedColumn);

            var keyColumn = d.GetColumn<string>(originalColumn);

            VBuffer<ReadOnlyMemory<char>> encodedValues = default;
            d.Schema[encodedColumn].GetSlotNames(ref encodedValues);
            var slots = encodedValues.GetValues();
            var categoryValue = new Dictionary<string, int>();
            for (int i = 0; i < slots.Length; i++)
            {

                //int final_index = i+offset;
                //Console.WriteLine("i: " + final_index + $" {slots[i]}  ");

                string featureContent = slots[i].ToString();
                categoryValue.Add(featureContent, i+offset);
            }

            return categoryValue;
        }



        public static string JoinFeaturesWithWeights(string leftQuery, string weightTable, string joinLeftParam, string joinRightParam, string leftWeight, string rightWeight, string[] selectColumns, float bias)
        {


            string selection = "";
            foreach (string column in selectColumns)
            {
                selection += column + ",";
            }

            selection += " ( " + leftWeight + " * " + rightWeight + " ) as dot_product ";

            string query = "select " + selection + " from";

            query += "(" + leftQuery + ") AS F " + "\n INNER JOIN " + weightTable +
                "\n ON (" + joinLeftParam + " = " + joinRightParam + " )";




            string subQuery = "Select Id, (SUM(dot_product) + " + bias + " ) as Score\n from ( " + query + ") as F group by Id";

            return subQuery;
        }


        public static void WriteModelWeight(string tablename_weights, string SQL_WEIGHTS_Path, float[] weights,string dbms,string db)
        {


            

            Console.WriteLine("BEFORE COMPOSE INSERT QUERY");

            String insertinto = "INSERT INTO " + tablename_weights + " VALUES ";
            String encoding = "set names utf8;";

            if (dbms.Equals("SQLSERVER"))
            {
                encoding = "";
            }

           

            string docPath = "";

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, SQL_WEIGHTS_Path), false))
            {

                if (dbms.Equals("SQLSERVER"))
                {
                    outputFile.WriteLine("use " + db + "\n");
                }

                outputFile.WriteLine(encoding);
                outputFile.WriteLine(insertinto);

                for (int i = 0; i < weights.Length; i++)
                {
                    String label = i + "";
                    String line = "('" + Regex.Replace(label, @"\p{Cs}", "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'") + "'," + weights[i] + ")" + ",";

                    if ((i + 1) == weights.Length)
                    {


                        Console.WriteLine("Last element: " + i + 1);
                        line = line.Substring(0, line.Length - 1);
                        line += ";";
                    }
                    outputFile.WriteLine(line);

                }
            }
        }

        public static string computeWeightingAndFeatureMapping(string idColumn, string nameColumn, string valueColumn, string weightColumn, string featureColumn, string[] columns, string tableName, Dictionary<string, bool> weightingDictionary, Dictionary<string, Dictionary<string, int>> valueMapping)
        {

            string query = "select " + idColumn + "," + nameColumn + "," + valueColumn + "," +
                generateSQLWeighColumnOneHot(weightingDictionary, columns, nameColumn, valueColumn
               ) + " as " + weightColumn + "," +
                generateSQLFeatureColumnOneHot(valueMapping, columns, nameColumn, valueColumn) + " as " + featureColumn + "\n FROM " + tableName;



            return query;
        }


        private static string generateSQLFeatureColumnOneHot(Dictionary<string, Dictionary<string, int>> valueMapping, string[] columns, string nameColumn, string valueColumn)

        {


            string query = "CASE \n";
            foreach (string column in columns)
            {
                if (valueMapping.ContainsKey(column))
                {
                    Dictionary<string, int> vv = valueMapping[column];

                    if (vv.Count == 1)
                    {
                        if (vv.
                           Keys.ElementAt(0).Equals(""))
                        {
                            query += "WHEN " + nameColumn + " = " + "'" + column + "'" + " THEN " + vv.Values.ElementAt(0) + "\n";
                        }
                        else
                        {
                            query += "WHEN " + nameColumn + " = " + "'" + column + "'" + " THEN " + vv.Values.ElementAt(0) + "\n";
                        }

                    }
                    else
                    {
                        foreach (string k in vv.Keys)
                        {
                            query += "WHEN " + nameColumn + " = " + "'" + column + "'" + " AND " + "" + valueColumn + "" + " = " + "'" + k + "'" + " THEN " + vv[k] + "\n";
                        }
                    }
                }


            }
            query += "\n END\n";



            return query;
        }

        private static string generateSQLWeighColumnOneHot(Dictionary<string, bool> weightingDictionary, string[] columns, string nameColumn, string valueColumn)
        {
            string query = "CASE \n";
            foreach (string column in columns)
            {
                string res = "1";
                if (!weightingDictionary[column])
                {
                    res = valueColumn;
                }


                query += "WHEN " + nameColumn + " = " + "'" + column + "'" + " THEN " + res + "\n";
            }
            query += "\n END\n";

            return query;
        }


        public static string trasposeColumnsToRows(string idColumn, string[] columns, string tableName)
        {
            string query = "";
            List<string> unionPiecies = new List<string>();

            foreach (string column in columns)
            {
                string q = "select " + idColumn + "," + "'" + column + "'" + " as name " + "," + column + " as value \n" + " from " + tableName + "\n";
                unionPiecies.Add(q);

            }

            for (int c = 0; c < unionPiecies.Count - 1; c++)
            {
                query += unionPiecies[c] + "\n UNION ALL \n";
            }


            query += "\n" + unionPiecies[unionPiecies.Count - 1];


            return query;
        }


        public static string GenerateMultiClassSdcaMaximumEntropy(string[] columns, string[] selectParams, string suffix, string tableName, List<float> biases, List<VBuffer<float>> weights, bool includeWhereClause)
        {


            string queryInternal = "select ";

            string wildcard = "class_";

            foreach (string param in selectParams)
            {
                queryInternal += param + ",";
            }

            for (int i = 0; i < biases.Count(); i++)
            {
                float bias = biases.ElementAt(i);

                VBuffer<float> w = weights.ElementAt(i);

                string q1 = GenerateLinearCombination(columns, selectParams, "", bias, w);

                queryInternal += "(" + q1 + ") as " + wildcard + i + ",";


            }

            queryInternal = queryInternal.Substring(0, queryInternal.Length - 1);

            queryInternal += "\n from " + tableName;

            if (includeWhereClause)
            {
                string whereClause = "\n where Id >= @id  and Id < ( @id + chuncksize ); \n";
                queryInternal += whereClause;
            }


            queryInternal = " ( " + queryInternal + " ) as F ";

            string query = "select ";
            foreach (string param in selectParams)
            {
                query += param + ",";
            }

            string sum = "(";
            for (int i = 0; i < biases.Count(); i++)
            {
                sum += "EXP(" + wildcard + i + ")+";
            }
            sum = sum.Substring(0, sum.Length - 1);

            sum += ")";


            for (int i = 0; i < biases.Count(); i++)
            {
                query += "(" + "EXP(" + wildcard + i + ")" + " / " + sum + ") as " + wildcard + i + ",";
            }

            query = query.Substring(0, query.Length - 1);
            query += "\n from " + queryInternal;




            return query;
        }



        private static string GenerateLinearCombination(string[] columns, string[] selectParams, string suffix, double bias, VBuffer<float> weights)
        {

            string query = "";
            for (int i = 0; i < columns.Length; i++)
            {
                String c = columns[i];
                query += "(" + c + " * " + weights.GetItemOrDefault(i) + " ) + ";
            }
            query += bias;
            //for (int i = 0; i < selectParams.Length; i++)
            //{
            //    query += selectParams[i] + ",";
            //}

            //query = query.Substring(0, query.Length - 1);




            return query;
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



        public static string GenerateSQLQueriesOnDifferentModes(string query, string databaseName, string outputDatabaseName, string procedureName, string idParams, string mode, bool includeWhereClause)
        {
            string outputMode = "";


            string q = " select * from (" + query + " ) AS F;";

            switch (mode)
            {
                case "":


                    break;
                //File output
                case "CSV":
                    /*q = "select * from(" + query + ") AS F " + "\n" + "INTO OUTFILE \'/home/matteo/Scaricati/ml2sql_mysql_files/output.csv\' " +
                        "FIELDS TERMINATED BY \',\'" +
                        "ENCLOSED BY \'\"\'" +
                        " LINES TERMINATED BY '\n\' ;";*/
                    q = "select * from(" + query + ") AS F " + "\n" + "INTO OUTFILE \'/mnt/mysql-files/output.csv\' " +
                        "FIELDS TERMINATED BY \',\'" +
                        "ENCLOSED BY \'\"\'" +
                        " LINES TERMINATED BY '\n\' ;";
                    break;
                //Output
                case "NO_OUTPUT":

                    outputMode = "";
                    q = " select count(1) from (" + query + " ) AS F;";
                    break;
                //No output
                case "MYSQL":
                    outputMode = "insert into " + outputDatabaseName + " \n";
                    //Database
                    break;

                case "SQLSERVER":
                    outputMode = "insert into " + outputDatabaseName + " \n";
                    //Database
                    break;
            }





            string whereClause = "\n where Id >= @id  and Id < ( @id + chuncksize ); \n";

            if (!includeWhereClause)
            {
                whereClause = "";
            }




            return outputMode + q + whereClause;

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
                if (header)
                {
                    file.WriteLine(lines1[0]);
                }

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




        public static string printPaths(int node, Microsoft.ML.Trainers.FastTree.RegressionTree tree, double treeWeight, string[] featureColumnNames, Dictionary<string, string> categoricalFeatureMap = null)
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

                    // FIXME: PART FOR MANAGING ONE HOT ENCODING FEATURES IN A FAST TREE
                    if (categoricalFeatureMap != null && categoricalFeatureMap.ContainsKey(featureString))
                    {
                        string oldOper = oper;
                        string oldFeatureString = featureString;
                        float oldNumericalSplit = numericalsplit;

                        oper = "=";
                        featureString = categoricalFeatureMap[oldFeatureString];
                        string numericalsplitString = "'" + oldFeatureString.Replace(featureString + "_", "") + "'";
                        if (oldOper == " < ")
                        {
                            if (0 <= oldNumericalSplit)
                            {
                                oper = "<>";
                            }
                        }
                        else
                        {
                            if (0 > oldNumericalSplit)
                            {
                                oper = "<>";
                            }
                        }

                        sql += featureString + oper + numericalsplitString;
                    }
                    else
                    {
                        sql += featureString + oper + numericalsplit;
                    }


                    //sql += featureString + oper + numericalsplit;




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


        public static string formatData( object o, string dbms)
        {
            string res = "";
            if(o is string)
            {
                return EscapeStringSQL((string)o);

            }
            else if( o is float)
            {
                float value = (float) o; 
                string formatted = String.Format("{0:F20}", value);
                return formatted;

            }
            else if(o is double)
            {
                double value = (double)o;
                string formatted = String.Format("{0:F20}", value);
                return formatted;
            }else if (o is int)
            {
                int value = (int)o;
               
                return value+"";
            }else if (o is bool)
            {

                bool value = (bool)o;
                switch (dbms){

                   
                    case "MYSQL":
                        return o + "";

                        break;

                    case "SQLSERVER":
                        if (value)
                        {
                            return "1";
                        }
                        return "0";
                        break;
                }
            }
            else
            {
                res = o + "";
            }

            return res;
        }

        public static string GenerateSQLNormalizeByVarianceColumns(string[] columns, string[] selectParams,string whereClause, float[] avgs, float[] stds, string suffix, string tableName)
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

            Console.WriteLine("QUERY: " + query + whereClause+ "\n\n");

            return query;
        }

        /*public static Dictionary<string, string> GenerateSQLTableAfterNormalization(string normalizationTableName, string normalizeQuery, string tableName)
        {
            Dictionary<string, string> normalizeQueries = new Dictionary<string, string>();
            // MYSQL
            string mysqlQuery = "CREATE TABLE IF NOT EXISTS " + normalizationTableName + " " + normalizeQuery + ";";
            normalizeQueries.Add("MYSQL", mysqlQuery);
            //SQL SERVER
            string sqlserverQuery = normalizeQuery.Replace("from " + tableName, "INTO " + normalizationTableName + " from " + tableName + ";");
            normalizeQueries.Add("SQLSERVER", sqlserverQuery);

            return normalizeQueries;

        }*/
        public static string GenerateSQLTableAfterNormalization(string normalizationTableName, string normalizeQuery, string whereClause)
        {

            string insertQuery = "INSERT INTO " + normalizationTableName + " " + normalizeQuery + " " + whereClause + " ;";

            return insertQuery;
        }


        public static string GenerateSQLMultiplyByLinearCombination(string[] columns, string[] selectParams, string whereClause, float[] weights, string suffix, string tableName)
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

            query += " from " + tableName + " " + whereClause;


            return query;
        }





        public static string GenerateSQLRegressionTree(string[] columns, string[] selectParams, string suffix, string tableName, string whereClause, List<double> treeWeights, List<RegressionTree> trees, Dictionary<string, string> categoricalFeatureMap = null)
        {

            string query = "select ";



            for (int i = 0; i < trees.Count(); i++)
            {

                double treeWeight = treeWeights[i];
                Microsoft.ML.Trainers.FastTree.RegressionTree tree = trees.ElementAt(i);




                String sqlCase = printPaths(0, tree, treeWeight, columns, categoricalFeatureMap);
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
            s += ") AS Score,";


            for (int i = 0; i < selectParams.Length; i++)
            {
                s += selectParams[i] + ",";
            }


            s = s.Substring(0, s.Length - 1);


            string externalQuery = s + " from (" + query +" "+ whereClause +" ) AS F";






            return externalQuery;
        }

        public static string GenerateSQLDeleteTemporaryTable(string table)
        {
            string q = "delete from " + table + ";";

            return q;
        }

        public static string GenerateSQLDeleteTemporaryTables(string[] tables)
        {
            string joinQuery = "";
            foreach (string table in tables)
            {
                string q = GenerateSQLDeleteTemporaryTable(table);
                joinQuery += q + "\n\n";
            }

            return joinQuery;
        }

        public static List<string> GenerateSQLRegressionTreeSplit(string[] columns, string[] selectParams, string suffix, string tableName, string whereClause, List<double> treeWeights, List<RegressionTree> trees, string[] Modes, string[] procedure_names_FastTree, string outputDB, string database, Dictionary<string, string> categoricalFeatureMap = null, string preProcessingTable=null, string treeScoresTable=null)
        {

            //string query = "create table " + treeScoresTable + " (TreeId varchar(255), TreeScore double, Id int);\n\n\n";
            string query = "";
            if (preProcessingTable == null)
            {
                preProcessingTable = tableName;
            }


            Console.WriteLine("SPLIT INIT");
            for (int i = 0; i < trees.Count(); i++)
            {

                double treeWeight = treeWeights[i];
                Microsoft.ML.Trainers.FastTree.RegressionTree tree = trees.ElementAt(i);

                String sqlCase = printPaths(0, tree, treeWeight, columns, categoricalFeatureMap);
                sqlCase += "END AS tree_" + i;

                for (int j = 0; j < selectParams.Length; j++)
                {
                    sqlCase += "," + selectParams[j];
                }

                sqlCase += " from " + preProcessingTable + " " + whereClause;
                string internalQuery = "INSERT INTO " + treeScoresTable + " select 'tree_" + i + "', tree_" + i;

                for (int j = 0; j < selectParams.Length; j++)
                {
                    internalQuery += "," + selectParams[j];
                }

                internalQuery += " from (select" + " " + sqlCase + ") as f;";

                query += "\n\n\n" + internalQuery;


            }

            string externalQuery = "select SUM(TreeScore)";
            for (int i = 0; i < selectParams.Length; i++)
            {
                externalQuery += "," + selectParams[i];
            }

            externalQuery += " from " + treeScoresTable + " " + whereClause + " GROUP BY Id";

            List<string> queryList = new List<string>();
            for (int i = 0; i < Modes.Length; i++)
            {
                string QQ = GenerateSQLQueriesOnDifferentModes(externalQuery, database, outputDB, procedure_names_FastTree[i], "Id", Modes[i], false);
                string newQuery = String.Copy(query);
                newQuery += QQ + "\n\n\n";
                queryList.Add(newQuery);
            }

            return queryList;
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


            string sb = "SELECT id, (SUM( F1.count * " + weightTable + ".weight) +" + bias + ") AS weight FROM" +
         "(" +
         "SELECT F1.id, F1.feature, 1/((1/(F1.count) )* F2.ww) as count from 	" +
         "(SELECT id, feature, count(*) * (1) as count FROM " +
         "(SELECT" +
         "  " + tableName + ".Id, CONCAT(\"t.\",REPLACE(REPLACE(REPLACE(lower(SUBSTRING(REPLACE(" + tableName + "." + textParam + ",' ','␠'), numbers.n,3)),'␠','<␠>'),'␂','<␂>'),'␃','<␃>')) as feature" +
         "FROM" +
         "  numbers INNER JOIN " + tableName + " " +
         "  ON CHAR_LENGTH(" + tableName + "." + textParam + ")-3 >= numbers.n-1" +
         " WHERE " + tableName + ".Id <= @id" +
         ") " +
         "AS F1 " +
         "group by id,feature) AS F1" +
         "LEFT JOIN " +
         "(SELECT id,SQRT(SUM(count)) as ww FROM" +
         "(SELECT id,feature, POW(count(*),2) as count FROM " +
         "(SELECT" +
         "  " + tableName + ".Id, " +
         "  CONCAT(\"t.\",REPLACE(REPLACE(lower(SUBSTRING(REPLACE(" + tableName + "." + textParam + ",' ','␠'), numbers.n,3)),'␠','<␠>'),'␂','<␂>')) as feature" +
         "FROM" +
         "  numbers INNER JOIN " + tableName +
         "  ON CHAR_LENGTH(" + tableName + "." + textParam + ")- 3 >= numbers.n-1" +
         " WHERE " + tableName + ".Id <= @id" +
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
         "  " + tableName + ".Id," +
         "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', numbers.n), ' ', -1) ) as feature" +
         "FROM" +
         "  numbers INNER JOIN " + tableName + "" +
         "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )))" +
         "     -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= numbers.n-1" +
         " WHERE " + tableName + ".Id <= @id" +
         ")" +
         "AS F1" +
         "group by id,feature) AS F1" +
         "LEFT JOIN " +
         "(SELECT id, SQRT(SUM(count)) as ww FROM" +
         "(SELECT id,feature, POW(count(*),2) as count FROM " +
         "(SELECT" +
         "  " + tableName + ".Id," +
         "  CONCAT(\"w.\",SUBSTRING_INDEX(SUBSTRING_INDEX( lower(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )) ) ,' ', numbers.n), ' ', -1) ) as feature" +
         "FROM" +
         "  numbers INNER JOIN " + tableName + "" +
         "  ON CHAR_LENGTH(TRIM(REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '), '  ', ' ' )))" +
         "     -CHAR_LENGTH(REPLACE( TRIM( REPLACE(REPLACE(REPLACE(REPLACE( " + tableName + "." + textParam + " ,'␂',''),'␃',''),'  ',' '),'  ',' ')  ) , ' ', '') ) >= numbers.n-1" +
         " WHERE " + tableName + ".Id <= @id" +
         ")" +
         "AS F1" +
         "group by id,feature )" +
         "AS F2" +
         "group by id) AS F2" +
         "ON (F2.id = F1.id) " +
         "#where F1.id < 1" +
         ")" +
         "as F1 INNER JOIN " + weightTable + " ON (" + weightTable + ".label = F1.feature) group by id;";



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
