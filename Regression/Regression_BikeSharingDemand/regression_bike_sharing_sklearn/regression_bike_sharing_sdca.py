import os
import pandas as pd
import numpy as np
from lightning.regression import SDCARegressor # pip install sklearn-contrib-lightning
import argparse
from python2sql.ml.ml_pipeline import MLPipeline
from Regression.Regression_BikeSharingDemand.regression_bike_sharing_sklearn.bike_sharing_sdca_sql_pipeline import BikeSharingSDCASQLPipeline
from python2sql.ml.utils import evaluate_regression_results
from python2sql.test.test_executor import TestExecutor


class BikeRegressionSDCAPipeline(object):
    def __init__(self, name, dataset_file, train_file, test_file, out_dir=None):
        self.name = name
        SKLEARN_DIR = os.path.dirname(os.path.abspath(__file__))
        DATA_DIR = os.path.abspath('..')
        self.dataset_file_path = os.path.join(DATA_DIR, "dataset", dataset_file)
        self.train_dataset_file_path = os.path.join(DATA_DIR, "dataset", train_file)
        self.test_dataset_file_path = os.path.join(DATA_DIR, "dataset", test_file)
        self.OUTPUT_DIR = os.path.join(SKLEARN_DIR, "assets", "output")

        # INITIALIZE MACHINE LEARNING PIPELINE
        self.ml_pipeline = MLPipeline(self.OUTPUT_DIR)

    def transform_and_fit(self, restore_save_trained_model=False):

        # LOAD DATASET --------------------------------------------------------------------------------
        self.class_attribute = "cnt"

        # COMPLETE DATASET
        self.data = pd.read_csv(self.dataset_file_path)
        self.y = self.data[self.class_attribute]
        self.X = self.data.drop([self.class_attribute], axis=1)
        self.data_header = self.data.columns.values
        self.data_class_labels = np.unique(self.y.values)

        # TRAIN
        self.train = pd.read_csv(self.train_dataset_file_path)
        self.y_train = self.train[self.class_attribute]
        self.X_train = self.train.drop([self.class_attribute], axis=1)

        # TEST
        self.test = pd.read_csv(self.test_dataset_file_path)
        self.y_test = self.test[self.class_attribute]
        self.X_test = self.test.drop([self.class_attribute], axis=1)
        # ---------------------------------------------------------------------------------------------

        # FEATURE REMOVAL
        self.drop_features = ["instant", "dteday", "casual", "registered", "Id"]
        selected_dataset = self.ml_pipeline.drop_features_from_data(self.X_train, self.drop_features)

        # TRAIN REGRESSION MODEL
        regressor = SDCARegressor(loss="squared", random_state=24, alpha=0.04)
        self.ml_pipeline.train_regressor(selected_dataset, self.y_train.values, selected_dataset.columns.values, regressor, save_restore_model=restore_save_trained_model)

    def predict(self, validation_data='test'):
        # MAKE PREDICTIONS AND EVALUATE RESULTS

        if validation_data == 'test':
            X = self.X_test
            y = self.y_test
        elif validation_data == 'all':
            X = self.X
            y = self.y
        else:
            raise ValueError("Invalid value for validation_data parameter. Use 'test' or 'all'.")

        self.predictions, self.probabilities, self.scores = self.ml_pipeline.execute_prediction_pipeline(X, y, ml_task_type="regression")

        return self.predictions, self.probabilities, self.scores

    def predict_sql(self):

        # TRANSLATE MACHINE LEARNING PIPELINE IN SQL
        table_name = "bike_sharing_Sdca"
        db_data = self.data.drop(self.drop_features[:-1], axis=1)  # i mantain in the data the id column
        ml_sql_pipepline = BikeSharingSDCASQLPipeline(db_data, table_name, self.ml_pipeline, self.predictions, self.probabilities,
                                                      self.scores)
        sql_queries = ml_sql_pipepline.generate_sql_queries()
        sql_predictions = ml_sql_pipepline.perform_query(sql_queries[-1])

        return sql_predictions

    def get_data(self):
        return self.data

    def get_data_header(self):
        return self.data_header

    def get_class_labels(self):
        return self.data_class_labels

    def get_ml_pipeline(self):
        return self.ml_pipeline

    def get_dataset_file_path(self):
        return self.dataset_file_path

    def get_class_attribute(self):
        return self.class_attribute

    def get_output_dir(self):
        return self.OUTPUT_DIR

    def get_y(self):
        return self.y

    def get_name(self):
        return self.name


if __name__ == '__main__':

    parser = argparse.ArgumentParser(description='SKLEARN vs SQL credit card pipeline.')
    parser.add_argument('-test', '--test_method', dest='test_method', type=str, action='store',
                        help="The name of test to be performed. The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")
    args = parser.parse_args()

    test_method = args.test_method

    bike_sdca_pipeline = BikeRegressionSDCAPipeline("BIKE_SDCA", "hour_all_with_id.csv", "hour_train_with_id.csv", "hour_test_with_id.csv")
    test_executor = TestExecutor(bike_sdca_pipeline, ml_task="regression")

    if test_method == 'sklearn_effectiveness':
        test_executor.evaluate_sklearn_pipeline_effectiveness()
    elif test_method == 'sklearn_efficiency':
        test_executor.evaluate_sklearn_pipeline_chunk_performance()
    elif test_method == 'sklearn_sql_comparison':
        test_executor.compare_sql_sklearn_predictions()
    else:
        raise ValueError(
            "Test method not available ({}). The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")