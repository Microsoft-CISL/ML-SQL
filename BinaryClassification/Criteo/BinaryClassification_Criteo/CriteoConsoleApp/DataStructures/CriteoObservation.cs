using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Criteo.DataStructures
{
    public class CriteoObservation:SQLData
    {

        [LoadColumn(0)]
        [ColumnName("Label")]
        public bool Label { get; set; }
        [LoadColumn(1)]
        public float FeatureInteger1 { get; set; }
        [LoadColumn(2)]
        public float FeatureInteger2 { get; set; }
        [LoadColumn(3)]
        public float FeatureInteger3 { get; set; }
        [LoadColumn(4)]
        public float FeatureInteger4 { get; set; }
        [LoadColumn(5)]
        public float FeatureInteger5 { get; set; }
        [LoadColumn(6)]
        public float FeatureInteger6 { get; set; }
        [LoadColumn(7)]
        public float FeatureInteger7 { get; set; }
        [LoadColumn(8)]
        public float FeatureInteger8 { get; set; }
        [LoadColumn(9)]
        public float FeatureInteger9 { get; set; }
        [LoadColumn(10)]
        public float FeatureInteger10 { get; set; }
        [LoadColumn(11)]
        public float FeatureInteger11 { get; set; }
        [LoadColumn(12)]
        public float FeatureInteger12 { get; set; }
        [LoadColumn(13)]
        public float FeatureInteger13 { get; set; }
        [LoadColumn(14)]
        public string CategoricalFeature1 { get; set; }
        [LoadColumn(15)]
        public string CategoricalFeature2 { get; set; }
        [LoadColumn(16)]
        public string CategoricalFeature3 { get; set; }
        [LoadColumn(17)]
        public string CategoricalFeature4 { get; set; }
        [LoadColumn(18)]
        public string CategoricalFeature5 { get; set; }
        [LoadColumn(19)]
        public string CategoricalFeature6 { get; set; }
        [LoadColumn(20)]
        public string CategoricalFeature7 { get; set; }
        [LoadColumn(21)]
        public string CategoricalFeature8 { get; set; }
        [LoadColumn(22)]
        public string CategoricalFeature9 { get; set; }
        [LoadColumn(23)]
        public string CategoricalFeature10 { get; set; }
        [LoadColumn(24)]
        public string CategoricalFeature11 { get; set; }
        [LoadColumn(25)]
        public string CategoricalFeature12 { get; set; }        
        [LoadColumn(26)]
        public string CategoricalFeature13 { get; set; } 
        [LoadColumn(27)]
        public string CategoricalFeature14 { get; set; }
        [LoadColumn(28)]
        public string CategoricalFeature15 { get; set; }
        [LoadColumn(29)]
        public string CategoricalFeature16 { get; set; }
        [LoadColumn(30)]
        public string CategoricalFeature17 { get; set; }
        [LoadColumn(31)]
        public string CategoricalFeature18 { get; set; }
        [LoadColumn(32)]
        public string CategoricalFeature19 { get; set; }
        [LoadColumn(33)]
        public string CategoricalFeature20 { get; set; }
        [LoadColumn(34)]
        public string CategoricalFeature21 { get; set; }
        [LoadColumn(35)]
        public string CategoricalFeature22 { get; set; }
        [LoadColumn(36)]
        public string CategoricalFeature23 { get; set; }
        [LoadColumn(37)]
        public string CategoricalFeature24 { get; set; }
        [LoadColumn(38)]
        public string CategoricalFeature25 { get; set; }
        [LoadColumn(39)]
        public string CategoricalFeature26 { get; set; }       
        [LoadColumn(40)]
        public float Id { get; set; }



        public string getData(string separator)
        {
            return this.Label + separator + this.FeatureInteger1 + separator + this.FeatureInteger2 + separator + this.FeatureInteger3 + separator +
                this.FeatureInteger4 + separator + this.FeatureInteger5 + separator + this.FeatureInteger6 + separator +
                this.FeatureInteger7 + separator + this.FeatureInteger8 + separator + this.FeatureInteger9 + separator +
                this.FeatureInteger10 + separator + this.FeatureInteger11 + separator + this.FeatureInteger12 + separator +
                this.FeatureInteger13 + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature1) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature2) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature3) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature4) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature5) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature6) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature7) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature8) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature9) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature10) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature11) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature12) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature13) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature14) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature15) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature16) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature17) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature18) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature19) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature20) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature21) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature22) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature23) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature24) + separator +
                MLSQL.EscapeStringSQL(this.CategoricalFeature25) + separator + MLSQL.EscapeStringSQL(this.CategoricalFeature26) + separator + this.Id;
        }

        public string GetHeader(string separator)
        {
            return "Label" + separator + "FeatureInteger1" + separator + "FeatureInteger2" + separator + "FeatureInteger3" + separator +
                "FeatureInteger4" + separator + "FeatureInteger5" + separator + "FeatureInteger6" + separator +
                "FeatureInteger7" + separator + "FeatureInteger8" + separator + "FeatureInteger9" + separator +
                "FeatureInteger10" + separator + "FeatureInteger11" + separator + "FeatureInteger12" + separator +
                "FeatureInteger13" + separator + "CategoricalFeature1" + separator + "CategoricalFeature2" + separator +
                "CategoricalFeature3" + separator + "CategoricalFeature4" + separator + "CategoricalFeature5" + separator +
                "CategoricalFeature6" + separator + "CategoricalFeature7" + separator + "CategoricalFeature8" + separator +
                "CategoricalFeature9" + separator + "CategoricalFeature10" + separator +
                "CategoricalFeature11" + separator + "CategoricalFeature12" + separator +
                "CategoricalFeature13" + separator + "CategoricalFeature14" + separator +
                "CategoricalFeature15" + separator + "CategoricalFeature16" + separator +
                "CategoricalFeature17" + separator + "CategoricalFeature18" + separator +
                "CategoricalFeature19" + separator + "CategoricalFeature20" + separator +
                "CategoricalFeature21" + separator + "CategoricalFeature22" + separator +
                "CategoricalFeature23" + separator + "CategoricalFeature24" + separator +
                "CategoricalFeature25" + separator + "CategoricalFeature26" + separator + "Id";
        }

        public string getId()
        {
            //return this.Id + "";
            return this.Id.ToString(new System.Globalization.CultureInfo("en-US"));
        }

        public string getMySQLData(string separator)
        {
            return getData(separator);
        }

        public string getScores(string separator)
        {
            throw new System.NotImplementedException();
        }

        public string getSQLData(string separator)
        {
            return this.getData(separator);
        }

        public string getSQLServerData(string separator)
        {
            return this.getData(separator);
        }

        public void ReadDataFromMYSQL(MySqlDataReader reader)
        {
            this.Label = reader.GetBoolean("label");
            this.FeatureInteger1 = reader.GetFloat("FeatureInteger1");
            this.FeatureInteger2 = reader.GetFloat("FeatureInteger2");
            this.FeatureInteger3 = reader.GetFloat("FeatureInteger3");
            this.FeatureInteger4 = reader.GetFloat("FeatureInteger4");
            this.FeatureInteger5 = reader.GetFloat("FeatureInteger5");
            this.FeatureInteger6 = reader.GetFloat("FeatureInteger6");
            this.FeatureInteger7 = reader.GetFloat("FeatureInteger7");
            this.FeatureInteger8 = reader.GetFloat("FeatureInteger8");
            this.FeatureInteger9 = reader.GetFloat("FeatureInteger9");
            this.FeatureInteger10 = reader.GetFloat("FeatureInteger10");
            this.FeatureInteger11 = reader.GetFloat("FeatureInteger11");
            this.FeatureInteger12 = reader.GetFloat("FeatureInteger12");
            this.FeatureInteger13 = reader.GetFloat("FeatureInteger13");
            this.CategoricalFeature1 = reader.GetString("CategoricalFeature1");
            this.CategoricalFeature2 = reader.GetString("CategoricalFeature2");
            this.CategoricalFeature3 = reader.GetString("CategoricalFeature3");
            this.CategoricalFeature4 = reader.GetString("CategoricalFeature4");
            this.CategoricalFeature5 = reader.GetString("CategoricalFeature5");
            this.CategoricalFeature6 = reader.GetString("CategoricalFeature6");
            this.CategoricalFeature7 = reader.GetString("CategoricalFeature7");
            this.CategoricalFeature8 = reader.GetString("CategoricalFeature8");
            this.CategoricalFeature9 = reader.GetString("CategoricalFeature9");
            this.CategoricalFeature10 = reader.GetString("CategoricalFeature10");
            this.CategoricalFeature11 = reader.GetString("CategoricalFeature11");
            this.CategoricalFeature12 = reader.GetString("CategoricalFeature12");
            this.CategoricalFeature13 = reader.GetString("CategoricalFeature13");
            this.CategoricalFeature14 = reader.GetString("CategoricalFeature14");
            this.CategoricalFeature15 = reader.GetString("CategoricalFeature15");
            this.CategoricalFeature16 = reader.GetString("CategoricalFeature19");
            this.CategoricalFeature17 = reader.GetString("CategoricalFeature17");
            this.CategoricalFeature18 = reader.GetString("CategoricalFeature18");
            this.CategoricalFeature19 = reader.GetString("CategoricalFeature19");
            this.CategoricalFeature20 = reader.GetString("CategoricalFeature20");
            this.CategoricalFeature21 = reader.GetString("CategoricalFeature21");
            this.CategoricalFeature22 = reader.GetString("CategoricalFeature22");
            this.CategoricalFeature23 = reader.GetString("CategoricalFeature23");
            this.CategoricalFeature24 = reader.GetString("CategoricalFeature24");
            this.CategoricalFeature25 = reader.GetString("CategoricalFeature25");
            this.CategoricalFeature26 = reader.GetString("CategoricalFeature26");
            this.Id = reader.GetInt32("Id");
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {
            this.Label = reader.GetBoolean(0);
            this.FeatureInteger1 = (float) reader.GetDouble(1);
            this.FeatureInteger2 = (float) reader.GetDouble(2);
            this.FeatureInteger3 = (float) reader.GetDouble(3);
            this.FeatureInteger4 = (float) reader.GetDouble(4);
            this.FeatureInteger5 = (float) reader.GetDouble(5);
            this.FeatureInteger6 = (float) reader.GetDouble(6);
            this.FeatureInteger7 = (float) reader.GetDouble(7);
            this.FeatureInteger8 = (float) reader.GetDouble(8);
            this.FeatureInteger9 = (float) reader.GetDouble(9);
            this.FeatureInteger10 = (float) reader.GetDouble(10);
            this.FeatureInteger11 = (float) reader.GetDouble(11);
            this.FeatureInteger12 = (float) reader.GetDouble(12);
            this.FeatureInteger13 = (float) reader.GetDouble(13);
            this.CategoricalFeature1 = reader.GetString(14);
            this.CategoricalFeature2 = reader.GetString(15);
            this.CategoricalFeature3 = reader.GetString(16);
            this.CategoricalFeature4 = reader.GetString(17);
            this.CategoricalFeature5 = reader.GetString(18);
            this.CategoricalFeature6 = reader.GetString(19);
            this.CategoricalFeature7 = reader.GetString(20);
            this.CategoricalFeature8 = reader.GetString(21);
            this.CategoricalFeature9 = reader.GetString(22);
            this.CategoricalFeature10 = reader.GetString(23);
            this.CategoricalFeature11 = reader.GetString(24);
            this.CategoricalFeature12 = reader.GetString(25);
            this.CategoricalFeature13 = reader.GetString(26);
            this.CategoricalFeature14 = reader.GetString(27);
            this.CategoricalFeature15 = reader.GetString(28);
            this.CategoricalFeature16 = reader.GetString(29);
            this.CategoricalFeature17 = reader.GetString(30);
            this.CategoricalFeature18 = reader.GetString(31);
            this.CategoricalFeature19 = reader.GetString(32);
            this.CategoricalFeature20 = reader.GetString(33);
            this.CategoricalFeature21 = reader.GetString(34);
            this.CategoricalFeature22 = reader.GetString(35);
            this.CategoricalFeature23 = reader.GetString(36);
            this.CategoricalFeature24 = reader.GetString(37);
            this.CategoricalFeature25 = reader.GetString(38);
            this.CategoricalFeature26 = reader.GetString(39);
            this.Id = reader.GetInt32(40);
        }
    }
}
