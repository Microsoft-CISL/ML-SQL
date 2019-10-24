using System;
using Microsoft.ML;
using System.IO;
using Criteo.DataStructures;

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

namespace Criteo
{
    internal static class Program
    {
        private static string DatasetsLocation = "../../../../Data";
        private static string SQLLocation = "../../../../SQL";

        private static string ModelsLocation = "../../../../MLModels";
        private static string TrainingDataWithIDRelativePath = $"{DatasetsLocation}/criteo_sample_train_data.csv";
        private static string TestDataRelativeWithIdPath = $"{DatasetsLocation}/criteo_sample_test_data.csv";
        private static string AllDataRelativeWithIdPath = $"{DatasetsLocation}/criteo_data_sample.csv";
        private static string SQL_INSERT_FastTree = $"{SQLLocation}/SQL_INSERT_Criteo_FastTree.sql";
        private static readonly string SQL_WEIGHTS_RelativePath = $"{SQLLocation}/SQL_insert_weights.sql";
        private static string MYSQL_INSERT_FastTree = $"{SQLLocation}/02_MYSQL_INSERT_Criteo_FastTree.sql";
        private static string SQLSERVER_INSERT_FastTree = $"{SQLLocation}/02_SQLSERVER_INSERT_Criteo_FastTree.sql";
        private static string SamplesInput = $"{DatasetsLocation}/Input/";
        private static string SamplesOutput = $"{DatasetsLocation}/Output/"; 

        private static string RESULT_PATH_FastTree = "../../../ResultsCriteoFastTree.csv";             

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

            string outputQueryFastTree = "INSERT into criteo_with_output (Id,Score ) VALUES ";  

            int numberOfTrainElements = countRows(TrainingDataWithIDRelativePath, true, ",", 0);

            Console.WriteLine("Number of train examples: " + numberOfTrainElements);

            int numberOfTestElements = countRows(TestDataRelativeWithIdPath, true, ",", numberOfTrainElements);

            Console.WriteLine("Number of test examples: " + numberOfTestElements);
            int numberOfElements = numberOfTestElements;        

            // 1. Common data loading configuration
            var trainingDataView = mlContext.Data.LoadFromTextFile<CriteoObservation>(path: TrainingDataWithIDRelativePath, hasHeader:true, separatorChar: ',');
            var testDataView = mlContext.Data.LoadFromTextFile<CriteoObservation>(path: TestDataRelativeWithIdPath, hasHeader:true, separatorChar: ',');


            // int[] chunckSizes = { 1, 10, 100, 1000, 10000, 100000 };
            bool header = true;
            int[] chunckSizes = {1000, 10000, 100000 };
            //int[] chunckSizes = {10000 };
            MLSQL.createDataSample(AllDataRelativeWithIdPath, SamplesInput, chunckSizes, true);

            //Get all the feature column names (All except the Label and the IdPreservationColumn)
            string[] numericalFeatureNames = trainingDataView.Schema.AsQueryable()
                .Select(column => column.Name)                               // Get alll the column names
                .Where(name => name != "Id")
                .Where(name => name != "Label")         // Do not include the Label column
                .Where(name => name != "CategoricalFeature1")
                .Where(name => name != "CategoricalFeature2")
                .Where(name => name != "CategoricalFeature3")
                .Where(name => name != "CategoricalFeature4")
                .Where(name => name != "CategoricalFeature5")
                .Where(name => name != "CategoricalFeature6")
                .Where(name => name != "CategoricalFeature7")
                .Where(name => name != "CategoricalFeature8")
                .Where(name => name != "CategoricalFeature9")
                .Where(name => name != "CategoricalFeature10")
                .Where(name => name != "CategoricalFeature11")
                .Where(name => name != "CategoricalFeature12")
                .Where(name => name != "CategoricalFeature13")
                .Where(name => name != "CategoricalFeature14")
                .Where(name => name != "CategoricalFeature15")
                .Where(name => name != "CategoricalFeature16")
                .Where(name => name != "CategoricalFeature17")
                .Where(name => name != "CategoricalFeature18")
                .Where(name => name != "CategoricalFeature19")
                .Where(name => name != "CategoricalFeature20")
                .Where(name => name != "CategoricalFeature21")
                .Where(name => name != "CategoricalFeature22")
                .Where(name => name != "CategoricalFeature23")
                .Where(name => name != "CategoricalFeature24")
                .Where(name => name != "CategoricalFeature25")
                .Where(name => name != "CategoricalFeature26")
                .ToArray();
            string[] originalCategoricalFeatureNames = {"CategoricalFeature1", "CategoricalFeature2", "CategoricalFeature3", "CategoricalFeature4", 
                                                        "CategoricalFeature5", "CategoricalFeature6", "CategoricalFeature7", "CategoricalFeature8", 
                                                        "CategoricalFeature9", "CategoricalFeature10", "CategoricalFeature11", "CategoricalFeature12", 
                                                        "CategoricalFeature13", "CategoricalFeature14", "CategoricalFeature15", "CategoricalFeature16", 
                                                        "CategoricalFeature17", "CategoricalFeature18", "CategoricalFeature19", "CategoricalFeature20", 
                                                        "CategoricalFeature21", "CategoricalFeature22", "CategoricalFeature23", "CategoricalFeature24", 
                                                        "CategoricalFeature25", "CategoricalFeature26"};
            List<string> originalFeatureColumnNamesList = new List<string>();
            originalFeatureColumnNamesList.AddRange(originalCategoricalFeatureNames);
            originalFeatureColumnNamesList.AddRange(numericalFeatureNames);
            string[] originalFeatureColumnNames = originalFeatureColumnNamesList.ToArray();

            string[] categoricalFeatureNames = {"EncodedCategoricalFeature1", "EncodedCategoricalFeature2", "EncodedCategoricalFeature3", "EncodedCategoricalFeature4", 
                                                "EncodedCategoricalFeature5", "EncodedCategoricalFeature6", "EncodedCategoricalFeature7", "EncodedCategoricalFeature8", 
                                                "EncodedCategoricalFeature9", "EncodedCategoricalFeature10", "EncodedCategoricalFeature11", "EncodedCategoricalFeature12", 
                                                "EncodedCategoricalFeature13", "EncodedCategoricalFeature14", "EncodedCategoricalFeature15", "EncodedCategoricalFeature16", 
                                                "EncodedCategoricalFeature17", "EncodedCategoricalFeature18", "EncodedCategoricalFeature19", "EncodedCategoricalFeature20", 
                                                "EncodedCategoricalFeature21", "EncodedCategoricalFeature22", "EncodedCategoricalFeature23", "EncodedCategoricalFeature24", 
                                                "EncodedCategoricalFeature25", "EncodedCategoricalFeature26"};
            List<string> featureColumnNamesList = new List<string>();
            featureColumnNamesList.AddRange(categoricalFeatureNames);
            featureColumnNamesList.AddRange(numericalFeatureNames);
            string[] featureColumnNames = featureColumnNamesList.ToArray();

            // 2. Common data pre-process with pipeline data transformations
            // Transform categorical features to numerical and concatenate all the numeric columns into a single features column
            var dataProcessPipeline = mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature1", inputColumnName: nameof(CriteoObservation.CategoricalFeature1))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature2", inputColumnName: nameof(CriteoObservation.CategoricalFeature2)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature3", inputColumnName: nameof(CriteoObservation.CategoricalFeature3)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature4", inputColumnName: nameof(CriteoObservation.CategoricalFeature4)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature5", inputColumnName: nameof(CriteoObservation.CategoricalFeature5)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature6", inputColumnName: nameof(CriteoObservation.CategoricalFeature6)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature7", inputColumnName: nameof(CriteoObservation.CategoricalFeature7)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature8", inputColumnName: nameof(CriteoObservation.CategoricalFeature8)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature9", inputColumnName: nameof(CriteoObservation.CategoricalFeature9)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature10", inputColumnName: nameof(CriteoObservation.CategoricalFeature10)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature11", inputColumnName: nameof(CriteoObservation.CategoricalFeature11)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature12", inputColumnName: nameof(CriteoObservation.CategoricalFeature12)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature13", inputColumnName: nameof(CriteoObservation.CategoricalFeature13)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature14", inputColumnName: nameof(CriteoObservation.CategoricalFeature14)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature15", inputColumnName: nameof(CriteoObservation.CategoricalFeature15)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature16", inputColumnName: nameof(CriteoObservation.CategoricalFeature16)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature17", inputColumnName: nameof(CriteoObservation.CategoricalFeature17)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature18", inputColumnName: nameof(CriteoObservation.CategoricalFeature18)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature19", inputColumnName: nameof(CriteoObservation.CategoricalFeature19)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature20", inputColumnName: nameof(CriteoObservation.CategoricalFeature20)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature21", inputColumnName: nameof(CriteoObservation.CategoricalFeature21)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature22", inputColumnName: nameof(CriteoObservation.CategoricalFeature22)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature23", inputColumnName: nameof(CriteoObservation.CategoricalFeature23)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature24", inputColumnName: nameof(CriteoObservation.CategoricalFeature24)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature25", inputColumnName: nameof(CriteoObservation.CategoricalFeature25)))
                                        .Append( mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "EncodedCategoricalFeature26", inputColumnName: nameof(CriteoObservation.CategoricalFeature26)))
                                        .Append(mlContext.Transforms.Concatenate("Features", featureColumnNames));
            
            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            Common.ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 10);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 10);
            
            // STEP 3: Set the training algorithm, then create and config the modelBuilder - Selected Trainer (FastTree Regression algorithm)

            var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: nameof(CriteoObservation.Label));
            var trainingPipeline = dataProcessPipeline.Append(trainer).AppendCacheCheckpoint(mlContext);

            // STEP 4: Train the model fitting to the DataSet
            //The pipeline is trained on the dataset that has been loaded and transformed.
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            IDataView predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");
            Common.ConsoleHelper.PrintBinaryClassificationMetrics(trainer.ToString(), metrics);

            // [BEGIN]  ML.NET PREDICTION TIME ------------------------------------------------------------------------
            PredictorExecutor<CriteoObservation, CriteoPrediction> predictorExecutor = new PredictorExecutor<CriteoObservation, CriteoPrediction>();

            string modelRelativeLocation_FastTree = $"{ModelsLocation}/FastTree_Model.zip";

            mlContext.Model.Save(trainedModel, trainingDataView.Schema, modelRelativeLocation_FastTree);

            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTree, AllDataRelativeWithIdPath, "criteo_FastTree", "MLtoSQL", MYSQL_INSERT_FastTree, "MYSQL", numberOfElements, false,',');
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTree, AllDataRelativeWithIdPath, "criteo_FastTree", "mltosql", SQLSERVER_INSERT_FastTree, "SQLSERVER", numberOfElements, false,',');            

            // at this point make sure that the db includes all the data to be queried
            predictorExecutor.executePredictions("CRITEO_FASTTREE", modelRelativeLocation_FastTree, SamplesInput, SamplesOutput, RESULT_PATH_FastTree, chunckSizes, configurations, "criteo_FastTree", outputQueryFastTree, numberOfElements, separator,header);
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
            string treeScoresTable = "criteo_tree_scores";
            string[] Modes = { "CONSOLE", "NO_OUTPUT", "MYSQL", "CSV" };
            string[] procedure_names_FastTree = { "criteo_fasttree", "criteo_fasttree_no_output", "criteo_fasttree_db" , "criteo_fasttree_csv" };

            Microsoft.ML.Trainers.FastTree.RegressionTreeEnsemble a = trainedModel.LastTransformer.Model.SubModel.TrainedTreeEnsemble;
            List<RegressionTree> trees = (System.Collections.Generic.List<Microsoft.ML.Trainers.FastTree.RegressionTree>)a.Trees;
            List<double> treeWeights = (System.Collections.Generic.List<double>)a.TreeWeights;
            string[] selectParams = {"Id"};
            //string query_FastTree = MLSQL.GenerateSQLRegressionTree(allFeatures.ToArray(), selectParams, "SCORE", "", "criteo_FastTree", whereClause, treeWeights, trees, categoricalFeatureMap: categoricalFeatureMap);
            List<string> queryList = MLSQL.GenerateSQLRegressionTreeSplit(allFeatures.ToArray(), selectParams, "", "criteo_FastTree", whereClause, treeWeights, trees, Modes, procedure_names_FastTree, "criteo_with_output", "mltosql", categoricalFeatureMap: categoricalFeatureMap, treeScoresTable: treeScoresTable);
            //Environment.Exit(1);
            //Console.WriteLine("\n\n"+query_FastTree+"\n\n");

            

            for (int i = 0; i < Modes.Length; i++)
            {
                //string QQ = MLSQL.GenerateSQLQueriesOnDifferentModes(query_FastTree, "mltosql", "criteo_with_output", procedure_names_FastTree[i], "Id", Modes[i], false);
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
