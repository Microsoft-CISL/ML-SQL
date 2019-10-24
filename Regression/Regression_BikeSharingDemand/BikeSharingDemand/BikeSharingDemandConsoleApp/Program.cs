using System;
using System.IO;
using BikeSharingDemand.DataStructures;

using Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ML2SQL;
using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
using MySql.Data.MySqlClient;
using Microsoft.ML.Data;

namespace BikeSharingDemand
{
    internal static class Program
    {
        private static string ModelsLocation = @"../../../../MLModels";

        private static string DatasetsLocation = @"../../../../Data";

        private static string SQLLocation = @"../../../../SQL";


        private static string TrainingDataRelativePath = $"{DatasetsLocation}/hour_train.csv";
        private static string TrainingDataWithIDRelativePath = $"{DatasetsLocation}/hour_train_with_id.csv";

        private static string TestDataRelativePath = $"{DatasetsLocation}/hour_test.csv";
        private static string TestDataRelativeWithIdPath = $"{DatasetsLocation}/hour_test_with_id.csv";


        private static string AllDataRelativeWithIdPath = $"{DatasetsLocation}/hour_all_with_id.csv";

        private static string TrainingDataLocation = GetAbsolutePath(TrainingDataRelativePath);
        private static string TestDataLocation = GetAbsolutePath(TestDataRelativePath);

        private static string MYSQL_INSERT_FastTree = $"{SQLLocation}/02_MYSQL_INSERT_FastTree.sql";
        private static string SQLSERVER_INSERT_FastTree = $"{SQLLocation}/02_SQLSERVER_INSERT_FastTree.sql";
        private static string MYSQL_INSERT_Lbfgs = $"{SQLLocation}/02_MYSQL_INSERT_Lbfgs.sql";
        private static string SQLSERVER_INSERT_Lbfgs = $"{SQLLocation}/02_SQLSERVER_INSERT_Lbfgs.sql";
        private static string MYSQL_INSERT_Sdca = $"{SQLLocation}/02_MYSQL_INSERT_Sdca.sql";
        private static string SQLSERVER_INSERT_Sdca = $"{SQLLocation}/02_SQLSERVER_INSERT_Sdca.sql";
        private static string MYSQL_INSERT_FastTreeTweedie = $"{SQLLocation}/02_MYSQL_INSERT_FastTreeTweedie.sql";
        private static string SQLSERVER_INSERT_FastTreeTweedie = $"{SQLLocation}/02_SQLSERVER_INSERT_FastTreeTweedie.sql";


        private static string RESULT_PATH_FastTree = "../../../ResultsBikeSharingFastTree.csv";
        private static string RESULT_PATH_FastTreeTweedie = "../../../ResultsBikeSharingFastTreeTweedie.csv";
        private static string RESULT_PATH_SDCA = "../../../ResultsBikeSharingSDCA.csv";
        private static string RESULT_PATH_LBFGS = "../../../ResultsBikeSharingLBFGS.csv";

        private static string SamplesInput = $"{DatasetsLocation}/Input/";
        private static string SamplesOutput = $"{DatasetsLocation}/Output/";

        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);

            char separator = ',';

            string whereClause = " \n where Id >= @id  and Id < ( @id + chuncksize ) ";

            string[] inputModes = { "CSV", "MYSQL", "SQLSERVER" };
            string[] outputModes = { "CSV", "CONSOLE", "NO_OUTPUT", "MYSQL", "SQLSERVER" };

            Tuple<string, string>[] configurations = {
            new Tuple<string,string> (inputModes[0],outputModes[0]) ,
            new Tuple<string,string> (inputModes[0],outputModes[1]),
            new Tuple<string,string> (inputModes[0],outputModes[2]),
            new Tuple<string,string> (inputModes[0],outputModes[3]),
            new Tuple<string,string> (inputModes[0],outputModes[4]),
            new Tuple<string,string> (inputModes[1],outputModes[3]),
            new Tuple<string,string> (inputModes[2],outputModes[4])
            };


            string outputQueryFastTree = "INSERT into bike_sharing_with_output (Id,Score ) VALUES ";
            string outputQueryFastTreeTweedie = "INSERT into bike_sharing_with_output (Id,Score ) VALUES ";
            string outputQuerySDCA = "INSERT into bike_sharing_with_output (Id,Score ) VALUES ";
            string outputQueryLBFGS = "INSERT into bike_sharing_with_output (Id,Score ) VALUES ";


            int numberOfTrainElements = MLSQL.insertUniqueId(TrainingDataRelativePath, TrainingDataWithIDRelativePath, true, ",", 0);

            Console.WriteLine("Number of train examples: " + numberOfTrainElements);

            int numberOfTestElements = MLSQL.insertUniqueId(TestDataRelativePath, TestDataRelativeWithIdPath, true, ",", numberOfTrainElements);

            Console.WriteLine("Number of test examples: " + numberOfTestElements);
            int numberOfElements = numberOfTestElements;
            Console.WriteLine("Number of elements: " + numberOfElements);

            MLSQL.JoinTrainAndTest(TrainingDataWithIDRelativePath, TestDataRelativeWithIdPath, AllDataRelativeWithIdPath, true);




            // 1. Common data loading configuration
            var trainingDataView = mlContext.Data.LoadFromTextFile<DemandObservation>(path: TrainingDataWithIDRelativePath, hasHeader: true, separatorChar: ',');
            var testDataView = mlContext.Data.LoadFromTextFile<DemandObservation>(path: TestDataRelativeWithIdPath, hasHeader: true, separatorChar: ',');



            int[] chunckSizes = { 1, 10, 100, 1000, 10000, 100000 };

            bool header = true;

          //  int[] chunckSizes = {1000, 10000, 100000 };


            MLSQL.createDataSample(AllDataRelativeWithIdPath, SamplesInput, chunckSizes, true);

            // 2. Common data pre-process with pipeline data transformations

            // Concatenate all the numeric columns into a single features column



            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features",
                                                     nameof(DemandObservation.Season), nameof(DemandObservation.Year), nameof(DemandObservation.Month),
                                                     nameof(DemandObservation.Hour), nameof(DemandObservation.Holiday), nameof(DemandObservation.Weekday),
                                                     nameof(DemandObservation.WorkingDay), nameof(DemandObservation.Weather), nameof(DemandObservation.Temperature),
                                                     nameof(DemandObservation.NormalizedTemperature), nameof(DemandObservation.Humidity), nameof(DemandObservation.Windspeed))
                                         .AppendCacheCheckpoint(mlContext);


            //var dataProcessPipeline = mlContext.Transforms.Concatenate("Features",
            //                                        nameof(DemandObservation.Season))
            //                            .AppendCacheCheckpoint(mlContext);

            // Use in-memory cache for small/medium datasets to lower training time. 
            // Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets.

            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            Common.ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 10);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 10);

            PredictorExecutor<DemandObservation, DemandPrediction> predictorExecutor = new PredictorExecutor<DemandObservation, DemandPrediction>();


            // Definition of regression trainers/algorithms to use
            //var regressionLearners = new (string name, IEstimator<ITransformer> value)[]
            (string name, IEstimator<ITransformer> value)[] regressionLearners =
            {
                ("FastTree", mlContext.Regression.Trainers.FastTree().AppendCacheCheckpoint(mlContext)),
                ("Poisson", mlContext.Regression.Trainers.LbfgsPoissonRegression().AppendCacheCheckpoint(mlContext)),
                ("SDCA", mlContext.Regression.Trainers.Sdca().AppendCacheCheckpoint(mlContext)),
                ("FastTreeTweedie", mlContext.Regression.Trainers.FastTreeTweedie().AppendCacheCheckpoint(mlContext)),
                //Other possible learners that could be included
                //...FastForestRegressor...
                //...GeneralizedAdditiveModelRegressor...
                //...OnlineGradientDescent... (Might need to normalize the features first)
            };


            // ================================================== FAST TREE ==================================================



            var t_model_1 = dataProcessPipeline.Append(mlContext.Regression.Trainers.FastTree()).AppendCacheCheckpoint(mlContext).Fit(trainingDataView);

            var m1 = t_model_1.LastTransformer.Model.TrainedTreeEnsemble;

            //GenerateSQLDataWithPrediction(mlContext,t_model_1
            //    ,AllDataRelativeWithIdPath, "bike_sharing_FastTree", SQL_INSERT_FastTree, numberOfElements - 1, false);



            string modelRelativeLocation_FastTree = $"{ModelsLocation}/FastTree_Model.zip";

            mlContext.Model.Save(t_model_1, trainingDataView.Schema, modelRelativeLocation_FastTree);






            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTree, AllDataRelativeWithIdPath, "bike_sharing_FastTree", "MLtoSQL", MYSQL_INSERT_FastTree, "MYSQL", numberOfElements - 1, false,',');
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTree, AllDataRelativeWithIdPath, "bike_sharing_FastTree", "mltosql", SQLSERVER_INSERT_FastTree, "SQLSERVER", numberOfElements - 1, false,',');

            predictorExecutor.executePredictions("BIKE_SHARING_FASTTREE", modelRelativeLocation_FastTree, SamplesInput, SamplesOutput, RESULT_PATH_FastTree, chunckSizes, configurations, "bike_sharing_FastTree", outputQueryFastTree, numberOfElements - 1, separator,header);

            string[] columns = { nameof(DemandObservation.Season), nameof(DemandObservation.Year), nameof(DemandObservation.Month),
                                                     nameof(DemandObservation.Hour), nameof(DemandObservation.Holiday), nameof(DemandObservation.Weekday),
                                                     nameof(DemandObservation.WorkingDay), nameof(DemandObservation.Weather), nameof(DemandObservation.Temperature),
                                                     nameof(DemandObservation.NormalizedTemperature), nameof(DemandObservation.Humidity), nameof(DemandObservation.Windspeed)};


            //string[] columns = { nameof(DemandObservation.Season)};




            Microsoft.ML.Trainers.FastTree.RegressionTreeEnsemble a = t_model_1.LastTransformer.Model.TrainedTreeEnsemble;
            List<RegressionTree> trees = (System.Collections.Generic.List<Microsoft.ML.Trainers.FastTree.RegressionTree>)a.Trees;
            List<double> treeWeights = (System.Collections.Generic.List<double>)a.TreeWeights;

            string[] selectParams = { "Id" };
            string query_FastTree = MLSQL.GenerateSQLRegressionTree(columns, selectParams, "SCORE", "", "bike_sharing_FastTree", whereClause, treeWeights, trees);

            Console.WriteLine("\n\n" + query_FastTree + "\n\n");

            string outputTablename = "bike_sharing_with_output";

            //string[] Modes = { "CSV", "CONSOLE", "MYSQL" };

            string[] Modes = { "CONSOLE", "NO_OUTPUT", "MYSQL", "CSV" };


            string[] procedure_names_FastTree = { "bike_sharing_fasttree", "bike_sharing_fasttree_no_output", "bike_sharing_fasttree_db" , "bike_sharing_fasttree_csv" };


            //            for (int i = 0; i < Modes.Length; i++)
            //            {
            //                Console.WriteLine("PROCEDURE: " + procedure_names_FastTree[i]);
            //                //string s = MLSQL.GenerateSQLProcededure(query_FastTree, "MLtoSQL", "bike_sharing_with_output", procedure_names_FastTree[i], "Id", modes_procedure[i], true);

            //                MLSQL.GenerateSQLQueriesOnDifferentModes(query_FastTree, "MLtoSQL", outputTablename, procedure_names_FastTree[i], "Id",


            //}
            for (int i = 0; i < Modes.Length; i++)
            {
                string QQ = MLSQL.GenerateSQLQueriesOnDifferentModes(query_FastTree, "mltosql", "bike_sharing_with_output", procedure_names_FastTree[i], "Id", Modes[i], false);
                string MYSQLPATH = $"{SQLLocation}/MYSQL_PREDICTION_WITH_ID_" + procedure_names_FastTree[i] + ".sql";
                string SQLSERVERPATH = $"{SQLLocation}/SQLSERVER_PREDICTION_WITH_ID_" + procedure_names_FastTree[i] + ".sql";
                MLSQL.WriteSQL(QQ, GetAbsolutePath(MYSQLPATH));
                MLSQL.WriteSQL(QQ, GetAbsolutePath(SQLSERVERPATH));
            }







            // ================================================== LBFGS POISSON REGRESSION ==================================================



            var t_model_2 = dataProcessPipeline.Append(mlContext.Regression.Trainers.LbfgsPoissonRegression()).AppendCacheCheckpoint(mlContext).Fit(trainingDataView);

            var m2 = t_model_2.LastTransformer.Model;

            string modelRelativeLocation_Lbfgs = $"{ModelsLocation}/Lbfgs_Model.zip";

            mlContext.Model.Save(t_model_2, trainingDataView.Schema, modelRelativeLocation_Lbfgs);

            //  GenerateSQLDataWithPrediction(mlContext, t_model_2, AllDataRelativeWithIdPath, "bike_sharing_Lbfgs", SQL_INSERT_Lbfgs, numberOfElements - 1, false);

            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTree, AllDataRelativeWithIdPath, "bike_sharing_Lbfgs", "MLtoSQL", MYSQL_INSERT_Lbfgs, "MYSQL", numberOfElements - 1, false,',');
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTree, AllDataRelativeWithIdPath, "bike_sharing_Lbfgs", "mltosql", SQLSERVER_INSERT_Lbfgs, "SQLSERVER", numberOfElements - 1, false,',');

            predictorExecutor.executePredictions("BIKE_SHARING_LBFGS", modelRelativeLocation_Lbfgs, SamplesInput, SamplesOutput, RESULT_PATH_LBFGS, chunckSizes, configurations, "bike_sharing_Lbfgs", outputQueryLBFGS, numberOfElements - 1, separator,header);





            int nw = m2.Weights.Count;


            float[] weights = new float[nw];

            for (int i = 0; i < nw; i++)
            {
                weights[i] = m2
                    .Weights[i];
            }

            float bias = m2.Bias;

            Console.WriteLine
                ("Number of weights: " + nw);

            string b1 = MLSQL.GenerateSQLMultiplyByLinearCombination(columns, selectParams, whereClause, weights
                , "", "bike_sharing_Lbfgs");

            string query_Lbfgs = MLSQL.GenerateSQLSumANDExpColumns(columns, selectParams, bias
                , "Score", "(" + b1 + ") as F ");


            Console.WriteLine("\n\n" + query_Lbfgs + "\n\n");

            string[] procedure_names_Lbfgs = { "bike_sharing_lbfgs", "bike_sharing_lbfgs_no_output", "bike_sharing_lbfgs_db", "bike_sharing_lbfgs_csv" };


            //for (int i = 0; i < modes_procedure.Length; i++)
            //{
            //    Console.WriteLine("PROCEDURE: " + procedure_names_FastTree[i]);
            //    string s = MLSQL.GenerateSQLProcededure(query_Lbfgs, "MLtoSQL", "bike_sharing_with_output", procedure_names_Lbfgs[i], "Id", modes_procedure[i], true);
            //    string path = $"{SQLLocation}/SQL_PROCEDURE_" + procedure_names_Lbfgs[i] + ".sql";
            //    MLSQL.WriteSQL(s, GetAbsolutePath(path));

            //    for (int k = 0; k < slices.Length; k++)
            //    {
            //        Console.WriteLine("call " + procedure_names_Lbfgs[i] + "(" + numberOfElements + ", " + slices[k] + ");");
            //    }

            for (int i = 0; i < Modes.Length; i++)
            {
                string QQ = MLSQL.GenerateSQLQueriesOnDifferentModes(query_Lbfgs, "mltosql", "bike_sharing_with_output", procedure_names_Lbfgs[i], "Id", Modes[i], false);
                string MYSQLPATH = $"{SQLLocation}/MYSQL_PREDICTION_WITH_ID_" + procedure_names_Lbfgs[i] + ".sql";
                string SQLSERVERPATH = $"{SQLLocation}/SQLSERVER_PREDICTION_WITH_ID_" + procedure_names_Lbfgs[i] + ".sql";
                MLSQL.WriteSQL(QQ, GetAbsolutePath(MYSQLPATH));
                MLSQL.WriteSQL(QQ, GetAbsolutePath(SQLSERVERPATH));
            }



            //}


            // ================================================== SDCA ==================================================



            var t_model_3 = dataProcessPipeline.Append(mlContext.Regression.Trainers.Sdca()).AppendCacheCheckpoint(mlContext).Fit(trainingDataView);

            var m3 = t_model_3.LastTransformer.Model;

            //GenerateSQLDataWithPrediction(mlContext, t_model_3, AllDataRelativeWithIdPath, "bike_sharing_Sdca", SQL_INSERT_Sdca, numberOfElements - 1, false);


            string modelRelativeLocation_Sdca = $"{ModelsLocation}/Sdca_Model.zip";

            mlContext.Model.Save(t_model_3, trainingDataView.Schema, modelRelativeLocation_Sdca);


            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_Sdca, AllDataRelativeWithIdPath, "bike_sharing_Sdca", "MLtoSQL", MYSQL_INSERT_Sdca, "MYSQL", numberOfElements - 1, false,',');
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_Sdca, AllDataRelativeWithIdPath, "bike_sharing_Sdca", "mltosql", SQLSERVER_INSERT_Sdca, "SQLSERVER", numberOfElements - 1, false,',');

            predictorExecutor.executePredictions("BIKE_SHARING_SDCA", modelRelativeLocation_Sdca, SamplesInput, SamplesOutput, RESULT_PATH_SDCA, chunckSizes, configurations, "bike_sharing_Sdca", outputQuerySDCA, numberOfElements - 1, separator,header);





            int nw1 = m3.Weights.Count;


            float[] weights1 = new float[nw1];

            for (int i = 0; i < nw; i++)
            {
                weights1[i] = m3
                    .Weights[i];
            }

            float bias1 = m3.Bias;


            string b1_1 = MLSQL.GenerateSQLMultiplyByLinearCombination(columns, selectParams, whereClause, weights1
                , "", "bike_sharing_Sdca");

            string query_Sdca = MLSQL.GenerateSQLSumColumns(columns, selectParams, bias1
                , "Score", "(" + b1_1 + ") as F ");


            Console.WriteLine("\n\n" + query_Sdca + "\n\n");




            string[] procedure_names_Sdca = { "bike_sharing_sdca", "bike_sharing_sdca_no_output", "bike_sharing_sdca_db", "bike_sharing_sdca_csv" };


            //for (int i = 0; i < modes_procedure.Length; i++)
            //{
            //    Console.WriteLine("PROCEDURE: " + procedure_names_FastTree[i]);
            //    string s = MLSQL.GenerateSQLProcededure(query_Sdca, "MLtoSQL", "bike_sharing_with_output", procedure_names_Sdca[i], "Id", modes_procedure[i], true);
            //    string path = $"{SQLLocation}/SQL_PROCEDURE_" + procedure_names_Sdca[i] + ".sql";
            //    MLSQL.WriteSQL(s, GetAbsolutePath(path));

            //    for (int k = 0; k < slices.Length; k++)
            //    {
            //        Console.WriteLine("call " + procedure_names_Sdca[i] + "(" + numberOfElements + ", " + slices[k] + ");");
            //    }



            //}

            for (int i = 0; i < Modes.Length; i++)
            {
                string QQ = MLSQL.GenerateSQLQueriesOnDifferentModes(query_Sdca, "mltosql", "bike_sharing_with_output", procedure_names_Sdca[i], "Id", Modes[i], false);
                string MYSQLPATH = $"{SQLLocation}/MYSQL_PREDICTION_WITH_ID_" + procedure_names_Sdca[i] + ".sql";
                string SQLSERVERPATH = $"{SQLLocation}/SQLSERVER_PREDICTION_WITH_ID_" + procedure_names_Sdca[i] + ".sql";
                MLSQL.WriteSQL(QQ, GetAbsolutePath(MYSQLPATH));
                MLSQL.WriteSQL(QQ, GetAbsolutePath(SQLSERVERPATH));
            }







            // ================================================== FAST TREE TWEEDIE ==================================================


            var t_model_4 = dataProcessPipeline.Append(mlContext.Regression.Trainers.FastTreeTweedie()).AppendCacheCheckpoint(mlContext).Fit(trainingDataView);

            var m4 = t_model_4.LastTransformer.Model.TrainedTreeEnsemble;

            // GenerateSQLDataWithPrediction(mlContext, t_model_4, AllDataRelativeWithIdPath, "bike_sharing_FastTreeTweedie", SQL_INSERT_FastTreeTweedie, numberOfElements - 1, false);

            string modelRelativeLocation_FastTreeTweedie = $"{ModelsLocation}/FastTreeTweedie_Model.zip";

            mlContext.Model.Save(t_model_4, trainingDataView.Schema, modelRelativeLocation_FastTreeTweedie);


            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTreeTweedie, AllDataRelativeWithIdPath, "bike_sharing_FastTreeTweedie", "MLtoSQL", MYSQL_INSERT_FastTreeTweedie, "MYSQL", numberOfElements - 1, false,',');
            predictorExecutor.GenerateSQLDataWithPrediction(mlContext, modelRelativeLocation_FastTreeTweedie, AllDataRelativeWithIdPath, "bike_sharing_FastTreeTweedie", "mltosql", SQLSERVER_INSERT_FastTreeTweedie, "SQLSERVER", numberOfElements - 1, false,',');

            predictorExecutor.executePredictions("BIKE_SHARING_FastTreeTweedie", modelRelativeLocation_FastTreeTweedie, SamplesInput, SamplesOutput, RESULT_PATH_FastTreeTweedie, chunckSizes, configurations, "bike_sharing_FastTreeTweedie", outputQueryFastTreeTweedie, numberOfElements - 1, separator,header);




            Microsoft.ML.Trainers.FastTree.RegressionTreeEnsemble a1 = t_model_4.LastTransformer.Model.TrainedTreeEnsemble;
            List<RegressionTree> trees1 = (System.Collections.Generic.List<Microsoft.ML.Trainers.FastTree.RegressionTree>)a1.Trees;
            List<double> treeWeights1 = (System.Collections.Generic.List<double>)a1.TreeWeights;


            string query_FastTreeTweedie = MLSQL.GenerateSQLFastTreeTweedie(columns, selectParams, "Score", "", "bike_sharing_FastTreeTweedie", whereClause, treeWeights1, trees1);



            Console.WriteLine("\n\n" + query_FastTreeTweedie + "\n\n");



            string[] procedure_names_FastTreeTweedie = { "bike_sharing_fasttreetweedie", "bike_sharing_fasttreetweedie_no_output", "bike_sharing_fasttreetweedie_db", "bike_sharing_fasttreetweedie_csv" };


            //for (int i = 0; i < modes_procedure.Length; i++)
            //{
            //    Console.WriteLine("PROCEDURE: " + procedure_names_FastTree[i]);
            //    string s = MLSQL.GenerateSQLProcededure(query_FastTreeTweedie, "MLtoSQL", "bike_sharing_with_output", procedure_names_FastTreeTweedie[i], "Id", modes_procedure[i], true);
            //    string path = $"{SQLLocation}/SQL_PROCEDURE_" + procedure_names_FastTreeTweedie[i] + ".sql";
            //    MLSQL.WriteSQL(s, GetAbsolutePath(path));

            //    for (int k = 0; k < slices.Length; k++)
            //    {
            //        Console.WriteLine("call " + procedure_names_FastTreeTweedie[i] + "(" + numberOfElements + ", " + slices[k] + ");");
            //    }



            //}

            for (int i = 0; i < Modes.Length; i++)
            {
                string QQ = MLSQL.GenerateSQLQueriesOnDifferentModes(query_FastTreeTweedie, "mltosql", "bike_sharing_with_output", procedure_names_FastTreeTweedie[i], "Id", Modes[i], false);
                string MYSQLPATH = $"{SQLLocation}/MYSQL_PREDICTION_WITH_ID_" + procedure_names_FastTreeTweedie[i] + ".sql";
                string SQLSERVERPATH = $"{SQLLocation}/SQLSERVER_PREDICTION_WITH_ID_" + procedure_names_FastTreeTweedie[i] + ".sql";
                MLSQL.WriteSQL(QQ, GetAbsolutePath(MYSQLPATH));
                MLSQL.WriteSQL(QQ, GetAbsolutePath(SQLSERVERPATH));
            }



            Console.WriteLine("\n\nEND");


            Environment.Exit(0);



            Console.WriteLine("\n\n\nExperiment on ML.NET\n\n");

            string[] modelPaths = { modelRelativeLocation_FastTree, modelRelativeLocation_Lbfgs, modelRelativeLocation_Sdca, modelRelativeLocation_FastTreeTweedie };



            Console.WriteLine("\n\n AFTER DATA GENERATION :) ");

            foreach (string modelPath in modelPaths
               )
            {
                int[] modes = { 3 };
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
                        Console.WriteLine("MODEL: " + modelPath);
                        Console.WriteLine("Chunck size: " + chunk);
                        string r = runSinglePredictionDB("bike_sharing_FastTreeTweedie", modelPath, chunk, SamplesOutput, mode, numberOfElements, true);
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

                    Console.WriteLine("MODEL: " + modelPath);
                    foreach (string r in results)
                    {
                        Console.WriteLine("" + r);
                    }


                    Console.WriteLine("==============================================================================");
                }


            }



            //Environment.Exit(0);







            foreach (string modelPath in modelPaths
                )
            {
                //int[] modes = { 0, 1, 2, 3 };
                int[] modes = { 3 };
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
                        Console.WriteLine("MODEL: " + modelPath);
                        Console.WriteLine("Chunck size: " + chunk);
                        string r = runSinglePrediction(SamplesInput + chunk + "/", modelPath, chunk, SamplesOutput, mode, true);
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

                    Console.WriteLine("MODEL: " + modelPath);
                    foreach (string r in results)
                    {
                        Console.WriteLine("" + r);
                    }


                    Console.WriteLine("==============================================================================");
                }


            }





            Environment.Exit(0);


            // t.LastTransformer.Model.TrainedTreeEnsemble.


            // 3. Phase for Training, Evaluation and model file persistence
            // Per each regression trainer: Train, Evaluate, and Save a different model
            foreach (var trainer in regressionLearners)
            {
                Console.WriteLine("=============== Training the current model ===============");
                var trainingPipeline = dataProcessPipeline.Append(trainer.value);
                var trainedModel = trainingPipeline.Fit(trainingDataView);


                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                IDataView predictions = trainedModel.Transform(testDataView);
                var metrics = mlContext.Regression.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");
                ConsoleHelper.PrintRegressionMetrics(trainer.value.ToString(), metrics);

                //Save the model file that can be used by any application
                string modelRelativeLocation = $"{ModelsLocation}/{trainer.name}Model.zip";
                string modelPath = GetAbsolutePath(modelRelativeLocation);
                mlContext.Model.Save(trainedModel, trainingDataView.Schema, modelPath);
                Console.WriteLine("The model is saved to {0}", modelPath);
            }

            // 4. Try/test Predictions with the created models
            // The following test predictions could be implemented/deployed in a different application (production apps)
            // that's why it is seggregated from the previous loop
            // For each trained model, test 10 predictions           
            foreach (var learner in regressionLearners)
            {
                //Load current model from .ZIP file
                string modelRelativeLocation = $"{ModelsLocation}/{learner.name}Model.zip";
                string modelPath = GetAbsolutePath(modelRelativeLocation);
                ITransformer trainedModel = mlContext.Model.Load(modelPath, out var modelInputSchema);




                // Create prediction engine related to the loaded trained model
                var predEngine = mlContext.Model.CreatePredictionEngine<DemandObservation, DemandPrediction>(trainedModel);

                Console.WriteLine($"================== Visualize/test 10 predictions for model {learner.name}Model.zip ==================");
                //Visualize 10 tests comparing prediction with actual/observed values from the test dataset
                ModelScoringTester.VisualizeSomePredictions(mlContext, learner.name, TestDataLocation, predEngine, 10);
            }

            Common.ConsoleHelper.ConsolePressAnyKey();
        }

        public static string GetAbsolutePath(string relativePath)
        {
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







                List<DemandObservation> inputs = new List<DemandObservation>();
                while (reader.Read())
                {

                    //reader.ge
                    //Console.WriteLine("r_"+ reader.ToString());
                    DemandObservation obs = new DemandObservation()
                    {
                        Season = reader.GetFloat("Season"),
                        Year = reader.GetFloat("Year"),
                        Month = reader.GetFloat("Month"),
                        Hour = reader.GetFloat("Hour"),
                        Holiday = reader.GetFloat("Holiday"),
                        Weekday = reader.GetFloat("Weekday"),
                        WorkingDay = reader.GetFloat("WorkingDay"),
                        Weather = reader.GetFloat("Weather"),
                        Temperature = reader.GetFloat("Temperature"),
                        NormalizedTemperature = reader.GetFloat("NormalizedTemperature"),
                        Humidity = reader.GetFloat("Humidity"),
                        Windspeed = reader.GetFloat("Windspeed"),
                        Id = reader.GetFloat("Id")
                    };


                    inputs.Add
                        (obs);



                }


                //Console.WriteLine
                //    ("Number of elements reads from database: "+ inputs.Count);

                IDataView inputDataForPredictions = mlContext.Data.LoadFromEnumerable(inputs);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<DemandObservation, DemandPrediction>(model);

                string outpath = outputPath;

                bool exists = System.IO.Directory.Exists(outpath);

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(outpath);
                }
                //Console.WriteLine("output path: " + outpath);
                using (System.IO.StreamWriter file1 = new System.IO.StreamWriter(outpath + "output_" + chunck + "_" + i + ".csv", false))
                {
                    List<DemandObservation> transactions = mlContext.Data.CreateEnumerable<DemandObservation>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();

                    if (singleMode)
                    {
                        string values = "";
                        StringBuilder sb = new StringBuilder();

                        transactions.ForEach
                        (testData =>
                        {

                            DemandPrediction t = predictionEngine.Predict(testData);
                            String line = MLSQL.generateLineCSV(testData.getData(separator), t.getData(separator), separator
                                );

                            if (outputMode == 0)
                            {
                                file1.WriteLine(MLSQL.generateINSERTINTOLine(testData.getSQLData(separator), t.getSQLData(separator), separator));

                            }
                            else if (outputMode == 1)
                            {
                                Console.WriteLine(testData.Id + "," + t.PredictedCount);

                            }
                            else if (outputMode == 2)
                            {
                                // DO NOTHING
                            }
                            else if (outputMode == 3)
                            {
                                // Insert into the database
                                string ll = "(" + testData.Id + "," + t.PredictedCount + "),";
                                //Console.WriteLine("LINE: "+ll);
                                //values += ll;
                                sb.Append(ll);


                            }



                        });

                        if (outputMode == 3)
                        {
                            mConnection.Close();

                            values = sb.ToString();
                            values = values.Substring(0, values.Length - 1);
                            values += ";";
                            mConnection.Open();

                            string insert = "INSERT into bike_sharing_with_output (Id,Score ) VALUES ";
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

                        List<DemandPrediction> l = mlContext.Data.CreateEnumerable<DemandPrediction>(predictions, reuseRowObject: false).Take(chunck).ToList();

                        for (int j = 0; j < l.Count; j++)
                        {
                            DemandPrediction t = l[j];
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





        private static string runSinglePrediction(String inputFolderPath, string ModelPath, int chunck, String outputPath, int outputMode, bool singleMode)
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
                IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<DemandObservation>(file, separatorChar: ',', hasHeader: true);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<DemandObservation, DemandPrediction>(model);

                string outpath = outputPath;

                bool exists = System.IO.Directory.Exists(outpath);

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(outpath);
                }
                //Console.WriteLine("output path: " + outpath);
                using (System.IO.StreamWriter file1 = new System.IO.StreamWriter(outpath + "output_" + chunck + "_" + i + ".csv", false))
                {
                    List<DemandObservation> transactions = mlContext.Data.CreateEnumerable<DemandObservation>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();

                    if (singleMode)
                    {
                        string values = "";

                        StringBuilder sb = new StringBuilder();

                        transactions.ForEach
                        (testData =>
                        {

                            DemandPrediction t = predictionEngine.Predict(testData);
                            String line = MLSQL.generateLineCSV(testData.getData(separator), t.getData(separator), separator
                                );

                            if (outputMode == 0)
                            {
                                file1.WriteLine(MLSQL.generateINSERTINTOLine(testData.getSQLData(separator), t.getSQLData(separator), separator));

                            }
                            else if (outputMode == 1)
                            {
                                Console.WriteLine(testData.Id + "," + t.PredictedCount);

                            }
                            else if (outputMode == 2)
                            {
                                // DO NOTHING
                            }
                            else if (outputMode == 3)
                            {
                                // Insert into the database
                                string ll = "(" + testData.Id + "," + t.PredictedCount + "),";
                                //Console.WriteLine("LINE: "+ll);
                                //values += ll;
                                sb.Append(ll);



                            }



                        });

                        if (outputMode == 3)
                        {
                            values = sb.ToString();
                            values = values.Substring(0, values.Length - 1);
                            values += ";";
                            mConnection.Open();

                            string insert = "INSERT into bike_sharing_with_output (Id,Score ) VALUES ";
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

                        List<DemandPrediction> l = mlContext.Data.CreateEnumerable<DemandPrediction>(predictions, reuseRowObject: false).Take(chunck).ToList();

                        for (int j = 0; j < l.Count; j++)
                        {
                            DemandPrediction t = l[j];
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

        private static string GenerateSQLDataWithPrediction(MLContext mlContext, ITransformer trainedModel, string _alldatasetFile, string tablename, string sqlPath, int n, bool debug)
        {




            // Create prediction engine related to the loaded trained model
            var predictionEngine = mlContext.Model.CreatePredictionEngine<DemandObservation, DemandPrediction>(trainedModel);

            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<DemandObservation>(_alldatasetFile, separatorChar: ',', hasHeader: true);

            DemandObservation d = new DemandObservation();

            DemandPrediction p = new DemandPrediction();

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

                mlContext.Data.CreateEnumerable<DemandObservation>(inputDataForPredictions, reuseRowObject: false).Take(n).ToList().

                ForEach(testData =>
                {


                    DemandPrediction t = predictionEngine.Predict(testData);


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


    }
}
