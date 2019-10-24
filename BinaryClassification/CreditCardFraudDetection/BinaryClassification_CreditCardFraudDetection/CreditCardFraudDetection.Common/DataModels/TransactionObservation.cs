using System;
using System.Data.SqlClient;
using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;

namespace CreditCardFraudDetection.Common.DataModels
{
    public interface IModelEntity
    {
        void PrintToConsole();
    }

    public class TransactionObservation : SQLData
    {
        // Note we're not loading the 'Time' column, since que don't need it as a feature
        [LoadColumn(0)]
        public float Time;

        [LoadColumn(1)]
        public float V1;

        [LoadColumn(2)]
        public float V2;

        [LoadColumn(3)]
        public float V3;

        [LoadColumn(4)]
        public float V4;

        [LoadColumn(5)]
        public float V5;

        [LoadColumn(6)]
        public float V6;

        [LoadColumn(7)]
        public float V7;

        [LoadColumn(8)]
        public float V8;

        [LoadColumn(9)]
        public float V9;

        [LoadColumn(10)]
        public float V10;

        [LoadColumn(11)]
        public float V11;

        [LoadColumn(12)]
        public float V12;

        [LoadColumn(13)]
        public float V13;

        [LoadColumn(14)]
        public float V14;

        [LoadColumn(15)]
        public float V15;

        [LoadColumn(16)]
        public float V16;

        [LoadColumn(17)]
        public float V17;

        [LoadColumn(18)]
        public float V18;

        [LoadColumn(19)]
        public float V19;

        [LoadColumn(20)]
        public float V20;

        [LoadColumn(21)]
        public float V21;

        [LoadColumn(22)]
        public float V22;

        [LoadColumn(23)]
        public float V23;

        [LoadColumn(24)]
        public float V24;

        [LoadColumn(25)]
        public float V25;

        [LoadColumn(26)]
        public float V26;

        [LoadColumn(27)]
        public float V27;

        [LoadColumn(28)]
        public float V28;

        [LoadColumn(29)]
        public float Amount;

        [LoadColumn(30)]
        public bool Label;

        [LoadColumn(31)]
        public float Id;

        public string getData(string separator)
        {
            string dbms = "MYSQL";
            return MLSQL.formatData(this.Time, dbms) + separator + MLSQL.formatData(this.V1, dbms) + separator + MLSQL.formatData(this.V2, dbms) + separator +
                MLSQL.formatData(this.V3, dbms) + separator + MLSQL.formatData(this.V4, dbms) + separator + MLSQL.formatData(this.V5, dbms) + separator + MLSQL.formatData(this.V6, dbms) + separator +
                MLSQL.formatData(this.V7, dbms) + separator + MLSQL.formatData(this.V8, dbms) + separator + MLSQL.formatData(this.V9, dbms) + separator + MLSQL.formatData(this.V10, dbms) + separator +
                 MLSQL.formatData(this.V11, dbms) + separator + MLSQL.formatData(this.V12, dbms) + separator + MLSQL.formatData(this.V13, dbms) + separator + MLSQL.formatData(this.V14, dbms) + separator +
                 MLSQL.formatData(this.V15, dbms) + separator + MLSQL.formatData(this.V16, dbms) + separator + MLSQL.formatData(this.V17, dbms) + separator + MLSQL.formatData(this.V18, dbms) + separator +
                   MLSQL.formatData(this.V19, dbms) + separator + MLSQL.formatData(this.V20, dbms) + separator + MLSQL.formatData(this.V21, dbms) + separator + MLSQL.formatData(this.V22, dbms) + separator +
                    MLSQL.formatData(this.V23, dbms) + separator + MLSQL.formatData(this.V24, dbms) + separator + MLSQL.formatData(this.V25, dbms) + separator + MLSQL.formatData(this.V26, dbms) + separator +
                    MLSQL.formatData(this.V27, dbms) + separator + MLSQL.formatData(this.V28, dbms) + separator + MLSQL.formatData(this.Amount, dbms) + separator + MLSQL.formatData(this.Label, dbms) + separator + MLSQL.formatData(this.Id, dbms);

        }

        public string GetHeader(string separator)
        {
            return "Tim" + separator + "V1" + separator + "V2" + separator +
                "V3" + separator + "V4" + separator + "V5" + separator + "V6" + separator +
                "V7" + separator + "V8" + separator + "V9" + separator + "V10" + separator +
                 "V11" + separator + "V12" + separator + "V13" + separator + "V14" + separator +
                  "V15" + separator + "V16" + separator + "V17" + separator + "V18" + separator +
                  "V19" + separator + "V20" + separator + "V21" + separator + "V22" + separator +
                    "V23" + separator + "V24" + separator + "V25" + separator + "V26" + separator +
                     "V27" + separator + "V28" + separator + "Amount" + separator + "Label" + separator + "Id";
        }

        public string getId()
        {
            return this.Id + "";
        }

        public string getMySQLData(string separator)
        {
            return this.getData(separator);
        }

        public string getScores(string separator)
        {
            throw new NotImplementedException();
        }

        public string getSQLServerData(string separator)
        {
            string dbms = "SQLSERVER";
            return MLSQL.formatData(this.Time, dbms) + separator + MLSQL.formatData(this.V1, dbms) + separator + MLSQL.formatData(this.V2, dbms) + separator +
                MLSQL.formatData(this.V3, dbms) + separator + MLSQL.formatData(this.V4, dbms) + separator + MLSQL.formatData(this.V5, dbms) + separator + MLSQL.formatData(this.V6, dbms) + separator +
                MLSQL.formatData(this.V7, dbms) + separator + MLSQL.formatData(this.V8, dbms) + separator + MLSQL.formatData(this.V9, dbms) + separator + MLSQL.formatData(this.V10, dbms) + separator +
                 MLSQL.formatData(this.V11, dbms) + separator + MLSQL.formatData(this.V12, dbms) + separator + MLSQL.formatData(this.V13, dbms) + separator + MLSQL.formatData(this.V14, dbms) + separator +
                 MLSQL.formatData(this.V15, dbms) + separator + MLSQL.formatData(this.V16, dbms) + separator + MLSQL.formatData(this.V17, dbms) + separator + MLSQL.formatData(this.V18, dbms) + separator +
                   MLSQL.formatData(this.V19, dbms) + separator + MLSQL.formatData(this.V20, dbms) + separator + MLSQL.formatData(this.V21, dbms) + separator + MLSQL.formatData(this.V22, dbms) + separator +
                    MLSQL.formatData(this.V23, dbms) + separator + MLSQL.formatData(this.V24, dbms) + separator + MLSQL.formatData(this.V25, dbms) + separator + MLSQL.formatData(this.V26, dbms) + separator +
                    MLSQL.formatData(this.V27, dbms) + separator + MLSQL.formatData(this.V28, dbms) + separator + MLSQL.formatData(this.Amount, dbms) + separator + MLSQL.formatData(this.Label, dbms) + separator + MLSQL.formatData(this.Id, dbms);

        }

        public void PrintToConsole()
        {
            Console.WriteLine($"Label: {Label}");

            Console.WriteLine($"Id: {Id}");
            Console.WriteLine($"Time: {Time}");
            // Console.WriteLine($"Features: [V1] {V1} [V2] {V2} [V3] {V3} ... [V28] {V28} Amount: {Amount}");




            Console.WriteLine($"[V1] {V1}");
            Console.WriteLine($"[V2] {V2}");
            Console.WriteLine($"[V3] {V3}");
            Console.WriteLine($"[V4] {V4}");
            Console.WriteLine($"[V5] {V5}");
            Console.WriteLine($"[V6] {V6}");
            Console.WriteLine($"[V7] {V7}");
            Console.WriteLine($"[V8] {V8}");
            Console.WriteLine($"[V9] {V9}");
            Console.WriteLine($"[V10] {V10}");
            Console.WriteLine($"[V11] {V11}");
            Console.WriteLine($"[V12] {V12}");
            Console.WriteLine($"[V13] {V13}");
            Console.WriteLine($"[V14] {V14}");
            Console.WriteLine($"[V15] {V15}");
            Console.WriteLine($"[V16] {V16}");
            Console.WriteLine($"[V17] {V17}");
            Console.WriteLine($"[V18] {V18}");
            Console.WriteLine($"[V19] {V19}");
            Console.WriteLine($"[V20] {V20}");
            Console.WriteLine($"[V21] {V21}");
            Console.WriteLine($"[V22] {V22}");
            Console.WriteLine($"[V23] {V23}");
            Console.WriteLine($"[V24] {V24}");
            Console.WriteLine($"[V25] {V25}");
            Console.WriteLine($"[V26] {V26}");
            Console.WriteLine($"[V27] {V27}");
            Console.WriteLine($"[V28] {V28}");
            Console.WriteLine($"[AMOUNT] {Amount}");



            Console.WriteLine("");



        }

        public void ReadDataFromMYSQL(MySqlDataReader reader)
        {
            this.Time = reader.GetFloat("Tim");
            this.V1 = reader.GetFloat("V1");
            this.V2 = reader.GetFloat("V2");
            this.V3 = reader.GetFloat("V3");
            this.V4 = reader.GetFloat("V4");
            this.V5 = reader.GetFloat("V5");
            this.V6 = reader.GetFloat("V6");
            this.V7 = reader.GetFloat("V7");
            this.V8 = reader.GetFloat("V8");
            this.V9 = reader.GetFloat("V9");
            this.V10 = reader.GetFloat("V10");
            this.V11 = reader.GetFloat("V11");
            this.V12 = reader.GetFloat("V12");
            this.V13 = reader.GetFloat("V13");
            this.V14 = reader.GetFloat("V14");
            this.V15 = reader.GetFloat("V15");
            this.V16 = reader.GetFloat("V16");
            this.V17 = reader.GetFloat("V17");
            this.V18 = reader.GetFloat("V18");
            this.V19 = reader.GetFloat("V19");
            this.V20 = reader.GetFloat("V20");
            this.V21 = reader.GetFloat("V21");
            this.V22 = reader.GetFloat("V22");
            this.V23 = reader.GetFloat("V23");
            this.V24 = reader.GetFloat("V24");
            this.V25 = reader.GetFloat("V25");
            this.V26 = reader.GetFloat("V26");
            this.V27 = reader.GetFloat("V27");
            this.V28 = reader.GetFloat("V28");
            this.Amount = reader.GetFloat("Amount");
            this.Label = reader.GetBoolean("Label");
            this.Id = reader.GetFloat("Id");
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {
            this.Time = (float)reader.GetDouble(0);
            this.V1 = (float)reader.GetDouble(1);
            this.V2 = (float)reader.GetDouble(2);
            this.V3 = (float)reader.GetDouble(3);
            this.V4 = (float)reader.GetDouble(4);
            this.V5 = (float)reader.GetDouble(5);
            this.V6 = (float)reader.GetDouble(6);
            this.V7 = (float)reader.GetDouble(7);
            this.V8 = (float)reader.GetDouble(8);
            this.V9 = (float)reader.GetDouble(9);
            this.V10 = (float)reader.GetDouble(10);
            this.V11 = (float)reader.GetDouble(11);
            this.V12 = (float)reader.GetDouble(12);
            this.V13 = (float)reader.GetDouble(13);
            this.V14 = (float)reader.GetDouble(14);
            this.V15 = (float)reader.GetDouble(15);
            this.V16 = (float)reader.GetDouble(16);
            this.V17 = (float)reader.GetDouble(17);
            this.V18 = (float)reader.GetDouble(18);
            this.V19 = (float)reader.GetDouble(19);
            this.V20 = (float)reader.GetDouble(20);
            this.V21 = (float)reader.GetDouble(21);
            this.V22 = (float)reader.GetDouble(22);
            this.V23 = (float)reader.GetDouble(23);
            this.V24 = (float)reader.GetDouble(24);
            this.V25 = (float)reader.GetDouble(25);
            this.V26 = (float)reader.GetDouble(26);
            this.V27 = (float)reader.GetDouble(27);
            this.V28 = (float)reader.GetDouble(28);
            this.Amount = (float)reader.GetDouble(29);
            int l = reader.GetInt32(30);
            if (l == 0)
            {
                this.Label = false;
            }
            else
            {
                this.Label = true;
            }

            this.Id = (float)reader.GetDouble(31);
        }

        //public static List<KeyValuePair<ColumnRole, string>>  Roles() {
        //    return new List<KeyValuePair<ColumnRole, string>>() {
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Label, "Label"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V1"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V2"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V3"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V4"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V5"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V6"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V7"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V8"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V9"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V10"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V11"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V12"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V13"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V14"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V15"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V16"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V17"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V18"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V19"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V20"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V21"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V22"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V23"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V24"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V25"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V26"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V27"),
        //            new KeyValuePair<ColumnRole, string>(ColumnRole.Feature, "V28"),
        //            new KeyValuePair<ColumnRole, string>(new ColumnRole("Amount"), ""),

        //        };
        //}
    }

}
