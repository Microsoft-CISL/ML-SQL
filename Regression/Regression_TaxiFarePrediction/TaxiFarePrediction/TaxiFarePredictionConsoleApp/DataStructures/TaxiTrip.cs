

using System;
using System.Data.SqlClient;
using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;

namespace Regression_TaxiFarePrediction.DataStructures
{
    public class TaxiTrip : ML2SQL.SQLData
    {
        [LoadColumn(0)]
        public string VendorId;

        [LoadColumn(1)]
        public string RateCode;

        [LoadColumn(2)]
        public float PassengerCount;

        [LoadColumn(3)]
        public float TripTime;

        [LoadColumn(4)]
        public float TripDistance;

        [LoadColumn(5)]
        public string PaymentType;

        [LoadColumn(6)]
        public float FareAmount;

        [LoadColumn(7)]
        public int Id;

        public string getData(string separator)
        {
            return MLSQL.EscapeStringSQL(this.VendorId) + separator + MLSQL.EscapeStringSQL(this.RateCode) + separator + this.PassengerCount + separator + this.TripTime + separator + this.TripDistance + separator +
                MLSQL.EscapeStringSQL(this.PaymentType) + separator + this.FareAmount + separator + this.Id;
        }

        public string GetHeader(string separator)
        {
            return "VendorId" + separator + "RateCode" + separator + "PassengerCount" + separator + "TripTime" + separator + "TripDistance" + separator + "PaymentType" + separator + "FareAmount" + separator + "Id";
        }

        public string getId()
        {
            return this.Id + "";
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
            return MLSQL.EscapeStringSQL(this.VendorId) + separator + MLSQL.EscapeStringSQL(this.RateCode) + separator + this.PassengerCount + separator + this.TripTime + separator + this.TripDistance + separator +
                MLSQL.EscapeStringSQL(this.PaymentType) + separator + this.FareAmount + separator + this.Id;
        }

        public string getSQLServerData(string separator)
        {
            return getSQLData(separator);
        }

        public void ReadDataFromMYSQL(MySqlDataReader reader)
        {
            this.VendorId = reader.GetString("VendorId");

            this.RateCode = reader.GetString("RateCode");

            this.PassengerCount = reader.GetFloat("PassengerCount");

            this.TripTime = reader.GetFloat("TripTime");

            this.TripDistance = reader.GetFloat("TripDistance");

            this.PaymentType = reader.GetString("PaymentType");

            this.FareAmount = reader.GetFloat("FareAmount");
            this.Id = reader.GetInt32("Id");
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {

            //for(int i =0; i <8;i++)
            //{
            //    Console.WriteLine("i: "+i +" -> "+reader.GetName(i) + "\t" +reader.GetFieldType(i));
            //}

            //Console.WriteLine(reader.ToString());
            this.VendorId = reader.GetString(0);

            this.RateCode = reader.GetString(1);

            this.PassengerCount = (float)reader.GetDouble(2);
            //Console.WriteLine(" VendorId: "+this.VendorId);
            //Console.WriteLine(" RateCode: " + this.RateCode);
            //Console.WriteLine(" PassengerCount: " + this.PassengerCount);
            this.TripTime = (float)reader.GetDouble(3);

            this.TripDistance = (float)reader.GetDouble(4);

            this.PaymentType = reader.GetString(5);

            this.FareAmount = (float)reader.GetDouble(6);
            this.Id = reader.GetInt32(7);
        }
    }
}