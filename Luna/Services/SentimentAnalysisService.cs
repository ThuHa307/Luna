using Microsoft.ML;
using Microsoft.ML.Data;
using System.IO;
using Luna.Models;

namespace Luna.Services
{
    public class SentimentAnalysisService
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly PredictionEngine<SentimentData, SentimentPrediction> _predictionEngine;

        public SentimentAnalysisService()
        {
            _mlContext = new MLContext();

            // Tải dữ liệu
            var dataPath = Path.Combine(Environment.CurrentDirectory, "Data", "LunaFeedbacksTrainning.txt");
            var dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(dataPath, hasHeader: false);

            // Chia tách dữ liệu thành tập huấn luyện và tập kiểm tra
            var trainTestData = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var trainDataView = trainTestData.TrainSet;
            var testDataView = trainTestData.TestSet;

            // Xây dựng pipeline học máy
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.SentimentText))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: nameof(SentimentData.Sentiment), featureColumnName: "Features"));
            //                                   dùng thuật toán SdcaLogisticRegression để train
            // khúc này là train data này=)))
            _model = pipeline.Fit(trainDataView);

            // Tạo PredictionEngine
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
        }

        public bool PredictSentiment(string text)
        {
            var inputData = new SentimentData { SentimentText = text };
            var prediction = _predictionEngine.Predict(inputData);
            return prediction.Prediction;
        }
    }

    public class SentimentPrediction : SentimentData
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}
