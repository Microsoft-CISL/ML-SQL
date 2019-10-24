import os
import pandas as pd
import numpy as np
from sklearn.preprocessing import StandardScaler
from sklearn.preprocessing import OneHotEncoder
from sklearn.base import BaseEstimator, TransformerMixin
from sklearn.pipeline import Pipeline, FeatureUnion
from sklearn.compose import ColumnTransformer
import argparse
import category_encoders as ce # pip install category_encoders
from lightning.regression import SDCARegressor # pip install sklearn-contrib-lightning

from python2sql.ml.ml_pipeline import MLPipeline
from Regression.Regression_TaxiFarePrediction.taxi_fare_sklearn.taxi_fare_sql_pipeline import TaxiFareSQLPipeline
from python2sql.ml.utils import evaluate_regression_results
from python2sql.test.test_executor import TestExecutor


class ItemSelector(BaseEstimator, TransformerMixin):

    def __init__(self, columns):
        self.columns = columns

    def fit(self, x, y=None):
        return self

    def transform(self, data_array):
        return data_array[:, self.columns]


class TaxiFareRegressionPipeline(object):
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
        self.class_attribute = "fare_amount"

        # COMPLETE DATASET
        self.data = pd.read_csv(self.dataset_file_path)
        self.y = self.data[self.class_attribute]
        self.X = self.data.drop([self.class_attribute], axis=1)
        self.data_header = self.data.columns.values
        self.data_class_labels = np.unique(self.y.values)

        # TRAIN
        self.train = pd.read_csv(self.train_dataset_file_path)
        self.train = self.train[(self.train[self.class_attribute] >= 1) & (self.train[self.class_attribute] < 150)]
        # self.train = pd.read_csv(self.train_dataset_file_path, sep='\t', decimal=',')                     # prova con il file di training usato da ML.NET dopo la rimozione degli dati non significativi
        self.y_train = self.train[self.class_attribute]
        self.X_train = self.train.drop([self.class_attribute], axis=1)

        # TEST
        self.test = pd.read_csv(self.test_dataset_file_path)
        self.y_test = self.test[self.class_attribute]
        self.X_test = self.test.drop([self.class_attribute], axis=1)
        # ---------------------------------------------------------------------------------------------

        # FEATURE REMOVAL
        self.drop_features = ["Id"]
        selected_dataset = self.ml_pipeline.drop_features_from_data(self.X_train, self.drop_features)

        # APPLY TRANSFORMATIONS ----------------------------------------------------------------------------------------
        # Scale the numeric columns (i.e., passenger_count, trip_time_in_secs, trip_distance columns)
        scaler = Pipeline(steps=[
            ('scaler', StandardScaler(with_mean=False))
        ])

        # Encode with one-hot encoder the categorical columns
        # ohe = ce.OneHotEncoder(handle_unknown='ignore', cols=["vendor_id", "rate_code", "payment_type"], use_cat_names=True)
        ohe = ce.OneHotEncoder(handle_unknown='value', cols=["vendor_id", "rate_code", "payment_type"],
                               use_cat_names=True)
        cat_encoder = Pipeline(steps=[
            ('one_hot_encoder', ohe)
        ])

        transformer = ColumnTransformer(
            remainder='passthrough',  # passthough features not listed
            transformers=[
                ('continous', scaler, ["passenger_count", "trip_time_in_secs", "trip_distance"]),
                ('categorical', cat_encoder, ["vendor_id", "rate_code", "payment_type"])
            ])
        transformed_dataset = self.ml_pipeline.apply_transformation(selected_dataset, transformer)
        #print(transformed_dataset)
        #print(np.isnan(transformed_dataset).sum())
        # FIXME: non combaciano le features sottoposte alla normalizzazione per media e varianza
        # --------------------------------------------------------------------------------------------------------------

        # TRAIN REGRESSION MODEL
        features = ["passenger_count", "trip_time_in_secs", "trip_distance"]
        for col in ["vendor_id", "rate_code", "payment_type"]:
            values = np.unique(selected_dataset[[col]])
            for val in values:
                features.append("{}_{}".format(col, val))

        regressor = SDCARegressor(loss="squared", random_state=24, alpha=0.1)
        self.ml_pipeline.train_regressor(transformed_dataset, self.y_train.values, features, regressor, save_restore_model=restore_save_trained_model)

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
        table_name = "taxi_fare_with_score"

        ml_sql_pipepline = TaxiFareSQLPipeline(self.data, table_name, self.ml_pipeline, self.predictions, self.probabilities, self.scores)
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

    taxi_fare_pipeline = TaxiFareRegressionPipeline("TAXI_FARE", "taxi-fare-all_with_id.csv", "taxi-fare-train_with_id.csv", "taxi-fare-test_with_id.csv")
    test_executor = TestExecutor(taxi_fare_pipeline, ml_task="regression")

    if test_method == 'sklearn_effectiveness':
        test_executor.evaluate_sklearn_pipeline_effectiveness()
    elif test_method == 'sklearn_efficiency':
        test_executor.evaluate_sklearn_pipeline_chunk_performance()
    elif test_method == 'sklearn_sql_comparison':
        test_executor.compare_sql_sklearn_predictions()
    else:
        raise ValueError(
            "Test method not available ({}). The available choices are 'sklearn_effectiveness', 'sklearn_efficiency' or 'sklearn_sql_comparison'")