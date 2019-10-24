using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace BikeSharingDemand.DataStructures
{
    public class DemandObservation : SQLData
    {
        // Note that we're loading only some columns (certain indexes) starting on column number 2
        // Also, the label column is number 16. 
        // Columns 14, 15 are not being loaded from the file.
        [LoadColumn(2)]
        public float Season { get; set; }
        [LoadColumn(3)]
        public float Year { get; set; }
        [LoadColumn(4)]
        public float Month { get; set; }
        [LoadColumn(5)]
        public float Hour { get; set; }
        [LoadColumn(6)]
        public float Holiday { get; set; }
        [LoadColumn(7)]
        public float Weekday { get; set; }
        [LoadColumn(8)]
        public float WorkingDay { get; set; }
        [LoadColumn(9)]
        public float Weather { get; set; }
        [LoadColumn(10)]
        public float Temperature { get; set; }
        [LoadColumn(11)]
        public float NormalizedTemperature { get; set; }
        [LoadColumn(12)]
        public float Humidity { get; set; }
        [LoadColumn(13)]
        public float Windspeed { get; set; }
        [LoadColumn(16)]
        [ColumnName("Label")]
        public float Count { get; set; }   // This is the observed count, to be used a "label" to predict

        [LoadColumn(17)]
        [ColumnName("Id")]
        public float Id { get; set; }



        public string getData(string separator)
        {
            return this.Season + separator + this.Year + separator + this.Month + separator + this.Hour + separator + this.Holiday + separator + Weekday + separator + this.WorkingDay + separator + this.Weather + separator + this.Temperature + separator + this.NormalizedTemperature + separator + this.Humidity + separator + this.Windspeed + separator
                + this.Count + separator + this.Id;
        }

        public string GetHeader(string separator)
        {
            return "Season" + separator + "Year" + separator + "Month" + separator + "Hour" + separator + "Holiday" + separator + "WeekDay" + separator + "WorkingDay" + separator + "Weather" + separator + "Temperature" + separator + "NormalizedTemperature" + separator + "Humidity" + separator + "Windspeed" + separator
                + "Label" + separator + "Id";
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
            return this.getData(separator);
        }

        public string getSQLServerData(string separator)
        {
            return this.getData(separator);
        }

        public void ReadDataFromMYSQL(MySqlDataReader reader)
        {

            this.Season = reader.GetFloat("Season");
            this.Year = reader.GetFloat("Year");
            this.Month = reader.GetFloat("Month");
            this.Hour = reader.GetFloat("Hour");
            this.Holiday = reader.GetFloat("Holiday");
            this.Weekday = reader.GetFloat("Weekday");
            this.WorkingDay = reader.GetFloat("WorkingDay");
            this.Weather = reader.GetFloat("Weather");
            this.Temperature = reader.GetFloat("Temperature");
            this.NormalizedTemperature = reader.GetFloat("NormalizedTemperature");
            this.Humidity = reader.GetFloat("Humidity");
            this.Windspeed = reader.GetFloat("WindSpeed");
            this.Count = reader.GetFloat("Label");  // This is the observed count, to be used a "label" to predict
            this.Id = reader.GetInt32("Id");
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {
            this.Season = (float)reader.GetDouble(0);
            this.Year = (float)reader.GetDouble(1);
            this.Month = (float)reader.GetDouble(2);
            this.Hour = (float)reader.GetDouble(3);
            this.Holiday = (float)reader.GetDouble(4);
            this.Weekday = (float)reader.GetDouble(5);
            this.WorkingDay = (float)reader.GetDouble(6);
            this.Weather = (float)reader.GetDouble(7);
            this.Temperature = (float)reader.GetDouble(8);
            this.NormalizedTemperature = (float)reader.GetDouble(9);
            this.Humidity = (float)reader.GetDouble(10);
            this.Windspeed = (float)reader.GetDouble(11);
            this.Count = (float) reader.GetDouble(12);  // This is the observed count, to be used a "label" to predict
            this.Id = reader.GetInt32(13);
        }
    }

    public static class DemandObservationSample
    {
        public static DemandObservation SingleDemandSampleData =>
                                        // Single data
                                        // instant,dteday,season,yr,mnth,hr,holiday,weekday,workingday,weathersit,temp,atemp,hum,windspeed,casual,registered,cnt
                                        // 13950,2012-08-09,3,1,8,10,0,4,1,1,0.8,0.7576,0.55,0.2239,72,133,205
                                        new DemandObservation()
                                        {
                                            Season = 3,
                                            Year = 1,
                                            Month = 8,
                                            Hour = 10,
                                            Holiday = 0,
                                            Weekday = 4,
                                            WorkingDay = 1,
                                            Weather = 1,
                                            Temperature = 0.8f,
                                            NormalizedTemperature = 0.7576f,
                                            Humidity = 0.55f,
                                            Windspeed = 0.2239f
                                        };
    }
}
