using System;
using System.Collections.Generic;

namespace QueryExecutor
{
    class Program
    {
        static void Main(string[] args)



        {
            int[] b = { 1, 10, 100, 1000 };
            string[] dbmss_ = { "MYSQL" };

            //Configuration configuration = new Configuration();

            //configuration.MYSQL_PATH__QUERY_CONSOLE = "/Users/paolosottovia/Downloads/testQuery.sql";
            //configuration.MYSQL_PATH__QUERY_CSV = "";
            //configuration.MYSQL_PATH__QUERY_DB = "";
            //configuration.MYSQL_PATH__QUERY_NO_OUTPUT = "";

            //configuration.BATCH_SIZE = b;
            //configuration.N_ELEMENTS = 301;
            //configuration.DBMSS = dbmss_;

            //configuration.NAME = "HEART_DISEASE";
            //configuration.EXECUTION_TIME_SECONDS = 30;


            List<Configuration> confs = new List<Configuration>();
            //confs.Add(Configurations.heart_disease_server_index_id);
            //confs.Add(Configurations.taxi_fare_server_index_id);
            //confs.Add(Configurations.bike_sharing_fasttree);
            //confs.Add(Configurations.bike_sharing_fasttree_tweedie);
            //confs.Add(Configurations.bike_sharing_sdca);
            //confs.Add(Configurations.bike_sharing_lbfgs);

            confs.Add(Configurations.credit_card);





            foreach (Configuration configuration in confs) {


               // configuration = Configurations.bike_sharing_lbfgs;

                List<RunResult> results = new List<RunResult>();
                string[] MYSQL_query_paths = { configuration.MYSQL_PATH__QUERY_CONSOLE, configuration.MYSQL_PATH__QUERY_CSV, configuration.MYSQL_PATH__QUERY_DB, configuration.MYSQL_PATH__QUERY_NO_OUTPUT };
                string[] SQLSERVER_query_paths = { configuration.SQLSERVER_PATH__QUERY_CONSOLE, configuration.SQLSERVER_PATH__QUERY_CSV, configuration.SQLSERVER_PATH__QUERY_DB, configuration.SQLSERVER_PATH__QUERY_NO_OUTPUT };
                string[][] queryPaths = { MYSQL_query_paths, SQLSERVER_query_paths };

                string[] modes = { "CONSOLE", "CSV", "DB", "NO_OUTPUT" };


                Executor executor = new Executor("server=localhost;Uid=root;Pwd=ML+matteo<3paolo+SQL;Database=MLtoSQL", "server=localhost;Uid=SA;Pwd=ML+matteo<3paolo+SQL;Database=mltosql");
                executor.name = configuration.NAME;


                //string[] dbmss = configuration.DBMSS;
                //
                
                string MysqlPreProcessingQuery = "";
                if (!configuration.MYSQL_PREPROCESS_QUERY.Equals(""))
                {
                    MysqlPreProcessingQuery = Utility.readQueryFromFile(configuration.MYSQL_PREPROCESS_QUERY);
                }
                string SqlserverPreProcessingQuery = "";
                if (!configuration.SQLSERVER_PREPROCESS_QUERY.Equals(""))
                {
                    SqlserverPreProcessingQuery = Utility.readQueryFromFile(configuration.SQLSERVER_PREPROCESS_QUERY);
                }




                for (int x = 0; x < configuration.DBMSS.Length; x++)
                {
                    bool checkResults = false;
                    string db = configuration.DBMSS[x];

                    int queryPathIndex = -1;

                    switch (db)
                    {
                        case "MYSQL":
                            queryPathIndex = 0;
                            break;

                        case "SQLSERVER":
                            queryPathIndex = 1;
                            break;
                    }


                    string[] query_paths = queryPaths[queryPathIndex];

                    for (int j = 0; j < query_paths.Length; j++)
                    {

                        string path = query_paths[j];

                        if (db.Equals("SQLSERVER") && modes[j].Equals("CSV"))
                        {
                            path = "";
                        }
                        Console.WriteLine("PATH: " + path);
                        if (!path.Equals(""))
                        {
                            Console.WriteLine("====================================================================================================================================");
                            string query = Utility.readQueryFromFile(path);
                            Console.WriteLine("DB: " + db + "\tMODE: " + modes[j]);
                            //Console.WriteLine("Query: " + query);
                            for (int i = 0; i < configuration.BATCH_SIZE.Length; i++)
                            {
                                int batchSize = configuration.BATCH_SIZE[i];

                                string preProcessingQuery = "";
                                if (db.Equals("SQLSERVER"))
                                {
                                    preProcessingQuery = SqlserverPreProcessingQuery;
                                }
                                else
                                {
                                    if (db.Equals("MYSQL"))
                                    {
                                        preProcessingQuery = MysqlPreProcessingQuery;
                                    }
                                }
                                if (!preProcessingQuery.Equals(""))
                                {
                                    executor.ExecutePreProcessingQuery(preProcessingQuery, db);
                                }
                                //Environment.Exit(1);

                                RunResult res = executor.ExecuteQueryWithChunckSize(query, db, modes[j], db, batchSize, configuration.N_ELEMENTS, checkResults);
                                res.WriteResult();
                                results.Add(res);
                            }
                        }

                    }
                }

                //foreach (string db in configuration.DBMSS)
                for (int x = 0; x < configuration.DBMSS.Length; x++)
                {
                    bool checkResults = false;
                    string db = configuration.DBMSS[x];

                    int queryPathIndex = -1;

                    switch (db)
                    {
                        case "MYSQL":
                            queryPathIndex = 0;
                            break;

                        case "SQLSERVER":
                            queryPathIndex = 1;
                            break;
                    }


                    string[] query_paths = queryPaths[queryPathIndex];


                    for (int j = 0; j < query_paths.Length; j++)
                    {
                        string path = query_paths[j];

                        if (db.Equals("SQLSERVER") && modes[j].Equals("CSV"))
                        {
                            path = "";
                        }
                        Console.WriteLine("PATH: " + path);
                        string mode = modes[j];
                        Console.WriteLine("PATH: " + path);
                        if (!path.Equals(""))
                        {
                            string query = Utility.readQueryFromFile(path);
                            Console.WriteLine("Query: " + query);
                            for (int i = 0; i < configuration.BATCH_SIZE.Length; i++)
                            {
                                int batchSize = configuration.BATCH_SIZE[i];
                                RunResult res = executor.ExecuteQueryForACertainPeriodOfTime(query, db, modes[j], db, batchSize, configuration.N_ELEMENTS, configuration.EXECUTION_TIME_SECONDS, checkResults);
                                res.WriteResult();
                                results.Add(res);
                            }
                        }

                    }


                }


                Console.WriteLine("\n\n\nResults: \n");
                for (int i = 0; i < results.Count; i++)
                {
                    Console.WriteLine("===============================================================================");
                    results[i].WriteResult();
                    Console.WriteLine("===============================================================================");
                }



                Utility.writeResultsOnCsvFile(results, "../../../" + configuration.RESULT_FILE_NAME);

            }

            Console.WriteLine("END of the PROOCESS!");

            //RunResult res1 = executor.ExecuteQueryForACertainPeriodOfTime(query, "MLtoSQL", "MYSQL", 10, 301, 10,checkResults);

        }
    }
}
