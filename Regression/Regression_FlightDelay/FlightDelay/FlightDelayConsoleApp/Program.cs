using System;
using Microsoft.ML;
using System.IO;
using FlightDelay.DataStructures;

using Microsoft.ML.Data;
using Common;
//using CreditCardFraudDetection.Trainer;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Microsoft.ML.Trainers.FastTree;
using System.Text;
using ML2SQL;
using Microsoft.ML.Transforms;

namespace FlightDelay
{
    internal static class Program
    {
        private static string DatasetsLocation = "../../../../Data";
        private static string SQLLocation = "../../../../SQL";

        private static string ModelsLocation = "../../../../MLModels";

        //private static string TrainingDataWithIDRelativePath = $"{DatasetsLocation}/flight_delay_train_data.csv";
        //private static string TestDataRelativeWithIdPath = $"{DatasetsLocation}/flight_delay_test_data.csv";
        //private static string AllDataRelativeWithIdPath = $"{DatasetsLocation}/flight_complete_data.csv";
        private static string TrainingDataWithIDRelativePath = $"{DatasetsLocation}/flight_delay_train_data_very_small.csv";
        private static string TestDataRelativeWithIdPath = $"{DatasetsLocation}/flight_delay_test_data_very_small.csv";
        private static string AllDataRelativeWithIdPath = $"{DatasetsLocation}/flight_complete_data_very_small.csv";
        private static string SQL_INSERT_FastTree = $"{SQLLocation}/SQL_INSERT_Flight_FastTree.sql";
        private static readonly string SQL_WEIGHTS_RelativePath = $"{SQLLocation}/SQL_insert_weights.sql";
        private static string MYSQL_INSERT_FastTree = $"{SQLLocation}/02_MYSQL_INSERT_Flight_Delay_FastTree.sql";
        private static string SQLSERVER_INSERT_FastTree = $"{SQLLocation}/02_SQLSERVER_INSERT_Flight_Delay_FastTree.sql";
        private static string SamplesInput = $"{DatasetsLocation}/Input/";
        private static string SamplesOutput = $"{DatasetsLocation}/Output/"; 

        private static string RESULT_PATH_FastTree = "../../../ResultsFlightDelayFastTree.csv";             

        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";    
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            char separator = ',';

            string whereClause = " \n where Id >= @id  and Id < ( @id + chuncksize ) ";

            string[] inputModes = { "CSV", "MYSQL", "SQLSERVER" };
            string[] outputModes = { "CSV", "CONSOLE", "NO_OUTPUT", "MYSQL", "SQLSERVER" };

            Tuple<string, string>[] configurations = {
            new Tuple<string,string> (inputModes[0],outputModes[0]) ,
            new Tuple<string,string> (inputModes[0],outputModes[1]),
            new Tuple<string,string> (inputModes[0],outputModes[2]),
            new Tuple<string,string> (inputModes[0],outputModes[3]),
            //new Tuple<string,string> (inputModes[0],outputModes[4]),
            new Tuple<string,string> (inputModes[1],outputModes[3]),
            //new Tuple<string,string> (inputModes[2],outputModes[4])
            };  

            string outputQueryFastTree = "INSERT into flight_delay_with_output (Id,Score ) VALUES ";  

            int numberOfTrainElements = countRows(TrainingDataWithIDRelativePath, true, ",", 0);

            Console.WriteLine("Number of train examples: " + numberOfTrainElements);

            int numberOfTestElements = countRows(TestDataRelativeWithIdPath, true, ",", numberOfTrainElements);

            Console.WriteLine("Number of test examples: " + numberOfTestElements);
            int numberOfElements = numberOfTestElements;

            // 1. Common data loading configuration
            var trainingDataView = mlContext.Data.LoadFromTextFile<FlightDelayObservation>(path: TrainingDataWithIDRelativePath, hasHeader:true, separatorChar: ',');
            var testDataView = mlContext.Data.LoadFromTextFile<FlightDelayObservation>(path: TestDataRelativeWithIdPath, hasHeader:true, separatorChar: ',');


            // int[] chunckSizes = { 1, 10, 100, 1000, 10000, 100000 };
            bool header = true;
            int[] chunckSizes = {1000, 10000, 100000 };
            //int[] chunckSizes = {10000 };
            MLSQL.createDataSample(AllDataRelativeWithIdPath, SamplesInput, chunckSizes, true);


            //Get all the feature column names (All except the Label and the IdPreservationColumn)
            string[] numericalFeatureNames = trainingDataView.Schema.AsQueryable()
                .Select(column => column.Name)                               // Get alll the column names
                .Where(name => name != "Id")
                .Where(name => name != "UniqueCarrier")
                .Where(name => name != "Origin")
                .Where(name => name != "Dest")
                .Where(name => name != "Label")         // Do not include the Label column
                .ToArray();
            string[] originalCategoricalFeatureNames = {"UniqueCarrier", "Origin", "Dest"};
            List<string> originalFeatureColumnNamesList = new List<string>();
            originalFeatureColumnNamesList.AddRange(originalCategoricalFeatureNames);
            originalFeatureColumnNamesList.AddRange(numericalFeatureNames);
            string[] originalFeatureColumnNames = originalFeatureColumnNamesList.ToArray();

            string[] categoricalFeatureNames = {"UniqueCarrierEncoded", "OriginEncoded", "DestEncoded"};
            List<string> featureColumnNamesList = new List<string>();
            featureColumnNamesList.AddRange(categoricalFeatureNames);
            featureColumnNamesList.AddRange(numericalFeatureNames);
            string[] featureColumnNames = featureColumnNamesList.ToArray();        

            // 2. Common data pre-process with pipeline data transformations
            // Transform categorical features to numerical and concatenate all the numeric columns into a single features column
            var dataProcessPipeline = mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "UniqueCarrierEncoded", inputColumnName: nameof(FlightDelayObservation.UniqueCarrier))
                                        .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "OriginEncoded", inputColumnName: nameof(FlightDelayObservation.Origin)))
                                        .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DestEncoded", inputColumnName: nameof(FlightDelayObservation.Dest)))
                                        .Append(mlContext.Transforms.Concatenate("Features", featureColumnNames));
            
            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            Common.ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 10);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 10);
            
            // STEP 3: Set the training algorithm, then create and config the modelBuilder - Selected Trainer (FastTree Regression algorithm)

            var trainer = mlContext.Regression.Trainers.FastTree(numberOfTrees: 100);
            var trainingPipeline = dataProcessPipeline.Append(trainer).AppendCacheCheckpoint(mlContext);

            // STEP 4: Train the model fitting to the DataSet
            //The pipeline is trained on the dataset that has been loaded and transformed.
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            IDataView predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");
            Common.ConsoleHelper.PrintRegressionMetrics(trainer.ToString(), metrics);
            //Environment.Exit(1);

            // [BEGIN]  ML.NET PREDICTION TIME ------------------------------------------------------------------------
            PredictorExecutor<FlightDelayObservation, FlightPrediction> predictorExecutor = new PredictorExecutor<FlightDelayObservation, FlightPrediction>();

            string modelRelativeLocation_FastTree = $"{ModelsLocation}/FastTree_Model.zip";

            mlContext.Model.Save(trainedModel, trainingDataView.Schema, modelRelativeLocation_FastTree);

            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTree, AllDataRelativeWithIdPath, "flight_delay_FastTree", "MLtoSQL", MYSQL_INSERT_FastTree, "MYSQL", numberOfElements, false,',');
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTree, AllDataRelativeWithIdPath, "flight_delay_FastTree", "mltosql", SQLSERVER_INSERT_FastTree, "SQLSERVER", numberOfElements, false,',');            

            // at this point make sure that the db includes all the data to be queried
            predictorExecutor.executePredictions("FLIGHT_DELAY_FASTTREE", modelRelativeLocation_FastTree, SamplesInput, SamplesOutput, RESULT_PATH_FastTree, chunckSizes, configurations, "flight_delay_FastTree", outputQueryFastTree, numberOfElements, separator,header);
            // [END]  ML.NET PREDICTION TIME --------------------------------------------------------------------------
            Environment.Exit(1);

            // [BEGIN] CONVERT PIPELINE IN SQL ------------------------------------------------------------------------            

            IDataView d = trainedModel.Transform(trainingDataView);

            // RETRIEVE ONE HOT ENCODING FEATURE MAPPING
            int offset = 0;
            List<string> allFeatures = new List<string>();
            Dictionary<string, string> categoricalFeatureMap = new Dictionary<string, string>();
            for (int i = 0; i < originalCategoricalFeatureNames.Count(); i++)
            {
                string categoricalFeature = originalCategoricalFeatureNames[i];
                string encodedCategoricalFeature = categoricalFeatureNames[i];

                var featureEncoding = MLSQL.createValueMappingOneHotEnconding(d, categoricalFeature, encodedCategoricalFeature, offset, true);
                offset += featureEncoding.Count();
                foreach (string catVal in featureEncoding.Keys)
                {
                    categoricalFeatureMap.Add(categoricalFeature + "_" + catVal, categoricalFeature);
                    allFeatures.Add(categoricalFeature + "_" + catVal);
                }
                //allFeatures.AddRange(from key in featureEncoding.Keys select categoricalFeature + "_" + key);
            }
            allFeatures.AddRange(numericalFeatureNames);

            // EXTRACT RULES FROM THE FAST TREE MODEL AND CREATE A SQL QUERY
            string treeScoresTable = "flight_delay_tree_scores";
            string[] Modes = { "CONSOLE", "NO_OUTPUT", "MYSQL", "CSV" };

            string[] procedure_names_FastTree = { "flight_delay_fasttree", "flight_delay_fasttree_no_output", "flight_delay_fasttree_db" , "flight_delay_fasttree_csv" };

            Microsoft.ML.Trainers.FastTree.RegressionTreeEnsemble a = trainedModel.LastTransformer.Model.TrainedTreeEnsemble;
            List<RegressionTree> trees = (System.Collections.Generic.List<Microsoft.ML.Trainers.FastTree.RegressionTree>)a.Trees;
            List<double> treeWeights = (System.Collections.Generic.List<double>)a.TreeWeights;
            string[] selectParams = {"Id"};
            //string query_FastTree = MLSQL.GenerateSQLRegressionTree(allFeatures.ToArray(), selectParams, "SCORE", "", "flight_delay_FastTree", whereClause, treeWeights, trees, categoricalFeatureMap: categoricalFeatureMap);
            List<string> queryList = MLSQL.GenerateSQLRegressionTreeSplit(allFeatures.ToArray(), selectParams, "", "flight_delay_FastTree", whereClause, treeWeights, trees, Modes, procedure_names_FastTree, "flight_delay_with_output", "mltosql", categoricalFeatureMap: categoricalFeatureMap, treeScoresTable: treeScoresTable);

            //Environment.Exit(1);
            //Console.WriteLine("\n\n"+query_FastTree+"\n\n");

            //string outputTablename = "flight_delay_with_output";

            //string[] Modes = { "CONSOLE", "NO_OUTPUT", "MYSQL", "CSV" };

            //string[] procedure_names_FastTree = { "flight_delay_fasttree", "flight_delay_fasttree_no_output", "flight_delay_fasttree_db" , "flight_delay_fasttree_csv" };

            for (int i = 0; i < Modes.Length; i++)
            {
                //string QQ = MLSQL.GenerateSQLQueriesOnDifferentModes(query_FastTree, "mltosql", "flight_delay_with_output", procedure_names_FastTree[i], "Id", Modes[i], false);
                string QQ = queryList[i];
                string MYSQLPATH = $"{SQLLocation}/MYSQL_PREDICTION_WITH_ID_" + procedure_names_FastTree[i] + ".sql";
                string SQLSERVERPATH = $"{SQLLocation}/SQLSERVER_PREDICTION_WITH_ID_" + procedure_names_FastTree[i] + ".sql";
                MLSQL.WriteSQL(QQ, GetAbsolutePath(MYSQLPATH));
                MLSQL.WriteSQL(QQ, GetAbsolutePath(SQLSERVERPATH));
            }
            // [END] CONVERT PIPELINE IN SQL --------------------------------------------------------------------------

       
        }

        private static int countRows(String inputFilePath, bool header, string separator, int start_id)
        {
            string[] lines = System.IO.File.ReadAllLines(inputFilePath);

            int index = 0;
            if (header)
            {
                index = 1;
            }

            for (int i = index; i < lines.Length; i++)
            {
                start_id += 1;
            }

            Console.WriteLine("Start ID: " + start_id);

            return start_id;
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }        

    }
}
