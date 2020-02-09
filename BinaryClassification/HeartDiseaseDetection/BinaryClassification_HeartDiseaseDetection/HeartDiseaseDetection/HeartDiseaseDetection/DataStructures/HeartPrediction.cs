using System.Data.SqlClient;
using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;

namespace HeartDiseasePredictionConsoleApp.DataStructures
{
    public class HeartPrediction:SQLData
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public bool Prediction;

        // No need to specify ColumnName attribute, because the field
        // name "Probability" is the column name we want.
        public float Probability;

        public float Score;

       

        public string getData(string separator)
        {
            return this.Prediction + separator + this.Probability + separator + this.Score;
        }

        public string GetHeader(string separator)
        {
            return "PredictedLabel" + separator + "Probability" + separator + "Score";
        }

        public string getMySQLData(string separator)
        {
            return this.getData(separator);
        }

        public string getSQLServerData(string separator)
        {

            int PredictionBool =0;
            if (this.Prediction)
            {
                PredictionBool = 1;
            }

            return PredictionBool + separator + this.Probability + separator + this.Score;
        }

        public void ReadDataFromMYSQL(MySqlDataReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {
            throw new System.NotImplementedException();
        }

        public string getId()
        {
            throw new System.NotImplementedException();
        }

        public string getScores(string separator)
        {
            return this.Score + "";
        }

        public HeartPrediction()
        {

        }
    }
}
