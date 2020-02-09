using System;
using System.Data.SqlClient;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace QueryExecutor
{
    public class Executor
    {


        public string ConnectionStringMYSQL = "server=localhost;Uid=root;Pwd=19021990;Database=MLtoSQL";
        public string ConnectionStringSQLSERVER = "";
        public string name { set; get; }
        public bool debug { set; get; }


        public Executor()
        {
        }

        public Executor(string ConnectionStringMYSQL, string ConnectionStringSQLSERVER)
        {
            this.ConnectionStringMYSQL = ConnectionStringMYSQL;
            this.ConnectionStringSQLSERVER = ConnectionStringSQLSERVER;
        }



        public RunResult ExecuteQueryForACertainPeriodOfTime(string query, string inputMode, string outputMode, string dbms, int chunckSize, double nElements, int timeSecond, bool checkNumberOfResults)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            double elementsComputed = 0;

            float timeWithoutConnection = 0f;
            float timeWithConnection = 0f;

            while (s.Elapsed < TimeSpan.FromSeconds(timeSecond))
            {
                RunResult r = ExecuteQueryWithChunckSize(query, inputMode, outputMode, dbms, chunckSize, nElements, checkNumberOfResults);
                elementsComputed += nElements;
                timeWithConnection += r.timeWithConnection;
                timeWithoutConnection += r.timeWithOutConnection;

            }



            s.Stop();

            watch.Stop();

            float millisecondElapsed = watch.ElapsedMilliseconds;
            RunResult res = new RunResult(name, inputMode, outputMode, dbms, chunckSize, timeWithoutConnection, timeWithConnection, elementsComputed, "TIME_MODE");

            Console.WriteLine("Time Elapsed: " + millisecondElapsed);
            Console.WriteLine("Number of Elements Computed: " + elementsComputed);

            return res;
        }

        public void ExecutePreProcessingQuery(string q, string dbms)
        {
            switch (dbms)
                {
                    case "MYSQL":
                        {

                            var mConnection = new MySqlConnection(ConnectionStringMYSQL);

                            mConnection.Open();

                            MySqlCommand cmd = new MySqlCommand(q, mConnection);
                            cmd.CommandTimeout = 28800;

                            var reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                Console.WriteLine("MYSQL");
                                Console.WriteLine(reader.ToString());
                            }

                            reader.Close();
                            mConnection.Close();

                        }
                        break;

                    case "SQLSERVER":
                        {
                            var mConnection = new SqlConnection(ConnectionStringSQLSERVER);

                            mConnection.Open();

                            SqlCommand cmd = new SqlCommand(q, mConnection);

                            var reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                Console.WriteLine("SQLSERVER");
                                Console.WriteLine(reader.ToString());
                            }

                            reader.Close();

                            mConnection.Close();

                        }
                        break;
                }
        }


        public RunResult ExecuteQueryWithChunckSize(string query, string inputMode, string outputMode, string dbms, int batchSize, double nElements, bool checkNumberOfResults)
        {

            float timeExecutionAndConnection = 0f;
            float timeExecution = 0f;


            RunResult res = null;


            for (int i = 1; i < nElements; i = i + batchSize)
            {
                string id = i + "";
                string batch = batchSize + "";
                string q = ReplaceWildCardInQuery(query, id, batch);

                // for test pursose only
                // q = "SET @@profiling = 1;\n " + q;

                // Console.WriteLine("Modifying input query: "+ q);


                int elementsAnalyzed = 0;

                if (outputMode.Equals("CSV"))
                {

                    int unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    Random rnd = new Random();
                    int random = rnd.Next(1, 1000000);

                    int random1 = rnd.Next(1, 1000000);
                    int r = Math.Abs(unixTimestamp + random);

                    //Console.WriteLine("Random: "+r);
                    q = q.Replace("output.csv", "output_" + r + "_" + random1 + ".csv");
                    //Environment.Exit(0);
                }

                // Console.WriteLine("Query:\n\n " + q);

                // Console.WriteLine("ID: " + id + " batchsize: " + batchSize);
                switch (dbms)
                {
                    case "MYSQL":

                        {

                            // for test pursose only
                            //q = "SET @@profiling = 1;\n " + q;



                           // Console.WriteLine("Modifying input query: " + q);

                            var watch = System.Diagnostics.Stopwatch.StartNew();

                            var mConnection = new MySqlConnection(ConnectionStringMYSQL);

                            mConnection.Open();



                            string[] queries1 = { "SET @@profiling = 0;", "SET @@profiling_history_size = 0;", "SET @@profiling_history_size = 100;", "SET @@profiling = 1;" };


                            //cmd = new MySqlCommand("SET @@profiling = 0; SET @@profiling_history_size = 0; SET @@profiling_history_size = 100;SET @@profiling = 1; ", mConnection);


                            for (int k = 0; k < queries1.Length; k++)
                            {
                                //Console.WriteLine("Query: " + queries1[k]);
                                var cmd1 = new MySqlCommand(queries1[k], mConnection);
                                var reader1 = cmd1.ExecuteReader();
                                while (reader1.Read())
                                {
                                    //Console.WriteLine("CICCIOO");
                                }
                                reader1.Close();
                            }

                            var watchOnlyExecution = System.Diagnostics.Stopwatch.StartNew();

                            MySqlCommand cmd = new MySqlCommand(q, mConnection);
                            cmd.CommandTimeout = 28800;

                            //int commandResult = cmd.ExecuteNonQuery();
                            int numberOfResults = 0;
                            var reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                // do whatever you need, access reader data as reader[i]
                                if (!outputMode.Equals("NO_OUTPUT"))
                                {
                                    int ID = (int)reader["Id"];
                                }
                                //Console.WriteLine("Id: " + ID);

                                numberOfResults += 1;
                            }
                            // Console.WriteLine("Number of Result: " + numberOfResults);
                            elementsAnalyzed += numberOfResults;

                            if (checkNumberOfResults)
                            {
                                if (numberOfResults == batchSize)
                                {

                                }
                                else if (elementsAnalyzed == nElements)
                                {

                                }
                                else
                                {
                                    throw new System.InvalidOperationException("Number of results mismatched :( ");
                                }

                            }


                            reader.Close();

                           // Console.WriteLine("Query: " + "SHOW PROFILES;");

                            cmd = new MySqlCommand("SHOW PROFILES;", mConnection);
                            reader = cmd.ExecuteReader();

                            float durationQuery = 0.0f;
                            while (reader.Read())
                            {
                                // do whatever you need, access reader data as reader[i]
                                //if (!outputMode.Equals("NO_OUTPUT"))
                                //{

                                    var QueryID = reader.GetUInt32("Query_ID");
                                    //int QueryID = (int)reader["Query_ID"];

                                    double duration = (double)reader["Duration"];
                                    duration = duration * 1000;

                                    // Console.WriteLine("QueryId: "+QueryID);
                                    // Console.WriteLine("Duration: " + duration);
                                    string queryProcessed = (string)reader["Query"];


                                   // Console.WriteLine("Query processed: " + queryProcessed);
                                    if (q.Contains(queryProcessed))
                                    {
                                        //Console.WriteLine("Correct Query Analized!!!");
                                        durationQuery += (float)duration;
                                    }

                               // }
                                //Console.WriteLine("Id: " + ID);

                                numberOfResults += 1;
                            }

                            reader.Close
                                ();


                            string[] queries = { "SET @@profiling = 0;", "SET @@profiling_history_size = 0;", "SET @@profiling_history_size = 100;", "SET @@profiling = 1;", " SET profiling=1;" };


                            //cmd = new MySqlCommand("SET @@profiling = 0; SET @@profiling_history_size = 0; SET @@profiling_history_size = 100;SET @@profiling = 1; ", mConnection);


                            for (int k = 0; k < queries.Length; k++)
                            {
                                // Console.WriteLine("Query: "+ queries[k]);
                                cmd = new MySqlCommand(queries[k], mConnection);
                                reader = cmd.ExecuteReader();
                                while (reader.Read())


                                {
                                    // Console.WriteLine("Field count: "+reader.FieldCount);
                                    // Console.WriteLine("Reader to string: "+reader.ToString());
                                }
                                reader.Close();
                            }






                            watchOnlyExecution.Stop();
                            //float elapsedTimeMilliseconds = watchOnlyExecution.ElapsedMilliseconds;
                            //timeExecution += elapsedTimeMilliseconds;


                            //last change
                            timeExecution += durationQuery;

                            mConnection.Close();

                            watch.Stop();


                            float elapsedTimeWithConnectionMilliseconds = watch.ElapsedMilliseconds;

                            timeExecutionAndConnection += elapsedTimeWithConnectionMilliseconds;


                        }


                        break;

                    case "SQLSERVER":
                        {

                            var watch = System.Diagnostics.Stopwatch.StartNew();

                            var mConnection = new SqlConnection(ConnectionStringSQLSERVER);
                            mConnection.StatisticsEnabled = true;

                            mConnection.Open();

                            var watchOnlyExecution = System.Diagnostics.Stopwatch.StartNew();

                            SqlCommand cmd = new SqlCommand(q, mConnection);

                            //int commandResult = cmd.ExecuteNonQuery();
                            int numberOfResults = 0;
                            var reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                // do whatever you need, access reader data as reader[i]
                                if (!outputMode.Equals("NO_OUTPUT"))
                                {
                                    int ID = (int)reader["Id"];
                                }
                                //Console.WriteLine("Id: " + ID);

                                numberOfResults += 1;
                            }
                            // Console.WriteLine("Number of Result: " + numberOfResults);
                            elementsAnalyzed += numberOfResults;

                            if (checkNumberOfResults)
                            {
                                if (numberOfResults == batchSize)
                                {

                                }
                                else if (elementsAnalyzed == nElements)
                                {

                                }
                                else
                                {
                                    throw new System.InvalidOperationException("Number of results mismatched :( ");
                                }

                            }



                            reader.Close();



                            watchOnlyExecution.Stop();
                            float elapsedTimeMilliseconds = watchOnlyExecution.ElapsedMilliseconds;
                            // timeExecution += elapsedTimeMilliseconds;


                            mConnection.Close();

                            watch.Stop();

                            var stats = mConnection.RetrieveStatistics();

                            // Test new time execution
                            var ExecutionTimeInMs = (long)stats["ExecutionTime"];
                            //Console.WriteLine("EXECution time: "+ExecutionTimeInMs);
                            mConnection.ResetStatistics();

                            timeExecution += ExecutionTimeInMs;

                            float elapsedTimeWithConnectionMilliseconds = watch.ElapsedMilliseconds;

                            timeExecutionAndConnection += elapsedTimeWithConnectionMilliseconds;

                            timeExecutionAndConnection += ExecutionTimeInMs;
                        }

                        break;
                }

            }



            res = new RunResult(name, inputMode, outputMode, dbms, batchSize, timeExecution, timeExecutionAndConnection, nElements, "BATCH_MODE");

            // Console.WriteLine("Time For Execution and Connection: " + timeExecutionAndConnection);
            // Console.WriteLine("Time For Exection: " + timeExecution);



            return res;
        }



        private string ReplaceWildCardInQuery(string query, string id, string batchSize)

        {

            if (query.Contains("@id") && query.Contains("chuncksize"))
            {

            }
            else
            {
                throw new Exception("The input query does not contain the id or chuncksize parameter!!!");
            }


            string q = query.Replace("@id", id).Replace("chuncksize", batchSize);

            return q;

        }
    }
}
