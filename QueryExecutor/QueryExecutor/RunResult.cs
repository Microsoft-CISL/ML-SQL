using System;
namespace QueryExecutor
{
    public class RunResult
    {



        public string name { get; set; }
        public string inputMode { get; set; }
        public string outputMode { get; set; }
        public string dbms { get; set; }
        public int batchSize { get; set; }
        public float timeWithOutConnection { get; set; }
        public float timeWithConnection { get; set; }
        public double elementProcessed { get; set; }
        public string executionMode { get; set; }

        public RunResult(string name,string inputMode,string outputMode,string dbms, int batchSize, float timeWithOutConnection, float timeWithConnection, double elementProcessed,string executionMode)
        {

            this.name = name;
            this.inputMode = inputMode;
            this.outputMode = outputMode;
            this.dbms = dbms;
            this.batchSize = batchSize;
            this.timeWithConnection = timeWithConnection;
            this.timeWithOutConnection = timeWithOutConnection;
            this.elementProcessed = elementProcessed;
            this.executionMode = executionMode;
        }



        public void WriteResult()
        {
            Console.WriteLine("name\t"+this.name);
            Console.WriteLine("inputMode\t"+this.inputMode);
            Console.WriteLine("outputMode\t"+this.outputMode);
            Console.WriteLine("dbms\t"+this.dbms);
            Console.WriteLine("batchSize\t"+this.batchSize);
            Console.WriteLine("timeWithOutConnection\t"+this.timeWithOutConnection);
            Console.WriteLine("timeWithConnection\t"+this.timeWithConnection);
            Console.WriteLine("elementProcessed\t"+this.elementProcessed);
            Console.WriteLine("executionMode\t" + this.executionMode);
        }

        public string GetLineCsv(string separator)
        {
            return this.name + separator +this.inputMode +separator+ this.outputMode + separator + this.dbms + separator + this.batchSize + separator + this.timeWithOutConnection + separator + this.timeWithConnection + separator + elementProcessed+separator + executionMode;
        }


        public static string getHeader(string separator)
        {
            return "name"+separator+"inputMode"+separator+"ouputMode"+separator+"dbms"+separator+"batchSize"+separator+"timeWithOutConnection"+separator+"timeWithConnection"+separator+"elementProcessed"+separator+"executionMode";
        }





    }
}
