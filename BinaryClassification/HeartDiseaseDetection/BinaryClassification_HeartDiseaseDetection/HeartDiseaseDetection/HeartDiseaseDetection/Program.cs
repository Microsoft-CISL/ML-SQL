using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using HeartDiseasePredictionConsoleApp.DataStructures;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using ML2SQL;
using MySql.Data.MySqlClient;

namespace HeartDiseasePredictionConsoleApp
{
    public class Program
    {
        private static string BaseDatasetsRelativePath = @"../../../../Data";


        private static string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/HeartTraining.csv";
        private static string TestDataRelativePath = $"{BaseDatasetsRelativePath}/HeartTest.csv";



        private static string BaseSQLRelativePath = @"../../../../SQL_scripts";
        private static string MYSQL_INSERTRelativePath = $"{BaseSQLRelativePath}/02_MYSQL_INSERT.sql";
        private static string SQLSERVER_INSERTRelativePath = $"{BaseSQLRelativePath}/02_SQLSERVER_INSERT.sql";



        private static string MYSQL_QUERY_RelativePath = $"{BaseSQLRelativePath}/03_MYSQL_PREDICTION.sql";
        private static string SQLSERVER_QUERY_RelativePath = $"{BaseSQLRelativePath}/03_SQLSERVER_PREDICTION.sql";

        private static string MYSQL_QUERY = GetAbsolutePath(MYSQL_QUERY_RelativePath);
        private static string SQLSERVER_QUERY = GetAbsolutePath(SQLSERVER_INSERTRelativePath);

        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);


        private static string TrainDataRelativePathWithId = $"{BaseDatasetsRelativePath}/HeartTrainingWithId.csv";
        private static string TrainDataPathWithId = GetAbsolutePath(TrainDataRelativePathWithId);

        private static string TestDataRelativePathWithId = $"{BaseDatasetsRelativePath}/HeartTestWithId.csv";
        private static string TestDataPathWithId = GetAbsolutePath(TestDataRelativePathWithId);

        private static string AllDataRelativePathWithId = $"{BaseDatasetsRelativePath}/HeartAllWithId.csv";
        private static string DataPathWithId = GetAbsolutePath(AllDataRelativePathWithId);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/HeartClassification.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);


        private static string MYSQL_INSERT_Path = GetAbsolutePath(MYSQL_INSERTRelativePath);
        private static string SQLSERVER_INSERT_Path = GetAbsolutePath(SQLSERVER_INSERTRelativePath);

        private static string BaseDatasetsRelativePathSamples = @"../../../../Data/Samples";
        private static string SamplesInput = $"{BaseDatasetsRelativePathSamples}/Input/";
        private static string SamplesOutput = $"{BaseDatasetsRelativePathSamples}/Output/";


        private static string ResultOutput = @"../../../heartDiseaseResults.csv";

        private static string SampleInputDirectory = GetAbsolutePath(SamplesInput);

        private static string SampleOutputDirectory = GetAbsolutePath(SamplesOutput);

        public static void Main(string[] args)
        {
            Console.WriteLine("START!!");
            var mlContext = new MLContext();
            Console.WriteLine("TrainDataPath: " + TrainDataPath);

            BuildTrainEvaluateAndSaveModel(mlContext);

            TestPrediction(mlContext);

            int[] chunckSizes = { 1, 10, 100, 1000 };
            MLSQL.createDataSample(DataPathWithId, SamplesInput, chunckSizes, false);


            int nElements = 300;
            string tablename = "heart_disease_detection_with_score";

            string name = "HEART_DISEASE";

            string outputQuery = " INSERT into heart_disease_detection_with_score_output(Id, Score ) VALUES ";

            string[] inputModes = { "CSV", "MYSQL", "SQLSERVER" };

            string[] outputModes = { "CSV", "CONSOLE", "NO_OUTPUT", "MYSQL", "SQLSERVER" };

            char separator = ';';
            bool header = false;

            Tuple<string, string>[] configurations = {
            new Tuple<string,string> (inputModes[0],outputModes[0]) ,
            new Tuple<string,string> (inputModes[0],outputModes[1]),
            new Tuple<string,string> (inputModes[0],outputModes[2]),
            new Tuple<string,string> (inputModes[0],outputModes[3]),
            new Tuple<string,string> (inputModes[0],outputModes[4]),
            new Tuple<string,string> (inputModes[1],outputModes[3]),
            new Tuple<string,string> (inputModes[2],outputModes[4])
            };




            PredictorExecutor<HeartData, HeartPrediction> predictorExecutor = new PredictorExecutor<HeartData, HeartPrediction>();

            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, ModelPath, AllDataRelativePathWithId, "heart_disease_detection_with_score", "MLtoSQL", MYSQL_INSERTRelativePath, "MYSQL", nElements, false,';');
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, ModelPath, AllDataRelativePathWithId, "heart_disease_detection_with_score", "mltosql", SQLSERVER_INSERTRelativePath, "SQLSERVER", nElements, false,';');

            predictorExecutor.executePredictions(name, ModelPath, SamplesInput, SamplesOutput, ResultOutput, chunckSizes, configurations, tablename, outputQuery, nElements, separator, header);


            Environment.Exit(0);

            List<RunResult> runResults = new List<RunResult>();

            foreach (Tuple<string, string> configuration in configurations)
            {

                string inputMode = configuration.Item1;
                string outputMode = configuration.Item2;




                List<string> results = new List<string>();

                Console.WriteLine("INPUT MODE: " + inputMode);
                Console.WriteLine("OUTPUT MODE: " + outputMode);

                foreach (int chunk in chunckSizes)
                {
                    Console.WriteLine("Chunck size: " + chunk);
                    RunResult r = runSinglePrediction(name, ModelPath, SamplesInput + chunk + "/", chunk, SamplesOutput, inputMode, outputMode, tablename, outputQuery, nElements, ';', true);
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

        }



        private static void a()
        {

        }



        public static void executePredictions(string Name, string modelPath, string inputSamplesFolder, string outputSamplesFolder, string resultFilePath, int[] chunckSizes, Tuple<string, string>[] Configurations, string tablename, string outputQuery, int nElements, char separator)
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
                    RunResult r = runSinglePrediction(Name, modelPath, inputSamplesFolder + chunk + "/", chunk, outputSamplesFolder, inputMode, outputMode, tablename, outputQuery, nElements, separator, true);
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
        }


        private void writeRunResult(List<RunResult> results, string path)
        {
            System.IO.StreamWriter file1 = new System.IO.StreamWriter(path, false);
            file1.WriteLine(RunResult.getHeader(","));
            foreach (RunResult r in results)
            {
                file1.WriteLine(r.GetLineCsv(","));
            }

        }




        private static RunResult runSinglePrediction(string name, string modelPath, string inputFolderPath, int chunck, String outputPath, string inputMode, string outputMode, string tablename, string outputQuery, int nElements, char separator, bool singleMode)
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




            switch (inputMode)
            {

                case "CSV":

                    foreach (string file in Directory.EnumerateFiles(inputFolderPath, "*.csv"))
                    {

                        IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<HeartData>(file, separatorChar: separator, hasHeader: false);
                        //var predictionEngine = mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(model);



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

                        List<HeartData> inputs = new List<HeartData>();
                        while (reader.Read())
                        {

                            //reader.ge

                            if (reader.HasRows)
                            {
                                //Console.WriteLine("r_" + reader.ToString());
                                HeartData obs = new HeartData();
                                obs.ReadDataFromMYSQL(reader);
                                //{
                                //    Age = reader.GetFloat("Age"),
                                //    Sex = reader.GetFloat("Sex"),
                                //    Cp = reader.GetFloat("Sex"),
                                //    TrestBps = reader.GetFloat("TrestBps"),
                                //    Chol = reader.GetFloat("Chol"),
                                //    Fbs = reader.GetFloat("Fbs"),
                                //    RestEcg = reader.GetFloat("RestEcg"),
                                //    Thalac = reader.GetFloat("Thalac"),
                                //    Exang = reader.GetFloat("Exang"),
                                //    OldPeak = reader.GetFloat("OldPeak"),
                                //    Slope = reader.GetFloat("Slope"),
                                //    Ca = reader.GetFloat("Ca"),
                                //    Thal = reader.GetFloat("Thal"),
                                //    Label = reader.GetBoolean("Label"),
                                //    Id = reader.GetInt32("Id")
                                //};


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

                        List<HeartData> inputs = new List<HeartData>();
                        while (reader.Read())
                        {

                            //reader.ge
                            //Console.WriteLine("r_"+ reader.ToString());
                            HeartData
                                obs = new HeartData
                                ();
                            //{

                            //    Age = reader.GetFloat(0),
                            //    Sex = reader.GetFloat(1),
                            //    Cp = reader.GetFloat(2),
                            //    TrestBps = reader.GetFloat(3),
                            //    Chol = reader.GetFloat(4),
                            //    Fbs = reader.GetFloat(5),
                            //    RestEcg = reader.GetFloat(6),
                            //    Thalac = reader.GetFloat(7),
                            //    Exang = reader.GetFloat(8),
                            //    OldPeak = reader.GetFloat(9),
                            //    Slope = reader.GetFloat(10),
                            //    Ca = reader.GetFloat(11),
                            //    Thal = reader.GetFloat(12),
                            //    Label = reader.GetBoolean(13),
                            //    Id = reader.GetInt32(14)
                            //};

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




        private static RunResult OutputPrediction(MLContext mlContext, string modelPath, string name, string outpath, string outputQuery, int chunck, IDataView inputDataForPredictions, string outputMode, MySqlConnection mySQLConnection, SqlConnection sqlserverConnection)
        {

            int i = 0;
            int nElements = 0;
            float timeWithConnection = 0f;
            float timeWithoutConnection = 0f;
            System.IO.StreamWriter file1 = new System.IO.StreamWriter(outpath + "output_" + chunck + "_" + i + ".csv", false);

            var watchWithConnection = System.Diagnostics.Stopwatch.StartNew();
            var watchWithOutConnection = System.Diagnostics.Stopwatch.StartNew();

            ITransformer model = mlContext.Model.Load(modelPath, out var inputSchema);
            List<HeartData> transactions = mlContext.Data.CreateEnumerable<HeartData>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();
            var predictionEngine = mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(model);
            string separator = "";

            string values = "";
            StringBuilder sb = new StringBuilder();
            transactions.ForEach
            (testData =>
            {


                nElements++;
                HeartPrediction t = predictionEngine.Predict(testData);
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
                    // Insert into the database                            
                    string ll = "(" + testData.Id + "," + t.Score + "),";
                    sb.Append(ll);

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


                watchWithOutConnection.Start();
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
                values = sb.ToString();
                values = values.Substring(0, values.Length - 1);
                values += ";";

                // string insert = "INSERT into heart_disease_detection_with_score_output (Id,Score ) VALUES ";
                string insert = outputQuery;
                insert += "" + values;

                watchWithOutConnection.Stop();
                timeWithoutConnection += watchWithOutConnection.ElapsedMilliseconds;
                sqlserverConnection.Open();

                SqlCommand command = new SqlCommand(insert, sqlserverConnection);
                //command.Parameters.AddWithValue("@tPatSName", "Your-Parm-Value");
                command.ExecuteNonQuery();
                watchWithOutConnection.Stop();
                timeWithoutConnection += watchWithOutConnection.ElapsedMilliseconds;
                sqlserverConnection.Close();
            }


            watchWithConnection.Stop();
            timeWithConnection += watchWithConnection.ElapsedMilliseconds;


            RunResult r = new RunResult(name, "", outputMode, "ML.NET", chunck, timeWithoutConnection, timeWithConnection, nElements, "BATCH");


            return r;

        }




        private static void BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // STEP 1: Common data loading configuration
            var trainingDataView = mlContext.Data.LoadFromTextFile<HeartData>(TrainDataPath, hasHeader: true, separatorChar: ';');
            var testDataView = mlContext.Data.LoadFromTextFile<HeartData>(TestDataPath, hasHeader: true, separatorChar: ';');

            // STEP 2: Concatenate the features and set the training algorithm
            var pipeline = mlContext.Transforms.Concatenate("Features", "Age", "Sex", "Cp", "TrestBps", "Chol", "Fbs", "RestEcg", "Thalac", "Exang", "OldPeak", "Slope", "Ca", "Thal")
                .Append(mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features")).AppendCacheCheckpoint(mlContext);

            string[] featureColumnNames = { "Age", "Sex", "Cp", "TrestBps", "Chol", "Fbs", "RestEcg", "Thalac", "Exang", "OldPeak", "Slope", "Ca", "Thal" };

            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = pipeline.Fit(trainingDataView);

            Microsoft.ML.Trainers.FastTree.RegressionTreeEnsemble a = trainedModel.LastTransformer.Model.SubModel.TrainedTreeEnsemble;
            List<RegressionTree> trees = (System.Collections.Generic.List<Microsoft.ML.Trainers.FastTree.RegressionTree>)a.Trees;
            List<double> treeWeights = (System.Collections.Generic.List<double>)a.TreeWeights;

            string[] selectParams = { "Id" };
            string whereClause = "\n where Id >= @id  and Id < ( @id + chuncksize ) \n";

            String query = MLSQL.GenerateSQLRegressionTree(featureColumnNames, selectParams, "", "heart_disease_detection_with_score", whereClause,treeWeights, trees);

            Console.WriteLine("Query: " + query);


            string query_SQLSERVER = " use mltosql \n go \n " + query + " \n go \n";

            ML2SQL.MLSQL.WriteSQL(query, MYSQL_QUERY_RelativePath);
            MLSQL.WriteSQL(query_SQLSERVER
                , SQLSERVER_QUERY_RelativePath);

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("=============== Finish the train model. Push Enter ===============");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);

            var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"************************************************************");
            Console.WriteLine($"*       Metrics for {trainedModel.ToString()} binary classification model      ");
            Console.WriteLine($"*-----------------------------------------------------------");
            Console.WriteLine($"*       Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"*       Area Under Roc Curve:      {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"*       Area Under PrecisionRecall Curve:  {metrics.AreaUnderPrecisionRecallCurve:P2}");
            Console.WriteLine($"*       F1Score:  {metrics.F1Score:P2}");
            Console.WriteLine($"*       LogLoss:  {metrics.LogLoss:#.##}");
            Console.WriteLine($"*       LogLossReduction:  {metrics.LogLossReduction:#.##}");
            Console.WriteLine($"*       PositivePrecision:  {metrics.PositivePrecision:#.##}");
            Console.WriteLine($"*       PositiveRecall:  {metrics.PositiveRecall:#.##}");
            Console.WriteLine($"*       NegativePrecision:  {metrics.NegativePrecision:#.##}");
            Console.WriteLine($"*       NegativeRecall:  {metrics.NegativeRecall:P2}");
            Console.WriteLine($"************************************************************");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("=============== Saving the model to a file ===============");
            mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPath);
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("=============== Model Saved ============= ");



            //  string alldatasetFile;
            // string sqlOutputPath;
            // int n = 0;
            string separator = ";";
            //generate all dataset with id
            // int start_id = 0;
            int id = MLSQL.insertUniqueId(TrainDataPath, TrainDataPathWithId, false, separator, 0);
            Console.WriteLine("id: " + id);
            id = MLSQL.insertUniqueId(TestDataPath, TestDataPathWithId, false, separator, id + 1);
            Console.WriteLine("id: " + id);


            MLSQL.JoinTrainAndTest(TrainDataPathWithId, TestDataPathWithId, AllDataRelativePathWithId, false);



            // generate sql scripts
            // GenerateSQLDataWithPrediction(mlContext, AllDataRelativePathWithId, MYSQL_INSERT_Path, "MYSQL", id);

            // GenerateSQLDataWithPrediction(mlContext, AllDataRelativePathWithId, SQLSERVER_INSERT_Path, "SQLSERVER", id);


            string[] modes = { "CONSOLE", "NO_OUTPUT", "MYSQL", "CSV" };
            int[] slices = { 1, 10, 100, 1000 };
            string[] procedure_names = { "heart", "heart_no_output", "heart_db", "heart_csv" };

            int n = 302;
            for (int i = 0; i < modes.Length; i++)
            {
                Console.WriteLine("PROCEDURE: " + procedure_names[i]);
                //string s = MLSQL.GenerateSQLProcededure(query, "MLtoSQL", "heart_disease_detection_with_score_output", procedure_names[i], "Id", modes[i]);

                bool includeWhereClause = false;

                string s = MLSQL.GenerateSQLQueriesOnDifferentModes(query, "MLtoSQL", "heart_disease_detection_with_score_output", procedure_names[i], "Id", modes[i], includeWhereClause);

                string path = $"{BaseSQLRelativePath}/MYSQL_PREDICTION_WITH_ID_" + procedure_names[i] + ".sql";
                MLSQL.WriteSQL(s, GetAbsolutePath(path));


                string path1 = $"{BaseSQLRelativePath}/SQLSERVER_PREDICTION_WITH_ID_" + procedure_names[i] + ".sql";
                MLSQL.WriteSQL(s, GetAbsolutePath(path1));

            }

        }



        private static string GenerateSQLDataWithPrediction(MLContext mlContext, string _alldatasetFile, string sqlPath, string db, int n)
        {



            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predictionEngine = mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(trainedModel);

            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<HeartData>(_alldatasetFile, separatorChar: ';', hasHeader: true);

            HeartData d = new HeartData();

            HeartPrediction p = new HeartPrediction();

            string separator = ",";
            List<string> headers = new List<string>();
            headers.Add(d.GetHeader(","));
            headers.Add(p.GetHeader(","));
            string header = generateHeader(headers, separator);
            string tablename = "heart_disease_detection_with_score";


            string sqlQuery = "";

            switch (db)
            {
                case "MYSQL":
                    break;

                case "SQLSERVER":
                    sqlQuery += "use mltosql\n";
                    sqlQuery += "go\n";
                    break;
            }
            sqlQuery += generateInsertIntoHeader(tablename, header);


            mlContext.Data.CreateEnumerable<HeartData>(inputDataForPredictions, reuseRowObject: false).Take(n).ToList().

                ForEach(testData =>
                {


                    HeartPrediction t = predictionEngine.Predict(testData);

                    string q = "";

                    switch (db)
                    {
                        case "MYSQL":
                            q = generateINSERTINTOLine(testData.getMySQLData(separator), t.getMySQLData(separator), separator) + ",";
                            break;

                        case "SQLSERVER":
                            q = generateINSERTINTOLine(testData.getSQLServerData(separator), t.getSQLServerData(separator), separator) + ",";
                            break;

                    }


                    sqlQuery += q;
                    Console.WriteLine(q);

                });


            sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
            sqlQuery += ";\n";

            switch (db)
            {
                case "MYSQL":
                    break;

                case "SQLSERVER":
                    sqlQuery += "go\n";
                    break;
            }


            writeInsertionFileDB(sqlPath, sqlQuery);
            return sqlQuery;


        }


        private static void writeInsertionFileDB(String path, string insert)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(path, false))
            {
                file.WriteLine(insert);
            }
        }


        private static string generateInsertIntoHeader(string tablename, string header)
        {
            string query = "INSERT INTO " + tablename + "  ( " + header + " ) VALUES ";

            return query;
        }


        private static string generateHeader(List<string> headers, string separator)
        {
            String line = "";
            foreach (string header in headers)
            {
                line += header + separator;
            }
            line = line.Substring(0, line.Length - separator.Length);

            return line;

        }


        private static string generateLineCSV(string data, string prediction, string separator)
        {
            String line = "";

            line += data;
            line += separator;
            line += prediction;


            return line;
        }


        private static string generateINSERTINTOLine(string data, string prediction, string separator)
        {
            String line = "(";

            line += data;
            line += separator;
            line += prediction;


            line += ")";

            return line;
        }


        private static void TestPrediction(MLContext mlContext)
        {
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predictionEngine = mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(trainedModel);

            foreach (var heartData in HeartSampleData.heartDataList)
            {
                var prediction = predictionEngine.Predict(heartData);

                Console.WriteLine($"=============== Single Prediction  ===============");
                Console.WriteLine($"Age: {heartData.Age} ");
                Console.WriteLine($"Sex: {heartData.Sex} ");
                Console.WriteLine($"Cp: {heartData.Cp} ");
                Console.WriteLine($"TrestBps: {heartData.TrestBps} ");
                Console.WriteLine($"Chol: {heartData.Chol} ");
                Console.WriteLine($"Fbs: {heartData.Fbs} ");
                Console.WriteLine($"RestEcg: {heartData.RestEcg} ");
                Console.WriteLine($"Thalac: {heartData.Thalac} ");
                Console.WriteLine($"Exang: {heartData.Exang} ");
                Console.WriteLine($"OldPeak: {heartData.OldPeak} ");
                Console.WriteLine($"Slope: {heartData.Slope} ");
                Console.WriteLine($"Ca: {heartData.Ca} ");
                Console.WriteLine($"Thal: {heartData.Thal} ");
                Console.WriteLine($"Prediction Value: {prediction.Prediction} ");
                Console.WriteLine($"Prediction: {(prediction.Prediction ? "A disease could be present" : "Not present disease")} ");
                Console.WriteLine($"Probability: {prediction.Probability} ");
                Console.WriteLine($"==================================================");
                Console.WriteLine("");
                Console.WriteLine("");
            }

        }


        public static string GetAbsolutePath(string relativePath)
        {
            Console.WriteLine("relative path: " + relativePath);
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;

        }


        private static string runSinglePredictionDB(string tablename, string ModelPath, int chunck, String outputPath, int outputMode, int nElements, bool singleMode)
        {






            var watch = System.Diagnostics.Stopwatch.StartNew();

            var mlContext = new MLContext();

            ITransformer model = mlContext.Model.Load(ModelPath, out var inputSchema);

            string ConnectionString = "server=localhost;Uid=root;Pwd=19021990;Database=MLtoSQL";

            var mConnection = new MySqlConnection(ConnectionString);
            int i = 0;

            string separator = ",";


            for (int k = 1; k < nElements
                ; k = k + chunck)
            {

                string q = "select * from " + tablename + " where Id >= " + k + " and Id < " + (k + chunck);

                //Console
                //    .WriteLine("Query: "+ q);

                mConnection.Open();

                MySqlCommand cmd1 = new MySqlCommand(q, mConnection

                    );
                MySqlDataReader reader = cmd1.ExecuteReader();







                List<HeartData> inputs = new List<HeartData>();
                while (reader.Read())
                {

                    //reader.ge
                    //Console.WriteLine("r_"+ reader.ToString());
                    HeartData
                        obs = new HeartData
                        ()
                        {
                            Age = reader.GetFloat("Age"),
                            Sex = reader.GetFloat("Sex"),
                            Cp = reader.GetFloat("Sex"),
                            TrestBps = reader.GetFloat("TrestBps"),
                            Chol = reader.GetFloat("Chol"),
                            Fbs = reader.GetFloat("Fbs"),
                            RestEcg = reader.GetFloat("RestEcg"),
                            Thalac = reader.GetFloat("Thalac"),
                            Exang = reader.GetFloat("Exang"),
                            OldPeak = reader.GetFloat("OldPeak"),
                            Slope = reader.GetFloat("Slope"),
                            Ca = reader.GetFloat("Ca"),
                            Thal = reader.GetFloat("Thal"),
                            Label = reader.GetBoolean("Label"),
                            Id = reader.GetInt32("Id")
                        };


                    inputs.Add
                        (obs);



                }


                //Console.WriteLine
                //    ("Number of elements reads from database: "+ inputs.Count);

                IDataView inputDataForPredictions = mlContext.Data.LoadFromEnumerable(inputs);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(model);

                string outpath = outputPath;

                bool exists = System.IO.Directory.Exists(outpath);

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(outpath);
                }
                //Console.WriteLine("output path: " + outpath);
                using (System.IO.StreamWriter file1 = new System.IO.StreamWriter(outpath + "output_" + chunck + "_" + i + ".csv", false))
                {
                    List<HeartData> transactions = mlContext.Data.CreateEnumerable<HeartData>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();

                    if (singleMode)
                    {
                        string values = "";
                        StringBuilder sb = new StringBuilder();

                        transactions.ForEach
                        (testData =>
                        {

                            HeartPrediction t = predictionEngine.Predict(testData);
                            String line = MLSQL.generateLineCSV(testData.getData(separator), t.getData(separator), separator
                                );

                            if (outputMode == 0)
                            {
                                file1.WriteLine(MLSQL.generateINSERTINTOLine(testData.getMySQLData(separator), t.getMySQLData(separator), separator));

                            }
                            else if (outputMode == 1)
                            {
                                Console.WriteLine(testData.Id + "," + t.Score);

                            }
                            else if (outputMode == 2)
                            {
                                // DO NOTHING
                            }
                            else if (outputMode == 3)
                            {
                                // Insert into the database
                                string ll = "(" + testData.Id + "," + t.Score + "),";
                                //Console.WriteLine("LINE: "+ll);
                                //values += ll;
                                sb.Append(ll);


                            }



                        });

                        if (outputMode == 3)
                        {
                            mConnection.Close();

                            values = sb.ToString();

                            if (values.Trim().Length != 0)
                            {


                                values = values.Substring(0, values.Length - 1);
                                values += ";";
                                mConnection.Open();

                                string insert = "INSERT into heart_disease_detection_with_score_output (Id,Score ) VALUES  ";
                                insert += "" + values;

                                //  string cmdText = generateINSERTINTOLine(testData.Id, t.getData(separator), separator);
                                MySqlCommand cmd = new MySqlCommand(insert, mConnection);

                                // Console.WriteLine(insert);
                                cmd.ExecuteNonQuery();

                                mConnection.Close();
                            }
                        }


                    }
                    else

                    {
                        IDataView predictions = model.Transform(inputDataForPredictions);

                        float[] scoreColumn1 = predictions.GetColumn<float>("Score").ToArray();

                        List<HeartPrediction> l = mlContext.Data.CreateEnumerable<HeartPrediction>(predictions, reuseRowObject: false).Take(chunck).ToList();

                        for (int j = 0; j < l.Count; j++)
                        {
                            HeartPrediction t = l[j];
                            String line = MLSQL.generateLineCSV(transactions[j].getData(separator), t.getData(separator), separator);

                            if (outputMode == 0)
                            {
                                file1.WriteLine(MLSQL.generateINSERTINTOLine(transactions[j].getMySQLData(separator), t.getMySQLData(separator), separator) + "");

                            }
                            else if (outputMode == 1)
                            {
                                Console.WriteLine(MLSQL.generateINSERTINTOLine(transactions[j].getMySQLData(separator), t.getMySQLData(separator), separator) + "");

                            }
                            else if (outputMode == 2)
                            {
                                // DO NOTHING
                            }
                            else if (outputMode == 3)
                            {
                                // Insert into the database

                            }
                        }


                    }

                }
                i++;
            }
            watch.Stop();

            var elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine("Time needed: " + elapsedMs);

            return "Time needed: " + elapsedMs;

        }
    }
}
