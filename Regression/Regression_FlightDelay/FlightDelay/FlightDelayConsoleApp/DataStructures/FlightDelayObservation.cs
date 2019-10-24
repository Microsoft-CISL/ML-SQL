using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace FlightDelay.DataStructures
{
    public class FlightDelayObservation:SQLData
    {

        [LoadColumn(0)]
        public float Year { get; set; }
        [LoadColumn(1)]
        public float Month { get; set; }
        [LoadColumn(2)]
        public float DayofMonth { get; set; }
        [LoadColumn(3)]
        public float DayOfWeek { get; set; }
        [LoadColumn(4)]
        public float DepTime { get; set; }
        [LoadColumn(5)]
        public float CRSDepTime { get; set; }
        [LoadColumn(6)]
        public float ArrTime { get; set; }
        [LoadColumn(7)]
        public float CRSArrTime { get; set; }
        [LoadColumn(8)]
        public string UniqueCarrier { get; set; }
        [LoadColumn(9)]
        public float FlightNum { get; set; }
        //[LoadColumn(10)]
        //public string TailNum { get; set; }
        [LoadColumn(11)]
        public float ActualElapsedTime { get; set; }
        [LoadColumn(12)]
        public float CRSElapsedTime { get; set; }
        [LoadColumn(13)]
        public float AirTime { get; set; }
        [LoadColumn(14)]
        public float ArrDelay { get; set; }
        [LoadColumn(15)]
        public float DepDelay { get; set; }
        [LoadColumn(16)]
        public string Origin { get; set; }
        [LoadColumn(17)]
        public string Dest { get; set; }
        [LoadColumn(18)]
        public float Distance { get; set; }
        [LoadColumn(19)]
        public float TaxiIn { get; set; }
        [LoadColumn(20)]
        public float TaxiOut { get; set; }
        [LoadColumn(21)]
        public float Cancelled { get; set; }
        [LoadColumn(22)]
        public float Diverted { get; set; }
        [LoadColumn(23)]
        public float CarrierDelay { get; set; }
        [LoadColumn(24)]
        public float WeatherDelay { get; set; }
        [LoadColumn(25)]
        public float NASDelay { get; set; }
        [LoadColumn(26)]
        public float SecurityDelay { get; set; }        
        [LoadColumn(27)]
        [ColumnName("Label")]
        public float LateAircraftDelay { get; set; } 
        [LoadColumn(28)]
        public float Id { get; set; }



        public string getData(string separator)
        {
            //return this.Year + separator + this.Month + separator + this.DayofMonth + separator + this.DayOfWeek + separator + this.DepTime + separator + this.CRSDepTime + separator + this.ArrTime + separator + this.CRSArrTime + separator + MLSQL.EscapeStringSQL(this.UniqueCarrier) + separator + this.FlightNum + separator + MLSQL.EscapeStringSQL(this.TailNum) + separator + this.ActualElapsedTime + separator + this.CRSElapsedTime + separator + this.AirTime + separator + this.ArrDelay + separator + this.DepDelay + separator + MLSQL.EscapeStringSQL(this.Origin) + separator + MLSQL.EscapeStringSQL(this.Dest) + separator + this.Distance + separator + this.TaxiIn + separator + this.TaxiOut + separator + this.Cancelled + separator + this.Diverted + separator + this.CarrierDelay + separator + this.WeatherDelay + separator + this.NASDelay + separator + this.SecurityDelay + separator + this.Id + separator + this.LateAircraftDelay;
            return this.Year + separator + this.Month + separator + this.DayofMonth + separator + this.DayOfWeek + separator + this.DepTime + separator + this.CRSDepTime + separator + this.ArrTime + separator + this.CRSArrTime + separator + MLSQL.EscapeStringSQL(this.UniqueCarrier) + separator + this.FlightNum + separator + this.ActualElapsedTime + separator + this.CRSElapsedTime + separator + this.AirTime + separator + this.ArrDelay + separator + this.DepDelay + separator + MLSQL.EscapeStringSQL(this.Origin) + separator + MLSQL.EscapeStringSQL(this.Dest) + separator + this.Distance + separator + this.TaxiIn + separator + this.TaxiOut + separator + this.Cancelled + separator + this.Diverted + separator + this.CarrierDelay + separator + this.WeatherDelay + separator + this.NASDelay + separator + this.SecurityDelay + separator + this.LateAircraftDelay + separator + this.Id;
            //return this.Year + separator + this.Month + separator + this.DayofMonth + separator + this.DayOfWeek + separator + this.DepTime + separator + this.CRSDepTime + separator + this.ArrTime + separator + this.CRSArrTime + separator + this.FlightNum + separator + this.ActualElapsedTime + separator + this.CRSElapsedTime + separator + this.AirTime + separator + this.ArrDelay + separator + this.DepDelay + separator + this.Distance + separator + this.TaxiIn + separator + this.TaxiOut + separator + this.Cancelled + separator + this.Diverted + separator + this.CarrierDelay + separator + this.WeatherDelay + separator + this.NASDelay + separator + this.SecurityDelay + separator + this.Id + separator + this.LateAircraftDelay;
        }

        public string GetHeader(string separator)
        {
            //return "Year" + separator + "Month" + separator + "DayofMonth" + separator + "DayOfWeek" + separator + "DepTime" + separator + "CRSDepTime" + separator + "ArrTime" + separator + "CRSArrTime" + separator + "UniqueCarrier" + separator + "FlightNum" + separator + "TailNum" + separator + "ActualElapsedTime" + separator + "CRSElapsedTime" + separator + "AirTime" + separator + "ArrDelay" + separator + "DepDelay" + separator + "Origin" + separator + "Dest" + separator + "Distance" + separator + "TaxiIn" + separator + "TaxiOut" + separator + "Cancelled" + separator + "Diverted" + separator + "CarrierDelay" + separator + "WeatherDelay" + separator + "NASDelay" + separator + "SecurityDelay" + separator + "Id" + separator + "Label";
            return "Year" + separator + "Month" + separator + "DayofMonth" + separator + "DayOfWeek" + separator + "DepTime" + separator + "CRSDepTime" + separator + "ArrTime" + separator + "CRSArrTime" + separator + "UniqueCarrier" + separator + "FlightNum" + separator + "ActualElapsedTime" + separator + "CRSElapsedTime" + separator + "AirTime" + separator + "ArrDelay" + separator + "DepDelay" + separator + "Origin" + separator + "Dest" + separator + "Distance" + separator + "TaxiIn" + separator + "TaxiOut" + separator + "Cancelled" + separator + "Diverted" + separator + "CarrierDelay" + separator + "WeatherDelay" + separator + "NASDelay" + separator + "SecurityDelay" + separator + "Label" + separator + "Id";
            //return "Year" + separator + "Month" + separator + "DayofMonth" + separator + "DayOfWeek" + separator + "DepTime" + separator + "CRSDepTime" + separator + "ArrTime" + separator + "CRSArrTime" + separator + "FlightNum" + separator + "ActualElapsedTime" + separator + "CRSElapsedTime" + separator + "AirTime" + separator + "ArrDelay" + separator + "DepDelay" + separator + "Distance" + separator + "TaxiIn" + separator + "TaxiOut" + separator + "Cancelled" + separator + "Diverted" + separator + "CarrierDelay" + separator + "WeatherDelay" + separator + "NASDelay" + separator + "SecurityDelay" + separator + "Id" + separator + "LateAircraftDelay";
            //return "Year" + separator + "Month" + separator + "DayofMonth" + separator + "DayOfWeek" + separator + "DepTime" + separator + "CRSDepTime" + separator + "ArrTime" + separator + "CRSArrTime" + separator + "FlightNum" + separator + "ActualElapsedTime" + separator + "CRSElapsedTime" + separator + "AirTime" + separator + "ArrDelay" + separator + "DepDelay" + separator + "Distance" + separator + "TaxiIn" + separator + "TaxiOut" + separator + "Cancelled" + separator + "Diverted" + separator + "CarrierDelay" + separator + "WeatherDelay" + separator + "NASDelay" + separator + "SecurityDelay" + separator + "Id" + separator + "Label";
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
            this.Year = reader.GetFloat("Year");
            this.Month = reader.GetFloat("Month");
            this.DayofMonth = reader.GetFloat("DayofMonth");
            this.DayOfWeek = reader.GetFloat("DayOfWeek");
            this.DepTime = reader.GetFloat("DepTime");
            this.CRSDepTime = reader.GetFloat("CRSDepTime");
            this.ArrTime = reader.GetFloat("ArrTime");
            this.CRSArrTime = reader.GetFloat("CRSArrTime");
            this.UniqueCarrier = reader.GetString("UniqueCarrier");
            this.FlightNum = reader.GetFloat("FlightNum");
            //this.TailNum = reader.GetString("TailNum");
            this.ActualElapsedTime = reader.GetFloat("ActualElapsedTime");
            this.CRSElapsedTime = reader.GetFloat("CRSElapsedTime");
            this.AirTime = reader.GetFloat("AirTime");
            this.ArrDelay = reader.GetFloat("ArrDelay");
            this.DepDelay = reader.GetFloat("DepDelay");
            this.Origin = reader.GetString("Origin");
            this.Dest = reader.GetString("Dest");
            this.Distance = reader.GetFloat("Distance");
            this.TaxiIn = reader.GetFloat("TaxiIn");
            this.TaxiOut = reader.GetFloat("TaxiOut");
            this.Cancelled = reader.GetFloat("Cancelled");
            this.Diverted = reader.GetFloat("Diverted");
            this.CarrierDelay = reader.GetFloat("CarrierDelay");
            this.WeatherDelay = reader.GetFloat("WeatherDelay");
            this.NASDelay = reader.GetFloat("NASDelay");
            this.SecurityDelay = reader.GetFloat("SecurityDelay");            
            this.LateAircraftDelay = reader.GetFloat("Label");
            this.Id = reader.GetInt32("Id");
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {
            this.Year = (float)reader.GetDouble(0);
            this.Month = (float)reader.GetDouble(1);
            this.DayofMonth = (float)reader.GetDouble(2);
            this.DayOfWeek = (float)reader.GetDouble(3);
            this.DepTime = (float)reader.GetDouble(4);
            this.CRSDepTime = (float)reader.GetDouble(5);
            this.ArrTime = (float)reader.GetDouble(6);
            this.CRSArrTime = (float)reader.GetDouble(7);
            this.UniqueCarrier = reader.GetString(8);
            this.FlightNum = (float)reader.GetDouble(9);
            //this.TailNum = reader.GetString(10); // remember to shift all the other values
            this.ActualElapsedTime = (float)reader.GetDouble(10);
            this.CRSElapsedTime = (float)reader.GetDouble(11);
            this.AirTime = (float)reader.GetDouble(12);
            this.ArrDelay = (float)reader.GetDouble(13);
            this.DepDelay = (float)reader.GetDouble(14);
            this.Origin = reader.GetString(15);
            this.Dest = reader.GetString(16);
            this.Distance = (float)reader.GetDouble(17);
            this.TaxiIn = (float)reader.GetDouble(18);
            this.TaxiOut = (float)reader.GetDouble(19);
            this.Cancelled = (float)reader.GetDouble(20);
            this.Diverted = (float)reader.GetDouble(21);
            this.CarrierDelay = (float)reader.GetDouble(22);
            this.WeatherDelay = (float)reader.GetDouble(23);
            this.NASDelay = (float)reader.GetDouble(24);
            this.SecurityDelay = (float)reader.GetDouble(25);            
            this.LateAircraftDelay = (float)reader.GetDouble(26);
            this.Id = reader.GetInt32(27);
        }
    }
}
