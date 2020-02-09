using CreditCardFraudDetection.Common.DataModels;
using Microsoft.ML;
using Microsoft.ML.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CreditCardFraudDetection.Predictor
{
    public class Predictor
    {
        private readonly string _modelfile;
        private readonly string _dasetFile;
        private readonly string _alldatasetFile;

        public Predictor(string modelfile, string dasetFile) {
            _modelfile = modelfile ?? throw new ArgumentNullException(nameof(modelfile));
            _dasetFile = dasetFile ?? throw new ArgumentNullException(nameof(dasetFile));
        }


        public Predictor(string modelfile, string dasetFile,string allDatasets)
        {
            _modelfile = modelfile ?? throw new ArgumentNullException(nameof(modelfile));
            _dasetFile = dasetFile ?? throw new ArgumentNullException(nameof(dasetFile));
            _alldatasetFile = allDatasets ?? throw new ArgumentNullException(nameof(allDatasets));
        }

        public void RunMultiplePredictions(int numberOfPredictions) {

            var mlContext = new MLContext();

            //Load data as input for predictions
            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<TransactionObservation>(_dasetFile, separatorChar: ',', hasHeader: true);

            Console.WriteLine($"Predictions from saved model:");

            ITransformer model = mlContext.Model.Load(_modelfile, out var inputSchema);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(model);
            Console.WriteLine($"\n \n Test {numberOfPredictions} transactions, from the test datasource, that should be predicted as fraud (true):");

            mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
                        .Where(x => x.Label == true)
                        .Take(numberOfPredictions)
                        .Select(testData => testData)
                        .ToList()
                        .ForEach(testData => 
                                    {
                                        Console.WriteLine($"--- Transaction ---");
                                        testData.PrintToConsole();
                                        predictionEngine.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });


             Console.WriteLine($"\n \n Test {numberOfPredictions} transactions, from the test datasource, that should NOT be predicted as fraud (false):");
             mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
                        .Where(x => x.Label == false)
                        .Take(numberOfPredictions)
                        .ToList()
                        .ForEach(testData =>
                                    {
                                        Console.WriteLine($"--- Transaction ---");
                                        testData.PrintToConsole();
                                        predictionEngine.Predict(testData).PrintToConsole();
                                        Console.WriteLine($"-------------------");
                                    });
        }


        public void RunPredictionOverChuncks(String inputFolderPath, int [] chuncks, String outputPath,int outputMode,bool singlePred)
        {
            Console.WriteLine("MODE: " + outputMode);

            switch (outputMode)
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
            foreach (int chunck in chuncks)

            {
                Console.WriteLine("Chunck size: "+chunck);
                string res = runSinglePrediction(inputFolderPath + chunck + "/",chunck, "", outputMode, singlePred);

                results.Add("Chunck size: " + chunck + "\t" + res);
            }


            switch (outputMode)
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
                Console.WriteLine(""+r);
            }


            Console.WriteLine("==============================================================================");
        }



        private string runSinglePrediction(String inputFolderPath, int chunck,String outputPath, int outputMode,bool singleMode)
        {

            // Read all files
           

            var watch = System.Diagnostics.Stopwatch.StartNew();

            string ConnectionString = "server=localhost;Uid=root;Pwd=19021990;Database=MLtoSQL";

            var mConnection = new MySqlConnection(ConnectionString);


            var mlContext = new MLContext();

            ITransformer model = mlContext.Model.Load(_modelfile, out var inputSchema);

            int i = 0;
           

            foreach (string file in Directory.EnumerateFiles(inputFolderPath, "*.csv"))
            {
                //string contents = File.ReadAllText(file);



               //Console.WriteLine("" + file);
                IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<TransactionObservation>(file, separatorChar: ',', hasHeader: true);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(model);

                using (System.IO.StreamWriter file1 = new System.IO.StreamWriter("output_"+chunck+"_"+i +".csv", false))
                {
                    List<TransactionObservation> transactions = mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false).Take(chunck).ToList();

                    if (singleMode)
                    {

                        string values = "";

                        transactions.ForEach
                    (testData =>
                    {

                        TransactionFraudPrediction t = predictionEngine.Predict(testData);
                        String line = generateLineCSV(testData, t);

                        if (outputMode == 0)
                        {
                            file1.WriteLine(generateINSERTINTOLine(testData, t) + "");

                        }
                        else if (outputMode == 1)
                        {
                            Console.WriteLine(testData.Id + "," + t.Score + "");

                        }
                        else if (outputMode == 2)
                        {
                        // DO NOTHING
                        }
                        else if (outputMode == 3)
                        {
                            // Insert into the database

                            string ll = "("  + testData.Id + "," + t.Score + "),";
                            //Console.WriteLine("LINE: "+ll);
                            values += ll;


                        }



                        



                    });
                        if (outputMode == 3)
                        {
                            values = values.Substring(0, values.Length - 1);
                            values += ";";
                            mConnection.Open();

                            string insert = "INSERT into credit_card_with_score_output (Id,Score ) VALUES ";
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

                        List<TransactionFraudPrediction> l = mlContext.Data.CreateEnumerable<TransactionFraudPrediction>(predictions, reuseRowObject: false).Take(chunck).ToList();

                        for (int j =0; j < l.Count;j++)
                        {
                            TransactionFraudPrediction t = l[j];
                            String line = generateLineCSV(transactions[j], t);

                            if (outputMode == 0)
                            {
                                file1.WriteLine(generateINSERTINTOLine(transactions[j], t) + "");

                            }
                            else if (outputMode == 1)
                            {
                                Console.WriteLine(generateINSERTINTOLine(transactions[j], t) + "");

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

            Console.WriteLine("Time needed: "+elapsedMs);

            return "Time needed: " + elapsedMs;


        }




        public void RunAllPredictions(int numberOfPredictions, string filePath,string sqlPath)
        {

            var mlContext = new MLContext();

            //Load data as input for predictions
            IDataView inputDataForPredictions = mlContext.Data.LoadFromTextFile<TransactionObservation>(_alldatasetFile, separatorChar: ',', hasHeader: true);




            Console.WriteLine($"Predictions all:");

            ITransformer model = mlContext.Model.Load(_modelfile, out var inputSchema);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(model);

            String header = generateHeader();




            Console.WriteLine("HEADER\n");

            Console.WriteLine("" + header);


            string insertIntoQuery = generateInsertIntoHeader("creditcard_with_prediction");


            int i = 0;


            var watch = System.Diagnostics.Stopwatch.StartNew();

            // Generate test data 

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(sqlPath, false))
            {
                file.WriteLine(insertIntoQuery);
           

            mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false).Take(numberOfPredictions).ToList().

                ForEach(testData => { 


                TransactionFraudPrediction t = predictionEngine.Predict(testData);


                    String line = generateLineCSV(testData, t);

                    //Console.WriteLine(""+line);

                    if(i % 1000 == 0)
                    {
                        //Console.WriteLine("Processed: "+i);
                    }

                    i += 1;

                    if (i != numberOfPredictions)
                    {
                        file.WriteLine(generateINSERTINTOLine(testData, t) + ",");
                    }
                    else
                    {
                        file.WriteLine(generateINSERTINTOLine(testData, t) +";");
                    }

                   //insertIntoQuery += generateINSERTINTOLine(testData,t) +",";




            });

            }
            watch.Stop();

            var elapsedMs = watch.ElapsedMilliseconds;



            Console.WriteLine("Time elapsed: "+ elapsedMs);


            var watch1 = System.Diagnostics.Stopwatch.StartNew();

            IDataView predictions = model.Transform(inputDataForPredictions);

            float[] scoreColumn = predictions.GetColumn<float>("Score").ToArray();



            watch1.Stop();

            var elapsedMs1 = watch1.ElapsedMilliseconds;

            Console.WriteLine("Time elapsed: " + elapsedMs1);

            insertIntoQuery = insertIntoQuery.Substring(0, insertIntoQuery.Length - 1);

            insertIntoQuery += ";";


            Console.WriteLine("Query for data insertion: ");


           // Console.WriteLine(""+insertIntoQuery);


          //  writeInsertionFileDB(sqlPath,insertIntoQuery);

            Console.WriteLine("Write insertion query! Path:  "+sqlPath);




            Console.WriteLine("");





            int[] modes = { 0, 2 };
            //int[] chuncks = {1000,10000,100000};
            int[] chuncks = {numberOfPredictions };
            // int[] chuncks = { 10 };


            foreach (int mode in modes)
            {

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
                    
                foreach (int chunck in chuncks)
                {
                    bool printProgress = false;
                    if (chunck == 10)
                    {
                        printProgress = true;
                    }

                    calculateTimeChunks(mlContext, model, inputDataForPredictions, numberOfPredictions, chunck, printProgress, mode);
                }

                Console.WriteLine("-------------------------------------------------------------------------\n\n");

            }

            //Console.WriteLine($"\n \n Test {numberOfPredictions} transactions, from the test datasource, that should be predicted as fraud (true):");

            //mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
            //            .Where(x => x.Label == true)
            //            .Take(numberOfPredictions)
            //            .Select(testData => testData)
            //            .ToList()
            //            .ForEach(testData =>
            //            {
            //                Console.WriteLine($"--- Transaction ---");
            //                testData.PrintToConsole();
            //                predictionEngine.Predict(testData).PrintToConsole();
            //                Console.WriteLine($"-------------------");
            //            });


            //Console.WriteLine($"\n \n Test {numberOfPredictions} transactions, from the test datasource, that should NOT be predicted as fraud (false):");
            //mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false)
            //.Where(x => x.Label == false)
            //.Take(numberOfPredictions)
            //.ToList()
            //.ForEach(testData =>
            //{
            //    Console.WriteLine($"--- Transaction ---");
            //    testData.PrintToConsole();
            //    predictionEngine.Predict(testData).PrintToConsole();
            //    Console.WriteLine($"-------------------");
            //});
        }



        private void calculateTimeSingle(MLContext mlContext, ITransformer model, IDataView inputDataForPredictions, int numberOfPredictions, int outputType,bool singleMode)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter("file.sql", false))
            {


                var predictionEngine = mlContext.Model.CreatePredictionEngine<TransactionObservation, TransactionFraudPrediction>(model);
                var watch1 = System.Diagnostics.Stopwatch.StartNew();
                int i = 0;
                List<TransactionObservation> list = mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false).Take(numberOfPredictions).ToList();

                if (singleMode)
                {


                    list.ForEach(testData =>
                    {


                        TransactionFraudPrediction t = predictionEngine.Predict(testData);


                        String line = generateLineCSV(testData, t);

                        //Console.WriteLine(""+line);

                        if (i % 1000 == 0)
                        {
                            //Console.WriteLine("Processed: "+i);

                        }

                        i += 1;

                        if (outputType == 0)
                        {

                            if (i != numberOfPredictions)
                            {
                                file.WriteLine(generateINSERTINTOLine(testData, t) + ",");
                            }
                            else
                            {
                                file.WriteLine(generateINSERTINTOLine(testData, t) + ";");
                            }

                        }
                        else if (outputType == 1)
                        {
                            if (i != numberOfPredictions)
                            {
                                Console.WriteLine(generateINSERTINTOLine(testData, t) + ",");
                            }
                            else
                            {
                                Console.WriteLine(generateINSERTINTOLine(testData, t) + ";");
                            }
                        }
                        else if (outputType == 2)
                        {

                            // DO NOTHING

                        }
                        else if (outputType == 3)
                        {

                            // Insert into the database

                        }
                        //insertIntoQuery += generateINSERTINTOLine(testData,t) +",";




                    });

                }
                else
                {
                   
                }

                watch1.Stop();

                var elapsedMs1 = watch1.ElapsedMilliseconds;



            }
        }


        private void calculateTimeChunks(MLContext mlContext, ITransformer model , IDataView inputDataForPredictions, int numberOfPredictions, int chunckSize,bool printProgress,int outputType)
        {
            List<TransactionObservation> c = mlContext.Data.CreateEnumerable<TransactionObservation>(inputDataForPredictions, reuseRowObject: false).Take(numberOfPredictions).ToList();



            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter("file1.sql", true))
            {
                int count = c.Count();

                //Console.WriteLine("Number of elements: " + count);
                int originalChunck = chunckSize;

                long time2 = 0;

                for (int k = 0; k < count; k = k + chunckSize)

                {
                    if (k + chunckSize > count)
                    {
                        chunckSize = count - k;
                    }

                    if (printProgress)
                    {
                        Console.WriteLine("k: " + k + " chunck size: " + chunckSize);
                    }

                    List<TransactionObservation> t = c.GetRange(k, chunckSize);

                    IDataView w = mlContext.Data.LoadFromEnumerable(t);


                    float[] scoreColumn2 = w.GetColumn<float>("V1").ToArray();

                    var watch2 = System.Diagnostics.Stopwatch.StartNew();

                    // IDataView predictionsC = model.Transform(w);
                    IDataView predictions = model.Transform(inputDataForPredictions);

                    float[] scoreColumn1 = predictions.GetColumn<float>("Score").ToArray();

                    List<TransactionFraudPrediction> l = mlContext.Data.CreateEnumerable<TransactionFraudPrediction>(predictions, reuseRowObject: false).Take(chunckSize).ToList();



                    if (outputType == 0)
                    {


                        for (int i = 0; i < l.Count; i++)
                        {
                            file.WriteLine(generateINSERTINTOLine(t[i], l[i]) + ",");
                        }

                        //file.WriteLine(generateINSERTINTOLine(testData, t) + ",");


                        //ile.WriteLine(generateINSERTINTOLine(testData, t) + ";");


                    }
                    else if (outputType == 1)
                    {

                        //Console.WriteLine(generateINSERTINTOLine(testData, t) + ",");

                        for (int i = 0; i < l.Count; i++)
                        {
                            Console.WriteLine(generateINSERTINTOLine(t[i], l[i]) );
                        }

                    }
                    else if (outputType == 2)
                    {

                        // DO NOTHING

                    }
                    else if (outputType == 3)
                    {

                        // Insert into the database

                    }







                    watch2.Stop();
                    long elapsedMs2 = watch2.ElapsedMilliseconds;
                    time2 = time2 + elapsedMs2;
                    //Console.WriteLine("");


                }


                Console.WriteLine("Chunck size: " + originalChunck + " Time elapsed: " + time2);

            }



           
        }

        private void writeInsertionFileDB(String path, string insert)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(path, false))
            {
             file.WriteLine(insert);
            }
        }


        private string generateInsertIntoHeader(String tablename)
        {
            string query = "INSERT INTO "+tablename+"  ( "+generateHeader() +" ) VALUES " ;

            return query;
        }


        private string generateHeader()
        {
            String line = "";

            line += "Tim" + "," +"V1" + "," + "V2" + "," +"V3" + "," + "V4" + "," + "V5"+ "," + "V6" + "," + "V7" + "," + "V8" + "," + "V9" + "," + "V10";


            line += "," + "V11" + "," + "V12" + "," + "V13" + "," + "V14" + "," + "V15" + "," + "V16" + "," + "V17" + "," + "V18" + "," + "V19" + "," + "V20";

            line += "," + "V21" + "," + "V22" + "," + "V23" + "," + "V24" + "," +"V25" + "," +"V26" + "," + "V27" + "," + "V28" + "," + "Amount" + "," + "Label" + "," +"Id" + ",";

            line += "PredictedLabel" + "," + "Probability" + "," + "Score";

            return line;

        }


        private string generateLineCSV(TransactionObservation observation, TransactionFraudPrediction prediction)
        {
            String line = "";


            line += observation.Time + "," + observation.V1 + "," + observation.V2 + "," + observation.V3 + "," + observation.V4 + "," + observation.V5 + "," + observation.V6 + "," + observation.V7 + "," + observation.V8 + "," + observation.V9 + "," + observation.V10;


            line += "," + observation.V11 + "," + observation.V12 + "," + observation.V13 + "," + observation.V14 + "," + observation.V15 + "," + observation.V16 + "," + observation.V17 + "," + observation.V18 + "," + observation.V19 + "," + observation.V20;

            line += "," + observation.V21 + "," + observation.V22 + "," + observation.V23 + "," + observation.V24 + "," + observation.V25 + "," + observation.V26 + "," + observation.V27 + "," + observation.V28 + "," + observation.Amount + "," + observation.Label + "," + observation.Id + ",";

            line += prediction.PredictedLabel + "," + prediction.Probability + "," + prediction.Score;

            line += "";

            return line;
        }


        private string generateINSERTINTOLine(TransactionObservation observation, TransactionFraudPrediction prediction)
        {
            String line = "(";


            line += observation.Time + "," + observation.V1 + "," + observation.V2 + "," + observation.V3 + "," + observation.V4 + "," + observation.V5 + "," + observation.V6 + "," + observation.V7 + "," + observation.V8 + "," + observation.V9 + "," + observation.V10;


            line += "," + observation.V11 + "," + observation.V12 + "," + observation.V13 + "," + observation.V14 + "," + observation.V15 + "," + observation.V16 + "," + observation.V17 + "," + observation.V18 + "," + observation.V19 + "," + observation.V20;

            line += "," + observation.V21 + "," + observation.V22 + "," + observation.V23 + "," + observation.V24 + "," + observation.V25 + "," + observation.V26 + "," + observation.V27 + "," + observation.V28 + "," + observation.Amount +","+observation.Label+","+observation.Id+",";

            line += prediction.PredictedLabel + "," + prediction.Probability + "," + prediction.Score;

            line += ")";

            return line;
        }

    }




}
