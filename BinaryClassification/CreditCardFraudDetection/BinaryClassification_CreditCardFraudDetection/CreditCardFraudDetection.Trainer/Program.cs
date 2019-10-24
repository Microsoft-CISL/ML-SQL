using Microsoft.ML;
using System.Linq;
using System.IO;
using System;
using CreditCardFraudDetection.Common.DataModels;
using System.IO.Compression;
using Microsoft.ML.Trainers.FastTree;
using System.Collections.Generic;
using System.Collections.Immutable;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Transforms;
using CreditCardFraudDetection.Trainer.Common;
using Microsoft.ML.Data;
using static Microsoft.ML.Transforms.NormalizingTransformer;
using ML2SQL;

namespace CreditCardFraudDetection.Trainer
{
    class Program
    {

        private static string SQLLocation = @"../../../../SQL";

        static void Main(string[] args)
        {
            //File paths
            char separator = ',';

            string AssetsRelativePath = @"../../../assets";
            string DataFolder = @"../../../../Data/Samples";
            string SamplesInput = $@"../../../../Data/Samples/Input/";
            string SamplesOutput = $"{DataFolder}/Output/";


            string SQLFolder = @"../../../../SQL/";

            string ResultOutput = @"../../../../resultCreditCard.csv";
            string assetsPath = GetAbsolutePath(AssetsRelativePath);
            string zipDataSet = Path.Combine(assetsPath, "input", "creditcardfraud-dataset.zip");
            string fullDataSetFilePath = Path.Combine(assetsPath, "input", "creditcard_sample.csv");
            string trainDataSetFilePath = Path.Combine(assetsPath, "output", "trainData.csv");
            string testDataSetFilePath = Path.Combine(assetsPath, "output", "testData.csv");
            string modelFilePath = Path.Combine(assetsPath, "output", "fastTree.zip");

            string MYSQL_INSERT_RelativePath = $"{SQLFolder}02_MYSQL_INSERT.sql";
            string SQLSERVER_INSERT_RelativePath = $"{SQLFolder}02_SQLSERVER_INSERT.sql";

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";    
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;


            Console.WriteLine("SampleInput: " + SamplesInput);
            // Unzip the original dataset as it is too large for GitHub repo if not zipped
            UnZipDataSet(zipDataSet, fullDataSetFilePath);

            // Create a common ML.NET context.
            // Seed set to any number so you have a deterministic environment for repeateable results
            MLContext mlContext = new MLContext(seed: 1);

            // Prepare data and create Train/Test split datasets
            PrepDatasets(mlContext, fullDataSetFilePath, trainDataSetFilePath, testDataSetFilePath);

            // Load Datasets
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<TransactionObservation>(trainDataSetFilePath, separatorChar: ',', hasHeader: true);
            IDataView testDataView = mlContext.Data.LoadFromTextFile<TransactionObservation>(testDataSetFilePath, separatorChar: ',', hasHeader: true);

            // Train Model
            (ITransformer model, string trainerName) = TrainModel(mlContext, trainingDataView);

            // Evaluate quality of Model
            //EvaluateModel(mlContext, model, testDataView, trainerName);

            // Save model
            SaveModel(mlContext, model, modelFilePath, trainingDataView.Schema);


            PredictorExecutor<TransactionObservation, TransactionFraudPrediction> predictorExecutor = new PredictorExecutor<TransactionObservation, TransactionFraudPrediction>();

            int numberOfElements = 284806;
            string outputQuery = " INSERT into credit_card_with_score_output(Id, Score) VALUES ";

            int[] chunckSizes = { 1, 10, 100, 1000, 10000, 100000, 1000000 };
            //int[] chunckSizes = { 100000, 1000000 };

            string[] inputModes = { "CSV", "MYSQL", "SQLSERVER" };

            string[] outputModes = { "CSV", "CONSOLE", "NO_OUTPUT", "MYSQL", "SQLSERVER" };

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

            MLSQL.createDataSample(fullDataSetFilePath, SamplesInput, chunckSizes, true);



            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelFilePath, fullDataSetFilePath, "creditcard_with_prediction", "MLtoSQL", MYSQL_INSERT_RelativePath, "MYSQL", numberOfElements - 1, false, separator);
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelFilePath, fullDataSetFilePath, "creditcard_with_prediction", "mltosql", SQLSERVER_INSERT_RelativePath, "SQLSERVER", numberOfElements - 1, false, separator);
            //GetAbsolutePath(SamplesInput)
            Environment.Exit(1);
            predictorExecutor.executePredictions("CREDITCARD", modelFilePath, SamplesInput, SamplesOutput, ResultOutput, chunckSizes, configurations, "creditcard_with_prediction", outputQuery, numberOfElements, separator, header);





            Console.WriteLine("END OF THE PROCESS");


        }

        public static void PrepDatasets(MLContext mlContext, string fullDataSetFilePath, string trainDataSetFilePath, string testDataSetFilePath)
        {
            //Only prep-datasets if train and test datasets don't exist yet

            if (!File.Exists(trainDataSetFilePath) &&
                !File.Exists(testDataSetFilePath))
            {
                Console.WriteLine("===== Preparing train/test datasets =====");

                //Load the original single dataset
                IDataView originalFullData = mlContext.Data.LoadFromTextFile<TransactionObservation>(fullDataSetFilePath, separatorChar: ',', hasHeader: true);

                // Split the data 80:20 into train and test sets, train and evaluate.
                TrainTestData trainTestData = mlContext.Data.TrainTestSplit(originalFullData, testFraction: 0.2, seed: 1);
                IDataView trainData = trainTestData.TrainSet;
                IDataView testData = trainTestData.TestSet;

                //Inspect TestDataView to make sure there are true and false observations in test dataset, after spliting 
                InspectData(mlContext, testData, 4);

                // save train split
                using (var fileStream = File.Create(trainDataSetFilePath))
                {
                    mlContext.Data.SaveAsText(trainData, fileStream, separatorChar: ',', headerRow: true, schema: true);
                }

                // save test split 
                using (var fileStream = File.Create(testDataSetFilePath))
                {
                    mlContext.Data.SaveAsText(testData, fileStream, separatorChar: ',', headerRow: true, schema: true);
                }
            }
        }

        public static (ITransformer model, string trainerName) TrainModel(MLContext mlContext, IDataView trainDataView)
        {
            //Get all the feature column names (All except the Label and the IdPreservationColumn)
            string[] featureColumnNames = trainDataView.Schema.AsQueryable()
                .Select(column => column.Name)                               // Get alll the column names
                .Where(name => name != nameof(TransactionObservation.Label)) // Do not include the Label column
                .Where(name => name != "IdPreservationColumn")               // Do not include the IdPreservationColumn/StratificationColumn
                .Where(name => name != "Time")
                .Where(name => name != "Id")
                .Where(name => name != "PredictedLabel")                                  // Do not include the Time column. Not needed as feature column
                .ToArray();



            NormalizingTransformer binningTransformer = null;


            // Create the data process pipeline
            IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Concatenate("Features", featureColumnNames)
                                            .Append(mlContext.Transforms.DropColumns(new string[] { "Time" }))
                                            .Append(mlContext.Transforms.NormalizeMeanVariance(inputColumnName: "Features",
                                                                                 outputColumnName: "FeaturesNormalizedByMeanVar").WithOnFitDelegate(fittedTransformer => binningTransformer = fittedTransformer));


            Console.WriteLine("BINNING TRANSFORMER: " + binningTransformer == null);

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            Common.ConsoleHelper.PeekDataViewInConsole(mlContext, trainDataView, dataProcessPipeline, 2);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainDataView, dataProcessPipeline, 1);

            // Set the training algorithm
            //IEstimator<ITransformer>
            var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: nameof(TransactionObservation.Label),
                                                                                                featureColumnName: "FeaturesNormalizedByMeanVar",
                                                                                                numberOfLeaves: 100,
                                                                                                numberOfTrees: 100,
                                                                                                minimumExampleCountPerLeaf: 1,
                                                                                                learningRate: 0.2).AppendCacheCheckpoint(mlContext);



            //var modelAAA =  dataProcessPipeline.AppendCacheCheckpoint(mlContext).Fit(trainDataView);





            //IEstimator <ITransformer> 
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            ConsoleHelper.ConsoleWriteHeader("=============== Training model ===============");

            // ITransformer 
            var model = trainingPipeline.Fit(trainDataView);


            Console.WriteLine("BINNING TRANSFORMER: " + binningTransformer == null);

            //var a1 = default;
            //var a2 = default;

            //var model = trainingPipeline.WithOnFitDelegate(a1,a2).AppendCacheCheckpoint(mlContext).Fit(trainDataView);

            VBuffer<float> ww = default;





            model.LastTransformer.LastTransformer.Model.SubModel.GetFeatureWeights(ref ww);



            float[] weights1 = new float[ww.Length];

            for (int i = 0; i < ww.Length; i++)
            {
                weights1[i] = ww.GetItemOrDefault(i);
            }


            //var binningParam = binningTransformer.GetNormalizerModelParameters(0) as
            //CdfNormalizerModelParameters<ImmutableArray<float>>;


            var noCdfParams = binningTransformer.GetNormalizerModelParameters(0) as AffineNormalizerModelParameters<ImmutableArray<float>>;
            Console.WriteLine("Offset length: " + noCdfParams.Offset.Length);
            Console.WriteLine("Scale lenght: " + noCdfParams.Scale.Length);
            var offset = noCdfParams.Offset.Length == 0 ? 0 : noCdfParams.Offset[0];
            var scale = noCdfParams.Scale[0];
            Console.WriteLine($"Values for slot 1 would be transfromed by applying y = (x - ({offset})) * {scale}");

            int length = noCdfParams.Scale.Length;


            float[] avgs = new float[length];
            float[] stds = new float[length];


            for (int i = 0; i < length; i++)
            {
                if (noCdfParams.Offset.Length == 0)
                {
                    avgs[i] = 0;
                }
                else
                {
                    avgs[i] = noCdfParams.Offset[i];
                }
                stds[i] = noCdfParams.Scale[i];
            }

            string tablename = "creditcard_with_prediction";
            string normalizationTableName = "credit_card_normalized_data";
            string treeScoresTable = "credit_card_tree_scores";

            //string[] selectParams = { "Id", "Score" };
            string[] selectParams = { "Id"};

            string whereClause = "\n where Id >= @id  and Id < ( @id + chuncksize ) \n";


            String normalizeQuery = MLSQL.GenerateSQLNormalizeByVarianceColumns(featureColumnNames, selectParams, whereClause, avgs, stds, "", tablename);

            Console.WriteLine("Normalize QUERY: " + normalizeQuery);

            string[] temporaryTables = {normalizationTableName, treeScoresTable};
            string preProcessingQuery = MLSQL.GenerateSQLDeleteTemporaryTables(temporaryTables);
            string PREMYSQLPATH = $"{SQLLocation}/MYSQL_CREDIT_CARD_PREPROCESSING.sql";
            string PRESQLSERVERPATH = $"{SQLLocation}/SQLSERVER_CREDIT_CARD_PREPROCESSING.sql";
            MLSQL.WriteSQL(preProcessingQuery, GetAbsolutePath(PREMYSQLPATH));
            MLSQL.WriteSQL(preProcessingQuery, GetAbsolutePath(PRESQLSERVERPATH));



            //Dictionary<string, string> normalizeQueries = MLSQL.GenerateSQLTableAfterNormalization(normalizationTableName, normalizeQuery, tablename);
            string insertNormalizeQuery = MLSQL.GenerateSQLTableAfterNormalization(normalizationTableName, normalizeQuery, whereClause);

            /*foreach (string s in normalizeQueries.Keys)
            {
                Console.WriteLine(s);
                Console.WriteLine(normalizeQueries[s]);
            }
            Environment.Exit(1);*/


            String internalQuery1 = MLSQL.GenerateSQLMultiplyByLinearCombination(featureColumnNames, selectParams, "", weights1, "", tablename);

            Console.WriteLine("Internal query 1: " + internalQuery1);

            //var d = mlContext.Transforms.NormalizeMeanVariance(inputColumnName: "Features",
            //                                                                     outputColumnName: "FeaturesNormalizedByMeanVar").
            //                                                                     Fit(trainDataView);


            //var aaa = mlContext.Transforms.Concatenate("Features", featureColumnNames)
            //.Append(mlContext.Transforms.DropColumns(new string[] { "Time" })).Fit(trainDataView);





            // var transformParams = d.GetNormalizerModelParameters(0) as CdfNormalizerModelParameters<ImmutableArray<float>>;




            Microsoft.ML.Trainers.FastTree.RegressionTreeEnsemble a = model.LastTransformer.LastTransformer.Model.SubModel.TrainedTreeEnsemble;
            double bias = a.Bias;
            List<RegressionTree> trees = (System.Collections.Generic.List<Microsoft.ML.Trainers.FastTree.RegressionTree>)a.Trees;
            List<double> treeWeights = (System.Collections.Generic.List<double>)a.TreeWeights;            

            string[] Modes = { "CONSOLE", "NO_OUTPUT", "MYSQL", "CSV" };


            string[] procedure_names_FastTree = { "credit_card", "credit_card_no_output", "credit_card_db" , "credit_card_csv" };


            //String internalQuery2 = MLSQL.GenerateSQLRegressionTree(featureColumnNames, selectParams, "", " ( " + normalizeQuery + " ) as F", whereClause, treeWeights, trees);
            List<string> queryList = MLSQL.GenerateSQLRegressionTreeSplit(featureColumnNames, selectParams, "", " ( " + normalizeQuery + " ) as F", whereClause, treeWeights, trees, Modes, procedure_names_FastTree, "credit_card_with_score_output", "mltosql", preProcessingTable: normalizationTableName, treeScoresTable: treeScoresTable);


            //Console.WriteLine("Internal query 2: " + internalQuery2);


            // Environment.Exit(0);

            Console.WriteLine("Number of weights: " + ww.Length);
            //Console.WriteLine("A: " + a.);

            Console.WriteLine("Number of trees: " + trees.Count());
            Console.WriteLine("Number of trees weights: " + treeWeights.Count());


            for (int i = 0; i < treeWeights.Count; i++)
            {

                Console.WriteLine("Tree: " + i + " weight: " + treeWeights[i]);
            }

            Console.WriteLine("Weights: \n");
            for (int i = 0; i < ww.Length; i++)
            {
                Console.WriteLine("i: " + i + " w: " + ww.GetItemOrDefault(i));
            }

            Console.WriteLine("\nBias: " + bias);

            //Console.WriteLine("\n\n\n");

            //for ( int i =0; i< trees.Count();i++)

            //{



            //RegressionTree tree = trees[i];
            //double treeWeight = treeWeights[i];
            //int numberOfnodes = tree.NumberOfNodes;
            //int numberOfLeaves = tree.NumberOfLeaves;
            //Console.WriteLine("Number of nodes: " + numberOfnodes);
            //Console.WriteLine("Number of leaves: " + numberOfLeaves);


            ////var categoricalSplit = tree.CategoricalSplitFlags;



            //// Print tree



            //ImmutableArray<double> leafValues = (System.Collections.Immutable.ImmutableArray<double>)tree.LeafValues;

            //ImmutableArray<int> leftChild = (System.Collections.Immutable.ImmutableArray<int>)tree.LeftChild;
            //ImmutableArray<int> rightChild = (System.Collections.Immutable.ImmutableArray<int>)tree.RightChild;

            //ImmutableArray<int> numericalSplitFeatureIndexes = (System.Collections.Immutable.ImmutableArray<int>)tree.NumericalSplitFeatureIndexes;

            //ImmutableArray<float> numericalSplitThreshold = (System.Collections.Immutable.ImmutableArray<float>)tree.NumericalSplitThresholds;
            //ImmutableArray<bool> categoricalSplit = (System.Collections.Immutable.ImmutableArray<bool>)tree.CategoricalSplitFlags;

            //Console.WriteLine("Number of left children: " + leftChild.Length);
            //Console.WriteLine("Number of right children: " + rightChild.Length);


            //Console.WriteLine("");
            //Console.WriteLine("----------------------------------------------------------------");
            //Console.WriteLine("----------------------------------------------------------------");
            ////Final leaf are containing the prediction

            //int start = numericalSplitFeatureIndexes[0];

            //Console.WriteLine("START: "+start);

            //Console.WriteLine("");



            //for (int k = 0; k < leftChild.Length; k++)
            //{
            //    Console.WriteLine("index: "+k);
            //    Console.WriteLine("\tnumerical split: " + numericalSplitFeatureIndexes[k]);
            //    Console.WriteLine("\tnumerical threshold: " + numericalSplitThreshold[k]);
            //    Console.WriteLine("\tcategorical split: " + categoricalSplit[k]);
            //    Console.WriteLine("\tleaf value: " + leafValues[k]);

            //    Console.WriteLine("LEFT: "+leftChild[k]);
            //    if(leftChild[k] <0)
            //    {
            //        Console.WriteLine("\t leaf value: " + leafValues[~leftChild[k]]);
            //    }  
            //    Console.WriteLine("RIGHT: "+rightChild[k]);
            //    if (rightChild[k] < 0)
            //    {
            //        Console.WriteLine("\t leaf value: " + leafValues[~rightChild[k]]);
            //    }

            //    Console.WriteLine("");
            //    Console.WriteLine("");
            //    Console.WriteLine("");

            //}





            //String sqlCase = printPaths(0, tree,treeWeight,featureColumnNames);


            //}



            // print case;


            //String query1 = MLSQL.GenerateSQLNormalizeByVarianceColumns(featureColumnNames, selectParams, whereClause, avgs, stds, "", tablename);

            //String query2 = MLSQL.GenerateSQLRegressionTree(featureColumnNames, selectParams, "", tablename, whereClause, treeWeights, trees);


            //PrintSQLTransformation("Normalize by Mean and Variance", query1);
            //PrintSQLTransformation("Fast Tree", query2);

            ConsoleHelper.ConsoleWriteHeader("=============== End of training process ===============");


            /*string baseProjectPath = "/Users/paolosottovia/Documents/Repositories/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/";


            int[] modes = { 0, 2, 3 };
            int[] slices = { 1, 10, 100, 1000, 10000, 100000, 1000000 };
            string[] procedure_names = { "credit_card", "credit_card_no_output", "credit_card_db" };

            int n = 284806;
            for (int i = 0; i < modes.Length; i++)
            {
                //Console.WriteLine("PROCEDURE: " + procedure_names[i]);
                //string s = MLSQL.GenerateSQLProcededure(internalQuery2, "MLtoSQL", "credit_card_with_score_output", procedure_names[i], "Id", modes[i]);
                //string path = baseProjectPath + "SQL" + "/SQL_PROCEDURE_" + procedure_names[i] + ".sql";

                //MLSQL.WriteSQL(s, GetAbsolutePath(path));

                //for (int k = 0; k < slices.Length; k++)
                //{
                //    Console.WriteLine("call " + procedure_names[i] + "(" + n + ", " + slices[k] + ");");
                //}



            }*/


            //string[] Modes = { "CONSOLE", "NO_OUTPUT", "MYSQL", "CSV" };


            //string[] procedure_names_FastTree = { "credit_card", "credit_card_no_output", "credit_card_db" , "credit_card_csv" };

            for (int i = 0; i < Modes.Length; i++)
            {
                //string QQ = MLSQL.GenerateSQLQueriesOnDifferentModes(internalQuery2, "mltosql", "credit_card_with_score_output", procedure_names_FastTree[i], "Id", Modes[i], false);
                //string QQ1 = normalizeQueries["MYSQL"] + "\n\n\n" + queryList[i];
                //string QQ2 = normalizeQueries["SQLSERVER"] + "\n\n\n" + queryList[i];
                //string QQ = queryList[i];
                string QQ = insertNormalizeQuery  + "\n\n\n" + queryList[i];
                string MYSQLPATH = $"{SQLLocation}/MYSQL_PREDICTION_WITH_ID_" + procedure_names_FastTree[i] + ".sql";
                string SQLSERVERPATH = $"{SQLLocation}/SQLSERVER_PREDICTION_WITH_ID_" + procedure_names_FastTree[i] + ".sql";
                MLSQL.WriteSQL(QQ, GetAbsolutePath(MYSQLPATH));
                MLSQL.WriteSQL(QQ, GetAbsolutePath(SQLSERVERPATH));
            }        

            return (model, trainer.ToString());

        }


        private static void PrintSQLTransformation(String transformationName, String query)
        {
            Console.WriteLine("===========================" + transformationName + "===========================");
            Console.WriteLine("" + query);

            Console.WriteLine("============================================================================");
        }








        private static void EvaluateModel(MLContext mlContext, ITransformer model, IDataView testDataView, string trainerName)
        {
            // Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = model.Transform(testDataView);

            var metrics = mlContext.BinaryClassification.Evaluate(data: predictions,
                                                                  labelColumnName: nameof(TransactionObservation.Label),
                                                                  scoreColumnName: "Score");

            ConsoleHelper.PrintBinaryClassificationMetrics(trainerName, metrics);

        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        public static void InspectData(MLContext mlContext, IDataView data, int records)
        {
            //We want to make sure we have True and False observations

            Console.WriteLine("Show 4 fraud transactions (true)");
            ShowObservationsFilteredByLabel(mlContext, data, label: true, count: records);

            Console.WriteLine("Show 4 NOT-fraud transactions (false)");
            ShowObservationsFilteredByLabel(mlContext, data, label: false, count: records);
        }

        public static void ShowObservationsFilteredByLabel(MLContext mlContext, IDataView dataView, bool label = true, int count = 2)
        {
            // Convert to an enumerable of user-defined type. 
            var data = mlContext.Data.CreateEnumerable<TransactionObservation>(dataView, reuseRowObject: false)
                                            .Where(x => x.Label == label)
                                            // Take a couple values as an array.
                                            .Take(count)
                                            .ToList();

            // print to console
            data.ForEach(row => { row.PrintToConsole(); });
        }

        public static void UnZipDataSet(string zipDataSet, string destinationFile)
        {
            if (!File.Exists(destinationFile))
            {
                var destinationDirectory = Path.GetDirectoryName(destinationFile);
                ZipFile.ExtractToDirectory(zipDataSet, $"{destinationDirectory}");
            }
        }

        private static void SaveModel(MLContext mlContext, ITransformer model, string modelFilePath, DataViewSchema trainingDataSchema)
        {
            mlContext.Model.Save(model, trainingDataSchema, modelFilePath);

            Console.WriteLine("Saved model to " + modelFilePath);
        }
    }
}
