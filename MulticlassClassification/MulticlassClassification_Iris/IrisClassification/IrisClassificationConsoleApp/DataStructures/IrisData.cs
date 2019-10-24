
using System.Data.SqlClient;
using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;

namespace MulticlassClassification_Iris.DataStructures
{
    public class IrisData : SQLData
    {
        [LoadColumn(0)]
        public float Label;

        [LoadColumn(1)]
        public float SepalLength;

        [LoadColumn(2)]
        public float SepalWidth;

        [LoadColumn(3)]
        public float PetalLength;

        [LoadColumn(4)]
        public float PetalWidth;

        [LoadColumn(5)]
        public int Id;



        public string getData(string separator)
        {
            return this.Label + separator + this.SepalLength + separator + this.SepalWidth + separator + this.PetalLength + separator + PetalWidth + separator + this.Id;
        }

        public string GetHeader(string separator)
        {
            return "Label" + separator + "SepalLength" + separator + "SepalWidth" + separator + "PetalLength" + separator + "PetalWidth" + separator + "Id";
        }

        public string getId()
        {
            return this.Id + "";
        }

        public string getMySQLData(string separator)
        {
            return
                MLSQL.formatData(this.Label, "MYSQL") + separator + MLSQL.formatData(this.SepalLength, "MYSQL") + separator +
                MLSQL.formatData(this.SepalWidth, "MYSQL") + separator + MLSQL.formatData(this.PetalLength, "MYSQL") + separator +
                MLSQL.formatData(PetalWidth, "MYSQL") + separator + MLSQL.formatData(this.Id, "MYSQL");
        }

        public string getScores(string separator)
        {
            throw new System.NotImplementedException();
        }

        public string getSQLServerData(string separator)
        {
            return
                MLSQL.formatData(this.Label, "SQLSERVER") + separator + MLSQL.formatData(this.SepalLength, "SQLSERVER") + separator +
                MLSQL.formatData(this.SepalWidth, "SQLSERVER") + separator + MLSQL.formatData(this.PetalLength, "SQLSERVER") + separator +
                MLSQL.formatData(PetalWidth, "SQLSERVER") + separator + MLSQL.formatData(this.Id, "SQLSERVER");
        }

        public void ReadDataFromMYSQL(MySqlDataReader reader)
        {

            this.Label = reader.GetFloat("Label");
            this.SepalLength = reader.GetFloat("SepalLength");
            this.SepalWidth = reader.GetFloat("SepalWidth");
            this.PetalLength = reader.GetFloat("PetalLength");
            this.PetalWidth = reader.GetFloat("PetalWidth");
            this.Id = reader.GetInt32("Id");
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {
            this.Label = reader.GetFloat(0);
            this.SepalLength = reader.GetFloat(1);
            this.SepalWidth = reader.GetFloat(2);
            this.PetalLength = reader.GetFloat(3);
            this.PetalWidth = reader.GetFloat(4);
            this.Id = reader.GetInt32(5);
        }
    }
}