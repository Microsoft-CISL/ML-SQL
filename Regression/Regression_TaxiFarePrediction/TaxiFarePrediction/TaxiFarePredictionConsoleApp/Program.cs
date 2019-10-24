using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using ML2SQL;
using MySql.Data.MySqlClient;
using Regression_TaxiFarePrediction.DataStructures;

using static Microsoft.ML.Transforms.NormalizingEstimator;
using static Microsoft.ML.Transforms.NormalizingTransformer;

namespace Regression_TaxiFarePrediction
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsRelativePath = @"../../../../Data";

        private static string ResultOutput = @"../../../result_taxi_fare.csv";


        private static readonly string BaseSQLRelativePath = @"../../../../SQL";
        private static readonly string BaseDatasetsRelativePathSamples = @"../../../../Data/Samples";
        private static string SamplesInput = $"{BaseDatasetsRelativePathSamples}/Input/";
        private static string SamplesOutput = $"{BaseDatasetsRelativePathSamples}/Output/";
        //private static string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/taxi-fare-train.csv";

        private static string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/taxi-fare-train_short.csv";
        private static string TestDataRelativePath = $"{BaseDatasetsRelativePath}/taxi-fare-test.csv";

        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/TaxiFareModel.zip";

        private static readonly string MYSQL_INSERT_RelativePath = $"{BaseSQLRelativePath}/02_MYSQL_insert.sql";
        private static readonly string MYSQL_WEIGHTS_RelativePath = $"{BaseSQLRelativePath}/03_MYSQL_insert_weights.sql";


        private static readonly string SQLSERVER_INSERT_RelativePath = $"{BaseSQLRelativePath}/02_SQLSERVER_insert.sql";
        private static readonly string SQLSERVER_WEIGHTS_RelativePath = $"{BaseSQLRelativePath}/03_SQLSERVER_insert_weights.sql";

        private static readonly string TrainDataWithIdRelativePath = $"{BaseDatasetsRelativePath}/taxi-fare-train_with_id.csv";

        private static readonly string TestDataWithIdRelativePath = $"{BaseDatasetsRelativePath}/taxi-fare-test_with_id.csv";

        private static readonly string AllDataWithIdRelativePath = $"{BaseDatasetsRelativePath}/taxi-fare-all_with_id.csv";
        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        static void Main(string[] args) //If args[0] == "svg" a vector-based chart will be created instead a .png chart
        {
            int numberOfTrainElements = MLSQL.insertUniqueId(TrainDataPath, TrainDataWithIdRelativePath, true, ",", 0);

            Console.WriteLine("Number of train examples: " + numberOfTrainElements);

            int numberOfTestElements = MLSQL.insertUniqueId(TestDataPath, TestDataWithIdRelativePath, true, ",", numberOfTrainElements);

            Console.WriteLine("Number of test examples: " + numberOfTestElements);
            int numberOfElements = numberOfTestElements;
            Console.WriteLine("Number of elements: " + numberOfElements);


            MLSQL.JoinTrainAndTest(TrainDataWithIdRelativePath, TestDataWithIdRelativePath, AllDataWithIdRelativePath, true);


            //Create ML Context with seed for repeteable/deterministic results
            MLContext mlContext = new MLContext(seed: 0);

            // Create, Train, Evaluate and Save a model
            BuildTrainEvaluateAndSaveModel(mlContext, numberOfElements);

            // Make a single test prediction loding the model from .ZIP file
            TestSinglePrediction(mlContext);


            //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            //builder.DataSource = "localhost";
            //builder.UserID = "SA";
            //builder.Password = "Matteo<3Paolo";
            //builder.InitialCatalog = "mltosql";


            //Console.WriteLine("CONNECTIONSTRING:\n" + builder.ConnectionString);


            //    GenerateSQLDataWithPrediction(mlContext, AllDataWithIdRelativePath, "taxi_fare_with_score", MYSQL_INSERT_RelativePath, numberOfElements - 1, false);
            //    GenerateSQLDataWithPrediction(mlContext, AllDataWithIdRelativePath, "taxi_fare_with_score", SQLSERVER_INSERT_RelativePath, numberOfElements - 1, false);
            Console.WriteLine("HERE");



            int[] chunckSizes = { 100000 };

            // int[] chunckSizes = { 1, 10, 100, 1000, 10000, 100000, 1000000 };

            //int[] chunckSizes = { 1000, 10000, 100000, 1000000 };
            string tablename_weights = "weights_taxi";
            MLSQL.createDataSample(AllDataWithIdRelativePath, SamplesInput, chunckSizes, true);


            string name = "TAXI_FARE";

            string outputQuery = " INSERT into taxi_fare_with_score_output(Id, Score) VALUES ";

            string[] inputModes = { "CSV", "MYSQL", "SQLSERVER" };

            string[] outputModes = { "CSV", "CONSOLE", "NO_OUTPUT", "MYSQL", "SQLSERVER" };
            string tablename = "taxi_fare_with_score";
            char separator = ',';

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




            PredictorExecutor<TaxiTrip, TaxiTripFarePrediction> predictorExecutor = new PredictorExecutor<TaxiTrip, TaxiTripFarePrediction>();

            char sep = ',';


            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, ModelPath, AllDataWithIdRelativePath, "taxi_fare_with_score", "MLtoSQL", MYSQL_INSERT_RelativePath, "MYSQL", numberOfElements - 1, false,sep);
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, ModelPath, AllDataWithIdRelativePath, "taxi_fare_with_score", "mltosql", SQLSERVER_INSERT_RelativePath, "SQLSERVER", numberOfElements - 1, false,sep);

            predictorExecutor.executePredictions(name, ModelPath, SamplesInput, SamplesOutput, ResultOutput, chunckSizes, configurations, tablename, outputQuery, numberOfElements, separator, header);










            Environment.Exit(0);

            // REMOVE HERE 
            int[] modes = { 0, 1, 2, 3 };
            foreach (int mode in modes)
            {
                Console.WriteLine("MODE: " + mode);

                switch (mode)
                {
                    case 0:
                        Console.WriteLine("File output\n");
                        break;

                    case 1:
                        Console.WriteLine("Console Output\n");
                        break;

                    case 2:
                        Console.WriteLine("Only calculation\n");
                        break;

                    case 3:
                        Console.WriteLine("DB insertion\n");
                        break;
                }

                List<string> results = new List<string>();
                foreach (int chunk in chunckSizes)
                {
                    Console.WriteLine("Chunck size: " + chunk);
                    string r = runSinglePrediction(SamplesInput + chunk + "/", chunk, SamplesOutput, mode, true);
                    results.Add("Chunck size: " + chunk + "\t" + r);
                }

                switch (mode)
                {
                    case 0:
                        Console.WriteLine("File output\n");
                        break;

                    case 1:
                        Console.WriteLine("Console Output\n");
                        break;

                    case 2:
                        Console.WriteLine("Only calculation\n");
                        break;

                    case 3:
                        Console.WriteLine("DB insertion\n");
                        break;
                }

                foreach (string r in results)
                {
                    Console.WriteLine("" + r);
                }


                Console.WriteLine("==============================================================================");
            }



            Environment.Exit(0);

            int[] Modes = { 0, 1, 2, 3 };

            string[] procedure_names = { "taxi_fare", "taxi_fare_csv", "taxi_fare_no_output", "taxi_fare_db" };

            string[] selectParams = { };





            //  string q = MLSQL.GenerateSQLFeatureTextWithChunkSize("", selectParams, "Id", "Comment", "", "sentiment_analysis_detection_with_score", tablename_weights, bias);


            //generate output queries!!!!

            string q = "";

            Console.WriteLine("Query: " + q);


            for (int i = 0; i < Modes.Length; i++)
            {
                Console.WriteLine("PROCEDURE: " + procedure_names[i]);
                string s = MLSQL.GenerateSQLProcededure(q, "MLtoSQL", "taxi_fare_with_score_output", procedure_names[i], "Id", Modes[i], false);
                string path = $"{BaseSQLRelativePath}/SQL_PROCEDURE_" + procedure_names[i] + ".sql";
                MLSQL.WriteSQL(s, GetAbsolutePath(path));

                for (int l = 0; l < chunckSizes.Length; l++)
                {
                    Console.WriteLine("call " + procedure_names[i] + "(" + numberOfElements + ", " + chunckSizes[l] + ");");
                }



            }
        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext, int numberOfElements)
        {
            // STEP 1: Common data loading configuration
            IDataView baseTrainingDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(TrainDataWithIdRelativePath, hasHeader: true, separatorChar: ',');
            IDataView testDataView = mlContext.Data.LoadFromTextFile<TaxiTrip>(TestDataWithIdRelativePath, hasHeader: true, separatorChar: ',');

            //Sample code of removing extreme data like "outliers" for FareAmounts higher than $150 and lower than $1 which can be error-data 
            var cnt = baseTrainingDataView.GetColumn<float>(nameof(TaxiTrip.FareAmount)).Count();
            IDataView trainingDataView = mlContext.Data.FilterRowsByColumn(baseTrainingDataView, nameof(TaxiTrip.FareAmount), lowerBound: 1, upperBound: 150);
            var cnt2 = trainingDataView.GetColumn<float>(nameof(TaxiTrip.FareAmount)).Count();


            //var binningTransformer = null;
            TransformerChain<OneHotEncodingTransformer> oneHotEncodingTransformer = null;

            //TransformerChain<NormalizingTransformer> normalizingTransformerChain = null;

            // NormalizingTransformer normalizingTransformer = null;

            NormalizingTransformer normalizerPassegerCount = null;
            NormalizingTransformer normalizerTripTime = null;
            NormalizingTransformer normalizerTripDistance = null;

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(TaxiTrip.FareAmount))
                            .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "VendorIdEncoded", inputColumnName: nameof(TaxiTrip.VendorId)))

                            .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "RateCodeEncoded", inputColumnName: nameof(TaxiTrip.RateCode)))
                            .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "PaymentTypeEncoded", inputColumnName: nameof(TaxiTrip.PaymentType)))

                            //.WithOnFitDelegate(fittedTransformer => oneHotEncodingTransformer = fittedTransformer)
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(TaxiTrip.PassengerCount)).WithOnFitDelegate(aa => normalizerPassegerCount = aa))

                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(TaxiTrip.TripTime)).WithOnFitDelegate(aa => normalizerTripTime = aa)
                             )
                            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(TaxiTrip.TripDistance)).WithOnFitDelegate(aa => normalizerTripDistance = aa)
                            )


                            .Append(mlContext.Transforms.Concatenate("Features", "VendorIdEncoded", "RateCodeEncoded", "PaymentTypeEncoded", nameof(TaxiTrip.PassengerCount)
                            , nameof(TaxiTrip.TripTime), nameof(TaxiTrip.TripDistance)));

            // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 5);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 5);

            // STEP 3: Set the training algorithm, then create and config the modelBuilder - Selected Trainer (SDCA Regression algorithm)                            
            var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer).AppendCacheCheckpoint(mlContext);

            // STEP 4: Train the model fitting to the DataSet
            //The pipeline is trained on the dataset that has been loaded and transformed.
            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");

            IDataView predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            Common.ConsoleHelper.PrintRegressionMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);


            Console.WriteLine("one hot enconding transformers: " + oneHotEncodingTransformer == null);



            var normPassengerCont = normalizerPassegerCount
                .GetNormalizerModelParameters(0) as
                 AffineNormalizerModelParameters<System.Single>;

            var offset_passengerCount = normPassengerCont.Offset;
            var scale_passegerCount = normPassengerCont.Scale;
            Console.WriteLine($"Values for PassengerCount would be transfromed by applying y = (x - ({offset_passengerCount})) * {scale_passegerCount}");
            Tuple<float, float> a = new Tuple<float, float>(offset_passengerCount, scale_passegerCount);

            var normTripTime = normalizerTripTime
                .GetNormalizerModelParameters(0) as
                 AffineNormalizerModelParameters<System.Single>;

            var offset_tripTime = normTripTime.Offset;
            var scale_tripTime = normTripTime.Scale;
            Console.WriteLine($"Values for TripTime would be transfromed by applying y = (x - ({offset_tripTime})) * {scale_tripTime}");
            Tuple<float, float> b = new Tuple<float, float>(offset_tripTime, scale_tripTime);



            var normTripDistance = normalizerTripDistance
                .GetNormalizerModelParameters(0) as
                 AffineNormalizerModelParameters<System.Single>;

            var offset_tripDistance = normTripDistance.Offset;
            var scale_tripDistance = normTripDistance.Scale;
            Console.WriteLine($"Values for slot 1 would be transfromed by applying y = (x - ({offset_tripDistance})) * {scale_tripDistance}");
            Tuple<float, float> c = new Tuple<float, float>(offset_tripDistance, scale_tripDistance);




            Dictionary<string, Tuple<float, float>> columnNormalizationParamenters = new Dictionary<string, Tuple<float, float>>();

            columnNormalizationParamenters.Add(nameof(TaxiTrip.PassengerCount), a);
            columnNormalizationParamenters.Add(nameof(TaxiTrip.TripTime), b);
            columnNormalizationParamenters.Add(nameof(TaxiTrip.TripDistance), c);




            string[] all_columns = { "Id", "VendorId", "RateCode", "PaymentType", nameof(TaxiTrip.PassengerCount), nameof(TaxiTrip.TripTime), nameof(TaxiTrip.TripDistance) };



            string normalize_table = MLSQL.generateNormalizeTable("taxi_fare_with_score", all_columns, columnNormalizationParamenters);

            Console.WriteLine("normalizeQuery: \n" + normalize_table);






            IDataView d = trainedModel.Transform(trainingDataView);


            //PrintDataColumn(d, "VendorIdEncoded");



            Console.WriteLine("======================================================================================================");


            // PrintDataColumn(d, "RateCodeEncoded");


            Console.WriteLine("======================================================================================================");


            //PrintDataColumn(d, "PaymentTypeEncoded");


            Console.WriteLine("======================================================================================================");


            Console.WriteLine(
                "One Hot Encoding of single column 'VendorId', with key type " +
                "output.");

            int offset = 0;
            var vendorEncoding = MLSQL.createValueMappingOneHotEnconding(d, "VendorId", "VendorIdEncoded", offset, true);
            Console.WriteLine("======================================================================================================");
            offset += vendorEncoding.Count();
            var rateCodeEncoding = MLSQL.createValueMappingOneHotEnconding(d, "RateCode", "RateCodeEncoded", offset, true);
            Console.WriteLine("======================================================================================================");
            offset += rateCodeEncoding.Count();
            Console.WriteLine("======================================================================================================");
            var paymentTypeEncoding = MLSQL.createValueMappingOneHotEnconding(d, "PaymentType", "PaymentTypeEncoded", offset, true);
            Console.WriteLine("======================================================================================================");

            offset += paymentTypeEncoding.Count();


            Dictionary<string, int> PassengerCountEnconding = new Dictionary<string, int>();
            PassengerCountEnconding.Add("", offset);
            offset += 1;
            Dictionary<string, int> TripTimeEnconding = new Dictionary<string, int>();
            TripTimeEnconding.Add("", offset);
            offset += 1;
            Dictionary<string, int> TripDistanceEnconding = new Dictionary<string, int>();
            TripDistanceEnconding.Add("", offset);
            //offset += 1;


            Dictionary<string, Dictionary<string, int>> columnsWeightPosition = new Dictionary<string, Dictionary<string, int>>();

            columnsWeightPosition.Add("VendorId", vendorEncoding);
            columnsWeightPosition.Add("RateCode", rateCodeEncoding);
            columnsWeightPosition.Add("PaymentType", paymentTypeEncoding);

            columnsWeightPosition.Add(nameof(TaxiTrip.PassengerCount), PassengerCountEnconding);
            columnsWeightPosition.Add(nameof(TaxiTrip.TripTime), TripTimeEnconding);
            columnsWeightPosition.Add(nameof(TaxiTrip.TripDistance), TripDistanceEnconding);


            string IdColumn;


            string[] columns = { "VendorId", "RateCode", "PaymentType", nameof(TaxiTrip.PassengerCount), nameof(TaxiTrip.TripTime), nameof(TaxiTrip.TripDistance) };

            Console.WriteLine("Colums: ");
            for (int i = 0; i < columns.Length; i++)
            {
                Console.WriteLine("index: " + i + " column: " + columns[i]);
            }

            string whereClause = "\n where Id >= @id  and Id < ( @id + chuncksize ) \n";

            string dataTransposeQuery = MLSQL.trasposeColumnsToRows("id", columns, " ( " + normalize_table + " " + whereClause + " ) as F ");

            Console.WriteLine("Query: \n" + dataTransposeQuery);

            var columnNeedWeight = new Dictionary<string, bool>();

            columnNeedWeight.Add("VendorId", true);
            columnNeedWeight.Add("RateCode", true);
            columnNeedWeight.Add("PaymentType", true);

            columnNeedWeight.Add(nameof(TaxiTrip.PassengerCount), false);
            columnNeedWeight.Add(nameof(TaxiTrip.TripTime), false);
            columnNeedWeight.Add(nameof(TaxiTrip.TripDistance), false);


            string weight_table_name = "weights_taxi";


            string q = MLSQL.computeWeightingAndFeatureMapping("Id", "name", "value", "l_w", "feature", columns, "(" + dataTransposeQuery + ") AS F", columnNeedWeight, columnsWeightPosition);

            Console.WriteLine("Query: \n" + q);

            string[] selectColumns = { "Id", "name", "value", "l_w", "feature" };

            IReadOnlyCollection<float> ww = trainedModel.LastTransformer.Model.Weights;

            int numberOfWeights = ww.Count;
            float[] weights = new float[numberOfWeights];
            for (int i = 0; i < numberOfWeights; i++)
            {
                weights[i] = ww.ElementAt(i);
            }


            float bias = trainedModel.LastTransformer.Model.Bias;

            Console.WriteLine("Number of weights: " + ww.Count());


            MLSQL.WriteModelWeight(weight_table_name, MYSQL_WEIGHTS_RelativePath, weights, "MYSQL","MLtoSQL");
            MLSQL.WriteModelWeight(weight_table_name, SQLSERVER_WEIGHTS_RelativePath, weights, "SQLSERVER","mltosql");

            Console.WriteLine("Bias: " + bias);



            string FinalQuery = MLSQL.JoinFeaturesWithWeights(q, weight_table_name, "feature", "label", "l_w", "weight", selectColumns, bias);


            Console.WriteLine("Query q1: \n " + FinalQuery);



            //int[] Modes = { 0, 2, 3 };
            string[] modes = { "CONSOLE", "NO_OUTPUT", "MYSQL", "CSV" };


            string[] procedure_names = { "taxi_fare", "taxi_fare_no_output", "taxi_fare_db", "taxi_fare_csv" };



            string[] selectParams = { };





            //  string q = MLSQL.GenerateSQLFeatureTextWithChunkSize("", selectParams, "Id", "Comment", "", "sentiment_analysis_detection_with_score", tablename_weights, bias);



            for (int i = 0; i < modes.Length; i++)
            {
                string QQ = MLSQL.GenerateSQLQueriesOnDifferentModes(FinalQuery, "mltosql", "taxi_fare_with_score_output", procedure_names[i], "Id", modes[i], false);
                string MYSQLPATH = $"{BaseSQLRelativePath}/MYSQL_PREDICTION_WITH_ID_" + procedure_names[i] + ".sql";
                string SQLSERVERPATH = $"{BaseSQLRelativePath}/SQLSERVER_PREDICTION_WITH_ID_" + procedure_names[i] + ".sql";
                MLSQL.WriteSQL(FinalQuery, GetAbsolutePath(MYSQLPATH));
                MLSQL.WriteSQL(FinalQuery, GetAbsolutePath(SQLSERVERPATH));
            }





            //Console.WriteLine("Query: " + FinalQuery);

            //int[] chunckSizes = { 10, 100, 1000, 10000, 100000, 1000000 };

            //for (int i = 0; i < Modes.Length; i++)
            //{
            //    Console.WriteLine("PROCEDURE: " + procedure_names[i]);
            //    string s = MLSQL.GenerateSQLProcededure(FinalQuery, "MLtoSQL", "taxi_fare_with_score_output", procedure_names[i], "Id", Modes[i], false);
            //    string path = $"{BaseSQLRelativePath}/SQL_PROCEDURE_" + procedure_names[i] + ".sql";
            //    MLSQL.WriteSQL(s, GetAbsolutePath(path));

            //    for (int l = 0; l < chunckSizes.Length; l++)
            //    {
            //        Console.WriteLine("call " + procedure_names[i] + "(" + numberOfElements + ", " + chunckSizes[l] + ");");
            //    }



            //}



            return trainedModel;
        }

        private static void PrintDataColumn(IDataView transformedData,
            string columnName)
        {
            var countSelectColumn = transformedData.GetColumn<float[]>(
                transformedData.Schema[columnName]);

            foreach (var row in countSelectColumn)
            {
                for (var i = 0; i < row.Length; i++)
                    Console.Write($"{row[i]}\t");

                Console.WriteLine();
            }
        }

        private static string GenerateSQLDataWithPrediction(MLContext mlContext, string _alldatasetFile, string tablename, string sqlPath, int n, bool debug)
        {



            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predictionEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);

            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<TaxiTrip>(_alldatasetFile, separatorChar: ',', hasHeader: true);

            TaxiTrip d = new TaxiTrip();

            TaxiTripFarePrediction p = new TaxiTripFarePrediction();

            string separator = ",";
            List<string> headers = new List<string>();
            headers.Add(d.GetHeader(","));
            headers.Add(p.GetHeader(","));
            string header = MLSQL.generateHeader(headers, separator);
            // string tablename = "sentiment_analysis_with_score";
            string sqlQuery = MLSQL.generateInsertIntoHeader(tablename, header);

            int k = 0;



            using (System.IO.StreamWriter file = new System.IO.StreamWriter(sqlPath, false))
            {

                file.WriteLine(sqlQuery);

                mlContext.Data.CreateEnumerable<TaxiTrip>(inputDataForPredictions, reuseRowObject: false).Take(n).ToList().

                ForEach(testData =>
                {


                    TaxiTripFarePrediction t = predictionEngine.Predict(testData);


                    //String line = generateLineCSV(testData, t);
                    //HeRE;

                    string q = MLSQL.generateINSERTINTOLine(testData.getSQLData(separator), t.getSQLData(separator), separator) + ",";
                    if (debug)
                    {
                        Console.WriteLine("" + q + "\n");
                    }
                    //sqlQuery += q;



                    if ((k + 1) == n)
                    {
                        Console.WriteLine("LAST CASE: ");

                        q = q.Substring(0, q.Length - 1);
                        q += ";";


                    }
                    file.WriteLine(q);





                    k++;
                });


                Console.WriteLine("Last index: " + k);

                //sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
                //sqlQuery += ";";

            }
            // MLSQL.WriteSQL(sqlQuery,sqlPath);
            return sqlQuery;


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



                //Console.WriteLine("" + file);
                IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<TaxiTrip>(file, separatorChar: ',', hasHeader: true);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(model);

                string outpath = outputPath;

                bool exists = System.IO.Directory.Exists(outpath);

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(outpath);
                }
                //Console.WriteLine("output path: " + outpath);
                using (System.IO.StreamWriter file1 = new System.IO.StreamWriter(outpath + "output_" + chunck + "_" + i + ".csv", false))
                {
                    List<TaxiTrip> transactions = mlContext.Data.CreateEnumerable<TaxiTrip>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();

                    if (singleMode)
                    {
                        string values = "";

                        transactions.ForEach
                        (testData =>
                        {

                            TaxiTripFarePrediction t = predictionEngine.Predict(testData);
                            String line = MLSQL.generateLineCSV(testData.getData(separator), t.getData(separator), separator
                                );

                            if (outputMode == 0)
                            {
                                file1.WriteLine(MLSQL.generateINSERTINTOLine(testData.getSQLData(separator), t.getSQLData(separator), separator));

                            }
                            else if (outputMode == 1)
                            {
                                Console.WriteLine(testData.Id + "," + t.FareAmount);

                            }
                            else if (outputMode == 2)
                            {
                                // DO NOTHING
                            }
                            else if (outputMode == 3)
                            {
                                // Insert into the database
                                string ll = "(" + testData.Id + "," + t.FareAmount + "),";
                                //Console.WriteLine("LINE: "+ll);
                                values += ll;



                            }



                        });

                        if (outputMode == 3)
                        {
                            values = values.Substring(0, values.Length - 1);
                            values += ";";
                            mConnection.Open();

                            string insert = "INSERT into taxi_fare_with_score_output (Id,Score ) VALUES ";
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

                        List<TaxiTripFarePrediction> l = mlContext.Data.CreateEnumerable<TaxiTripFarePrediction>(predictions, reuseRowObject: false).Take(chunck).ToList();

                        for (int j = 0; j < l.Count; j++)
                        {
                            TaxiTripFarePrediction t = l[j];
                            String line = MLSQL.generateLineCSV(transactions[j].getData(separator), t.getData(separator), separator);

                            if (outputMode == 0)
                            {
                                file1.WriteLine(MLSQL.generateINSERTINTOLine(transactions[j].getSQLData(separator), t.getSQLData(separator), separator) + "");

                            }
                            else if (outputMode == 1)
                            {
                                Console.WriteLine(MLSQL.generateINSERTINTOLine(transactions[j].getSQLData(separator), t.getSQLData(separator), separator) + "");

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


        private static void createOneHotEncondingMapping(int startIndex)
        {

        }

        private static void printNormalizeParameter(AffineNormalizerModelParameters<ImmutableArray<float>> noCdfParams)
        {


            Console.WriteLine("Offset length: " + noCdfParams.Offset.Length);
            Console.WriteLine("Scale lenght: " + noCdfParams.Scale.Length);
            var offset = noCdfParams.Offset.Length == 0 ? 0 : noCdfParams.Offset[0];
            var scale = noCdfParams.Scale[0];
            Console.WriteLine($"Values for slot 1 would be transfromed by applying y = (x - ({offset})) * {scale}");

        }

        private static void TestSinglePrediction(MLContext mlContext)
        {
            //Sample: 
            //vendor_id,rate_code,passenger_count,trip_time_in_secs,trip_distance,payment_type,fare_amount
            //VTS,1,1,1140,3.75,CRD,15.5

            var taxiTripSample = new TaxiTrip()
            {
                VendorId = "VTS",
                RateCode = "1",
                PassengerCount = 1,
                TripTime = 1140,
                TripDistance = 3.75f,
                PaymentType = "CRD",
                FareAmount = 0 // To predict. Actual/Observed = 15.5
            };

            ///
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);

            //Score
            var resultprediction = predEngine.Predict(taxiTripSample);
            ///

            Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted fare: {resultprediction.FareAmount:0.####}, actual fare: 15.5");
            Console.WriteLine($"**********************************************************************");
        }

        private static void PlotRegressionChart(MLContext mlContext,
                                                string testDataSetPath,
                                                int numberOfRecordsToRead,
                                                string[] args)
        {
            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
            }

            // Create prediction engine related to the loaded trained model
            var predFunction = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);

            string chartFileName = "";
            //using (var pl = new PLStream())
            //{
            //    // use SVG backend and write to SineWaves.svg in current directory
            //    if (args.Length == 1 && args[0] == "svg")
            //    {
            //        pl.sdev("svg");
            //        chartFileName = "TaxiRegressionDistribution.svg";
            //        pl.sfnam(chartFileName);
            //    }
            //    else
            //    {
            //        pl.sdev("pngcairo");
            //        chartFileName = "TaxiRegressionDistribution.png";
            //        pl.sfnam(chartFileName);
            //    }

            //    // use white background with black foreground
            //    pl.spal0("cmap0_alternate.pal");

            //    // Initialize plplot
            //    pl.init();

            //    // set axis limits
            //    const int xMinLimit = 0;
            //    const int xMaxLimit = 35; //Rides larger than $35 are not shown in the chart
            //    const int yMinLimit = 0;
            //    const int yMaxLimit = 35;  //Rides larger than $35 are not shown in the chart
            //    pl.env(xMinLimit, xMaxLimit, yMinLimit, yMaxLimit, AxesScale.Independent, AxisBox.BoxTicksLabelsAxes);

            //    // Set scaling for mail title text 125% size of default
            //    pl.schr(0, 1.25);

            //    // The main title
            //    pl.lab("Measured", "Predicted", "Distribution of Taxi Fare Prediction");

            //    // plot using different colors
            //    // see http://plplot.sourceforge.net/examples.php?demo=02 for palette indices
            //    pl.col0(1);

            //    int totalNumber = numberOfRecordsToRead;
            //    var testData = new TaxiTripCsvReader().GetDataFromCsv(testDataSetPath, totalNumber).ToList();

            //    //This code is the symbol to paint
            //    char code = (char)9;

            //    // plot using other color
            //    //pl.col0(9); //Light Green
            //    //pl.col0(4); //Red
            //    pl.col0(2); //Blue

            //    double yTotal = 0;
            //    double xTotal = 0;
            //    double xyMultiTotal = 0;
            //    double xSquareTotal = 0;

            //    for (int i = 0; i < testData.Count; i++)
            //    {
            //        var x = new double[1];
            //        var y = new double[1];

            //        //Make Prediction
            //        var FarePrediction = predFunction.Predict(testData[i]);

            //        x[0] = testData[i].FareAmount;
            //        y[0] = FarePrediction.FareAmount;

            //        //Paint a dot
            //        pl.poin(x, y, code);

            //        xTotal += x[0];
            //        yTotal += y[0];

            //        double multi = x[0] * y[0];
            //        xyMultiTotal += multi;

            //        double xSquare = x[0] * x[0];
            //        xSquareTotal += xSquare;

            //        double ySquare = y[0] * y[0];

            //        Console.WriteLine($"-------------------------------------------------");
            //        Console.WriteLine($"Predicted : {FarePrediction.FareAmount}");
            //        Console.WriteLine($"Actual:    {testData[i].FareAmount}");
            //        Console.WriteLine($"-------------------------------------------------");
            //    }

            //    // Regression Line calculation explanation:
            //    // https://www.khanacademy.org/math/statistics-probability/describing-relationships-quantitative-data/more-on-regression/v/regression-line-example

            //    double minY = yTotal / totalNumber;
            //    double minX = xTotal / totalNumber;
            //    double minXY = xyMultiTotal / totalNumber;
            //    double minXsquare = xSquareTotal / totalNumber;

            //    double m = ((minX * minY) - minXY) / ((minX * minX) - minXsquare);

            //    double b = minY - (m * minX);

            //    //Generic function for Y for the regression line
            //    // y = (m * x) + b;

            //    double x1 = 1;
            //    //Function for Y1 in the line
            //    double y1 = (m * x1) + b;

            //    double x2 = 39;
            //    //Function for Y2 in the line
            //    double y2 = (m * x2) + b;

            //    var xArray = new double[2];
            //    var yArray = new double[2];
            //    xArray[0] = x1;
            //    yArray[0] = y1;
            //    xArray[1] = x2;
            //    yArray[1] = y2;

            //    pl.col0(4);
            //    pl.line(xArray, yArray);

            //    // end page (writes output to disk)
            //    pl.eop();

            //    // output version of PLplot
            //    pl.gver(out var verText);
            //    Console.WriteLine("PLplot version " + verText);

            //} // the pl object is disposed here

            // Open Chart File In Microsoft Photos App (Or default app, like browser for .svg)

            //Console.WriteLine("Showing chart...");
            //var p = new Process();
            //string chartFileNamePath = @".\" + chartFileName;
            //p.StartInfo = new ProcessStartInfo(chartFileNamePath)
            //{
            //    UseShellExecute = true
            //};
            //p.Start();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }

    public class TaxiTripCsvReader
    {
        public IEnumerable<TaxiTrip> GetDataFromCsv(string dataLocation, int numMaxRecords)
        {
            IEnumerable<TaxiTrip> records =
                File.ReadAllLines(dataLocation)
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new TaxiTrip()
                {
                    VendorId = x[0],
                    RateCode = x[1],
                    PassengerCount = float.Parse(x[2], CultureInfo.InvariantCulture),
                    TripTime = float.Parse(x[3], CultureInfo.InvariantCulture),
                    TripDistance = float.Parse(x[4], CultureInfo.InvariantCulture),
                    PaymentType = x[5],
                    FareAmount = float.Parse(x[6], CultureInfo.InvariantCulture)
                })
                .Take<TaxiTrip>(numMaxRecords);

            return records;
        }
    }

}
