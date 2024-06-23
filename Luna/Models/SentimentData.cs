using Microsoft.ML.Data;
namespace Luna.Models
{
    public class SentimentData
    {

        [LoadColumn(0)]
        public string SentimentText;
        [LoadColumn(1)]
        public bool Sentiment;
    }
}
