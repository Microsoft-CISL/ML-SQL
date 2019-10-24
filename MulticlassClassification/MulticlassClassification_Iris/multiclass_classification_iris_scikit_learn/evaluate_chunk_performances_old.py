from python2sql.test.performance_evaluation import EvaluatePredictionTimes
from MulticlassClassification.MulticlassClassification_Iris.multiclass_classification_iris_scikit_learn.iris_multiclass_classification import IrisMultiClassificationPipeline


if __name__ == '__main__':
    #imcp = IrisMultiClassificationPipeline("iris-full.txt", "iris-train.txt", "iris-test.txt")
    imcp = IrisMultiClassificationPipeline("iris-full.txt", "iris-train_with_id.txt", "iris-test_with_id.txt")
    imcp.transform_and_fit()

    data_size = imcp.get_data().shape[0]
    full_dataset_file_path = imcp.get_dataset_file_path()
    data_header = imcp.get_data_header()
    class_attribute = imcp.get_class_attribute()
    ml_pipeline = imcp.get_ml_pipeline()
    OUTPUT_DIR = imcp.get_output_dir()

    # BEGIN SAVE PREDICTION TIMES --------------------------------------------------------------------------------------
    #chunk_sizes = [1, 10, 100, 1000, 10000, 100000, data_size]
    chunk_sizes = [100, 1000, 10000, 100000, data_size]
    output_methods = [None, "console", "db", "file"]
    times_evaluator = EvaluatePredictionTimes(full_dataset_file_path, data_size, data_header, class_attribute,
                                              chunk_sizes, output_methods, ml_pipeline, sep='\t', ml_task='multi-classification')
    times_evaluator.evaluate_pipeline_times()
    times_evaluator.save_performance_to_file(OUTPUT_DIR)
    # END SAVE PREDICTION TIMES ----------------------------------------------------------------------------------------