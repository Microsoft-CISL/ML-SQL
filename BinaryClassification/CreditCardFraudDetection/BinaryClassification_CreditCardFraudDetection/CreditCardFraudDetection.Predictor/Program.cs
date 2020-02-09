using CreditCardFraudDetection.Common;
using CreditCardFraudDetection.Trainer;
using ML2SQL;
using System;
using System.IO;

namespace CreditCardFraudDetection.Predictor
{
    class Program
    {

        private static string BaseDatasetsRelativePath = @"../../../../Data";

        static void Main(string[] args)
        {
            string assetsPath = GetAbsolutePath(@"../../../assets");
            string trainOutput = GetAbsolutePath(@"../../../../CreditCardFraudDetection.Trainer\assets\output");

            string baseProjectPath = "/Users/paolosottovia/Documents/Repositories/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/";

           

            string dataset = "";

            assetsPath = "/Users/paolosottovia/Downloads/machinelearning-samples-master/samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection/CreditCardFraudDetection.Predictor/assets";

            trainOutput = "/Users/paolosottovia/Downloads/machinelearning-samples-master/samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection/CreditCardFraudDetection.Trainer/assets/output";
            // string assetsPath = GetAbsolutePath(@"assets");
            //string trainOutput = GetAbsolutePath(@"../CreditCardFraudDetection.Trainer\assets\output");

            dataset = "/Users/paolosottovia/Downloads/machinelearning-samples-master/samples/csharp/getting-started/BinaryClassification_CreditCardFraudDetection/CreditCardFraudDetection.Trainer/assets/input";

            Console.WriteLine("assetsPath: "+assetsPath);
            Console.WriteLine("trainOutput: " + trainOutput);

            CopyModelAndDatasetFromTrainingProject(trainOutput, assetsPath);

            var inputDatasetForPredictions = Path.Combine(assetsPath,"input", "testData.csv");
            var modelFilePath = Path.Combine(assetsPath, "input", "fastTree.zip");

            var datasetFilePath = Path.Combine(dataset,"creditcard.csv");

            Console.WriteLine("datasetFilePath: "+datasetFilePath);

            int[] chunkSizes = { 10,100,1000,10000,100000,1000000};

            String dataSamplePath = baseProjectPath +"Data/Input/";
            MLSQL.createDataSample(datasetFilePath, dataSamplePath, chunkSizes, true);


           // Environment.Exit(0);

            var modelPredictor = new Predictor(modelFilePath, inputDatasetForPredictions, datasetFilePath);


            int[] modes = { 0,1,2,3 };
            foreach(int mode in modes) {

                modelPredictor.RunPredictionOverChuncks(dataSamplePath, chunkSizes, baseProjectPath +"Data/Output/", mode, true);
            }

           

            // Create model predictor to perform a few predictions
           //var modelPredictor = new Predictor(modelFilePath,inputDatasetForPredictions);

           //var modelPredictor = new Predictor(modelFilePath, inputDatasetForPredictions,datasetFilePath);

            // modelPredictor.RunMultiplePredictions(numberOfPredictions:5);


            string sqlPath = baseProjectPath +"SQL/SQL_insert_credit_card.sql";
            modelPredictor.RunAllPredictions(284806, "",sqlPath);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();

        }

        public static void CopyModelAndDatasetFromTrainingProject(string trainOutput, string assetsPath)
        {
            if (!File.Exists(Path.Combine(trainOutput, "testData.csv")) ||
                !File.Exists(Path.Combine(trainOutput, "fastTree.zip")))
            {
                Console.WriteLine("***** YOU NEED TO RUN THE TRAINING PROJECT IN THE FIRST PLACE *****");
                Console.WriteLine("=============== Press any key ===============");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // copy files from train output
            Directory.CreateDirectory(assetsPath);
            foreach (var file in Directory.GetFiles(trainOutput))
            {

                var fileDestination = Path.Combine(Path.Combine(assetsPath, "input"), Path.GetFileName(file));
                if (File.Exists(fileDestination))
                {
                    LocalConsoleHelper.DeleteAssets(fileDestination);
                }

                File.Copy(file, Path.Combine(Path.Combine(assetsPath, "input"), Path.GetFileName(file)));
            }

        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            Console.WriteLine("assemblyFolderPath: "+assemblyFolderPath);

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            Console.WriteLine("fullPath: " + fullPath);
            return fullPath;
        }
    }
}
