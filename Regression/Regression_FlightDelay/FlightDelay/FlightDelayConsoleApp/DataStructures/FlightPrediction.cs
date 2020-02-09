using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace FlightDelay.DataStructures
{
    public class FlightPrediction : SQLData
    {
        [ColumnName("Score")]
        public float PredictedCount;

        public string getData(string separator)
        {
            return this.PredictedCount+"";
        }

        public string GetHeader(string separator)
        {
           return "Score";
        }

        public string getId()
        {
            throw new System.NotImplementedException();
        }

        public string getMySQLData(string separator)
        {
            return getSQLData(separator);
        }

        public string getScores(string separator)
        {
            //return this.PredictedCount+"";
            return this.PredictedCount.ToString(new System.Globalization.CultureInfo("en-US"));
        }

        public string getSQLData(string separator)
        {
            return PredictedCount+"";
        }

        public string getSQLServerData(string separator)
        {
            return getSQLData(separator);
        }

        public void ReadDataFromMYSQL(MySqlDataReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {
            throw new System.NotImplementedException();
        }
    }
}
