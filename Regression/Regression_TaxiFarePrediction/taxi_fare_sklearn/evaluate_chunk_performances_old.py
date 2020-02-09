from python2sql.test.performance_evaluation import EvaluatePredictionTimes
from Regression.Regression_TaxiFarePrediction.taxi_fare_sklearn.taxi_fare_regression import TaxiFareRegressionPipeline


if __name__ == '__main__':
    tfrp = TaxiFareRegressionPipeline("taxi-fare-all_with_id.csv", "taxi-fare-train_with_id.csv", "taxi-fare-test_with_id.csv")
    tfrp.transform_and_fit()

    data_size = tfrp.get_data().shape[0]
    full_dataset_file_path = tfrp.get_dataset_file_path()
    data_header = tfrp.get_data_header()
    class_attribute = tfrp.get_class_attribute()
    ml_pipeline = tfrp.get_ml_pipeline()
    OUTPUT_DIR = tfrp.get_output_dir()

    # BEGIN SAVE PREDICTION TIMES --------------------------------------------------------------------------------------
    #chunk_sizes = [1, 10, 100, 1000, 10000, 100000, data_size]
    chunk_sizes = [100, 1000, 10000, 100000, data_size]
    output_methods = [None, "console", "db", "file"]
    times_evaluator = EvaluatePredictionTimes(full_dataset_file_path, data_size, data_header, class_attribute,
                                              chunk_sizes, output_methods, ml_pipeline, ml_task='regression')
    times_evaluator.evaluate_pipeline_times()
    times_evaluator.save_performance_to_file(OUTPUT_DIR)
    # END SAVE PREDICTION TIMES ----------------------------------------------------------------------------------------