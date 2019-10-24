
using HeartDiseaseDetection;
using Microsoft.ML.Data;

namespace SentimentAnalysisConsoleApp.DataStructures
{
    public class SentimentPrediction : SQLData
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        // No need to specify ColumnName attribute, because the field
        // name "Probability" is the column name we want.
        public float Probability { get; set; }

        public float Score { get; set; }

        public string getData(string separator)
        {
            return this.Prediction + separator + this.Probability + separator + this.Score;
        }

        public string GetHeader(string separator)
        {
            return "PredictedLabel" + separator + "Probability" + separator + "Score";
        }

        public string getSQLData(string separator)
        {
            return this.Prediction + separator + this.Probability + separator + this.Score;
        }

        //string SQLData.getData(string separator)
        //{
        //    return this.Prediction + separator + this.Probability + separator + this.Score;
        //}

        //string SQLData.GetHeader(string separator)
        //{
        //    return "PredictedLabel" + separator + "Probability" + separator + "Score";
        //}
    }
}