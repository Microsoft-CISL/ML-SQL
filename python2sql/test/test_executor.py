from python2sql.ml.utils import evaluate_regression_results
from python2sql.test.performance_evaluation import EvaluatePredictionTimes


class TestExecutor(object):

    def __init__(self, pipeline, sep=',', header=True, transformation_target_attribute=None, ml_task="classification", custom_dataset_format=False):
        self.pipeline = pipeline
        self.sep = sep
        self.header = header
        self.transformation_target_attribute = transformation_target_attribute
        self.ml_task = ml_task
        self.custom_dataset_format = custom_dataset_format

    def evaluate_sklearn_pipeline_effectiveness(self, restore_save_trained_model=False, validation_data='test'):
        self.pipeline.transform_and_fit(restore_save_trained_model=restore_save_trained_model)
        self.pipeline.predict(validation_data=validation_data)

    def compare_sql_sklearn_predictions(self, restore_save_trained_model=False):
        self.pipeline.transform_and_fit(restore_save_trained_model=restore_save_trained_model)
        predictions, probabilities, scores = self.pipeline.predict(validation_data='all')
        # print("SKLEARN predictions: {}".format(predictions[:10]))

        sql_predictions = self.pipeline.predict_sql()
        for sql_prediction in sql_predictions:
            evaluate_regression_results("{}_sql_sklearn_comparison".format(self.pipeline.get_name()),
                                        sql_prediction["real_scores"], sql_prediction["predicted_scores"], self.pipeline.get_output_dir())

    def evaluate_sklearn_pipeline_chunk_performance(self, restore_save_trained_model=False):
        self.pipeline.transform_and_fit(restore_save_trained_model=restore_save_trained_model)

        data_size = self.pipeline.get_data().shape[0]
        full_dataset_file_path = self.pipeline.get_dataset_file_path()
        data_header = self.pipeline.get_data_header()
        class_attribute = self.pipeline.get_class_attribute()
        ml_pipeline = self.pipeline.get_ml_pipeline()
        OUTPUT_DIR = self.pipeline.get_output_dir()
        example_name = self.pipeline.get_name()

        # chunk_sizes = [1, 10, 100, 1000, 10000, 100000, data_size]
        #chunk_sizes = [100, 1000, 10000, 100000, data_size]
        chunk_sizes = [1000]
        #output_methods = [None, "console", "db", "file"]
        input_output_methods = [
            #{"input": "file", "input_format": "CSV", "output": None, "output_format": "NO_OUTPUT"},
            #{"input": "file", "input_format": "CSV", "output": "console", "output_format": "CONSOLE"},
            {"input": "file", "input_format": "CSV", "output": "db", "output_format": "MYSQL"},
            {"input": "file", "input_format": "CSV", "output": "db", "output_format": "SQLSERVER"},
            {"input": "db", "input_format": "MYSQL", "output": "file", "output_format": "CSV"},
            {"input": "db", "input_format": "SQLSERVER", "output": "file", "output_format": "CSV"},
            {"input": "db", "input_format": "MYSQL", "output": "db", "output_format": "MYSQL"},
            {"input": "db", "input_format": "SQLSERVER", "output": "db", "output_format": "SQLSERVER"},
            {"input": "file", "input_format": "CSV", "output": "file", "output_format": "CSV"}
        ]
        times_evaluator = EvaluatePredictionTimes(example_name, full_dataset_file_path, data_size, data_header,
                                                  class_attribute, chunk_sizes, input_output_methods, ml_pipeline,
                                                  sep=self.sep, header=self.header,
                                                  transformation_target_attribute=self.transformation_target_attribute,
                                                  ml_task=self.ml_task, custom_dataset_format=self.custom_dataset_format)

        times_evaluator.evaluate_pipeline_times()
        times_evaluator.save_performance_to_file(OUTPUT_DIR)