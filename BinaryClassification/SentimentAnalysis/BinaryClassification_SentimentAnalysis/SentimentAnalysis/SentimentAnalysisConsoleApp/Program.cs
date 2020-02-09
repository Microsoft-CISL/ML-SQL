using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common;
using CreditCardFraudDetection.Trainer;
using Microsoft.ML;
using Microsoft.ML.Data;
using MySql.Data.MySqlClient;
using SentimentAnalysisConsoleApp.DataStructures;
using static Microsoft.ML.DataOperationsCatalog;

namespace SentimentAnalysisConsoleApp
{
    internal static class Program
    {
        private static readonly string BaseDatasetsRelativePath = @"../../../../Data";
        private static readonly string BaseSQLRelativePath = @"../../../../SQL";
        private static readonly string BaseDatasetsRelativePathSamples = @"../../../../Data/Samples";
        private static string SamplesInput = $"{BaseDatasetsRelativePathSamples}/Input/";
        private static string SamplesOutput = $"{BaseDatasetsRelativePathSamples}/Output/";
        private static readonly string DataRelativePath = $"{BaseDatasetsRelativePath}/wikiDetoxAnnotated40kRows.tsv";

        private static readonly string SQL_INSERT_RelativePath = $"{BaseSQLRelativePath}/insert.sql";
        private static readonly string SQL_NUMBERS_RelativePath = $"{BaseSQLRelativePath}/insert_numbers.sql";
        private static readonly string SQL_WEIGHTS_RelativePath = $"{BaseSQLRelativePath}/insert_weights.sql";
        private static readonly string DataWithIdRelativePath = $"{BaseDatasetsRelativePath}/wikiDetoxAnnotated40kRows_with_id.tsv";

        private static readonly string DataPath = GetAbsolutePath(DataRelativePath);

        private static readonly string BaseModelsRelativePath = @"../../../../MLModels";
        private static readonly string ModelRelativePath = $"{BaseModelsRelativePath}/SentimentModel.zip";

        private static readonly string ModelPath = GetAbsolutePath(ModelRelativePath);

        static void Main(string[] args)
        {

            int numberOfElements =MLSQL.insertUniqueId(DataPath, DataWithIdRelativePath, true,"\t",0);

            MLSQL.GenerateNumbersTable(SQL_NUMBERS_RelativePath);
            #region try
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 1);

            #region step1to3
            // STEP 1: Common data loading configuration
            IDataView dataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(DataWithIdRelativePath, hasHeader: true);

            TrainTestData trainTestSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            IDataView trainingData = trainTestSplit.TrainSet;
            IDataView testData = trainTestSplit.TestSet;

            // STEP 2: Common data process configuration with pipeline data transformations          
            var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentIssue.Text));

            // STEP 3: Set the training algorithm, then create and config the modelBuilder                            
            var trainer = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features").AppendCacheCheckpoint(mlContext);
            var trainingPipeline = dataProcessPipeline.Append(trainer);
            #endregion

            #region step4
            // STEP 4: Train the model fitting to the DataSet
            var trainedModel = trainingPipeline.Fit(trainingData);


            var modelParams = trainedModel.LastTransformer.LastTransformer.Model.SubModel.Weights.Count;
            float bias = trainedModel.LastTransformer.LastTransformer.Model.SubModel.Bias;
            Console.WriteLine("Model Params: " + modelParams);
            Console.WriteLine("Model to String: " + trainedModel.LastTransformer.LastTransformer.ToString());
            Console.WriteLine("Model bias: " + bias);

            var transformedDataView = trainedModel.Transform(trainingData);
            VBuffer<ReadOnlyMemory<char>> slotNames = default;
            transformedDataView.Schema["Features"].GetSlotNames(ref slotNames);
            var NgramFeaturesColumn = transformedDataView.GetColumn<VBuffer<Single>>(transformedDataView.Schema["Features"]);

            //var NgramFeaturesColumn1 = transformedDataView.GetColumn<VBuffer<double>>(transformedDataView.Schema["Features"]);
            var slots = slotNames.GetValues();
            Console.WriteLine("Features: ");
            int n = 0;
            //int k = 0;
            //foreach (var featureRow in NgramFeaturesColumn)
            //{
            //    foreach (var item in featureRow.Items())
            //    {
            //        Console.WriteLine($"{slots[item.Key]}  ");
            //        n++;
            //    }
            //    k++;
            //    Console.WriteLine();
            //}


            //parseSlotNames (Features label)

            var mapFeaturesLabel = new Dictionary<int, string>();
            var mapFeaturesWeights = new Dictionary<int, double>();




            var weights = trainedModel.LastTransformer.LastTransformer.Model.SubModel.Weights;
            for (int i = 0; i < weights.Count; i++)
            {
                double weight = weights[i];

                mapFeaturesWeights.Add(i, weight);
                Console.WriteLine("i: "+i +" \tweight: "+weight);
            }



           


            for (int i = 0; i < slots.Length; i++)
            {

                Console.WriteLine("i: " + i + $" {slots[i]}  ");

                string featureContent = slots[i].ToString();


                if (featureContent.Contains("Char."))
                {
                    featureContent = featureContent.Replace("Char.", "");
                    if (featureContent.Contains("|"))
                    {
                        string[] ngrams = featureContent.Split("|");
                        string feat = "";
                        foreach (var ngram in ngrams)
                        {
                            //Console.WriteLine("ngram: " + ngram);
                            feat += ngram;
                        }

                        //Console.WriteLine("FINAL: " + feat);
                        featureContent = "t." + feat;

                    }
                    else
                    {
                        Console.WriteLine("Error....");
                        System.Environment.Exit(0);
                    }

                }
                else if (featureContent.Contains("Word."))
                {
                    featureContent = "w." + featureContent.Replace("Word.", "");
                }



                mapFeaturesLabel.Add(i, featureContent);
                //Console.WriteLine("FEATURE: " + featureContent);

                //System.Environment.Exit(0);
            }



            //generate weight table

            string query = "";

            string tablename_weights = "weights_sentiment_analysis";


            Console.WriteLine("BEFORE COMPOSE INSERT QUERY");

            String insertinto = "INSERT INTO " + tablename_weights + " VALUES ";
            String encoding = "set names utf8;";
            int count = mapFeaturesLabel.Keys.Count();

            string docPath = "";

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, SQL_WEIGHTS_RelativePath), false))
            {

            query += encoding;
            query += insertinto;

                outputFile.WriteLine(query);
            int featureCount = 0;
            foreach (var entry in mapFeaturesLabel)
            {
                int key = entry.Key;
                string label = entry.Value;
                double weight = mapFeaturesWeights[key];

                String line = "('" + Regex.Replace(label, @"\p{Cs}", "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'") + "'," + weight + ")" + ",";
                //Console.WriteLine("Line: " + line);
                //query += line;

                    if ((featureCount +1 )== count)
                    {
                        Console.WriteLine("Last element: " + featureCount + 1);
                        line =line.Substring(0, line.Length - 1);
                        line += ";";
                    }
                    outputFile.WriteLine(line);
                featureCount++;
            }

            Console.WriteLine("AFTER COMPOSE INSERT QUERY");

           // query = query.Substring(0, query.Length - 2);
           // query += ";";

           

            // Append text to an existing file named "WriteLines.txt".
            
              //  outputFile.WriteLine(query);
            }

            //Environment.Exit(0);


            #endregion

            #region step5
            // STEP 5: Evaluate the model and show accuracy stats
            var predictions = trainedModel.Transform(testData);

            var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");
            #endregion

           ConsoleHelper.PrintBinaryClassificationMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel, trainingData.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);

            // TRY IT: Make a single test prediction loding the model from .ZIP file
            SentimentIssue sampleStatement = new SentimentIssue { Text = "This is a very rude movie" };

            #region consume
            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(trainedModel);





            // Score
            var resultprediction = predEngine.Predict(sampleStatement);
            #endregion

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Text: {sampleStatement.Text} | Prediction: {(Convert.ToBoolean(resultprediction.Prediction) ? "Toxic" : "Non Toxic")} sentiment | Probability of being toxic: {resultprediction.Probability} ");
            Console.WriteLine($"================End of Process.Hit any key to exit==================================");
          //  Console.ReadLine();
            #endregion



            int[] chunckSizes = { 1, 10, 100, 1000,10000,100000 };

            MLSQL.createDataSample(DataWithIdRelativePath, SamplesInput,chunckSizes, true);


            int[] modes = { 0, 1,2,3 };
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
                    string  r = runSinglePrediction(SamplesInput + chunk + "/", chunk, SamplesOutput, mode, true);
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



            GenerateSQLDataWithPrediction(mlContext, DataWithIdRelativePath, "sentiment_analysis_detection_with_score", SQL_INSERT_RelativePath, numberOfElements,false);

            int[] Modes = { 0, 2, 3 };
            
            string[] procedure_names = { "sentiment", "sentiment_no_output", "sentiment_db" };

            string[] selectParams = { };

            string q = MLSQL.GenerateSQLFeatureTextWithChunkSize("", selectParams, "Id", "Comment", "", "sentiment_analysis_detection_with_score", tablename_weights, bias);


            Console.WriteLine("Query: " + q);


           // Environment.Exit(0);




          //  string q = "";



            
            for (int i = 0; i < Modes.Length; i++)
            {
                Console.WriteLine("PROCEDURE: " + procedure_names[i]);
                string s = MLSQL.GenerateSQLProcededure(q, "MLtoSQL", "sentiment_analysis_detection_with_score_output", procedure_names[i], "Id", Modes[i],false);
                string path = $"{BaseSQLRelativePath}/SQL_PROCEDURE_" + procedure_names[i] + ".sql";
                MLSQL.WriteSQL(s, GetAbsolutePath(path));

                for (int l = 0; l < chunckSizes.Length; l++)
                {
                    Console.WriteLine("call " + procedure_names[i] + "(" + numberOfElements + ", " + chunckSizes[l] + ");");
                }



            }

        }


        private static string GenerateSQLDataWithPrediction(MLContext mlContext, string _alldatasetFile,string tablename, string sqlPath, int n,bool debug)
        {



            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predictionEngine = mlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(trainedModel);

            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<SentimentIssue>(_alldatasetFile, separatorChar: '\t', hasHeader: true);

            SentimentIssue d = new SentimentIssue();

            SentimentPrediction p = new SentimentPrediction();

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

                mlContext.Data.CreateEnumerable<SentimentIssue>(inputDataForPredictions, reuseRowObject: false).Take(n).ToList().

                ForEach(testData =>
                {


                    SentimentPrediction t = predictionEngine.Predict(testData);


                    //String line = generateLineCSV(testData, t);
                    //HeRE;

                    string q = MLSQL.generateINSERTINTOLine(testData.getSQLData(separator), t.getSQLData(separator), separator) + ",";
                    if (debug)
                    {
                        Console.WriteLine("" + q + "\n");
                    }
                    //sqlQuery += q;



                    if((k+1) == n)
                    {
                        q = q.Substring(0, q.Length - 1);
                        q += ";";


                    }
                    file.WriteLine(q);





                    k++;
                });


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
                IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<SentimentIssue>(file, separatorChar: '\t', hasHeader: true);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(model);

                string outpath = outputPath;

                bool exists = System.IO.Directory.Exists(outpath);

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(outpath);
                }
                //Console.WriteLine("output path: " + outpath);
                using (System.IO.StreamWriter file1 = new System.IO.StreamWriter(outpath + "output_" + chunck + "_" + i + ".csv", false))
                {
                    List<SentimentIssue> transactions = mlContext.Data.CreateEnumerable<SentimentIssue>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();

                    if (singleMode)
                    {
                        string values = "";

                        transactions.ForEach
                        (testData =>
                        {

                            SentimentPrediction t = predictionEngine.Predict(testData);
                            String line = MLSQL.generateLineCSV(testData.getData(separator), t.getData(separator), separator
                                );

                            if (outputMode == 0)
                            {
                                file1.WriteLine(MLSQL.generateINSERTINTOLine(testData.getSQLData(separator), t.getSQLData(separator), separator));

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
                                values += ll;



                            }



                        });

                        if(outputMode == 3)
                        {
                            values = values.Substring(0, values.Length - 1);
                            values += ";";
                            mConnection.Open();

                            string insert = "INSERT into sentiment_with_score_output (Id,Score ) VALUES ";
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

                        List<SentimentPrediction> l = mlContext.Data.CreateEnumerable<SentimentPrediction>(predictions, reuseRowObject: false).Take(chunck).ToList();

                        for (int j = 0; j < l.Count; j++)
                        {
                            SentimentPrediction t = l[j];
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


        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath , relativePath);

            return fullPath;
        }







    }
}