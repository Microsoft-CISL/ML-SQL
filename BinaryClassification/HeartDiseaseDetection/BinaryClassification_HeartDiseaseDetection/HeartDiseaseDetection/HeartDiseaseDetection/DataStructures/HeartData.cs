using System.Data.SqlClient;
using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;

namespace HeartDiseasePredictionConsoleApp.DataStructures
{
    public class HeartData: SQLData
    {
        [LoadColumn(0)]
        public float Age { get; set; }
        [LoadColumn(1)]
        public float Sex { get; set; }
        [LoadColumn(2)]
        public float Cp { get; set; }
        [LoadColumn(3)]
        public float TrestBps { get; set; }
        [LoadColumn(4)]
        public float Chol { get; set; }
        [LoadColumn(5)]
        public float Fbs { get; set; }
        [LoadColumn(6)]
        public float RestEcg { get; set; }
        [LoadColumn(7)]
        public float Thalac { get; set; }
        [LoadColumn(8)]
        public float Exang { get; set; }
        [LoadColumn(9)]
        public float OldPeak { get; set; }
        [LoadColumn(10)]
        public float Slope { get; set; }
        [LoadColumn(11)]
        public float Ca { get; set; }
        [LoadColumn(12)]
        public float Thal { get; set; }
        [LoadColumn(13)]
        public bool Label { get; set; }
        [LoadColumn(14)]
        public int Id { get; set; }


        public string getCsvLine()
        {
            return null;
        }

        public HeartData()
        {

        }

      

        public string GetHeader(string separator)
        {

            return "Age" + separator + "Sex" + separator + "Cp" + separator + "TrestBps" + separator + "Chol" + separator + "Fbs" + separator + "RestEcg" + separator
               + "Thalac" + separator + "Exang" + separator + "OldPeak" + separator + "Slope" + separator + "Ca" + separator + "Thal" + separator + "Label" + separator + "Id";
        }



        public string getMySQLData(string separator)
        {
            return this.Age + separator + this.Sex + separator + this.Cp + separator + this.TrestBps + separator + this.Chol +
                separator + this.Fbs + separator + this.RestEcg + separator + this.Thalac + separator + this.Exang + separator + this.OldPeak + separator + this.Slope +
                separator + this.Ca + separator + this.Thal + separator + this.Label + separator + this.Id;
        }

        public string getSQLServerData(string separator)


        {
            int LabelBool = 0;
            if (this.Label)
            {
                LabelBool = 1;
            }


            return this.Age + separator + this.Sex + separator + this.Cp + separator + this.TrestBps + separator + this.Chol +
                 separator + this.Fbs + separator + this.RestEcg + separator + this.Thalac + separator + this.Exang + separator + this.OldPeak + separator + this.Slope +
                 separator + this.Ca + separator + this.Thal + separator + LabelBool + separator + this.Id;
        }

        public string getData(string separator)
        {
            return this.Age + separator + this.Sex + separator + this.Cp + separator + this.TrestBps + separator + this.Chol +
                separator + this.Fbs + separator + this.RestEcg + separator + this.Thalac + separator + this.Exang + separator + this.OldPeak + separator + this.Slope +
                separator + this.Ca + separator + this.Thal + separator + this.Label + separator + this.Id;
        }

        public void ReadDataFromMYSQL(MySqlDataReader reader)
        {
            this.Age = reader.GetFloat("Age");
            this.Sex = reader.GetFloat("Sex");
            this.Cp = reader.GetFloat("Cp");
            this.TrestBps = reader.GetFloat("TrestBps");
            this.Chol = reader.GetFloat("Chol");
            this.Fbs = reader.GetFloat("Fbs");
            this.RestEcg = reader.GetFloat("RestEcg");
            this.Thalac = reader.GetFloat("Thalac");
            this.Exang = reader.GetFloat("Exang");
            this.OldPeak = reader.GetFloat("OldPeak");
            this.Slope = reader.GetFloat("Slope");
            this.Ca = reader.GetFloat("Ca");
            this.Thal = reader.GetFloat("Thal");
            this.Label = reader.GetBoolean("Label");
            this.Id = reader.GetInt32("Id");
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {
            this.Age = (float)reader.GetDouble(0);
            this.Sex = (float)reader.GetDouble(1);
            this.Cp = (float)reader.GetDouble(2);
            this.TrestBps = (float)reader.GetDouble(3);
            this.Chol = (float)reader.GetDouble(4);
            this.Fbs = (float)reader.GetDouble(5);
            this.RestEcg = (float)reader.GetDouble(6);
            this.Thalac = (float)reader.GetDouble(7);
            this.Exang = (float)reader.GetDouble(8);
            this.OldPeak = (float)reader.GetDouble(9);
            this.Slope = (float)reader.GetDouble(10);
            this.Ca = (float)reader.GetDouble(11);
            this.Thal = (float)reader.GetDouble(12);
            this.Label = reader.GetBoolean(13);
            this.Id = reader.GetInt32(14);
        }

        public string getId()
        {
            return this.Id+"";
        }

        public string getScores(string separator)
        {
            throw new System.NotImplementedException();
        }
    }





}
