
using CreditCardFraudDetection.Trainer;
using HeartDiseaseDetection;
using Microsoft.ML.Data;

namespace SentimentAnalysisConsoleApp.DataStructures
{
    public class SentimentIssue :SQLData
    {
        [LoadColumn(0)]
        public bool Label { get; set; }
        [LoadColumn(2)]
        public string Text { get; set; }
        [LoadColumn(8)]
        public string Id { get; set; }

        public string getData(string separator)
        {
            return this.Label + separator + this.Text + separator + this.Id;
        }

        public string GetHeader(string separator)
        {
            return "Label" + separator + "Comment" + separator + "Id";
        }

        public string getSQLData(string separator)
        {
            return this.Label + separator + MLSQL.EscapeStringQueryWithStartAndEnding(this.Text) + separator + this.Id;
        }



        //string SQLData.getData(string separator)
        //{
        //    return this.Label + separator + this.Text + separator + this.Id;
        //}

        //string SQLData.GetHeader(string separator)
        //{
        //    return "Label" + separator + "Text" + separator + "Id";
        //}
    }
}
