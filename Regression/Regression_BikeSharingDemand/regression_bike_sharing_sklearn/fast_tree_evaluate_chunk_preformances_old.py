from python2sql.test.performance_evaluation import EvaluatePredictionTimes
from Regression.Regression_BikeSharingDemand.regression_bike_sharing_sklearn.regression_bike_sharing_gradient_boosting import BikeRegressionFastTreePipeline


if __name__ == '__main__':
    brftp = BikeRegressionFastTreePipeline("hour_all_with_id.csv", "hour_train_with_id.csv", "hour_test_with_id.csv")
    brftp.transform_and_fit()

    data_size = brftp.get_data().shape[0]
    full_dataset_file_path = brftp.get_dataset_file_path()
    data_header = brftp.get_data_header()
    class_attribute = brftp.get_class_attribute()
    ml_pipeline = brftp.get_ml_pipeline()
    OUTPUT_DIR = brftp.get_output_dir()

    # BEGIN SAVE PREDICTION TIMES --------------------------------------------------------------------------------------
    chunk_sizes = [1, 10, 100, 1000, 10000, 100000, data_size]
    #chunk_sizes = [100, 1000, 10000, 100000, data_size]
    output_methods = [None, "console", "db", "file"]
    times_evaluator = EvaluatePredictionTimes(full_dataset_file_path, data_size, data_header, class_attribute,
                                              chunk_sizes, output_methods, ml_pipeline, ml_task='regression')
    times_evaluator.evaluate_pipeline_times()
    times_evaluator.save_performance_to_file(OUTPUT_DIR)
    # END SAVE PREDICTION TIMES ----------------------------------------------------------------------------------------