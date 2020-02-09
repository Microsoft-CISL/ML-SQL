using System;
namespace QueryExecutor
{
    public class Configuration
    {
        public Configuration()
        {
        }


        public string MYSQL_PATH__QUERY_CONSOLE;
        public string MYSQL_PATH__QUERY_DB;
        public string MYSQL_PATH__QUERY_CSV;
        public string MYSQL_PATH__QUERY_NO_OUTPUT;
        public string SQLSERVER_PATH__QUERY_CONSOLE;
        public string SQLSERVER_PATH__QUERY_DB;
        public string SQLSERVER_PATH__QUERY_CSV;
        public string SQLSERVER_PATH__QUERY_NO_OUTPUT;
        public string NAME;
        public int[] BATCH_SIZE;
        public string[] DBMSS;
        public int N_ELEMENTS;
        public int EXECUTION_TIME_SECONDS;
        public string RESULT_FILE_NAME;

        public string MYSQL_PREPROCESS_QUERY="";

        public string SQLSERVER_PREPROCESS_QUERY="";



    }
}
