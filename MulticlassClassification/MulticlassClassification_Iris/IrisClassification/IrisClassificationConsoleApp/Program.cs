using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using ML2SQL;
using MulticlassClassification_Iris.DataStructures;
using MySql.Data.MySqlClient;

namespace MulticlassClassification_Iris
{
    public static partial class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsRelativePath = @"../../../../Data";
        private static string BaseSQLRelativePath = @"../../../../SQL";
        private static string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/iris-train.txt";
        private static string TestDataRelativePath = $"{BaseDatasetsRelativePath}/iris-test.txt";

        private static readonly string BaseDatasetsRelativePathSamples = @"../../../../Data/Samples";
        private static string SamplesInput = $"{BaseDatasetsRelativePathSamples}/Input/";
        private static string SamplesOutput = $"{BaseDatasetsRelativePathSamples}/Output/";


        private static string TrainDataWithIdRelativePath = $"{BaseDatasetsRelativePath}/iris-train_with_id.txt";
        private static string TestDataWithIdRelativePath = $"{BaseDatasetsRelativePath}/iris-test_with_id.txt";

       private static string TestAllDataRelativePath = $"{BaseDatasetsRelativePath}/iris-full.txt";

       // private static string AllDataWithIdRelativePath = $"{BaseDatasetsRelativePath}/iris-full_with_id.txt";

        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/IrisClassificationModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        private static string MYSQL_INSERT_RelativePath = $"{BaseSQLRelativePath}/02_MYSQL_INSERT.sql";
        private static string SQLSERVER_INSERT_RelativePath = $"{BaseSQLRelativePath}/02_SQLSERVER_INSERT.sql";

        private static string ResultOutput = @"../../../result_iris.csv";


        private static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);





            //1.
            BuildTrainEvaluateAndSaveModel(mlContext);

            //2.
          //  TestSomePredictions(mlContext);



            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "localhost";
            builder.UserID = "SA";
            builder.Password = "Matteo<3Paolo";
            builder.InitialCatalog = "mltosql";


            Console.WriteLine("CONNECTIONSTRING:\n" + builder.ConnectionString);


            //    GenerateSQLDataWithPrediction(mlContext, AllDataWithIdRelativePath, "taxi_fare_with_score", MYSQL_INSERT_RelativePath, numberOfElements - 1, false);
            //    GenerateSQLDataWithPrediction(mlContext, AllDataWithIdRelativePath, "taxi_fare_with_score", SQLSERVER_INSERT_RelativePath, numberOfElements - 1, false);
            Console.WriteLine("HERE");



            //int[] chunckSizes = { 1000 };
            int[] chunckSizes = { 1, 10, 100, 1000 };

            MLSQL.createDataSample(TestDataWithIdRelativePath, SamplesInput, chunckSizes, true);


            string name = "IRIS_MULTICLASS";

            string outputQuery = " INSERT into iris_with_score_output(Id, score_0,score_1,score_2) VALUES ";

            string[] inputModes = { "CSV", "MYSQL", "SQLSERVER" };

            string[] outputModes = { "CSV", "CONSOLE", "NO_OUTPUT", "MYSQL", "SQLSERVER" };
            string tablename = "iris_with_score";
            char separator = '\t';

            bool header = true;

            Tuple<string, string>[] configurations = {
            new Tuple<string,string> (inputModes[0],outputModes[0]) ,
            new Tuple<string,string> (inputModes[0],outputModes[1]),
            new Tuple<string,string> (inputModes[0],outputModes[2]),
            new Tuple<string,string> (inputModes[0],outputModes[3]),
            new Tuple<string,string> (inputModes[0],outputModes[4]),
            new Tuple<string,string> (inputModes[1],outputModes[3]),
            new Tuple<string,string> (inputModes[2],outputModes[4])
            };




            PredictorExecutor<IrisData, IrisPrediction> predictorExecutor = new PredictorExecutor<IrisData, IrisPrediction>();

            int numberOfElements = 150;


            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, ModelPath, TestDataWithIdRelativePath, "iris_with_score", "MLtoSQL", MYSQL_INSERT_RelativePath, "MYSQL", numberOfElements - 1, false, separator);
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, ModelPath, TestDataWithIdRelativePath, "iris_with_score", "mltosql", SQLSERVER_INSERT_RelativePath, "SQLSERVER", numberOfElements - 1, false, separator);

            predictorExecutor.executePredictions(name, ModelPath, SamplesInput, SamplesOutput, ResultOutput, chunckSizes, configurations, tablename, outputQuery, numberOfElements, separator, header);





            Console.WriteLine("=============== End of process :) ===============");

        }

        private static void BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            int numberOfTrainElements = MLSQL.insertUniqueId(TrainDataPath, TrainDataWithIdRelativePath, true, "\t", 0);

            numberOfTrainElements = MLSQL.insertUniqueId(TestAllDataRelativePath, TestDataWithIdRelativePath, true, "\t", 0);


            //MLSQL.JoinTrainAndTest(TrainDataWithIdRelativePath, TestDataWithIdRelativePath, AllDataWithIdRelativePath, true);


            // STEP 1: Common data loading configuration
            var trainingDataView = mlContext.Data.LoadFromTextFile<IrisData>(TrainDataWithIdRelativePath, hasHeader: true);
            var testDataView = mlContext.Data.LoadFromTextFile<IrisData>(TestDataWithIdRelativePath, hasHeader: true);

            Console.WriteLine("Number of elements to test: " + numberOfTrainElements);


            //var AlltestDataView = mlContext.Data.LoadFromTextFile<IrisData>(TestAllDataRelativePath, hasHeader: true);

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "KeyColumn", inputColumnName: nameof(IrisData.Label))
                .Append(mlContext.Transforms.Concatenate("Features", nameof(IrisData.SepalLength),
                                                                                   nameof(IrisData.SepalWidth),
                                                                                   nameof(IrisData.PetalLength),
                                                                                   nameof(IrisData.PetalWidth))
                                                                       //.AppendCacheCheckpoint(mlContext)
                                                                       );
            // Use in-memory cache for small/medium datasets to lower training time. 
            // Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets. 

            // STEP 3: Set the training algorithm, then append the trainer to the pipeline
            Microsoft.ML.Data.MulticlassPredictionTransformer<Microsoft.ML.Trainers.MaximumEntropyModelParameters> b = null;


            var trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "KeyColumn", featureColumnName: "Features").WithOnFitDelegate(c => b = c).AppendCacheCheckpoint
                (mlContext)

            // .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: nameof(IrisData.Label) , inputColumnName: "KeyColumn")
            //)
            ;

            var trainingPipeline = dataProcessPipeline.Append(trainer).AppendCacheCheckpoint(mlContext);



            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingDataView);



            float[] biases = (float[])b.Model.GetBiases();

            List<float> biass = new List<float>();

            List<VBuffer<float>> ww = new List<VBuffer<float>>();

            for (int i = 0; i < biases.Length; i++)
            {
                Console.WriteLine("Bias" + i + 1 + " : " + biases[i]);
                biass.Add(biases[i]);
            }

            Console.WriteLine("\n\n");

            int numClasses;

            VBuffer<float>[] weights = null;

            b.Model.GetWeights(ref weights, out numClasses);



            for (int i = 0; i < biases.Length; i++)
            {
                ww.Add(weights[i]);
            }


            Console.WriteLine("Number of classes: " + numClasses);

            Console.WriteLine("Weights counts: " + weights.Length);
            string[] columns = { nameof(IrisData.SepalLength), nameof(IrisData.SepalWidth), nameof(IrisData.PetalLength), nameof(IrisData.PetalWidth) };
            string[] selectParams = { "Id" };
            string tablename = "iris";


            bool includeWhereClause = true;

            string query = MLSQL.GenerateMultiClassSdcaMaximumEntropy(columns, selectParams, "", tablename, biass, ww, includeWhereClause);


            Console.WriteLine("QUERY:+\n " + query + "\n\n");

            for (int i = 0; i < numClasses; i++)
            {
                Console.WriteLine("Class: " + i);
                Console.WriteLine("Number of weights: " + weights[i].Length);
                VBuffer<float> aaa = weights[i];
                for (int j = 0; j < aaa.Length; j++)
                {
                    Console.Write(" " + aaa.GetItemOrDefault(j));
                }
                Console.WriteLine("");
            }

            //trainedModel.LastTransformer.LastTransformer.Model.

            string[] Modes = { "CONSOLE", "NO_OUTPUT", "MYSQL", "CSV" };
            string[] procedure_names = { "iris", "iris_no_output", "iris_db", "iris_csv" };

            for (int i = 0; i < Modes.Length; i++)
            {
                Console.WriteLine("Modes: " +
                    Modes[i]);
                string s = MLSQL.GenerateSQLQueriesOnDifferentModes(query, "MLtoSQL", "iris_with_score_output", procedure_names[i], "Id", Modes[i], false);
                string pathMYSQL = $"{BaseSQLRelativePath}/MYSQL_PREDICTION_WITH_ID_" + procedure_names[i] + ".sql";
                string pathSQLSERVER = $"{BaseSQLRelativePath}/SQLSERVER_PREDICTION_WITH_ID_" + procedure_names[i] + ".sql";
                MLSQL.WriteSQL(s, GetAbsolutePath(pathMYSQL));
                MLSQL.WriteSQL(s, GetAbsolutePath(pathSQLSERVER));
            }



            //trainedModel.LastTransformer.LastTransformer.

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label", "Score");

            Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPath);
            Console.WriteLine("The model is saved to {0}", ModelPath);
        }

        private static void TestSomePredictions(MLContext mlContext)
        {
            //Test Classification Predictions with some hard-coded samples 
            var trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);




            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<IrisData, IrisPrediction>(trainedModel);

            // During prediction we will get Score column with 3 float values.
            // We need to find way to map each score to original label.
            // In order to do that we need to get TrainingLabelValues from Score column.
            // TrainingLabelValues on top of Score column represent original labels for i-th value in Score array.
            // Let's look how we can convert key value for PredictedLabel to original labels.
            // We need to read KeyValues for "PredictedLabel" column.
            VBuffer<float> keys = default;
            predEngine.OutputSchema["PredictedLabel"].GetKeyValues(ref keys);
            var labelsArray = keys.DenseValues().ToArray();

            // Since we apply MapValueToKey estimator with default parameters, key values
            // depends on order of occurence in data file. Which is "Iris-setosa", "Iris-versicolor", "Iris-virginica"
            // So if we have Score column equal to [0.2, 0.3, 0.5] that's mean what score for
            // Iris-setosa is 0.2
            // Iris-versicolor is 0.3
            // Iris-virginica is 0.5.
            //Add a dictionary to map the above float values to strings. 
            Dictionary<float, string> IrisFlowers = new Dictionary<float, string>();
            IrisFlowers.Add(0, "Setosa");
            IrisFlowers.Add(1, "versicolor");
            IrisFlowers.Add(2, "virginica");

            Console.WriteLine("=====Predicting using model====");
            //Score sample 1
            var resultprediction1 = predEngine.Predict(SampleIrisData.Iris1);

            Console.WriteLine($"Actual: setosa.     Predicted label and score:  {IrisFlowers[labelsArray[0]]}: {resultprediction1.Score[0]:0.####} score: " + resultprediction1.Score[0]);
            Console.WriteLine($"                                                {IrisFlowers[labelsArray[1]]}: {resultprediction1.Score[1]:0.####} score: " + resultprediction1.Score[1]);
            Console.WriteLine($"                                                {IrisFlowers[labelsArray[2]]}: {resultprediction1.Score[2]:0.####} score: " + resultprediction1.Score[2]);
            Console.WriteLine();

            //Score sample 2
            var resultprediction2 = predEngine.Predict(SampleIrisData.Iris2);

            Console.WriteLine($"Actual: Virginica.   Predicted label and score:  {IrisFlowers[labelsArray[0]]}: {resultprediction2.Score[0]:0.####}");
            Console.WriteLine($"                                                 {IrisFlowers[labelsArray[1]]}: {resultprediction2.Score[1]:0.####}");
            Console.WriteLine($"                                                 {IrisFlowers[labelsArray[2]]}: {resultprediction2.Score[2]:0.####}");
            Console.WriteLine();

            //Score sample 3
            var resultprediction3 = predEngine.Predict(SampleIrisData.Iris3);

            Console.WriteLine($"Actual: Versicolor.   Predicted label and score: {IrisFlowers[labelsArray[0]]}: {resultprediction3.Score[0]:0.####}");
            Console.WriteLine($"                                                 {IrisFlowers[labelsArray[1]]}: {resultprediction3.Score[1]:0.####}");
            Console.WriteLine($"                                                 {IrisFlowers[labelsArray[2]]}: {resultprediction3.Score[2]:0.####}");
            Console.WriteLine();

            //Environment.Exit(0);

            //int[] chunckSizes = { 1, 10, 100, 1000 };
            //MLSQL.createDataSample(TestAllDataRelativePath, SamplesInput, chunckSizes, true);


            //int[] modes = { 0, 1, 2, 3 };
            //foreach (int mode in modes)
            //{
            //    Console.WriteLine("MODE: " + mode);

            //    switch (mode)
            //    {
            //        case 0:
            //            Console.WriteLine("File output\n");
            //            break;

            //        case 1:
            //            Console.WriteLine("Console Output\n");
            //            break;

            //        case 2:
            //            Console.WriteLine("Only calculation\n");
            //            break;

            //        case 3:
            //            Console.WriteLine("DB insertion\n");
            //            break;
            //    }

            //    List<string> results = new List<string>();
            //    foreach (int chunk in chunckSizes)
            //    {
            //        Console.WriteLine("Chunck size: " + chunk);
            //        string r = runSinglePrediction(SamplesInput + chunk + "/", chunk, SamplesOutput, mode, true);
            //        results.Add("Chunck size: " + chunk + "\t" + r);
            //    }

            //    switch (mode)
            //    {
            //        case 0:
            //            Console.WriteLine("File output\n");
            //            break;

            //        case 1:
            //            Console.WriteLine("Console Output\n");
            //            break;

            //        case 2:
            //            Console.WriteLine("Only calculation\n");
            //            break;

            //        case 3:
            //            Console.WriteLine("DB insertion\n");
            //            break;
            //    }

            //    foreach (string r in results)
            //    {
            //        Console.WriteLine("" + r);
            //    }


            //    Console.WriteLine("==============================================================================");
            //}



            //Environment.Exit(0);
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }


        private static string runSinglePrediction(String inputFolderPath, int chunck, String outputPath, int outputMode, bool singleMode)
        {


            // Read all files 

            var watch = System.Diagnostics.Stopwatch.StartNew();

            var mlContext = new MLContext();

            ITransformer model = mlContext.Model.Load(ModelPath, out var inputSchema);

            string ConnectionString = "server=localhost;Uid=root;Pwd=19021990;Database=MLtoSQL";

            var mConnection = new MySqlConnection(ConnectionString);
            int i = 0;

            string separator = ",";


            foreach (string file in Directory.EnumerateFiles(inputFolderPath, "*.csv"))
            {
                //string contents = File.ReadAllText(file);


                string path = file;
                Console.WriteLine("Path: " + path);
                string absolute = Path.GetFullPath(path);

                Console.WriteLine("Absolute path: " + absolute);

                bool existsInputPath = System.IO.Directory.Exists(absolute);
                if (!existsInputPath)
                {
                    Console.WriteLine("Input path:" + path + "\n DOES not EXIST!!!");
                }


                IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<IrisData>(absolute, hasHeader: true, separatorChar: '\t');
                var predictionEngine = mlContext.Model.CreatePredictionEngine<IrisData, IrisPrediction>(model);

                string outpath = outputPath;

                bool exists = System.IO.Directory.Exists(outpath);

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(outpath);
                }

                IrisPrediction p = new IrisPrediction();
                //Console.WriteLine("output path: " + outpath);
                using (System.IO.StreamWriter file1 = new System.IO.StreamWriter(outpath + "output_" + chunck + "_" + i + ".csv", false))
                {
                    List<IrisData> transactions = mlContext.Data.CreateEnumerable<IrisData>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();

                    if (singleMode)
                    {
                        string values = "";

                        transactions.ForEach
                        (testData =>
                        {

                            IrisPrediction t = predictionEngine.Predict(testData);
                            String line = MLSQL.generateLineCSV(testData.getData(separator), t.getData(separator), separator
                                );

                            if (outputMode == 0)
                            {
                                file1.WriteLine(MLSQL.generateINSERTINTOLine(testData.getMySQLData(separator), t.getMySQLData(separator), separator));

                            }
                            else if (outputMode == 1)
                            {
                                Console.WriteLine(testData.Id + "," + t.getData(separator));

                            }
                            else if (outputMode == 2)
                            {
                                // DO NOTHING
                            }
                            else if (outputMode == 3)
                            {
                                // Insert into the database
                                string ll = "(" + testData.Id + "," + t.getData(separator) + "),";
                                //Console.WriteLine("LINE: "+ll);
                                values += ll;



                            }



                        });

                        if (outputMode == 3)
                        {
                            values = values.Substring(0, values.Length - 1);
                            values += ";";
                            mConnection.Open();

                            string insert = "INSERT into " + "iris_with_output" + " (Id," + p.GetHeader(separator) + " ) VALUES ";
                            insert += "" + values;

                            //  string cmdText = generateINSERTINTOLine(testData.Id, t.getData(separator), separator);
                            MySqlCommand cmd = new MySqlCommand(insert, mConnection);

                            // Console.WriteLine(insert);
                            cmd.ExecuteNonQuery();

                            mConnection.Close();
                        }


                    }
                    else

                    {
                        IDataView predictions = model.Transform(inputDataForPredictions);

                        float[] scoreColumn1 = predictions.GetColumn<float>("Score").ToArray();

                        List<IrisPrediction> l = mlContext.Data.CreateEnumerable<IrisPrediction>(predictions, reuseRowObject: false).Take(chunck).ToList();

                        for (int j = 0; j < l.Count; j++)
                        {
                            IrisPrediction t = l[j];
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
