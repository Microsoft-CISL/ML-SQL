using System.Data.SqlClient;
using ML2SQL;
using MySql.Data.MySqlClient;

namespace MulticlassClassification_Iris.DataStructures
{
    public class IrisPrediction:SQLData
    {
        public float[] Score;


        public string getData(string separator)
        {
            string data = "";
            for (int i =0;i < Score.Length;i++)
            {
                data += Score[i] + separator;
            }
            data = data.Substring(0, data.Length - separator.Length);

            return data;
        }

        public string GetHeader(string separator)
        {
            string data = "score_0,score_1,score_2";
            return data;
        }

        public string getId()
        {
            throw new System.NotImplementedException();
        }

        public string getMySQLData(string separator)
        {
            string data = "";
            for (int i = 0; i < Score.Length; i++)
            {
                data += MLSQL.formatData( Score[i],"MYSQL") + separator;
            }
            data = data.Substring(0, data.Length - separator.Length);

            return data;
        }

        public string getScores(string separator)
        {
            string data = "";
            for (int i = 0; i < Score.Length; i++)
            {
                data += MLSQL.formatData(Score[i], "MYSQL") + separator;
            }
            data = data.Substring(0, data.Length - separator.Length);

            return data;
        }

        public string getSQLServerData(string separator)
        {
            string data = "";
            for (int i = 0; i < Score.Length; i++)
            {
                data += MLSQL.formatData(Score[i], "SQLSERVER") + separator;
            }
            data = data.Substring(0, data.Length - separator.Length);

            return data;
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