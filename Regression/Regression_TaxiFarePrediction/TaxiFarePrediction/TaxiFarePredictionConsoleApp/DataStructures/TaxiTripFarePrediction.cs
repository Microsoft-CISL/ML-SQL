using System.Data.SqlClient;
using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;

namespace Regression_TaxiFarePrediction.DataStructures
{
    public class TaxiTripFarePrediction:SQLData
    {
        [ColumnName("Score")]
        public float FareAmount;

        public string getData(string separator)
        {
            return FareAmount+"";
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
            return this.getData(separator);
        }

        public string getScores(string separator)
        {
            return this.FareAmount+"";
        }

        public string getSQLData(string separator)
        {
            return FareAmount + "";
        }

        public string getSQLServerData(string separator)
        {
            return this.getData(separator);
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