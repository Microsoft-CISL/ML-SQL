using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.ML;
using MySql.Data.MySqlClient;

namespace ML2SQL
{
    public class PredictorExecutor<T1, T2> where T1 : class, SQLData, new() where T2 : class, SQLData, new()
    {
        public PredictorExecutor()
        {
        }


        public string GenerateSQLDataWithPrediction(MLContext mlContext, string modelPath, string _alldatasetFile, string tablename, string db, string sqlPath, string dbms, int n, bool debug, char sep)
        {



            ITransformer trainedModel = mlContext.Model.Load(modelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predictionEngine = mlContext.Model.CreatePredictionEngine<T1, T2>(trainedModel);

            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<T1>(_alldatasetFile, separatorChar: sep, hasHeader: true);

            T1 d = new T1();

            T2 p = new T2();

            string separator = ",";
            List<string> headers = new List<string>();
            headers.Add(d.GetHeader(","));
            headers.Add(p.GetHeader(","));
            string header = MLSQL.generateHeader(headers, separator);
            // string tablename = "sentiment_analysis_with_score";
            string sqlQuery = MLSQL.generateInsertIntoHeader(tablename, header);

            int k = 0;

            int accumulator = 1;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(sqlPath, false))
            {
                if (dbms.Equals("SQLSERVER"))
                {
                    file.WriteLine("use " + db + "\n");
                }
                file.WriteLine(sqlQuery);

                mlContext.Data.CreateEnumerable<T1>(inputDataForPredictions, reuseRowObject: false).Take(n).ToList().

                ForEach(testData =>
                {
                    T2 t = predictionEngine.Predict(testData);
                    string q = "";
                    switch (dbms)
                    {
                        case "MYSQL":
                            q = MLSQL.generateINSERTINTOLine(testData.getMySQLData(separator), t.getMySQLData(separator), separator) + ",";
                            //
                            if ((k + 1) == n)
                            {
                                Console.WriteLine("LAST CASE: ");

                                q = q.Substring(0, q.Length - 1);
                                q += ";";

                                file.WriteLine(q);
                            }
                            else
                            {
                                file.WriteLine(q);
                            }


                            break;

                        case "SQLSERVER":
                            q = MLSQL.generateINSERTINTOLine(testData.getSQLServerData(separator), t.getSQLServerData(separator), separator) + ",";
                            if (accumulator % 1000 == 0)
                            {
                                if ((k + 1) != n)
                                {
                                    q = q.Substring(0, q.Length - 1);
                                    q += ";";

                                    accumulator = 0;
                                    file.WriteLine(q);
                                    file.WriteLine("\ngo\n");
                                    q = "";
                                    file.WriteLine(sqlQuery);

                                }
                                else
                                {
                                    file.WriteLine(q);
                                }


                            }
                            else
                            {
                                if ((k + 1) != n)
                                {
                                    file.WriteLine(q);
                                    //file.WriteLine("\ngo\n");
                                }
                                else
                                {
                                    q = q.Substring(0, q.Length - 1);
                                    q += ";";
                                    accumulator = 0;
                                    file.WriteLine(q);
                                    file.WriteLine("\ngo\n");

                                }
                            }

                            break;


                    }

                    //q= MLSQL.generateINSERTINTOLine(testData.getSQLData(separator), t.getSQLData(separator), separator) + ",";



                    if (debug)
                    {
                        Console.WriteLine("" + q + "\n");
                    }
                    //sqlQuery += q;








                    accumulator++;

                    k++;
                });


                Console.WriteLine("Last index: " + k);

                //sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
                //sqlQuery += ";";

            }
            // MLSQL.WriteSQL(sqlQuery,sqlPath);
            return sqlQuery;


        }

        public void executePredictions(string Name, string modelPath, string inputSamplesFolder, string outputSamplesFolder, string resultFilePath, int[] chunckSizes, Tuple<string, string>[] Configurations, string tablename, string outputQuery, int nElements, char separator, bool header)
        {

            List<RunResult> runResults = new List<RunResult>();

            foreach (Tuple<string, string> configuration in Configurations)
            {

                string inputMode = configuration.Item1;
                string outputMode = configuration.Item2;




                List<string> results = new List<string>();

                Console.WriteLine("INPUT MODE: " + inputMode);
                Console.WriteLine("OUTPUT MODE: " + outputMode);




                foreach (int chunk in chunckSizes)
                {
                    Console.WriteLine("Chunck size: " + chunk);
                    RunResult r = runSinglePrediction(Name, modelPath, inputSamplesFolder + chunk + "/", chunk, outputSamplesFolder, inputMode, outputMode, tablename, outputQuery, nElements, separator, header, true);
                    runResults.Add(r);
                }


                Console.WriteLine("==============================================================================");
            }


            Console.WriteLine("\n\n\n\nResults: \n");

            foreach (RunResult r in runResults)
            {
                Console.WriteLine("==============================================================================");

                r.WriteResult();

                Console.WriteLine("==============================================================================");
            }


            writeRunResult(runResults, resultFilePath);
        }


        private void writeRunResult(List<RunResult> results, string path)
        {
            System.IO.StreamWriter file1 = new System.IO.StreamWriter(path, false);
            file1.WriteLine(RunResult.getHeader(","));
            foreach (RunResult r in results)
            {
                file1.WriteLine(r.GetLineCsv(","));
            }
            file1.Close();

        }


        private RunResult runSinglePrediction(string name, string modelPath, string inputFolderPath, int chunck, String outputPath, string inputMode, string outputMode, string tablename, string outputQuery, int nElements, char separator, bool header, bool singleMode)
        {


            // Read all files 

            var watch = System.Diagnostics.Stopwatch.StartNew();
            float timeWC = 0f;

            float timeWithConnection = 0f;
            float timeWithOutConnection = 0f;

            //string ConnectionStringMYSQL = "server=localhost;Uid=root;Pwd=19021990;Database=MLtoSQL";
            //string ConnectionStringSQLSERVER = "server=localhost;Uid=root;Pwd=19021990;Database=MLtoSQL";
            var mySQLConnection = new MySqlConnection(SQLConnections.MYSQLConnection);

            var sqlserverConnection = new SqlConnection(SQLConnections.SQLSERVERConnection);

            var mlContext = new MLContext();

            string outpath = outputPath;


            Console.WriteLine("InputFolderPath: " + inputFolderPath);

            switch (inputMode)
            {

                case "CSV":

                    foreach (string file in Directory.EnumerateFiles(inputFolderPath, "*.csv"))
                    {
                        //Console.WriteLine("Chunck: " + chunck);
                        //Console.WriteLine("File: "+file);

                        //string text = System.IO.File.ReadAllText(file);
                       // Console.WriteLine("FILE CONTENT: "+text);

                        IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<T1>(file, separatorChar: separator, hasHeader: header);
                        //var predictionEngine = mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(model);
                        List<T1> transactions = mlContext.Data.CreateEnumerable<T1>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();

                      //  Console.WriteLine("Number of Elements: " + transactions.Count);

                        var count = inputDataForPredictions.GetRowCount();

                        //if (count.HasValue)
                        //{
                        //    Console.WriteLine("Number of rows: " + count.Value);
                        //}

                        bool exists = System.IO.Directory.Exists(outpath);

                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(outpath);
                        }

                        // HERE OUTPUT RESULTS

                        RunResult r = OutputPrediction(mlContext, modelPath, name, outpath, outputQuery, chunck, inputDataForPredictions, outputMode, mySQLConnection, sqlserverConnection);
                        timeWithConnection += r.timeWithConnection;
                        timeWithOutConnection += r.timeWithOutConnection;

                    }



                    break;

                case "MYSQL":


                    for (int k = 1; k < nElements; k = k + chunck)
                    {

                        string q = "select * from " + tablename + " where Id >= " + k + " and Id < " + (k + chunck);

                        //Console.WriteLine("Query: " + q);

                        mySQLConnection.Open();
                        var watchWithOutConnection = System.Diagnostics.Stopwatch.StartNew();
                        MySqlCommand cmd1 = new MySqlCommand(q, mySQLConnection);
                        MySqlDataReader reader = cmd1.ExecuteReader();

                        List<T1> inputs = new List<T1>();
                        while (reader.Read())
                        {

                            //reader.ge

                            if (reader.HasRows)
                            {
                                //Console.WriteLine("r_" + reader.ToString());
                                T1 obs = new T1();
                                obs.ReadDataFromMYSQL(reader);
                                inputs.Add(obs);
                            }



                        }

                        watchWithOutConnection.Stop();

                        timeWC += watchWithOutConnection.ElapsedMilliseconds;

                        //Console.WriteLine("INPUTs Size: " + inputs.Count);


                        mySQLConnection.Close();
                        //Console.WriteLine
                        //    ("Number of elements reads from database: "+ inputs.Count);

                        if (inputs.Count > 0)
                        {
                            IDataView inputDataForPredictions = mlContext.Data.LoadFromEnumerable(inputs);

                            RunResult r = OutputPrediction(mlContext, modelPath, name, outpath, outputQuery, chunck, inputDataForPredictions, outputMode, mySQLConnection, sqlserverConnection);
                            timeWithConnection += r.timeWithConnection;
                            timeWithOutConnection += r.timeWithOutConnection;
                        }
                    }




                    break;


                case "SQLSERVER":


                    for (int k = 1; k < nElements; k = k + chunck)
                    {

                        string q = "select * from " + tablename + " where Id >= " + k + " and Id < " + (k + chunck);

                        //Console
                        //    .WriteLine("Query: "+ q);

                        sqlserverConnection.Open();
                        var watchWithOutConnection = System.Diagnostics.Stopwatch.StartNew();
                        SqlCommand cmd1 = new SqlCommand(q, sqlserverConnection);
                        SqlDataReader reader = cmd1.ExecuteReader();

                        List<T1> inputs = new List<T1>();
                        while (reader.Read())
                        {

                            //reader.ge
                            //Console.WriteLine("r_"+ reader.ToString());
                            T1 obs = new T1();
                            obs.ReadDataFromSQLServer(reader);
                            inputs.Add(obs);

                        }
                        watchWithOutConnection.Stop();

                        timeWC += watchWithOutConnection.ElapsedMilliseconds;
                        sqlserverConnection.Close();

                        //Console.WriteLine
                        //    ("Number of elements reads from database: "+ inputs.Count);

                        IDataView inputDataForPredictions = mlContext.Data.LoadFromEnumerable(inputs);

                        RunResult r = OutputPrediction(mlContext, modelPath, name, outpath, outputQuery, chunck, inputDataForPredictions, outputMode, mySQLConnection, sqlserverConnection);
                        timeWithConnection += r.timeWithConnection;
                        timeWithOutConnection += r.timeWithOutConnection;
                    }



                    break;


            }



            // Somme di tempi

            RunResult result = new RunResult(name, inputMode, outputMode, "ML.NET", chunck, timeWithOutConnection, timeWithConnection, nElements, "BATCH_MODE");

            watch.Stop();
            return result;
        }




        private RunResult OutputPrediction(MLContext mlContext, string modelPath, string name, string outpath, string outputQuery, int chunck, IDataView inputDataForPredictions, string outputMode, MySqlConnection mySQLConnection, SqlConnection sqlserverConnection)
        {

            int i = 0;
            int nElements = 0;
            int len = 0;
            float timeWithConnection = 0f;
            float timeWithoutConnection = 0f;
            System.IO.StreamWriter file1 = new System.IO.StreamWriter(outpath + "output_" + chunck + "_" + i + ".csv", false);

            var watchWithConnection = System.Diagnostics.Stopwatch.StartNew();
            var watchWithOutConnection = System.Diagnostics.Stopwatch.StartNew();
            var watchWithOutConnection1 = System.Diagnostics.Stopwatch.StartNew();
            var watchWithOutConnection2 = System.Diagnostics.Stopwatch.StartNew();

            ITransformer model = mlContext.Model.Load(modelPath, out var inputSchema);
            // Console.WriteLine("IN")
            List<T1> transactions = mlContext.Data.CreateEnumerable<T1>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();
            //Console.WriteLine("Number of elements: " + transactions.Count);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<T1, T2>(model);
            string separator = ",";

            string values = "";
            StringBuilder sb = new StringBuilder();
            transactions.ForEach
            (testData =>
            {


                nElements++;
                T2 t = predictionEngine.Predict(testData);
                String line = generateLineCSV(testData.getData(separator), t.getData(separator), separator
                    );

                if (outputMode.Equals("CSV"))
                {
                    file1.WriteLine(generateINSERTINTOLine(testData.getData(separator), t.getData(separator), separator));

                }
                else if (outputMode.Equals("CONSOLE"))
                {
                    Console.WriteLine(generateINSERTINTOLine(testData.getData(separator), t.getData(separator), separator));

                }
                else if (outputMode.Equals("NO_OUTPUT"))
                {
                    // DO NOTHING
                }
                else if (outputMode.Equals("MYSQL") || outputMode.Equals("SQLSERVER"))
                {
                    //Console.WriteLine("HERE 1");
                    // Insert into the database                            
                    string ll = "(" + testData.getId() + "," + t.getScores(separator) + "),";
                    sb.Append(ll);
                    len++;
                   // Console.WriteLine("" + ll);
                   // Console.WriteLine("HERE 2");
                    if (outputMode.Equals("SQLSERVER"))
                    {
                        if (chunck >= 1000)
                        {
                            if (nElements % 1000 == 0)
                            {
                                values = sb.ToString();
                                values = values.Substring(0, values.Length - 1);
                                values += ";";

                                // string insert = "INSERT into heart_disease_detection_with_score_output (Id,Score ) VALUES ";
                                string insert = outputQuery;
                                insert += "" + values;

                                watchWithOutConnection.Stop();
                                timeWithoutConnection += watchWithOutConnection.ElapsedMilliseconds;

                                sqlserverConnection.Open();

                                watchWithOutConnection = System.Diagnostics.Stopwatch.StartNew();

                                SqlCommand command = new SqlCommand(insert, sqlserverConnection);
                                //command.Parameters.AddWithValue("@tPatSName", "Your-Parm-Value");

                                //Console.WriteLine("INSERT: "+insert);
                                command.ExecuteNonQuery();
                                watchWithOutConnection.Stop();
                                timeWithoutConnection += watchWithOutConnection.ElapsedMilliseconds;
                                sqlserverConnection.Close();
                                sb = new StringBuilder();
                                len = 0;
                                watchWithOutConnection = System.Diagnostics.Stopwatch.StartNew();
                            }
                        }
                    }


                }

            });

            if (outputMode.Equals("MYSQL"))
            {
                values = sb.ToString();
                values = values.Substring(0, values.Length - 1);
                values += ";";
                watchWithOutConnection.Stop();
                timeWithoutConnection += watchWithOutConnection.ElapsedMilliseconds;
                mySQLConnection.Open();


                watchWithOutConnection = System.Diagnostics.Stopwatch.StartNew();
                // string insert = "INSERT into heart_disease_detection_with_score_output (Id,Score ) VALUES ";
                string insert = outputQuery;
                insert += "" + values;

                //  string cmdText = generateINSERTINTOLine(testData.Id, t.getData(separator), separator);
                MySqlCommand cmd = new MySqlCommand(insert, mySQLConnection);

                cmd.ExecuteNonQuery();
                watchWithOutConnection.Stop();
                timeWithoutConnection += watchWithOutConnection.ElapsedMilliseconds;
                mySQLConnection.Close();

            }
            else if (outputMode.Equals("SQLSERVER"))
            {
                if (len > 0)
                {
                    values = sb.ToString();
                    values = values.Substring(0, values.Length - 1);
                    values += ";";

                    // string insert = "INSERT into heart_disease_detection_with_score_output (Id,Score ) VALUES ";
                    string insert = outputQuery;
                    insert += "" + values;

                    watchWithOutConnection.Stop();
                    timeWithoutConnection += watchWithOutConnection.ElapsedMilliseconds;
                    sqlserverConnection.Open();

                    watchWithOutConnection = System.Diagnostics.Stopwatch.StartNew();
                    SqlCommand command = new SqlCommand(insert, sqlserverConnection);
                    //command.Parameters.AddWithValue("@tPatSName", "Your-Parm-Value");
                    // Console.WriteLine("INSERT: " + insert);
                    command.ExecuteNonQuery();
                    watchWithOutConnection.Stop();
                    timeWithoutConnection += watchWithOutConnection.ElapsedMilliseconds;
                    sqlserverConnection.Close();
                }

            }



            watchWithConnection.Stop();
            timeWithConnection += watchWithConnection.ElapsedMilliseconds;

            RunResult r = new RunResult(name, "", outputMode, "ML.NET", chunck, timeWithoutConnection, timeWithConnection, nElements, "BATCH");


            return r;

        }


        private string generateHeader(List<string> headers, string separator)
        {
            String line = "";
            foreach (string header in headers)
            {
                line += header + separator;
            }
            line = line.Substring(0, line.Length - separator.Length);

            return line;

        }


        private string generateLineCSV(string data, string prediction, string separator)
        {
            String line = "";

            line += data;
            line += separator;
            line += prediction;


            return line;
        }


        private string generateINSERTINTOLine(string data, string prediction, string separator)
        {
            String line = "(";

            line += data;
            line += separator;
            line += prediction;


            line += ")";

            return line;
        }



    }
}
