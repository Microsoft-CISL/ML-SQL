using System;
namespace QueryExecutor
{
    public class Configurations
    {
        internal static readonly Configuration heart_disease_pc = new Configuration()
        {
            
            MYSQL_PATH__QUERY_CONSOLE = "/Users/paolosottovia/Documents/Repositories/mlsql/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart.sql",
            MYSQL_PATH__QUERY_CSV = "/Users/paolosottovia/Documents/Repositories/mlsql/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_csv.sql",
            MYSQL_PATH__QUERY_DB = "/Users/paolosottovia/Documents/Repositories/mlsql/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/Users/paolosottovia/Documents/Repositories/mlsql/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/Users/paolosottovia/Documents/Repositories/mlsql/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/SQL_scripts/",
            SQLSERVER_PATH__QUERY_CSV = "",
            SQLSERVER_PATH__QUERY_DB = "",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "",
            BATCH_SIZE = new int []{ 1, 10, 100, 1000 },
            N_ELEMENTS = 301,
            DBMSS = new string[] { "MYSQL"},
            NAME = "HEART_DISEASE",
            EXECUTION_TIME_SECONDS = 30
        };

        internal static readonly Configuration heart_disease_server = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart_no_output.sql",
            BATCH_SIZE = new int[] { 1, 10, 100, 1000 },
            //BATCH_SIZE = new int[] { 1000 },
            N_ELEMENTS = 301,
            DBMSS = new string[] {"MYSQL", "SQLSERVER" },
            NAME = "HEART_DISEASE",
            EXECUTION_TIME_SECONDS = 300,
            RESULT_FILE_NAME = "RESULT_HEART_DISEASE.csv"
           
        };


        internal static readonly Configuration heart_disease_server_index_id = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart_no_output.sql",
            BATCH_SIZE = new int[] { 1, 10, 100, 1000 },
            //BATCH_SIZE = new int[] { 1000 },
            N_ELEMENTS = 301,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "HEART_DISEASE_INDEX_ID",
            EXECUTION_TIME_SECONDS = 300,
            RESULT_FILE_NAME = "RESULT_HEART_DISEASE_INDEX_ID.csv"

        };


        internal static readonly Configuration heart_disease_server_index_all = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/MYSQL_PREDICTION_WITH_ID_heart_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/BinaryClassification/HeartDiseaseDetection/BinaryClassification_HeartDiseaseDetection/HeartDiseaseDetection/SQL_scripts/SQLSERVER_PREDICTION_WITH_ID_heart_no_output.sql",
            BATCH_SIZE = new int[] { 1, 10, 100, 1000 },
            //BATCH_SIZE = new int[] { 1000 },
            N_ELEMENTS = 301,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "HEART_DISEASE_INDEX_ALL",
            EXECUTION_TIME_SECONDS = 300,
            RESULT_FILE_NAME = "RESULT_HEART_DISEASE_INDEX_ALL.csv"

        };


        internal static readonly Configuration taxi_fare_server = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare_no_output.sql",
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000, 1000000 },
            //BATCH_SIZE = new int[] {  100000 },
            N_ELEMENTS = 100199,
           // DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            DBMSS = new string[] { "MYSQL" },
            NAME = "TAXI_FARE",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_TAXI_FARE.csv"

        };


        internal static readonly Configuration taxi_fare_server_index_id = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare_no_output.sql",
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000, 1000000 },
            //BATCH_SIZE = new int[] {  100000 },
            N_ELEMENTS = 100199,
            // DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            DBMSS = new string[] { "MYSQL" },
            NAME = "TAXI_FARE_INDEX_ID",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_TAXI_FARE_INDEX_ID.csv"

        };


        internal static readonly Configuration taxi_fare_server_index_all = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/MYSQL_PREDICTION_WITH_ID_taxi_fare_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_TaxiFarePrediction/TaxiFarePrediction/SQL/SQLSERVER_PREDICTION_WITH_ID_taxi_fare_no_output.sql",
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000, 1000000 },
            //BATCH_SIZE = new int[] {  100000 },
            N_ELEMENTS = 100199,
            // DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            DBMSS = new string[] { "MYSQL" },
            NAME = "TAXI_FARE_INDEX_ALL",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_TAXI_FARE_INDEX_ALL.csv"

        };




        internal static readonly Configuration bike_sharing_fasttree = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree_no_output.sql",
            //BATCH_SIZE = new int[] {  10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
             N_ELEMENTS = 17379,
            //N_ELEMENTS = 10,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_FASTTREE",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_FASTTREE.csv"

        };



        internal static readonly Configuration bike_sharing_fasttree_index_id = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree_no_output.sql",
            //BATCH_SIZE = new int[] {  10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_FASTTREE_INDEX_ID",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_FASTTREE_INDEX_ID.csv"

        };


        internal static readonly Configuration bike_sharing_fasttree_index_all = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttree_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttree_no_output.sql",
            //BATCH_SIZE = new int[] {  10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_FASTTREE_INDEX_ALL",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_FASTTREE_INDEX_ALL.csv"

        };


        internal static readonly Configuration bike_sharing_fasttree_tweedie = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_no_output.sql",
            //BATCH_SIZE = new int[] {  10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
             N_ELEMENTS = 17379,
           // N_ELEMENTS = 10,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_FASTTREE_TWEEDIE",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_FASTTREE_TWEEDIE.csv"

        };


        internal static readonly Configuration bike_sharing_fasttree_tweedie_index_id = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_no_output.sql",
            //BATCH_SIZE = new int[] {  10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_FASTTREE_TWEEDIE_INDEX_ID",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_FASTTREE_TWEEDIE_INDEX_ID.csv"

        };

        internal static readonly Configuration bike_sharing_fasttree_tweedie_index_all = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_fasttreetweedie_no_output.sql",
            //BATCH_SIZE = new int[] {  10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_FASTTREE_TWEEDIE_INDEX_ALL",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_FASTTREE_TWEEDIE_INDEX_ALL.csv"

        };



        internal static readonly Configuration bike_sharing_sdca = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca_no_output.sql",
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            //N_ELEMENTS = 10,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_SDCA",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_SDCA.csv"

        };

        internal static readonly Configuration bike_sharing_sdca_index_id = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca_no_output.sql",
            BATCH_SIZE = new int[] { 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_SDCA_INDEX_ID",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_SDCA_INDEX_ID.csv"

        };


        internal static readonly Configuration bike_sharing_sdca_index_all = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_sdca_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_sdca_no_output.sql",
            BATCH_SIZE = new int[] { 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_SDCA_INDEX_ALL",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_SDCA_INDEX_ALL.csv"

        };

        internal static readonly Configuration bike_sharing_lbfgs = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs_no_output.sql",
            // BATCH_SIZE = new int[] {  10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            //N_ELEMENTS = 10,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_LBFGS",
            EXECUTION_TIME_SECONDS = 5,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_LBFGS.csv"

        };


        internal static readonly Configuration bike_sharing_lbfgs_index_id = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs_no_output.sql",
            // BATCH_SIZE = new int[] {  10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_LBFGS_INDEX_ID",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_LBFGS_INDEX_ID.csv"

        };

        internal static readonly Configuration bike_sharing_lbfgs_index_all = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs.sql",
            MYSQL_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/MYSQL_PREDICTION_WITH_ID_bike_sharing_lbfgs_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/hummingbird/Experiments/Regression/Regression_BikeSharingDemand/BikeSharingDemand/SQL/SQLSERVER_PREDICTION_WITH_ID_bike_sharing_lbfgs_no_output.sql",
            // BATCH_SIZE = new int[] {  10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100000 },
            N_ELEMENTS = 17379,
            DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            NAME = "BIKE_SHARING_LBFGS_INDEX_ALL",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_BIKE_SHARING_LBFGS_INDEX_ALL.csv"

        };


        internal static readonly Configuration credit_card_sample = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_PREDICTION_WITH_ID_credit_card.sql",
            MYSQL_PATH__QUERY_CSV = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_PREDICTION_WITH_ID_credit_card_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_PREDICTION_WITH_ID_credit_card_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_PREDICTION_WITH_ID_credit_card_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_PREDICTION_WITH_ID_credit_card.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_PREDICTION_WITH_ID_credit_card_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_PREDICTION_WITH_ID_credit_card_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_PREDICTION_WITH_ID_credit_card_no_output.sql",
            MYSQL_PREPROCESS_QUERY = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_CREDIT_CARD_PREPROCESSING.sql",
            SQLSERVER_PREPROCESS_QUERY = "/home/matteo/PycharmProjects/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_CREDIT_CARD_PREPROCESSING.sql",
            //BATCH_SIZE = new int[] {  100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 100 },
            N_ELEMENTS = 1000,
            //DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            DBMSS = new string[] { "MYSQL"},
            NAME = "CREDIT_CARD",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_CREDIT_CARD.csv"

        };


        internal static readonly Configuration credit_card = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_PREDICTION_WITH_ID_credit_card.sql",
            MYSQL_PATH__QUERY_CSV = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_PREDICTION_WITH_ID_credit_card_csv.sql",
            MYSQL_PATH__QUERY_DB = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_PREDICTION_WITH_ID_credit_card_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_PREDICTION_WITH_ID_credit_card_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_PREDICTION_WITH_ID_credit_card.sql",
            SQLSERVER_PATH__QUERY_CSV = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_PREDICTION_WITH_ID_credit_card_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_PREDICTION_WITH_ID_credit_card_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_PREDICTION_WITH_ID_credit_card_no_output.sql",
            MYSQL_PREPROCESS_QUERY = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/MYSQL_CREDIT_CARD_PREPROCESSING.sql",
            SQLSERVER_PREPROCESS_QUERY = "/mnt/repo/mlsql/BinaryClassification/CreditCardFraudDetection/BinaryClassification_CreditCardFraudDetection/SQL/SQLSERVER_CREDIT_CARD_PREPROCESSING.sql",
            BATCH_SIZE = new int[] {  100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 100 },
            N_ELEMENTS = 284806,
            //DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            DBMSS = new string[] { "MYSQL"},
            NAME = "CREDIT_CARD",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_CREDIT_CARD.csv"

        };


        internal static readonly Configuration flight_delay_sample = new Configuration()
        {

            MYSQL_PATH__QUERY_CONSOLE = "/home/matteo/PycharmProjects/mlsql/Regression/Regression_FlightDelay/FlightDelay/SQL/MYSQL_PREDICTION_WITH_ID_flight_delay_fasttree.sql",
            MYSQL_PATH__QUERY_CSV = "/home/matteo/PycharmProjects/mlsql/Regression/Regression_FlightDelay/FlightDelay/SQL/MYSQL_PREDICTION_WITH_ID_flight_delay_fasttree_csv.sql",
            MYSQL_PATH__QUERY_DB = "/home/matteo/PycharmProjects/mlsql/Regression/Regression_FlightDelay/FlightDelay/SQL/MYSQL_PREDICTION_WITH_ID_flight_delay_fasttree_db.sql",
            MYSQL_PATH__QUERY_NO_OUTPUT = "/home/matteo/PycharmProjects/mlsql/Regression/Regression_FlightDelay/FlightDelay/SQL/MYSQL_PREDICTION_WITH_ID_flight_delay_fasttree_no_output.sql",
            SQLSERVER_PATH__QUERY_CONSOLE = "/home/matteo/PycharmProjects/mlsql/Regression/Regression_FlightDelay/FlightDelay/SQL/SQLSERVER_PREDICTION_WITH_ID_flight_delay_fasttree.sql",
            SQLSERVER_PATH__QUERY_CSV = "/home/matteo/PycharmProjects/mlsql/Regression/Regression_FlightDelay/FlightDelay/SQL/SQLSERVER_PREDICTION_WITH_ID_flight_delay_fasttree_csv.sql",
            SQLSERVER_PATH__QUERY_DB = "/home/matteo/PycharmProjects/mlsql/Regression/Regression_FlightDelay/FlightDelay/SQL/SQLSERVER_PREDICTION_WITH_ID_flight_delay_fasttree_db.sql",
            SQLSERVER_PATH__QUERY_NO_OUTPUT = "/home/matteo/PycharmProjects/mlsql/Regression/Regression_FlightDelay/FlightDelay/SQL/SQLSERVER_PREDICTION_WITH_ID_flight_delay_fasttree_no_output.sql",
            //BATCH_SIZE = new int[] {  100, 1000, 10000, 100000 },
            //BATCH_SIZE = new int[] { 1, 10, 100, 1000, 10000, 100000 },
            BATCH_SIZE = new int[] { 1000 },
            N_ELEMENTS = 10000,
            //DBMSS = new string[] { "MYSQL", "SQLSERVER" },
            DBMSS = new string[] { "MYSQL"},
            NAME = "FLIGHT_DELAY",
            EXECUTION_TIME_SECONDS = 30,
            RESULT_FILE_NAME = "RESULT_FLIGHT_DELAY.csv"

        };


        internal static readonly Configuration example = new Configuration()
        {
            MYSQL_PATH__QUERY_CONSOLE = "/Users/paolosottovia/Downloads/testQuery.sql",
            MYSQL_PATH__QUERY_CSV = "",
            MYSQL_PATH__QUERY_DB = "",
            MYSQL_PATH__QUERY_NO_OUTPUT = "",
            BATCH_SIZE = new int[] { 1, 10, 100, 1000 },
            N_ELEMENTS = 301,
            DBMSS = new string[] { "MYSQL" },
            NAME = "HEART_DISEASE",
            EXECUTION_TIME_SECONDS = 30
        };
    }
}
