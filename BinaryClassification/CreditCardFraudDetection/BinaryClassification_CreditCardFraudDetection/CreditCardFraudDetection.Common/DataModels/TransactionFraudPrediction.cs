using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.ML.Data;
using ML2SQL;
using MySql.Data.MySqlClient;
//using static Microsoft.ML.Runtime.Data.RoleMappedSchema;

namespace CreditCardFraudDetection.Common.DataModels
{
    public class TransactionFraudPrediction :SQLData
    {
        public bool Label;
        public bool PredictedLabel;
        public float Score;
        public float Probability;

        public string getData(string separator)
        {
            return this.Score + "";
        }

        public string GetHeader(string separator)
        {
            return "Score";
        }

        public string getId()
        {
            throw new NotImplementedException();
        }

        public string getMySQLData(string separator)
        {
           return getData(separator);
        }

        public string getScores(string separator)
        {
            return this.Score+"";
        }

        public string getSQLServerData(string separator)
        {
            return getData(separator);
        }

        public void PrintToConsole()
        {
            Console.WriteLine($"Predicted Label: {PredictedLabel}");
            Console.WriteLine($"Probability: {Probability}  ({Score})");
        }

        public void ReadDataFromMYSQL(MySqlDataReader reader)
        {
            throw new NotImplementedException();
        }

        public void ReadDataFromSQLServer(SqlDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
